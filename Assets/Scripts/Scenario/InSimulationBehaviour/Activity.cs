using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SphereCollider))]
public class Activity : MonoBehaviour
{
    private const string IDLE_STATE_NAME = "Idling";

    private Animator _animator;
    private SphereCollider _sphereCollider;
    private string _paramName;
    private bool _complexAction;
    private bool _canExecuteComplexAction;

    public List<GameObject> _otherRequiredAgents;

    private bool[] _requiredAgentsNearbyCheck;
    private float _exitTime;
    private float _elapsedTimeCounter;
    private bool _isFinished;
    private NavMeshAgent _navMeshAgent;

    private string _nameToDisplay;
    private int _levelIndex;

    public bool IsFinished
    {
        get
        {
            return _isFinished;
        }
    }
    public float ExitTime
    {
        get
        {
            return _exitTime;
        }
        set
        {
            _exitTime = value;
        }
    }
    public string ParamName
    {
        get
        {
            return _paramName;
        }
        set
        {
            _paramName = value;
            string[] name = _paramName.Split('@');
            _nameToDisplay = name[1];
            _isFinished = false;
            _elapsedTimeCounter = 0.0f;
            GetComponent<NavMeshAgent>().obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.NoObstacleAvoidance;
        }
    }
    public List<GameObject> OtherAgents
    {
        get
        {
            return _otherRequiredAgents;
        }

        set
        {
            _otherRequiredAgents = value;
            if (_otherRequiredAgents != null)
            {
                _otherRequiredAgents.Remove(gameObject);
                _requiredAgentsNearbyCheck = new bool[_otherRequiredAgents.Count];
                _complexAction = true;
                _canExecuteComplexAction = false;
                _sphereCollider.enabled = true;
            }
            else
            {
                _complexAction = false;
            }
        }
    }
    public string NameToDisplay
    {
        get
        {
            if (_complexAction && !_canExecuteComplexAction)
            {
                _nameToDisplay = IDLE_STATE_NAME;
            }
            return _nameToDisplay;
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
            string[] name = _paramName.Split('@');
            return name[0];
        }
    }
    public bool IsComplex
    {
        get
        {
            return _complexAction && _canExecuteComplexAction && _nameToDisplay != IDLE_STATE_NAME;
        }
    }

    private void Awake()
    {
        _animator = gameObject.GetComponent<Animator>();
        _sphereCollider = gameObject.GetComponent<SphereCollider>();
        _navMeshAgent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        _sphereCollider.radius = 3.0f;
        _sphereCollider.isTrigger = true;
        _sphereCollider.enabled = false;
        _isFinished = true;
    }

    private void Update()
    {
        if (!IsFinished)
        {
            _navMeshAgent.enabled = false;
            if (!_complexAction)
            {
                if (ExitTime > 0.0f && _elapsedTimeCounter <= ExitTime)
                {
                    _navMeshAgent.enabled = false;
                    _elapsedTimeCounter += Time.deltaTime;
                }

                if (ExitTime > 0.0f && _elapsedTimeCounter >= ExitTime)
                {
                    _isFinished = true;
                    _navMeshAgent.enabled = true;
                }
            }
            else
            {
                if (!_canExecuteComplexAction)
                {
                    _nameToDisplay = IDLE_STATE_NAME;
                    _canExecuteComplexAction = true;
                    foreach (bool agentNearbyCheck in _requiredAgentsNearbyCheck)
                    {
                        if (!agentNearbyCheck)
                        {
                            _canExecuteComplexAction = false;
                            break;
                        }
                    }
                }
                else
                {
                    _sphereCollider.enabled = false;
                    string[] name = _paramName.Split('@');
                    _nameToDisplay = name[1];
                    if (ExitTime > 0.0f && _elapsedTimeCounter <= ExitTime)
                    {
                        _navMeshAgent.enabled = false;
                        _elapsedTimeCounter += Time.deltaTime;
                    }

                    if (ExitTime > 0.0f && _elapsedTimeCounter >= ExitTime)
                    {
                        _isFinished = true;
                        _navMeshAgent.enabled = true;
                    }
                }
            }
        }
        else
        {
            _navMeshAgent.enabled = true;
            _navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            _elapsedTimeCounter = 0.0f;
            _otherRequiredAgents = null;
            _complexAction = false;
            _sphereCollider.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetType() == typeof(SphereCollider))
        {
            if (!IsFinished)
            {
                if (_complexAction)
                {
                    for (int i = 0; i < _otherRequiredAgents.Count; i++)
                    {
                        if (other.gameObject == _otherRequiredAgents[i])
                        {
                            _requiredAgentsNearbyCheck[i] = true;
                        }
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetType() == typeof(SphereCollider))
        {
            if (!IsFinished && _complexAction)
            {
                for (int i = 0; i < _otherRequiredAgents.Count; i++)
                {
                    if (other.gameObject == _otherRequiredAgents[i])
                    {
                        _requiredAgentsNearbyCheck[i] = false;
                    }
                }
            }
        }
    }
}
