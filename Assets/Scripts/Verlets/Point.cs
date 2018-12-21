using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class keeps track of the position of each verlet, irrespective of the gameobject it
// was initiated from. This class overlaps with the BaseVertex/PhysicsVertex one. Ideally there
// would only be one or the other.
public class Point
{
    public float CurrentX { get; set; }
    public float CurrentY { get; set; }
    public float PreviousX { get; set; }
    public float PreviousY { get; set; }

    public Point(float x, float y, float oldX, float oldY)
    {
        CurrentX = x;
        CurrentY = y;
        PreviousX = oldX;
        PreviousY = oldY;
    }

    public Point(float x, float y)
    {
        CurrentX = x;
        CurrentY = y;
        PreviousX = x;
        PreviousY = y;
    }

    public Vector3 ToVector3()
    {
        return new Vector3(CurrentX, CurrentY, 0);
    }
}
