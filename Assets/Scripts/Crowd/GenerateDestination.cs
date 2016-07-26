using UnityEngine;

[RequireComponent(typeof(NavMeshAgent))]
public class GenerateDestination : MonoBehaviour
{
    private NavMeshAgent navAgent;
    private float destinationOffset;
    private float RangeX_Min;
    private float RangeX_Max;
    private float RangeZ_Min;
    private float RangeZ_Max;

    void Awake()
    {
        GetXZLimits(FindObjectOfType<SpawnCrowd>());
    }

    void Start ()
    {
        navAgent = GetComponent<NavMeshAgent>();
        destinationOffset = 1.0f;
        navAgent.destination = GenerateWaypoint();
	}
	
	void Update ()
    {
        if (navAgent.remainingDistance < navAgent.stoppingDistance + destinationOffset)
        {
            navAgent.destination = GenerateWaypoint();
        }      
    }

    public Vector3 GenerateWaypoint()
    {
        Vector3 waypoint = new Vector3
        {
            x = Random.Range(RangeX_Min, RangeX_Max),
            y = 0.0f,
            z = Random.Range(RangeZ_Min, RangeZ_Max)
        };
        return waypoint;
    }

    private void GetXZLimits(SpawnCrowd spawner)
    {
        RangeX_Min = spawner.RangeX_Min;
        RangeX_Max = spawner.RangeX_Max;
        RangeZ_Min = spawner.RangeZ_Min;
        RangeZ_Max = spawner.RangeZ_Max;
    }
}
