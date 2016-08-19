using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Animations;

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
    private List<GameObject> _otherRequiredAgents;
    private bool[] _requiredAgentsNearbyCheck;
    private float _exitTime;
    private float _elapsedTimeCounter;
    private bool _isFinished;

    public DynamicAnimationState _dynamicAnimationState;

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
            _isFinished = false;
            _elapsedTimeCounter = 0.0f;

            _dynamicAnimationState = new DynamicAnimationState(_animator, _paramName);
            _exitTime = _dynamicAnimationState.Length;
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

    private void CreateLocalAnimatorControllerCopy()
    {
        AnimatorController currentController = _animator.runtimeAnimatorController as AnimatorController;
        AnimatorController newController = currentController.Clone();
        newController.name = _animator.gameObject.name + "LocalController";
        _animator.runtimeAnimatorController = newController;
    }

    void Awake()
    {
        _animator = gameObject.GetComponent<Animator>();
        _sphereCollider = gameObject.GetComponent<SphereCollider>();
        _sphereCollider.radius = 1.5f;
        _sphereCollider.isTrigger = true;
        _sphereCollider.enabled = false;
        _isFinished = true;

        CreateLocalAnimatorControllerCopy();
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
                    _dynamicAnimationState.EnterState();
                }

                if (ExitTime > 0.0f && _elapsedTimeCounter >= ExitTime)
                {
                    _isFinished = true;
                    _dynamicAnimationState.ExitState();
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
                        _dynamicAnimationState.EnterState();
                    }

                    if (ExitTime > 0.0f && _elapsedTimeCounter >= ExitTime)
                    {
                        _dynamicAnimationState.ExitState();
                        _isFinished = true;
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

    void OnTriggerExit(Collider other)
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
