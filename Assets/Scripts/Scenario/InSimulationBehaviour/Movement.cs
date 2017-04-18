using System;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
public class Movement : MonoBehaviour
{
    private UnityEngine.AI.NavMeshAgent _nMA;
    private float _speed;
    public Vector3 _destination;
    private Quaternion _finalRotation;
    private Agent _agent;
    public bool _isFinished;
    public bool _isInPosition = false;
    private bool _settingDestinationFailed = false;
    private string _blendParam;

    private string _nameToDisplay;
    private int _levelIndex;

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
                _nMA.Resume();
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

    public Quaternion FinalRotation
    {
        get
        {
            return _finalRotation;
        }

        set
        {
            _finalRotation = value;
        }
    }

    public string NameToDisplay
    {
        get
        {
            return _nameToDisplay;
        }
    }

    public int LevelIndex
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

    void Awake()
    {
        _nMA = GetComponent<UnityEngine.AI.NavMeshAgent>();
        _isFinished = true;
        _agent = GetComponent<Agent>();
    }

    void Update()
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
                else
                {
                    
                    CheckRotation();
                }
            }
        }
    }

    private void CheckPosition()
    {     
        Clamping();
        if (Mathf.Abs(Vector3.Distance(transform.position, _destination)) <= 0.26f)
        {
            _isInPosition = true;
            _nMA.destination = transform.position;
            _nMA.Stop();
            SpeedAdjuster sA = GetComponent<SpeedAdjuster>();
            if (sA != null)
            {
                sA.Adjust = false;
            }
        }
        else if (Mathf.Abs(Vector3.Distance(_nMA.destination, _destination)) > 0.1f)
        {
            _nMA.Resume();
            _nMA.SetDestination(_destination);
        }

        _agent.MovementInPlace = _isInPosition;
    }

    private void Clamping()
    {
        if (tag != "Crowd")
        {
            if (Mathf.Abs(Vector3.Distance(transform.position, _destination)) < 0.75f)
            {
                transform.position = Vector3.Lerp(transform.position, _destination, Time.deltaTime);
            } 
        }
    }

    private void CheckRotation()
    {
        if (!_agent.ApplyFinalRotation || _agent.IsInPlace())
        {
            _isFinished = true;
        }
        if (tag == "Crowd")
        {
            _isFinished = true;
        }
    }
}
