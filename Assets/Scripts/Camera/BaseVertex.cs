using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseVertex : MonoBehaviour
{
    [SerializeField]
    private List<BaseVertex> _siblings;

    public bool _showMesh = false;

    public List<BaseVertex> Siblings
    {
        get
        {
            if(_siblings == null)
            {
                _siblings = new List<BaseVertex>();
            }

            return _siblings;
        }
    }

    // Use this for initialization
    protected virtual void Start ()
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if(renderer != null)
        {
            renderer.enabled = _showMesh;
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    // We use methods to manipulate the list of siblings instead of a property to ensure that the list
    // cannot be accessed directly.
    public bool AddSibling(BaseVertex newSibling)
    {
        if (_siblings == null)
        {
            _siblings = new List<BaseVertex>();
        }

        // Add sibling only if it doesn't already exist in the list of siblings.
        BaseVertex match = _siblings.Find(s => s.Equals(newSibling));
        if(match == null)
        {
            _siblings.Add(newSibling);
            return true;
        }

        return false;
    }

    public bool RemoveSibling(BaseVertex sibling)
    {
        if (_siblings == null)
        {
            return false;
        }

        // Remove sibling only if it exists in the list of siblings.
        BaseVertex match = _siblings.Find(s => s.Equals(sibling));
        if (match != null)
        {
            _siblings.Remove(sibling);
            return true;
        }

        return false;
    }
}
