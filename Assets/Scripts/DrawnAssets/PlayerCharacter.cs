using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    [SerializeField]
    private GameObject _cannonSprite;

    [SerializeField]
    private float _speed;

    [SerializeField]
    private float _sensitivity;

    [SerializeField]
    private GameObject _cannonBallPrefab;

    private EventManager _eventManager;

    // Use this for initialization
    void Start ()
    {
        _eventManager = (EventManager)FindObjectOfType(typeof(EventManager));
    }
	
	// Update is called once per frame
	void Update ()
    {
        /*Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 targetLocation = mousePosition - _cannonSprite.transform.position;

        float aimAngle = Mathf.Atan(targetLocation.y / targetLocation.x) * Mathf.Rad2Deg;

        Vector3 newRotation = _cannonSprite.transform.rotation.eulerAngles;
        newRotation.z = aimAngle;

        _cannonSprite.transform.rotation = Quaternion.Euler(newRotation);*/


        float aimAngle = _cannonSprite.transform.rotation.eulerAngles.z;
        
        if(Input.GetKey(KeyCode.UpArrow))
        {
            float delTheta = _sensitivity * Time.deltaTime;
            aimAngle -= delTheta;
            aimAngle = aimAngle < 0.0f ? aimAngle += 360.0f : aimAngle;
            aimAngle = aimAngle > 360.0f ? aimAngle -= 360.0f : aimAngle;
            
            if(aimAngle < 270.0f)
            {
                aimAngle = 270.0f;
            }

            _cannonSprite.transform.rotation = Quaternion.Euler(0, 0, aimAngle);
        }
        else if(Input.GetKey(KeyCode.DownArrow))
        {
            float delTheta = _sensitivity * Time.deltaTime;
            aimAngle += delTheta;
            
            if (aimAngle < 90.0f)
            {
                aimAngle = 0.0f;
            }

            _cannonSprite.transform.rotation = Quaternion.Euler(0, 0, aimAngle);
        }

        if(Input.GetKeyDown(KeyCode.Space))
        {
            Projectile cannonBall = Instantiate(_cannonBallPrefab).GetComponent<Projectile>();
            Vector3 initialPosition = _cannonSprite.transform.position;

            cannonBall.transform.position = initialPosition;
            cannonBall.PreviousPosition = new Vector3(initialPosition.x + Mathf.Cos(aimAngle * Mathf.Deg2Rad) * _speed,
                                                      initialPosition.y + Mathf.Sin(aimAngle * Mathf.Deg2Rad) * _speed);

            _eventManager.Invoke("UpdateCollidableObjects");
        }

        if (Input.GetMouseButtonDown(0))
        {
            //PhysicsVertex cannonBall = Instantiate(_cannonBallPrefab).GetComponent<PhysicsVertex>();
            Projectile cannonBall = Instantiate(_cannonBallPrefab).GetComponent<Projectile>();
            Vector3 initialPosition = _cannonSprite.transform.position;

            cannonBall.transform.position = initialPosition;
            cannonBall.PreviousPosition = new Vector3(initialPosition.x + Mathf.Cos(aimAngle * Mathf.Deg2Rad) * _speed,
                                                      initialPosition.y + Mathf.Sin(aimAngle * Mathf.Deg2Rad) * _speed);

            _eventManager.Invoke("UpdateCollidableObjects");
        }
    }
}
