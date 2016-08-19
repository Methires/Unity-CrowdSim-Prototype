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

    public float MaxSpeed = 2.0f;
    
    private Animator _animator;
    private NavMeshAgent _agent;
    private int _speedId;
    private int _angularSpeedId;
    private int _directionId;

    void Start ()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();

        _speedId = Animator.StringToHash("Speed");
        _angularSpeedId = Animator.StringToHash("AngularSpeed");
        _directionId = Animator.StringToHash("Direction");
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
            //speed = Mathf.Lerp(_agent.velocity.magnitude, _agent.desiredVelocity.magnitude, _agent.acceleration * Time.deltaTime);

            //Vector3 velocity = Quaternion.Inverse(transform.rotation) * _agent.desiredVelocity;
            //angle = Mathf.Atan2(velocity.x, velocity.z) * 180.0f / Mathf.PI;
            //angularSpeed = angle / (DirectionResponseTime); //* _agent.angularSpeed);   

            //speed = Vector3.Project(_agent.desiredVelocity, transform.forward).magnitude;
            angle = GetAngle(transform.forward, _agent.desiredVelocity);
            angularSpeed = (angle * Mathf.Deg2Rad) / DirectionResponseTime;
            speed = _agent.desiredVelocity.magnitude;
            speed = ScaleToRange(speed, 0.0f, _agent.speed, 0.0f, MaxSpeed - SpeedReductionWeight * angle);                     
        }

        AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
        bool inTransition = _animator.IsInTransition(0);
        bool inIdle = state.IsName("Idle");
        bool inTurn = state.IsName("TurnOnSpot");
        bool inWalkRun = state.IsName("Movement");

        float speedDampTime = inIdle ? 0 : SpeedDampTime;
        float angularSpeedDampTime = inWalkRun || inTransition ? AngularSpeedDampTime : 0;
        float directionDampTime = inTurn || inTransition ? 1000000 : 0;

        _animator.SetFloat(_speedId, speed, speedDampTime, Time.deltaTime);
        _animator.SetFloat(_angularSpeedId, angularSpeed, angularSpeedDampTime, Time.deltaTime);
        _animator.SetFloat(_directionId, angle, directionDampTime, Time.deltaTime);

        //if (worldDeltaPosition.magnitude > _agent.radius)
        //    transform.position = _agent.nextPosition - 0.9f * worldDeltaPosition;

        // Pull agent towards character
        if (worldDeltaPosition.magnitude > _agent.radius)
            _agent.nextPosition = transform.position + 0.99f * worldDeltaPosition;
    }

    void OnAnimatorMove()
    {
        // Update position based on animation movement using navigation surface height
        Vector3 position = _animator.rootPosition;
        position.y = _agent.nextPosition.y;
        transform.position = position;

        //_agent.velocity = _animator.deltaPosition / Time.deltaTime;
        //transform.rotation = _animator.rootRotation;
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
}
