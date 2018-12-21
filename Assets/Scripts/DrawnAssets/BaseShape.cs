using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseShape : MonoBehaviour
{
    [SerializeField]
    private List<BaseVertex> _vertices;

    private List<Edge> _edges;

    private GameObject[] _collidableObjects;

    private EventManager _eventManager;

    private Dictionary<Projectile, Dictionary<Edge, bool>> _collisionStateByEdge;

    public List<BaseVertex> Vertices
    {
        get { return _vertices; }
    }

    public List<Edge> Edges
    {
        get
        {
            if (_edges == null)
            {
                _edges = new List<Edge>();
            }

            return _edges;
        }
    }

    // Use this for initialization
    virtual protected void Start()
    {
        MakeEdges();

        if (Camera.main != null)
        {
            DrawLines dl = Camera.main.transform.GetComponent<DrawLines>();
            if (dl != null)
            {
                dl.AddShape(this);
            }
        }

        _eventManager = (EventManager)FindObjectOfType(typeof(EventManager));

        UnityAction updateCollidablesListener = () =>
        {
            _collidableObjects = GameObject.FindGameObjectsWithTag("PhysicsObject");
            UpdateCollisionStates();
        };

        _eventManager.Connect("UpdateCollidableObjects", updateCollidablesListener);
        _eventManager.Invoke("UpdateCollidableObjects");
    }

    // Update is called once per frame
    virtual protected void Update()
    {
        CheckForCollisions();
    }

    public void CheckForCollisions()
    {
        if (_collidableObjects == null)
        {
            return;
        }

        for (int i = 0; i < _collidableObjects.Length; i++)
        {
            PhysicsVertex physVertex = _collidableObjects[i].GetComponent<PhysicsVertex>();
            if(physVertex == null)
            {
                Projectile projectile = _collidableObjects[i].GetComponent<Projectile>();
                if(projectile != null)
                {
                    CheckProjectileCollision(projectile);
                }
                
                continue;
            }

            if (IsOwnedVertex(physVertex))
            {
                continue;
            }
            
            for (int j = 0; j < _edges.Count; j++)
            {
                Vector3 p = physVertex.transform.position;
                Vector3 e1 = _edges[j].FirstVertex.transform.position;
                Vector3 e2 = _edges[j].SecondVertex.transform.position;

                bool isWithinX = (p.x >= e1.x && p.x <= e2.x) || (p.x >= e2.x && p.x <= e1.x) ||
                                 (Mathf.Abs(p.x - e1.x) < Mathf.Abs(physVertex.CurrentMoveVector.x));
                bool isWithinY = (p.y >= e1.y && p.y <= e2.y) || (p.y >= e2.y && p.y <= e1.y) ||
                                 (Mathf.Abs(p.y - e1.y) < Mathf.Abs(physVertex.CurrentMoveVector.y));

                if (isWithinX && isWithinY)
                {
                    float currentPerpDotProd = SignedTriangleArea(e1, e2, p);
                    float previousPerpDotProd = SignedTriangleArea(e1, e2, physVertex.PreviousPosition);

                    if (physVertex.IsPreviousPositionVirtual)
                    {
                        previousPerpDotProd = SignedTriangleArea(e1, e2, p);
                        physVertex.IsPreviousPositionVirtual = false;
                    }

                    bool hasHitPoint = Mathf.Abs((_collidableObjects[i].transform.position - _edges[j].FirstVertex.transform.position).magnitude) < 0.1f ||
                                       Mathf.Abs((_collidableObjects[i].transform.position - _edges[j].SecondVertex.transform.position).magnitude) < 0.1f;

                    if ((Mathf.Abs(previousPerpDotProd) > 0.0001f && (currentPerpDotProd / previousPerpDotProd < 0.0f)) || hasHitPoint)
                    {

                        //physVertex.Collide(e1, e2);
                    }
                }
            }
        }
    }

    private void CheckProjectileCollision(Projectile projectile)
    {
        for (int i = 0; i < _edges.Count; i++)
        {
            float radius = projectile.Radius;

            Vector3 p = projectile.transform.position;
            Vector3 e1 = _edges[i].FirstVertex.transform.position;
            Vector3 e2 = _edges[i].SecondVertex.transform.position;

            bool isWithinX = (p.x + radius >= e1.x && p.x - radius <= e2.x) ||
                             (p.x + radius >= e2.x && p.x - radius <= e1.x);
            bool isWithinY = (p.y + radius >= e1.y && p.y - radius <= e2.y) ||
                             (p.y + radius >= e2.y && p.y - radius <= e1.y);

            if (isWithinX && isWithinY)
            {
                Vector3 edgeVector = e2 - e1;
                Vector3 unitNormal = new Vector3(-edgeVector.y, edgeVector.x, 0).normalized;

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

                Vector3 contactPoint = p - radius * unitNormal;

                float currentPerpDotProd = SignedTriangleArea(e1, e2, contactPoint);
                float previousPerpDotProd = SignedTriangleArea(e1, e2, projectile.PreviousPosition - radius * unitNormal);

                bool isPreviousOverlapping = false;
                if(_collisionStateByEdge.ContainsKey(projectile))
                {
                    Dictionary<Edge, bool> collisionByProjectile = _collisionStateByEdge[projectile];
                    if (collisionByProjectile.ContainsKey(_edges[i]))
                    {
                        isPreviousOverlapping = collisionByProjectile[_edges[i]];
                        collisionByProjectile[_edges[i]] = false;
                    }
                }

                if (!isPreviousOverlapping && (currentPerpDotProd / previousPerpDotProd < 0.0f))
                {
                    projectile.Collide(e1, e2, contactPoint, unitNormal);

                    if(projectile.CurrentDisplacement.y > 0.1f)
                    {
                        if (_collisionStateByEdge.ContainsKey(projectile))
                        {
                            Dictionary<Edge, bool> collisionByProjectile = _collisionStateByEdge[projectile];
                            if (collisionByProjectile.ContainsKey(_edges[i]))
                            {
                                collisionByProjectile[_edges[i]] = true;
                            }
                            else
                            {
                                collisionByProjectile.Add(_edges[i], true);
                            }
                        }
                    }
                    else
                    {
                        projectile.KickUpwards();
                    }
                }
            }
            else
            {
                if (_collisionStateByEdge.ContainsKey(projectile))
                {
                    Dictionary<Edge, bool> collisionByProjectile = _collisionStateByEdge[projectile];
                    if (collisionByProjectile.ContainsKey(_edges[i]))
                    {
                        collisionByProjectile[_edges[i]] = false;
                    }
                    else
                    {
                        collisionByProjectile.Add(_edges[i], false);
                    }
                }
            }
        }
    }

    private float SignedTriangleArea(Vector3 e1, Vector3 e2, Vector3 p)
    {
        return (e2.x - e1.x) * (p.y - e2.y) - (p.x - e2.x) * (e2.y - e1.y);
    }

    public void AddVertex(BaseVertex vertex)
    {
        if (_vertices == null)
        {
            _vertices = new List<BaseVertex>();
        }

        _vertices.Add(vertex);

        MakeEdges();
    }

    public void AddVertex(List<BaseVertex> vertices)
    {
        if (_vertices == null)
        {
            _vertices = new List<BaseVertex>();
        }

        _vertices.AddRange(vertices);

        MakeEdges();
    }

    public void MakeEdges()
    {
        if (_edges == null)
        {
            _edges = new List<Edge>();
        }

        foreach (BaseVertex currentVertex in _vertices)
        {
            for (int i = 0; i < currentVertex.Siblings.Count; i++)
            {
                if (!EdgeExists(currentVertex, currentVertex.Siblings[i]) && IsValidEdge(currentVertex, currentVertex.Siblings[i]))
                {
                    _edges.Add(new Edge(currentVertex, currentVertex.Siblings[i]));
                }
            }
        }
    }

    public bool EdgeExists(BaseVertex currentVertex, BaseVertex siblingVertex)
    {
        for (int i = 0; i < _edges.Count; i++)
        {
            bool isEdge = (_edges[i].FirstVertex.Equals(currentVertex) &&
                           _edges[i].SecondVertex.Equals(siblingVertex)) ||
                          (_edges[i].FirstVertex.Equals(siblingVertex) &&
                           _edges[i].SecondVertex.Equals(currentVertex));

            if (isEdge)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsValidEdge(BaseVertex currentVertex, BaseVertex siblingVertex)
    {
        return !currentVertex.Equals(siblingVertex);
    }

    public bool IsOwnedVertex(PhysicsVertex vertex)
    {
        for (int i = 0; i < _vertices.Count; i++)
        {
            PhysicsVertex comparand = _vertices[i].transform.GetComponent<PhysicsVertex>();
            if (comparand != null && comparand.Equals(vertex))
            {
                return true;
            }
        }

        return false;
    }

    public void UpdateCollisionStates()
    {
        if (_collisionStateByEdge == null)
        {
            _collisionStateByEdge = new Dictionary<Projectile, Dictionary<Edge, bool>>();
        }
        for(int i = 0; i < _collidableObjects.Length; i++)
        {
            Projectile collidable = _collidableObjects[i].GetComponent<Projectile>();
            if (collidable != null)
            {
                Dictionary<Edge, bool> statesByProjectile = new Dictionary<Edge, bool>();
                for (int j = 0; j < Edges.Count; j++)
                {
                    statesByProjectile.Add(Edges[j], false);
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
        foreach (KeyValuePair<Projectile, Dictionary<Edge, bool>> projectileStates in _collisionStateByEdge)
        {
            bool hasBeenDestroyed = false;
            for(int i = 0; i < _collidableObjects.Length; i++)
            {
                if(_collidableObjects[i].Equals(projectileStates.Key))
                {
                    hasBeenDestroyed = true;
                }
            }

            if(hasBeenDestroyed)
            {
                pairsToRemove.Add(projectileStates.Key);
            }
        }

        for(int i = 0; i < pairsToRemove.Count; i++)
        {
            _collisionStateByEdge.Remove(pairsToRemove[i]);
        }
    }
}
