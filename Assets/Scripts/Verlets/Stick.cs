using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This Stick class holds two points and is the equivalent of an edge for the verlet physics.
// Like the point class, this one overlaps with Edge and should ideally be one same class.
public class Stick
{
    private float _distance;

    public Point P0 { get; set; }
    public Point P1 { get; set; }
    public float Distance { get { return _distance; } }

    // if the stick is marked as a constraint, it will not be rendered and will not generate
    // collision events
    public bool IsConstraint { get; set; }

    // set both points and compute their initial distance, which defines the length of the stick
    public Stick(Point p0, Point p1)
    {
        P0 = p0;
        P1 = p1;

        float delX = P1.CurrentX - P0.CurrentX;
        float delY = P1.CurrentY - P0.CurrentY;

        _distance = Mathf.Sqrt(delX * delX + delY * delY);
    }
}
