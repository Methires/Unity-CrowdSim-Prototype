using UnityEngine;
using System.Collections.Generic;

public class CrowdController : MonoBehaviour
{
    public GameObject[] Characters;
    public int MaxPeople;

    private float _range = 100.0f;
    private List<GameObject> _crowd;

    public void GenerateCrowd()
    {
        _crowd = new List<GameObject>();
        NavMeshPointGenerator generator = new NavMeshPointGenerator(_range);
        if (MaxPeople > 0 && Characters.Length > 0)
        {
            for (int i = 0; i < MaxPeople; i++)
            {
                int index = Random.Range(0, Characters.Length);
                GameObject agent = (GameObject)Instantiate(Characters[index], generator.RandomPointOnNavMesh(transform.position), Quaternion.identity);
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

    public void RemoveCrowd()
    {
        for (int i = 0; i < MaxPeople; i++)
        {
            Destroy(_crowd[i].gameObject);
        }
        _crowd.Clear();
    }
}
