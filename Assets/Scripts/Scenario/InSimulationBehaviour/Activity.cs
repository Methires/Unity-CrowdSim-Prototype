using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SphereCollider))]
public class Activity : MonoBehaviour
{
    private Animator _animator;
    private SphereCollider _sphereCollider;
    private string _paramName;
    private string _blendParam;
    private bool _complexAction;
    private bool _canExecuteComplexAction;
    private GameObject[] _otherRequiredAgents;
    private bool[] _requiredAgentsNearbyCheck;
    private float _exitTime;
    private float _elapsedTimeCounter;
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
            _isFinished = false;
            _elapsedTimeCounter = 0.0f;
        }
    }
    public GameObject[] OtherAgents
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
                _complexAction = true;
                _canExecuteComplexAction = false;
                _requiredAgentsNearbyCheck = new bool[_otherRequiredAgents.Length];
                _sphereCollider.enabled = true;
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

    void Awake()
    {
        _animator = gameObject.GetComponent<Animator>();
        _sphereCollider = gameObject.GetComponent<SphereCollider>();
        _sphereCollider.radius = 1.5f;
        _sphereCollider.isTrigger = true;
        _sphereCollider.enabled = false;
        IsFinished = true;
    }

    void Update()
    {
        if (!IsFinished)
        {
            if (!_complexAction)
            {
                if (ExitTime > 0.0f)
                {
                    _elapsedTimeCounter += Time.deltaTime;
                }

                if (ExitTime > 0.0f && _elapsedTimeCounter >= ExitTime)
                {
                    IsFinished = true;
                }
            }
            else
            {
                if (!_canExecuteComplexAction)
                {
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
                    if (ExitTime > 0.0f)
                    {
                        _elapsedTimeCounter += Time.deltaTime;
                    }

                    if (ExitTime > 0.0f && _elapsedTimeCounter >= ExitTime)
                    {
                        IsFinished = true;
                    }
                }
            }
        }
        else
        {
            _elapsedTimeCounter = 0.0f;
            _otherRequiredAgents = null;
            _complexAction = false;
            _sphereCollider.enabled = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsFinished)
        {
            if (_complexAction)
            {
                for (int i = 0; i < _otherRequiredAgents.Length; i++)
                {
                    if (other.gameObject == _otherRequiredAgents[i])
                    {
                        _requiredAgentsNearbyCheck[i] = true;
                    }
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsFinished && _complexAction)
        {
            for (int i = 0; i < _otherRequiredAgents.Length; i++)
            {
                if (other.gameObject == _otherRequiredAgents[i])
                {
                    _requiredAgentsNearbyCheck[i] = false;
                }
            }
        }
    }
}
