using UnityEngine;
using System.Collections.Generic;

public class CrowdController : MonoBehaviour
{
    public GameObject[] Characters;
    public int MaxPeople;

    private float _range = 25.0f;
    private List<GameObject> _crowd;

    public void GenerateCrowd()
    {
        _crowd = new List<GameObject>();
        if (MaxPeople > 0 && Characters.Length > 0)
        {
            for (int i = 0; i < MaxPeople; i++)
            {
                int index = Random.Range(0, Characters.Length);
                GameObject agent = (GameObject)Instantiate(Characters[index], FindNewPosition(), Quaternion.identity);
                agent.tag = "Crowd";
                if (Random.Range(0.0f, 100.0f) < 90.0f)
                {
                    agent.GetComponent<NavMeshAgent>().speed = Random.Range(1.0f, 2.5f);
                }
                else
                {
                    agent.GetComponent<NavMeshAgent>().speed = 10.0f;
                }
                int avoid = Random.Range(10, 99);
                agent.GetComponent<NavMeshAgent>().avoidancePriority = avoid;
                _crowd.Add(agent);
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            GenerateCrowd();
        }
    }

    public void RemoveCrowd()
    {
        for (int i = 0; i < MaxPeople; i++)
        {
            Destroy(_crowd[i].gameObject);
        }
        _crowd.Clear();
    }

    private Vector3 FindNewPosition()
    {
        bool pointFound = false;
        Vector3 point;
        do
        {
            pointFound = RandomPointOnNavMesh(transform.position, _range, out point);
        }
        while (!pointFound);

        return point;
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
}
