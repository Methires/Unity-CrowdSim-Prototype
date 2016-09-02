using UnityEngine;
using UnityEditor;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class NavigationLocomotion : MonoBehaviour
{
    public float SpeedDampTime = 0.8f;
    public float AngularSpeedDampTime = 0.5f;
    public float DirectionResponseTime = 0.6f;
    public float SpeedReductionWeight = 0.3f;
    public float WarpDistance = 0.2f;
    public float MaxSpeed = 2.0f;
    
    private Animator _animator;
    private NavMeshAgent _agent;
    private int _speedId;
    private int _angularSpeedId;
    private int _directionId;
    private int _isInDynamicStateId;

    private bool _inTransition;
    private bool _inIdle;
    private bool _inTurn;
    private bool _inWalkRun;

    void Start ()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();

        _speedId = Animator.StringToHash("Speed");
        _angularSpeedId = Animator.StringToHash("AngularSpeed");
        _directionId = Animator.StringToHash("Direction");
        _isInDynamicStateId = Animator.StringToHash("IsInDynamicState");
        MaxSpeed = Mathf.Clamp(MaxSpeed, 0.0f, _agent.speed);
        _agent.speed = MaxSpeed;

        _agent.updatePosition = false;     
        //_agent.updateRotation = false;
    }
	
	void Update ()
    {
        Vector3 worldDeltaPosition = _agent.nextPosition - transform.position;
        float speed = 0.0f;
        float angle = 0.0f;
        float angularSpeed = 0.0f;
        if (!AgentDone())
        {
            angle = GetAngle(transform.forward, _agent.desiredVelocity);
            angularSpeed = (angle * Mathf.Deg2Rad) / DirectionResponseTime;
            speed = _agent.desiredVelocity.magnitude;
            speed = ScaleToRange(speed, 0.0f, _agent.speed, 0.0f, MaxSpeed - SpeedReductionWeight * angle);
            _agent.speed = speed;
        }

        AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
        _inTransition = _animator.IsInTransition(0);
        _inIdle = state.IsName("Idle");
        _inTurn = state.IsName("TurnOnSpot");
        _inWalkRun = state.IsName("Movement");
        

        float speedDampTime = _inIdle ? 0 : SpeedDampTime;
        float angularSpeedDampTime = _inWalkRun || _inTransition ? AngularSpeedDampTime : 0;
        float directionDampTime = _inTurn || _inTransition ? 1000000 : 0;

        _animator.SetFloat(_speedId, speed, speedDampTime, Time.deltaTime);
        _animator.SetFloat(_angularSpeedId, angularSpeed, angularSpeedDampTime, Time.deltaTime);
        _animator.SetFloat(_directionId, angle, directionDampTime, Time.deltaTime);      

        // Pull agent towards character
        if (worldDeltaPosition.magnitude > _agent.radius / 2.0f)
            _agent.nextPosition = transform.position + 0.99f * worldDeltaPosition;
    }

    void OnAnimatorMove()
    {

        //transform.position = _agent.nextPosition;
        if (!_inIdle || _animator.GetBool(_isInDynamicStateId))
        {
            if (!AgentInWarpingDistance())
            {
                Vector3 position = _animator.rootPosition;
                position.y = _agent.nextPosition.y;
                transform.position = position;

                _agent.velocity = _animator.deltaPosition / Time.deltaTime;
                transform.rotation = _animator.rootRotation;
            }
            else
            {
                transform.position = _agent.nextPosition;
            }
        }
        else
        {
            _animator.ApplyBuiltinRootMotion();
        }              
    }

    private float GetAngle(Vector3 from, Vector3 to)
    {
        float angle = Vector3.Angle(from, to);
        Vector3 normal = Vector3.Cross(from ,to);
        angle *= Mathf.Sign(Vector3.Dot(normal, transform.up));        
        return angle;
    }

    private float ScaleToRange(float value, float formerMin, float formerMax, float newMin, float newMax)
    {
        float percent = (value - formerMin) / (formerMax - formerMin);
        return percent * (newMax - newMin) + newMin;
    }

    protected bool AgentDone()
    {
        return !_agent.pathPending && AgentStopping();
    }

    protected bool AgentStopping()
    {
        return _agent.remainingDistance <= _agent.stoppingDistance;
    }

    protected bool AgentInWarpingDistance()
    {
        return _agent.remainingDistance <= WarpDistance;
    }
}
