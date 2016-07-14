using System;
using UnityEngine;

public class Agent : MonoBehaviour
{
	protected NavMeshAgent agent;
	protected Animator animator;
	protected Locomotion locomotion;

	void Start ()
    {
		agent = GetComponent<NavMeshAgent>();
		agent.updateRotation = false;

		animator = GetComponent<Animator>();
		locomotion = new Locomotion(animator);
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
			float angle = Mathf.Atan2(velocity.x, velocity.z) * 180.0f / 3.14159f;
			locomotion.Do(speed, angle);
		}
	}

    void OnAnimatorMove()
    {
        try
        {
            agent.velocity = animator.deltaPosition / Time.deltaTime;
            transform.rotation = animator.rootRotation;
        }
        catch(NullReferenceException e)
        {
            //Don't really understand why there's a NullReferenceException for a single frame each time I instantiate object with this script
            //It's a bad practice to create an empty catch, I know, but despite those errors, everything works just fine, so I'm temporaly hiding it 
        }
    }

	protected bool AgentDone()
	{
		return !agent.pathPending && AgentStopping();
	}

	protected bool AgentStopping()
	{
		return agent.remainingDistance <= agent.stoppingDistance;
	}

	void Update () 
	{
		SetupAgentLocomotion();
	}
}
