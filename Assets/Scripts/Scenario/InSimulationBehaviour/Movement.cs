using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Movement : MonoBehaviour
{
    [SerializeField]
    private bool _isFinished;
    [SerializeField]
    private bool _isInPosition = false;
    [SerializeField]
    private Vector3 _destination;

    private bool _settingDestinationFailed = false;

    private int _levelIndex;

    private float _speed;

    private string _nameToDisplay;

    private NavMeshAgent _nMA;

    public bool IsFinished
    {
        get
        {
            return _isFinished;
        }
    }
    public int LevelId
    {
        get
        {
            return _levelIndex;
        }

        set
        {
            _levelIndex = value;
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
            if (_speed < 3.0f)
            {
                _nameToDisplay = "Walk";
            }
            else
            {
                _nameToDisplay = "Run";
            }
        }
    }
    public string NameToDisplay
    {
        get
        {
            return _nameToDisplay;
        }
    }
    public string ActorName
    {
        get
        {
            return name;
        }
    }
    public string MocapId
    {
        get
        {
            return "";
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
            if (_nMA.enabled)
            {
                _nMA.isStopped = false;
                _nMA.destination = value;
                _isFinished = false;
                _isInPosition = false;
            }
            else
            {
                _settingDestinationFailed = true;
            }

        }
    }

    private void Awake()
    {
        _nMA = GetComponent<NavMeshAgent>();
        _isFinished = true;
    }

    private void Update()
    {
        if (!IsFinished)
        {
            if (_nMA.enabled)
            {
                if (_settingDestinationFailed)
                {
                    _settingDestinationFailed = false;
                    Destination = _destination;
                }

                if (!_isInPosition)
                {
                    CheckPosition();
                }
            }
        }
    }

    private void CheckPosition()
    {     
        ForcePositionClamp();
        if (Mathf.Abs(Vector3.Distance(transform.position, _destination)) <= 0.26f)
        {
            _isInPosition = true;
            _nMA.destination = transform.position;
            _nMA.isStopped = true;
            SpeedAdjuster sA = GetComponent<SpeedAdjuster>();
            if (sA != null)
            {
                sA.Adjust = false;
            }
        }
        else if (Mathf.Abs(Vector3.Distance(_nMA.destination, _destination)) > 0.1f)
        {
            _nMA.isStopped = false;
            _nMA.SetDestination(_destination);
        }
    }

    private void ForcePositionClamp()
    {
        if (tag != "Crowd")
        {
            if (Mathf.Abs(Vector3.Distance(transform.position, _destination)) < 0.75f)
            {
                transform.position = Vector3.Lerp(transform.position, _destination, Time.deltaTime);
            } 
        }
    }
}
