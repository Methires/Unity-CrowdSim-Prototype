using UnityEngine;

public class NavMeshPointGenerator
{
    private float _range;

    public NavMeshPointGenerator(float range)
    {
        _range = range;
    }

    public Vector3 RandomPointOnNavMesh(Vector3 center)
    {
        do
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * _range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 10.0f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        } while (true);
    }


}
