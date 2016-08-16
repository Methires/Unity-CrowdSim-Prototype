using UnityEngine;

[RequireComponent(typeof(NavMeshAgent))]
public class GenerateDestination : MonoBehaviour
{
    private NavMeshAgent _navAgent;
    private float _range;
    private NavMeshPointGenerator _generator;

    void Start()
    {
        _navAgent = GetComponent<NavMeshAgent>();
        _range = 25.0f;
        _generator = new NavMeshPointGenerator(_range);
    }

    void Update()
    {
        if (_navAgent.remainingDistance < _navAgent.stoppingDistance + Mathf.Epsilon)
        {
            _navAgent.SetDestination(_generator.RandomPointOnNavMesh(transform.position));
        }
    }
}
