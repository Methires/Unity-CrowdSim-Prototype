using UnityEngine;
using System.Collections;

public class Agent : MonoBehaviour
{
	protected NavMeshAgent		agent;
	protected Animator			animator;

	protected Locomotion locomotion;

	// Use this for initialization
	void Start () {
		agent = GetComponent<NavMeshAgent>();
		agent.updateRotation = false;

		animator = GetComponent<Animator>();
		locomotion = new Locomotion(animator);
	}

	protected void SetDestination(Vector3 dest)
	{
		// Construct a ray from the current mouse coordinates
		var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit = new RaycastHit();
		if (Physics.Raycast(ray, out hit))
		{
			agent.destination = hit.point;
		}
        if (dest.x != -1000000.0f)
        {
            agent.destination = dest;
        }
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

	// Update is called once per frame
	void Update () 
	{
        if (Input.GetButtonDown("Fire1"))
            SetDestination(new Vector3(-1000000.0f, 0.0f, 0.0f));

		SetupAgentLocomotion();
	}
}
