using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SphereCollider))]
public class Action : MonoBehaviour
{
    //Reference for required components
    private Animator _animator;
    private SphereCollider _sphereCollider;
    //Transition parameter name 
    private string _paramName;
    //End action conditions
    //Time related
    private float _exitTime;
    private float _elapsedTimeCounter;
    //Object in proximity related
    private GameObject _exitObject;

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
            IsFinished = false;
        }
    }
    public GameObject ExitObject
    {
        get
        {
            return _exitObject;
        }

        set
        {
            _exitObject = value;
            IsFinished = false;
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
            foreach (var parameter in _animator.parameters)
            {
                if (parameter.name == _paramName)
                {
                    _animator.SetBool(_paramName, true);
                }
            }
            _isFinished = false;
        }
    }

    void Awake()
    {
        _animator = gameObject.GetComponent<Animator>();
        _sphereCollider = gameObject.GetComponent<SphereCollider>();
        _sphereCollider.radius = 1.0f;
        _sphereCollider.isTrigger = true;
        IsFinished = true;
    }

    void Update()
    {
        if (!IsFinished)
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
            foreach (var parameter in _animator.parameters)
            {
                if (parameter.name == ParamName)
                {
                    _animator.SetBool(ParamName, false);
                }
            } 
            _exitTime = 0.0f;
            _elapsedTimeCounter = 0.0f;
            _exitObject = null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsFinished && ExitObject != null)
        { 
            if (other.gameObject == ExitObject.gameObject)
            {
                IsFinished = true;
            }
        }
    }
}
