using System;
using UnityEngine;

public class Agent : MonoBehaviour
{
    protected UnityEngine.AI.NavMeshAgent agent;
    protected Animator animator;
    protected Locomotion locomotion;

    private uint _agentId;
    private int _isInDynamicStateHash;
    private Quaternion _finalRotation;
    private bool _applyFinalRotation = false;
    private static uint idCounter = 0;
    private bool _movementInPlace = false;

    public uint AgentId
    {
        get
        {
            return _agentId;
        }
    }

    private static uint GetId()
    {
        return idCounter++;
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

    public bool ApplyFinalRotation
    {
        get
        {
            return _applyFinalRotation;
        }

        set
        {
            _applyFinalRotation = value;
        }
    }

    public bool MovementInPlace
    {
        get
        {
            return _movementInPlace;
        }

        set
        {
            _movementInPlace = value;
        }
    }

    void Awake()
    {
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.updateRotation = false;

        animator = GetComponent<Animator>();
        locomotion = new Locomotion(animator);

        _agentId = GetId();
        _isInDynamicStateHash = Animator.StringToHash("IsInDynamicState");
    }

    static int deathCounter = 0;
    void OnDestroy()
    {
        deathCounter++;
    }

    protected void SetupAgentLocomotion()
    {
        if (agent.enabled)
        {
            if (IsDone())
            {
                locomotion.Do(0.0f, 0.0f);
            }
            else
            {
                float speed = agent.desiredVelocity.magnitude;
                Vector3 velocity = Quaternion.Inverse(transform.rotation) * agent.desiredVelocity;
                float angle = Mathf.Atan2(velocity.x, velocity.z) * 180.0f / Mathf.PI;

                if (_applyFinalRotation && MovementInPlace)
                {
                    float angleDifference = AngleDifference(transform.rotation, _finalRotation);
                    if (Mathf.Abs(angleDifference) > 10.0f)
                    {
                        angle = angleDifference;
                        locomotion.Do(0.0f, angle);
                    }
                    else
                    {
                        if (Mathf.Abs(angleDifference) > 1.0f)
                        {
                            transform.rotation = Quaternion.Slerp(transform.rotation, _finalRotation, 10 * Time.deltaTime);
                        }
                    }
                }
                else
                {
                    locomotion.Do(speed, angle);
                }
            }
        }     
    }

    void OnAnimatorMove()
    {
        try
        {
            if (!animator.GetBool(_isInDynamicStateHash))
            {
                agent.velocity = animator.deltaPosition / Time.deltaTime;
                transform.rotation = animator.rootRotation;
                transform.position = new Vector3(transform.position.x, animator.rootPosition.y, transform.position.z);

            }
            else
            {
                animator.ApplyBuiltinRootMotion();
            }
        }
        catch (NullReferenceException e)
        {
        }
    }

    private bool IsDone()
    {
        return !agent.pathPending && IsInPlace();
    }

    public bool IsInPlace()
    {
        return MovementInPlace && (_applyFinalRotation ? Mathf.Abs(Quaternion.Angle(transform.rotation, _finalRotation)) <= 1.0f : true); //&& IsStopping();
    }

    private float AngleDifference(Quaternion a, Quaternion b)
    {
        Vector3 forwardA = a * Vector3.forward;
        Vector3 forwardB = b * Vector3.forward;

        float angleA = Mathf.Atan2(forwardA.x, forwardA.z) * Mathf.Rad2Deg;
        float angleB = Mathf.Atan2(forwardB.x, forwardB.z) * Mathf.Rad2Deg;

        return Mathf.DeltaAngle(angleA, angleB);
    }

    void Update()
    {
        SetupAgentLocomotion();
    }
}
