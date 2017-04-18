using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Animations;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SphereCollider))]
public class Activity : MonoBehaviour
{
    private const string IDLE_STATE_NAME = "Standing";
    private Animator _animator;
    private SphereCollider _sphereCollider;
    private string _paramName;
    private string _blendParam;
    private bool _complexAction;
    private bool _canExecuteComplexAction;

    public List<GameObject> _otherRequiredAgents;

    private bool[] _requiredAgentsNearbyCheck;
    private float _exitTime;
    private float _elapsedTimeCounter;
    private bool _isFinished;
    private UnityEngine.AI.NavMeshAgent _navMeshAgent;
    private Bounds _actionBounds;
    private Bounds _idleStateBounds;
    private GameObject _actionArena;
    private GameObject _idleStateArena;

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
            GetComponent<UnityEngine.AI.NavMeshAgent>().obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.NoObstacleAvoidance;
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

    public Bounds ActionBounds
    {
        get
        {
            return _actionBounds;
        }

        set
        {
            _actionBounds = value;
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

    void Awake()
    {
        _animator = gameObject.GetComponent<Animator>();
        _sphereCollider = gameObject.GetComponent<SphereCollider>();
        _navMeshAgent = gameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
        _sphereCollider.radius = 3.0f;
        _sphereCollider.isTrigger = true;
        _sphereCollider.enabled = false;
        _isFinished = true;
        _idleStateBounds = new Bounds(Vector3.zero, new Vector3(0.1f, 1.0f, 0.1f));
    }

    void Update()
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
            _navMeshAgent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            _elapsedTimeCounter = 0.0f;
            _otherRequiredAgents = null;
            _complexAction = false;
            _sphereCollider.enabled = false;
        }
    }

    void OnTriggerEnter(Collider other)
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

    void OnTriggerExit(Collider other)
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
