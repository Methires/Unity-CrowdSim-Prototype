using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SphereCollider))]
public class Action : MonoBehaviour
{
    //Reference for required components
    private Animator _animator;
    private SphereCollider _sphereCollider;
    //End action conditions
    //Time related
    private bool _considerExitTime;
    private float _exitTime;
    private float _elapsedTimeCounter;
    //Object in proximity related
    private bool _considerExitObject;
    private GameObject _exitObject;
    //Animation related
    private bool _considerAnimation;
    private AnimationClip _animationClip;
    private bool _loopAnimation;

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

    public bool ConsiderExitTime
    {
        get
        {
            return _considerExitTime;
        }

        set
        {
            _considerExitTime = value;
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

    public bool ConsiderExitObject
    {
        get
        {
            return _considerExitObject;
        }

        set
        {
            _considerExitObject = value;
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

    public bool ConsiderAnimation
    {
        get
        {
            return _considerAnimation;
        }

        set
        {
            _considerAnimation = value;
        }
    }

    public AnimationClip AnimationClip
    {
        get
        {
            return _animationClip;
        }

        set
        {
            _animationClip = value;
            IsFinished = false;
        }
    }

    public bool LoopAnimation
    {
        get
        {
            return _loopAnimation;
        }

        set
        {
            _loopAnimation = value;
        }
    }

    void Awake()
    {
        _animator = gameObject.GetComponent<Animator>();
        _sphereCollider = gameObject.GetComponent<SphereCollider>();
        _sphereCollider.radius = 1.0f;
        _sphereCollider.isTrigger = true;
        if (_animator == null || _sphereCollider == null)
        {
            Debug.Log(gameObject.name + " has no Animator or Sphere Component attached to it");
        }
        IsFinished = true;
    }

    void Start ()
    {
	}


    void Update()
    {
        if (!IsFinished && ConsiderExitTime)
        {
            _elapsedTimeCounter += Time.deltaTime;
        }

        if (ConsiderExitTime && _elapsedTimeCounter >= _exitTime)
        {
            IsFinished = true;
            Debug.Log(gameObject.name + " has run out of time");
        }

        if (IsFinished)
        {
            //:D
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsFinished && ConsiderExitObject)
        { 
            if (other.gameObject == ExitObject.gameObject)
            {
                IsFinished = true;
                Debug.Log(gameObject.name + " has " + other.gameObject.name + " in proximity");
            }
        }
    }
}
