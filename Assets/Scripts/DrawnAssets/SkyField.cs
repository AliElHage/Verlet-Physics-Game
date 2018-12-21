using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyField : MonoBehaviour
{
    [SerializeField]
    private float _minWindSpeed;

    [SerializeField]
    private float _maxWindSpeed;

    [SerializeField]
    private float _windChangeInterval;

    [SerializeField]
    private List<GameObject> _clouds;

    [SerializeField]
    private float _cloudLeftBound;

    [SerializeField]
    private float _cloudRightBound;

    private float _windChangeCounter;
    private float _currentWindSpeed;

    public float CurrentWindSpeed { get { return _currentWindSpeed; } }

    // Use this for initialization
    void Start ()
    {
        _windChangeCounter = _windChangeInterval += 1.0f;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(_windChangeCounter > _windChangeInterval)
        {
            _currentWindSpeed = Random.Range(_minWindSpeed, _maxWindSpeed);
            _windChangeCounter = 0.0f;
        }

        _windChangeCounter += Time.deltaTime;

        for(int i = 0; i < _clouds.Count; i++)
        {
            Vector3 newPosition = _clouds[i].transform.position;
            newPosition.x += _currentWindSpeed;

            if(newPosition.x > _cloudRightBound)
            {
                newPosition.x = _cloudLeftBound;
            }
            else if(newPosition.x < _cloudLeftBound)
            {
                newPosition.x = _cloudRightBound;
            }

            _clouds[i].transform.position = newPosition;
        }
    }
}
