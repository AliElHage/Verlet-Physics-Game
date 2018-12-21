using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// This is the Turkey class. Ideally, Turkeys would inherit from this class, but I ended up
// doing turkey-specific logic here because of time constrints
public class VerletShape : MonoBehaviour
{
    [SerializeField]
    private float _maxJumpHeight;

    [SerializeField]
    private float _jumpCooldown;

    [SerializeField]
    private float _minXCoord;

    [SerializeField]
    private float _maxXCoord;

    [SerializeField]
    private float _walkSpeed;

    [SerializeField]
    private float _jumpSpeed;

    [SerializeField]
    private List<GameObject> _vertices;

    [SerializeField]
    private List<StickLister> _verletSticks;

    [SerializeField]
    private SkyField _skyField;

    [SerializeField]
    private GameObject _eyeVertex;

    private List<Point> _points;
    private List<Stick> _sticks;

    private float _walkComponent;
    private float _jumpComponent;
    private float _jumpTimer;

    // only allow jumping if the shape is not jumping
    private bool _isJumping = false;

    // this is to update the list of objects that can collide with the shape
    private EventManager _eventManager;
    private GameObject[] _collidableObjects;

    // keep track for each projectile of which edges were just hit
    private Dictionary<Projectile, Dictionary<Stick, bool>> _collisionStateByEdge;

    public List<Stick> Sticks { get { return _sticks; } }

    // Use this for initialization
    void Start ()
    {
        if (Camera.main != null)
        {
            VerletDrawer vd = Camera.main.transform.GetComponent<VerletDrawer>();
            if (vd != null)
            {
                vd.Shapes.Add(this);
            }
        }

        // Make stick list from points currently stored
        MakeEdges();

        // connect action to event so that the list of collidable objects is updated every time
        // the event is fired.
        _eventManager = (EventManager)FindObjectOfType(typeof(EventManager));

        UnityAction updateCollidablesListener = () =>
        {
            _collidableObjects = GameObject.FindGameObjectsWithTag("PhysicsObject");
            UpdateCollisionStates();
        };

        _eventManager.Connect("UpdateCollidableObjects", updateCollidablesListener);

        // set the initial walk speed
        _walkComponent = _walkSpeed * Time.deltaTime;
    }
	
	// Update is called once per frame
	void Update ()
    {
        // update positions of each verlets using their individual movement components
        UpdatePositions();

        // handle collision with projectile, if it happens
        CheckProjectileCollision();

        // update constraints related to sticks and relative position. This step is repeated because
        // it yields nice results. Other ones are also valid.
        for(int i = 0; i < 3; i++)
        {
            UpdateSticks();
            UpdateConstraints();
        }

        // update the position of the eye of the turkey. Ideally this would not require logic
        // specific to it
        _eyeVertex.transform.position = _points[_points.Count - 1].ToVector3();

        // count time since last jump
        _jumpTimer += Time.deltaTime;
        _isJumping = false;
    }

    private void UpdatePositions()
    {
        // update walk direction if beyond a certain threshold
        Vector3 turkeyPosition = GetPosition();
        if (turkeyPosition.x > -50.0f)
        {
            _walkComponent = _walkSpeed * Time.deltaTime * -1;
        }
        else if (turkeyPosition.x < -85.0f)
        {
            _walkComponent = _walkSpeed * Time.deltaTime;
        }

        // check whether the turkey should jump
        bool shouldJump = ShouldJump();
        if(shouldJump)
        {
            _jumpComponent = _jumpSpeed;
            _isJumping = true;
            _jumpTimer = 0.0f;
        }
        
        // update verlet position
        for (int i = 0; i < _points.Count; i++)
        {
            Point p = _points[i];

            float delX = p.CurrentX - p.PreviousX;
            float delY = p.CurrentY - p.PreviousY;

            p.PreviousX = p.CurrentX + _walkComponent;
            p.PreviousY = p.CurrentY;

            // compute and cap fall speed. If it's too big, the turkey gets deformed and ugly
            float fallSpeed = delY - 0.1f + _jumpComponent;
            fallSpeed = fallSpeed < -2.0f ? -2.0f : fallSpeed;

            // wind is non-zero if there is wind and if the turkey is at the appropriate height.
            float windSpeed = 0.0f;
            if(_skyField != null && p.CurrentY > _skyField.transform.position.y)
            {
                windSpeed = _skyField.CurrentWindSpeed;
            }

            p.CurrentX += delX + _walkComponent + windSpeed * Time.deltaTime;
            p.CurrentY += fallSpeed;
        }

        // ensure the jump impulse is only applied once
        if(_isJumping)
        {
            _jumpComponent = 0.0f;
        }
    }

    private void UpdateSticks()
    {
        foreach (Stick stick in _sticks)
        {
            // compare original and current magnitudes and bring the latter closer to the former
            Vector3 stickVector = new Vector3(stick.P1.CurrentX - stick.P0.CurrentX,
                                              stick.P1.CurrentY - stick.P0.CurrentY, 0);

            float currentLength = stickVector.magnitude;

            float difference = stick.Distance - currentLength;
            float distortionFactor = difference / stick.Distance / 2.0f;

            float offsetX = distortionFactor * stickVector.x;
            float offsetY = distortionFactor * stickVector.y;

            // partially restore position. Otherwise, gross instabilities arise in the verlet system
            stick.P0.CurrentX -= offsetX * 0.85f;
            stick.P0.CurrentY -= offsetY * 0.85f;
            stick.P1.CurrentX += offsetX * 0.85f;
            stick.P1.CurrentY += offsetY * 0.85f;
        }
    }

    // check if the turkeys are colliding with the wall or ground
    private void UpdateConstraints()
    {
        foreach (Point p in _points)
        {
            float delX = p.CurrentX - p.PreviousX;
            float delY = p.CurrentY - p.PreviousY;

            if (p.CurrentY < -50.0f)
            {
                p.CurrentY = -50.0f;
                p.PreviousY = p.CurrentY + delY;
            }

            if (p.CurrentX < -100.0f)
            {
                p.CurrentX = -100.0f;
                p.PreviousX = p.CurrentX + delX;
            }
            else if(p.CurrentX > -40.0f)
            {
                p.CurrentX = -40.0f;
                p.PreviousX = p.CurrentX + delX;
            }
        }
    }

    private void CheckProjectileCollision()
    {
        if(_collidableObjects == null)
        {
            return;
        }

        // for each fired projectile
        for (int num = 0; num < _collidableObjects.Length; num++)
        {
            bool hasCollided = false;
            Projectile projectile = _collidableObjects[num].GetComponent<Projectile>();
            if(projectile == null)
            {
                continue;
            }

            // check collision for each stick
            for (int i = 0; i < _sticks.Count; i++)
            {
                if(!_sticks[i].IsConstraint)
                {
                    // get radius of spherical projectile
                    float radius = projectile.Radius;

                    // get edge and position points for SAT
                    Vector3 p = projectile.transform.position;
                    Vector3 e1 = _sticks[i].P0.ToVector3();
                    Vector3 e2 = _sticks[i].P1.ToVector3();

                    // only check for collisions if the stick is projectile is in range of the stick
                    bool isWithinX = (p.x + radius >= e1.x && p.x - radius <= e2.x) ||
                                     (p.x + radius >= e2.x && p.x - radius <= e1.x) ||
                                     (p.x < e1.x && p.x + radius > e2.x) ||
                                     (p.x < e2.x && p.x + radius > e1.x);
                    bool isWithinY = (p.y + radius >= e1.y && p.y - radius <= e2.y) ||
                                     (p.y + radius >= e2.y && p.y - radius <= e1.y) ||
                                     (p.y < e1.y && p.y + radius > e2.y) ||
                                     (p.y < e2.y && p.y + radius > e1.y);

                    if (isWithinX && isWithinY)
                    {
                        // compute unit normal of edge looking towards the point p
                        Vector3 edgeVector = e2 - e1;
                        Vector3 unitNormal = new Vector3(-edgeVector.y, edgeVector.x, 0).normalized;

                        // since there are two possible dot products, get the one looking towards p
                        float perpDotProduct = SignedTriangleArea(e1, e2, p);
                        if (perpDotProduct >= 0.0f)
                        {
                            if (unitNormal.y < 0.0f)
                            {
                                unitNormal *= -1.0f;
                            }
                        }
                        else
                        {
                            if (unitNormal.y >= 0.0f)
                            {
                                unitNormal *= -1.0f;
                            }
                        }

                        // compute the contact point considering that it happens along the radius
                        // of the projectile
                        Vector3 contactPoint = p - radius * unitNormal;

                        // if the current perp dot product is of an opposite sign than the previous one,
                        // then the point has collided
                        float currentPerpDotProd = SignedTriangleArea(e1, e2, contactPoint);
                        float previousPerpDotProd = SignedTriangleArea(e1, e2, projectile.PreviousPosition - radius * unitNormal);

                        // check whether the new previous position is overlapping with an edge
                        bool isPreviousOverlapping = false;
                        if (_collisionStateByEdge.ContainsKey(projectile))
                        {
                            Dictionary<Stick, bool> collisionByProjectile = _collisionStateByEdge[projectile];
                            if (collisionByProjectile.ContainsKey(_sticks[i]))
                            {
                                isPreviousOverlapping = collisionByProjectile[_sticks[i]];
                                collisionByProjectile[_sticks[i]] = false;
                            }
                        }

                        // only if the virtual previous position does not overlap, collide
                        if (!isPreviousOverlapping && (currentPerpDotProd / previousPerpDotProd < 0.0f))
                        {
                            hasCollided = true;
                            projectile.Collide(e1, e2, contactPoint, unitNormal);

                            // compute recoil of edge
                            Vector3 recoil = projectile.CurrentDisplacement.magnitude * unitNormal;
                            _sticks[i].P0.PreviousX += recoil.x;
                            _sticks[i].P0.PreviousY += recoil.y;
                            _sticks[i].P1.PreviousX += recoil.x;
                            _sticks[i].P1.PreviousY += recoil.y;

                            if (projectile.CurrentDisplacement.y > 0.1f)
                            {
                                if (_collisionStateByEdge.ContainsKey(projectile))
                                {
                                    Dictionary<Stick, bool> collisionByProjectile = _collisionStateByEdge[projectile];
                                    if (collisionByProjectile.ContainsKey(_sticks[i]))
                                    {
                                        collisionByProjectile[_sticks[i]] = true;
                                    }
                                    else
                                    {
                                        collisionByProjectile.Add(_sticks[i], true);
                                    }
                                }
                            }
                            else
                            {
                                // this is a makeshift way of solving a bug where the projectile would
                                // fall through the ground because of floating point errors
                                projectile.KickUpwards();
                            }
                        }
                    }
                    else
                    {
                        // book-keeping
                        if (_collisionStateByEdge.ContainsKey(projectile))
                        {
                            Dictionary<Stick, bool> collisionByProjectile = _collisionStateByEdge[projectile];
                            if (collisionByProjectile.ContainsKey(_sticks[i]))
                            {
                                collisionByProjectile[_sticks[i]] = false;
                            }
                            else
                            {
                                collisionByProjectile.Add(_sticks[i], false);
                            }
                        }
                    }
                }
            }

            // if the projectile collides with a turkey, it must disappear immediately
            if (hasCollided)
            {
                DestroyImmediate(projectile.gameObject);
                _eventManager.Invoke("UpdateCollidableObjects");
            }
        }
    }

    // compute SAT (perp dot prod)
    private float SignedTriangleArea(Vector3 e1, Vector3 e2, Vector3 p)
    {
        return (e2.x - e1.x) * (p.y - e2.y) - (p.x - e2.x) * (e2.y - e1.y);
    }

    // helper functions /////////////////////
    public void MakeEdges()
    {
        _points = new List<Point>();
        foreach (GameObject vert in _vertices)
        {
            Vector3 position = vert.transform.position;
            _points.Add(new Point(position.x, position.y));
        }

        _sticks = new List<Stick>();
        foreach (StickLister verletStick in _verletSticks)
        {
            
            if(verletStick.Pair.Count > 1)
            {
                GameObject firstVerlet = verletStick.Pair[0];
                GameObject secondVerlet = verletStick.Pair[1];

                Point firstPoint = _points[_vertices.IndexOf(firstVerlet)];
                Point secondPoint = _points[_vertices.IndexOf(secondVerlet)];

                Stick newStick = new Stick(firstPoint, secondPoint);
                newStick.IsConstraint = verletStick.isConstraint;

                _sticks.Add(newStick);
            }
        }
    }

    public void UpdateCollisionStates()
    {
        if (_collisionStateByEdge == null)
        {
            _collisionStateByEdge = new Dictionary<Projectile, Dictionary<Stick, bool>>();
        }
        for (int i = 0; i < _collidableObjects.Length; i++)
        {
            Projectile collidable = _collidableObjects[i].GetComponent<Projectile>();
            if (collidable != null)
            {
                Dictionary<Stick, bool> statesByProjectile = new Dictionary<Stick, bool>();
                for (int j = 0; j < _sticks.Count; j++)
                {
                    statesByProjectile.Add(_sticks[j], false);
                }

                if (!_collisionStateByEdge.ContainsKey(collidable))
                {
                    _collisionStateByEdge.Add(collidable, statesByProjectile);
                }
                else
                {
                    _collisionStateByEdge[collidable] = statesByProjectile;
                }
            }
        }

        List<Projectile> pairsToRemove = new List<Projectile>();
        foreach (KeyValuePair<Projectile, Dictionary<Stick, bool>> projectileStates in _collisionStateByEdge)
        {
            bool hasBeenDestroyed = false;
            for (int i = 0; i < _collidableObjects.Length; i++)
            {
                if (_collidableObjects[i].Equals(projectileStates.Key))
                {
                    hasBeenDestroyed = true;
                }
            }

            if (hasBeenDestroyed)
            {
                pairsToRemove.Add(projectileStates.Key);
            }
        }

        for (int i = 0; i < pairsToRemove.Count; i++)
        {
            _collisionStateByEdge.Remove(pairsToRemove[i]);
        }
    }

    private Vector3 GetPosition()
    {
        Vector3 position = new Vector3();
        for(int i = 0; i < _points.Count; i++)
        {
            position.x += _points[i].CurrentX;
            position.y += _points[i].CurrentY;
        }

        return position /= _points.Count;
    }

    private bool ShouldJump()
    {
        if(_jumpTimer < _jumpCooldown || _isJumping)
        {
            return false;
        }
        else
        {
            _jumpCooldown = Random.Range(7.5f, 12.5f);
            int lottery = Random.Range(0, 100);
            if(lottery < 10)
            {
                return true;
            }
        }

        return false;
    }
}
