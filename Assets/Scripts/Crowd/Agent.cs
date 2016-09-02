using System;
using UnityEngine;

public class Agent : MonoBehaviour
{
    protected NavMeshAgent agent;
    protected Animator animator;
    protected Locomotion locomotion;

    private uint _agentId;
    private int _isInDynamicStateHash;

    public uint AgentId
    {
        get
        {
            return _agentId;
        }
    }

    private static uint idCounter = 0;
    private static uint GetId()
    {
        return idCounter++;
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
        if (AgentDone())
        {
            locomotion.Do(0, 0);
        }
        else
        {
            float speed = agent.desiredVelocity.magnitude;
            Vector3 velocity = Quaternion.Inverse(transform.rotation) * agent.desiredVelocity;
            float angle = Mathf.Atan2(velocity.x, velocity.z) * 180.0f / Mathf.PI;
            locomotion.Do(speed, angle);
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

    protected bool AgentDone()
    {
        return !agent.pathPending && AgentStopping();
    }

    protected bool AgentStopping()
    {
        return agent.remainingDistance <= agent.stoppingDistance;
    }

    void Update()
    {
        SetupAgentLocomotion();
    }
}
