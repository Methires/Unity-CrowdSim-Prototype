using UnityEngine;

public class CustomCollisionAvoidance : MonoBehaviour
{
    private NavMeshAgent _navMeshAgent;
    private float _radius;
    private Vector3 _destination;
    private bool _isAvoiding;

    void Start()
    {
        _navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        _radius = 0.5f + Mathf.Epsilon;
    }

    void Update()
    {
        Vector3 dir = _navMeshAgent.desiredVelocity;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, 2.0f))
        {
            DrawnperpendicularPlane(transform.position, hit.point, Color.red);
            if (hit.collider.tag == "Crowd" || hit.collider.tag == "AgentScenario")
            { 
                DrawnperpendicularPlane(transform.position, hit.point, Color.red);
                DrawnperpendicularPlane(transform.position, hit.collider.transform.position, Color.blue);
                Vector3 a = hit.collider.transform.position - transform.position;
                Vector3 b = new Vector3(hit.collider.transform.position.x, hit.collider.transform.position.y + 1.0f, hit.collider.transform.position.z);
                Vector3 avoidancePoint = hit.collider.transform.position - (Vector3.Cross(a, b).normalized * _radius);
                DrawnperpendicularPlane(hit.collider.transform.position, avoidancePoint, Color.green);

                if (!_isAvoiding)
                {
                    _destination = _navMeshAgent.destination;
                }
                _navMeshAgent.destination = avoidancePoint;
                _isAvoiding = true;
            }
        }

        if (_isAvoiding)
        {
            if (_navMeshAgent.remainingDistance < _navMeshAgent.stoppingDistance + Mathf.Epsilon)
            {
                _isAvoiding = false;
                _navMeshAgent.destination = _destination;
            }
        }
    }

    private void DrawnperpendicularPlane(Vector3 point1, Vector3 point2, Color color)
    {
        for (int i = 0; i < 100; i++)
        {
            Vector3 start = new Vector3
            {
                x = point1.x,
                y = point1.y + i * 0.01f,
                z = point1.z
            };
            Vector3 end = new Vector3
            {
                x = point2.x,
                y = point2.y + i * 0.01f,
                z = point2.z
            };
            Debug.DrawLine(start, end, color, 0.1f);
        }
    }

    private int FindNearest(Vector3[] points, Vector3 point)
    {
        int index = 0;
        float currentMin = float.MaxValue;
        for (int i = 0; i < points.Length; i++)
        {
            float distance = Mathf.Abs(Vector3.Distance(points[i], point));
            if (distance < currentMin)
            {
                index = i;
                currentMin = distance;
            }
        }

        return index;
    }
}
