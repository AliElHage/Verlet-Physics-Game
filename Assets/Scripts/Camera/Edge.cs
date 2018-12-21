using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge
{
    // The extremities of each edge
    public BaseVertex FirstVertex;
    public BaseVertex SecondVertex;

    private float _distance;

    // this returns the initial distance between both ends. It allows us to compute the
    // restitution when the edge distorts
    public float Distance
    {
        get
        {
            return _distance;
        }
    }

    // Set the leftmost edge as the first edge by definition.
    public Edge(BaseVertex firstVertex, BaseVertex secondVertex)
    {
        if(firstVertex.transform.position.x <= secondVertex.transform.position.x)
        {
            FirstVertex = firstVertex;
            SecondVertex = secondVertex;
        }
        else
        {
            FirstVertex = secondVertex;
            SecondVertex = firstVertex;
        }

        _distance = (SecondVertex.transform.position - FirstVertex.transform.position).magnitude;
    }
}
