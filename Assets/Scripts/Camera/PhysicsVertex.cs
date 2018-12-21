using UnityEngine;

public class PhysicsVertex : BaseVertex
{
    // used for initializing movement vector
    public Vector3 _initialMoveVector;

    // set previous and current positions to buffer variables to ensure the actual new position
    // is only applied before rendering
    private Vector3 _tempPreviousPosition;
    private Vector3 _tempCurrentPosition;

    // set previous and current positions
    private Vector3 _previousPosition;
    private Vector3 _currentPosition;

    // store the position of the vertex when it's almost immobile
    private Vector3 _stationaryPosition;

    // false if vertex is not part of any edge
    private bool _isLonely = false;

    public Vector3 InitialMoveVector
    {
        set { _initialMoveVector = value; }
    }

    public Vector3 PreviousPosition
    {
        get { return _previousPosition; }
    }

    // return the current displacement vector
    public Vector3 CurrentMoveVector
    {
        get { return _currentPosition - _previousPosition; }
    }

    // if the previous position was set to one overlapping an obstacle, set to true
    public bool IsPreviousPositionVirtual { get; set; }

	// Use this for initialization
	protected override void Start ()
    {
        _isLonely = Siblings == null || Siblings.Count == 0;
        IsPreviousPositionVirtual = false;

        // set initial move vector
        _currentPosition = transform.position;
        _previousPosition = _currentPosition - _initialMoveVector;

        _tempCurrentPosition = _currentPosition;
        _tempPreviousPosition = _previousPosition;
    }
	
	// Update is called once per frame
	void Update ()
    {
        // if the vertex is lonely, it cannot rely on a shape to call the position update
        // function. Therefore, if lonely, it does it by itself.
        if(_isLonely)
        {
            UpdateVertexPosition();
        }
    }

    // set the vertex's position to be updated right before rendering
    public void SetVertexPositions()
    {
        _currentPosition = _tempCurrentPosition;
        _previousPosition = _tempPreviousPosition;

        transform.position = _currentPosition;
    }

    // add the offset due to edege constraints
    public void UpdateVertexFromConstraint(float offsetX, float offsetY)
    {
        _tempCurrentPosition.x += offsetX;
        _tempCurrentPosition.y += offsetY;
    }

    // update the vertex's position using verlet logic
    public void UpdateVertexPosition()
    {
        _tempCurrentPosition = _currentPosition;
        _tempPreviousPosition = _previousPosition;

        float delX = (_tempCurrentPosition.x - _tempPreviousPosition.x) * 0.95f;
        float delY = (_tempCurrentPosition.y - _tempPreviousPosition.y) * 0.95f;

        _tempPreviousPosition.x = _tempCurrentPosition.x;
        _tempPreviousPosition.y = _tempCurrentPosition.y;

        // the -0.1f represents the gravity. Ideally, it would NOT be a hard-coded value
        _tempCurrentPosition.x += delX;
        _tempCurrentPosition.y += delY - 0.1f;
    }

    // constrain the vertex' position if it's out of bounds
    public void ConstrainPoint()
    {
        // 0.95f is the tolerance. It yields better results to not completely restore the position
        // in one iteration
        float delY = (_tempCurrentPosition.y - _tempPreviousPosition.y) * 0.95f;

        // this is to prevent going underground. Ideally, it would NOT be a hard-coded value.
        if (_tempCurrentPosition.y < -30.0f)
        {
            _tempCurrentPosition.y = -30.0f;
            _tempPreviousPosition.y = _tempCurrentPosition.y + delY * 0.7f;
        }
    }

    public void Collide(Vector3 firstEdgeVertex, Vector3 secondEdgeVertex)
    {
        Vector3 edgeVector = secondEdgeVertex - firstEdgeVertex;

        Vector3 incomingVector = CurrentMoveVector;
        Vector3 normalVector = new Vector3(-edgeVector.y, edgeVector.x, 0).normalized;

        // rebound vector
        Vector3 reflexionVector = incomingVector - 2.0f * Vector3.Dot(incomingVector, normalVector) * normalVector;

        // point of collision
        Vector3 intersectPoint = new Vector3();
        float pointSlope = 0.0f;
        float edgeSlope = 0.0f;
        float pointOffset = 0.0f;
        float edgeOffset = 0.0f;

        bool isVerticalDisplacement = Mathf.Approximately(incomingVector.x, 0.0f);
        bool isVerticalEdge = Mathf.Approximately(edgeVector.x, 0.0f);

        // the vertex is sliding on the line
        if(isVerticalDisplacement && isVerticalEdge)
        {
            return;
        }
        // the vertex is falling straight down
        else if(isVerticalDisplacement)
        {
            intersectPoint.x = _previousPosition.x;

            edgeSlope = edgeVector.y / edgeVector.x;
            edgeOffset = firstEdgeVertex.y - edgeSlope * firstEdgeVertex.x;

            intersectPoint.y = edgeSlope * intersectPoint.x + edgeOffset;
        }
        // the vertex is hitting a vertical edge
        else if(isVerticalEdge)
        {
            intersectPoint.x = firstEdgeVertex.x;

            pointSlope = incomingVector.y / incomingVector.x;
            pointOffset = _previousPosition.y - pointSlope * _previousPosition.x;

            intersectPoint.y = pointSlope * intersectPoint.x + pointOffset;
        }
        // the edge and the displacement of the vertex have a finite slope
        else
        {
            pointSlope = incomingVector.y / incomingVector.x;
            pointOffset = _previousPosition.y - pointSlope * _previousPosition.x;

            edgeSlope = edgeVector.y / edgeVector.x;
            edgeOffset = firstEdgeVertex.y - edgeSlope * firstEdgeVertex.x;

            intersectPoint.x = (edgeOffset - pointOffset) / (pointSlope - edgeSlope);
            intersectPoint.y = edgeSlope * intersectPoint.x + edgeOffset;
        }

        // set the position back to the intersection since it has crossed it
        _currentPosition.x = intersectPoint.x;
        _currentPosition.y = intersectPoint.y;

        // update previous position to be the virtual reflexion of the displacement
        _previousPosition.x = _currentPosition.x - reflexionVector.x * 0.8f;
        _previousPosition.y = _currentPosition.y - reflexionVector.y * 0.7f;

        // kick the vertex upwards if its move component is too small to re-intersect with the
        // edge
        if (CurrentMoveVector.y < Time.deltaTime * 1.5f)
        {
            _currentPosition.y = intersectPoint.y + Time.deltaTime * 1.5f;
        }

        // previous position now overlaps the edge
        IsPreviousPositionVirtual = true;
    }
}
