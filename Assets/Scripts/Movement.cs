using UnityEngine;

[RequireComponent(typeof(NavMeshAgent))]
public class Movement : MonoBehaviour
{
    //Reference to required components
    private NavMeshAgent _nMA;
    //Parameters for NavMeshAgent
    private float _speed;
    private float _destinationOffset = 1.0f;
    //Point to be visited by agent
    private Vector3 _destination;
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
            _nMA.speed = _speed;
        }
    }

    public Vector3 Destination
    {
        get
        {
            return _destination;
        }

        set
        {
            _destination = value;
            _nMA.Resume();
            _nMA.destination = value;
            IsFinished = false;
        }
    }

    void Awake()
    {
        _nMA = GetComponent<NavMeshAgent>();
        IsFinished = true;
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
        if (_nMA.remainingDistance < _nMA.stoppingDistance + _destinationOffset)
        {
            IsFinished = true;
            _nMA.Stop();
        }
    }
}
