using UnityEngine;

[RequireComponent(typeof(NavMeshAgent))]
public class GenerateDestination : MonoBehaviour
{
    private SpawnCrowd spawner;
    private NavMeshAgent navAgent;
    private float destinationOffset;

    void Start ()
    {
        navAgent = GetComponent<NavMeshAgent>();
        spawner = FindObjectOfType<SpawnCrowd>();
        destinationOffset = 1.0f;
        navAgent.destination = GenerateWaypoint(spawner.RangeX_Min, spawner.RangeX_Max, spawner.RangeZ_Min, spawner.RangeZ_Max);
	}
	
	void Update ()
    {
        if (navAgent.remainingDistance < navAgent.stoppingDistance + destinationOffset)
        {
            navAgent.destination = GenerateWaypoint(spawner.RangeX_Min, spawner.RangeX_Max, spawner.RangeZ_Min, spawner.RangeZ_Max);
        }      
    }

    public Vector3 GenerateWaypoint(float RangeDownX, float RangeUpX, float RangeDownZ, float RangeUpZ)
    {
        Vector3 waypoint = new Vector3
        {
            x = Random.Range(RangeDownX, RangeUpX),
            y = 0.0f,
            z = Random.Range(RangeDownZ, RangeUpZ)
        };
        return waypoint;
    }
}
