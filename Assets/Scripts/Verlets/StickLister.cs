using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class simply makes Sticks visible in editor
[System.Serializable]
public class StickLister
{
    public List<GameObject> Pair = new List<GameObject>(2);
    public bool isConstraint = false;
}
