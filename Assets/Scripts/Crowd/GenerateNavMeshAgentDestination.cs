using UnityEngine;

[RequireComponent(typeof(NavMeshAgent))]
public class GenerateNavMeshAgentDestination : MonoBehaviour
{
    private NavMeshAgent _navAgent;
    private float _range;

    void Start ()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        _range = 50.0f;
	}
	
	void Update ()
    {
        if (_navAgent.remainingDistance < _navAgent.stoppingDistance + Mathf.Epsilon)
        {
            _navAgent.SetDestination(FindNewDestiation());
        }
    }

    private bool RandomPointOnNavMesh(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;

        return false;
    }

    private Vector3 FindNewDestiation()
    {
        bool pointFound, pointWithCorrectY = false;
        Vector3 point;
        do
        {
            do
            {
                pointFound = RandomPointOnNavMesh(transform.position, _range, out point);
            }
            while (!pointFound);

            if (Mathf.Abs(point.y - transform.position.y) < Mathf.Epsilon)
            {
                pointWithCorrectY = true;
            }
        }
        while (!pointWithCorrectY);

        return point;
    }
}
