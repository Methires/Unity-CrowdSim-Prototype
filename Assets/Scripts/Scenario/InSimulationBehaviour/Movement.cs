using UnityEngine;

[RequireComponent(typeof(NavMeshAgent))]
public class Movement : MonoBehaviour
{
    private NavMeshAgent _nMA;
    private float _speed;
    private Vector3 _destination;
    private bool _isFinished;
    private string _blendParam;

    private Vector3 _lastPosition;

    public bool IsFinished
    {
        get
        {
            return _isFinished;
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
            _isFinished = false;
        }
    }
    public string BlendParameter
    {
        get
        {
            return _blendParam;
        }

        set
        {
            _blendParam = value;
        }
    }

    void Awake()
    {
        _nMA = GetComponent<NavMeshAgent>();
        _isFinished = true;
    }

    void Update()
    {
        if (!IsFinished)
        {
            if (_lastPosition == transform.position)
            {
                Debug.Log(gameObject.name + " in the same place");
            }
            Debug.Log(gameObject.name + " moving");
            CheckPosition();
            _lastPosition = transform.position;
        }
    }

    private void CheckPosition()
    {
        if (_nMA.remainingDistance < _nMA.stoppingDistance + Mathf.Epsilon)
        {
            if (Vector3.Distance(_destination, transform.position) < 1.0f)
            {
                _isFinished = true;
                _nMA.Stop();
            }
            else
            {
                _nMA.SetDestination(_destination);
            }
        }
    }
}
