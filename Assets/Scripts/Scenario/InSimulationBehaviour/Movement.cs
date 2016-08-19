using UnityEngine;

[RequireComponent(typeof(NavMeshAgent))]
public class Movement : MonoBehaviour
{
    private NavMeshAgent _nMA;
    private float _speed;
    private Vector3 _destination;
    private bool _isFinished;
    private string _blendParam;

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
            CheckPosition();
        }
    }

    private void CheckPosition()
    {
        if (_nMA.remainingDistance < _nMA.stoppingDistance + Mathf.Epsilon)
        {
            if (Vector3.Distance(_destination, transform.position) < _nMA.stoppingDistance * 2)
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
