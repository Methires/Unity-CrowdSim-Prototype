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
        agent.velocity = animator.deltaPosition / Time.deltaTime;
		transform.rotation = animator.rootRotation;
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
