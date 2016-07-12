using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class Movement : MonoBehaviour
{
    //Reference to required components
    private NavMeshAgent _nMA;
    //Parameters for NavMeshAgent
    private float _speed = 10.0f;
    private float _destinationOffset = 1.0f;
    //List of points to be visited by agent
    private List<Vector3> _wayPoints;
    private int _nextWayPointIndex;
    //Values to be set when following an object is desired
    private bool isFollowing;
    private GameObject _followedGameObject;
    private bool _isFinished;

    public bool IsFinished
    {
        get
        {
            return _isFinished;
        }

        private set
        {
            _isFinished = value;
        }
    }

    public float Speed
    {
        get
        {
            return _speed;
        }

        set
        {
            _speed = Mathf.Clamp(value, 0.0f, float.MaxValue);
        }
    }

    public List<Vector3> WayPoints
    {
        get
        {
            return _wayPoints;
        }

        set
        {
            _wayPoints = value;
            _nextWayPointIndex = 0;
            SetNewDestination();
            _nMA.Resume();
            IsFinished = false;
        }
    }

    public GameObject FollowedGameObject
    {
        get
        {
            return _followedGameObject;
        }

        set
        {
            _followedGameObject = value;
            IsFinished = false;
            _nMA.Resume();
        }
    }

    public bool IsFollowing
    {
        get
        {
            return isFollowing;
        }

        set
        {
            isFollowing = value;
        }
    }

    void Awake()
    {
        _nMA = GetComponent<NavMeshAgent>();
        if (_nMA == null)
        {
            Debug.Log(gameObject.name + " has no NavMeshAgent Component attached to it");
        }
        IsFinished = true;
    }

    void Start()
    {     
    }

    void Update()
    {
        if (!IsFinished)
        {
            CheckPosition();
        }        
    }
 
    private void CheckPosition()
    {
        if (IsFollowing)
        {
            if ((Mathf.Abs(FollowedGameObject.transform.position.x - transform.position.x) < _destinationOffset
                && Mathf.Abs(FollowedGameObject.transform.position.z - transform.position.z) < _destinationOffset))
            {
                _nMA.Stop();
                IsFinished = true;
                
            }
            else
            {
                SetNewDestination();
            }
        }
        else
        {
            if ((Mathf.Abs(_nMA.destination.x - transform.position.x) < _destinationOffset && Mathf.Abs(_nMA.destination.z - transform.position.z) < _destinationOffset))
            {
                if (_nextWayPointIndex < WayPoints.Count )
                {
                    SetNewDestination();
                }
                else
                {
                    IsFinished = true;
                }
            }
        }
    }

    private void SetNewDestination()
    {
        if (IsFollowing)
        {
            _nMA.destination = new Vector3
            {
                x = FollowedGameObject.transform.position.x,
                y = transform.position.y,
                z = FollowedGameObject.transform.position.z
            };
            _nMA.speed = _speed;
        }
        else
        {
            _nMA.destination = new Vector3
            {
                x = WayPoints[_nextWayPointIndex].x,
                y = transform.position.y,
                z = WayPoints[_nextWayPointIndex].z
            }; ;
            _nextWayPointIndex++;
            _nMA.speed = _speed;
        }
    }
}
