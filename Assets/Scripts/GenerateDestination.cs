using UnityEngine;

[RequireComponent(typeof(NavMeshAgent))]
public class GenerateDestination : MonoBehaviour
{
    public float RangeX_Min;
    public float RangeX_Max;
    public float RangeZ_Min;
    public float RangeZ_Max;

    private NavMeshAgent navAgent;
    private float destinationOffset;

    void Start ()
    {
        navAgent = GetComponent<NavMeshAgent>();
        destinationOffset = 1.0f;
        navAgent.destination = GenerateWaypoint(RangeX_Min, RangeX_Max, RangeZ_Min, RangeZ_Max);
	}
	
	void Update ()
    {
        if (navAgent.remainingDistance < navAgent.stoppingDistance + destinationOffset)
        {
            navAgent.destination = GenerateWaypoint(RangeX_Min, RangeX_Max, RangeZ_Min, RangeZ_Max);
        }      
    }

    private Vector3 GenerateWaypoint(float RangeDownX, float RangeUpX, float RangeDownZ, float RangeUpZ)
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
