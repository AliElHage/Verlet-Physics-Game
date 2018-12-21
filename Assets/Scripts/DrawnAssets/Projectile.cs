using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A lot of the code here is duplicated from PhysicsVertex.cs. This one is barely commented, so if you have
// any uncertainties, you can refer to that class
public class Projectile : MonoBehaviour
{
    [SerializeField]
    private float _radius;

    [SerializeField]
    private float _maxLifetime;

    private SkyField _skyField;

    private Vector3 _previousPosition;
    private Vector3 _currentPosition;

    private float _immobileCounter = 0.0f;
    private float _lifetimeCounter = 0.0f;
    private int _kickUpCounter = 0;

    private EventManager _eventManager;

    public float Radius
    {
        get { return _radius; }
    }

    public Vector3 PreviousPosition
    {
        get { return _previousPosition; }
        set { _previousPosition = value; }
    }

    public Vector3 CurrentPosition
    {
        get { return _currentPosition; }
    }

    public Vector3 CurrentDisplacement
    {
        get { return _currentPosition - _previousPosition; }
    }

    // Use this for initialization
    void Start ()
    {
        _eventManager = (EventManager)FindObjectOfType(typeof(EventManager));
        _skyField = (SkyField)FindObjectOfType(typeof(SkyField));
        _currentPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update ()
    {
        float delX = _currentPosition.x - _previousPosition.x;
        float delY = _currentPosition.y - _previousPosition.y;

        _previousPosition.x = _currentPosition.x;
        _previousPosition.y = _currentPosition.y;

        float windSpeed = 0.0f;
        if (_skyField != null && _currentPosition.y > _skyField.transform.position.y)
        {
            windSpeed = _skyField.CurrentWindSpeed;
        }

        _currentPosition.x += delX + windSpeed * Time.deltaTime;
        _currentPosition.y += delY - 1.5f * Time.deltaTime;

        transform.position = _currentPosition;
    }

    public void LateUpdate()
    {
        if (CurrentDisplacement.magnitude < Time.deltaTime)
        {
            _immobileCounter += Time.deltaTime;
            if (_immobileCounter > 3.0f)
            {
                DestroyImmediate(gameObject);
                _eventManager.Invoke("UpdateCollidableObjects");
            }
        }

        if(_currentPosition.y < -55.0f || _kickUpCounter == 3 || _lifetimeCounter > _maxLifetime)
        {
            DestroyImmediate(gameObject);
            _eventManager.Invoke("UpdateCollidableObjects");
        }

        _lifetimeCounter += Time.deltaTime;
    }

    public void Collide(Vector3 e1, Vector3 e2, Vector3 contactPoint, Vector3 normalVector)
    {
        Vector3 edgeVector = e2 - e1;
        Vector3 incomingVector = CurrentDisplacement;
        Vector3 reflexionVector = incomingVector - 2.0f * Vector3.Dot(incomingVector, normalVector) * normalVector;

        Vector3 intersectPoint = new Vector3();
        float pointSlope = 0.0f;
        float edgeSlope = 0.0f;
        float pointOffset = 0.0f;
        float edgeOffset = 0.0f;

        bool isVerticalDisplacement = Mathf.Approximately(incomingVector.x, 0.0f);
        bool isVerticalEdge = Mathf.Approximately(edgeVector.x, 0.0f);

        // the vertex is sliding on the line
        if (isVerticalDisplacement && isVerticalEdge)
        {
            return;
        }
        // the vertex is falling straight down
        else if (isVerticalDisplacement)
        {
            intersectPoint.x = contactPoint.x;

            edgeSlope = edgeVector.y / edgeVector.x;
            edgeOffset = e1.y - edgeSlope * e1.x;

            intersectPoint.y = edgeSlope * intersectPoint.x + edgeOffset;
        }
        // the vertex is hitting a vertical edge
        else if (isVerticalEdge)
        {
            intersectPoint.x = e1.x;

            pointSlope = incomingVector.y / incomingVector.x;
            pointOffset = contactPoint.y - pointSlope * contactPoint.x;

            intersectPoint.y = pointSlope * intersectPoint.x + pointOffset;
        }
        // the edge and the displacement of the vertex have a finite slope
        else
        {
            pointSlope = incomingVector.y / incomingVector.x;
            pointOffset = contactPoint.y - pointSlope * contactPoint.x;

            edgeSlope = edgeVector.y / edgeVector.x;
            edgeOffset = e1.y - edgeSlope * e1.x;

            intersectPoint.x = (edgeOffset - pointOffset) / (pointSlope - edgeSlope);
            intersectPoint.y = edgeSlope * intersectPoint.x + edgeOffset;
        }

        _currentPosition.x = intersectPoint.x + _radius * normalVector.x;
        _currentPosition.y = intersectPoint.y + _radius * normalVector.y;
        
        // update previous position to be the virtual reflexion of the displacement
        _previousPosition.x = _currentPosition.x - reflexionVector.x * 0.8f;
        _previousPosition.y = _currentPosition.y - reflexionVector.y * 0.7f;
    }

    // bug handling related to floating point errors
    public void KickUpwards()
    {
        _previousPosition.y += 0.1f;
        _currentPosition.y += 0.1f;

        _kickUpCounter++;
    }
}
