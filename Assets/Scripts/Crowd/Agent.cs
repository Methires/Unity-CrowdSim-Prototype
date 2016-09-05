using System;
using UnityEngine;

public class Agent : MonoBehaviour
{
    protected NavMeshAgent agent;
    protected Animator animator;
    protected Locomotion locomotion;

    private uint _agentId;
    private int _isInDynamicStateHash;
    private Quaternion _finalRotation;
    private bool _applyFinalRotation = false;
    private static uint idCounter = 0;

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
            _applyFinalRotation = true;
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

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;

        animator = GetComponent<Animator>();
        locomotion = new Locomotion(animator);

        _agentId = GetId();
        _isInDynamicStateHash = Animator.StringToHash("IsInDynamicState");
    }

    protected void SetupAgentLocomotion()
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

            if (_applyFinalRotation && IsStopping())
            {
                float angleDifference = Quaternion.Angle(transform.rotation, _finalRotation);
                if (Mathf.Abs(angleDifference) > 10.0f)
                {

                    angle = angleDifference > 180.0f ? angleDifference - 360.0f : angleDifference;

                    locomotion.Do(0.0f, angle);
                    //angle = angleDifference;
                    //speed = 0.0f;
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

    void OnAnimatorMove()
    {
        if (!animator.GetBool(_isInDynamicStateHash))
        {
            try
            {
                agent.velocity = animator.deltaPosition / Time.deltaTime;
                transform.rotation = animator.rootRotation;
                transform.position = new Vector3(transform.position.x, animator.rootPosition.y, transform.position.z);
            }
            catch (NullReferenceException e)
            {
            }
        }
        else
        {
            animator.ApplyBuiltinRootMotion();
        }

        //Vector3 position = animator.rootPosition;
        //position.y = agent.nextPosition.y;
        //transform.position = position;
    }

    private bool IsDone()
    {
        return !agent.pathPending && IsInPlace();
    }

    private bool IsStopping()
    {
        return agent.remainingDistance <= agent.stoppingDistance;
    }

    public bool IsInPlace()
    {
        return IsStopping() && (_applyFinalRotation ? Mathf.Abs(Quaternion.Angle(transform.rotation, _finalRotation)) <= 1.0f : true);
    }

    private float GetAngle(Vector3 from, Vector3 to)
    {
        float angle = Vector3.Angle(from, to);
        Vector3 normal = Vector3.Cross(from, to);
        angle *= Mathf.Sign(Vector3.Dot(normal, transform.up));
        return angle;
    }

    void Update()
    {
        SetupAgentLocomotion();
    }
}
