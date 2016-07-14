using UnityEngine;
using System.Collections.Generic;

public class SpawnCrowd : MonoBehaviour
{
    public GameObject[] Characters;
    public int MaxPeople;
    public float RangeX_Min;
    public float RangeX_Max;
    public float RangeZ_Min;
    public float RangeZ_Max;

    private List<GameObject> _crowd;

    public void GenerateCrowd()
    {
        _crowd = new List<GameObject>();
        if (MaxPeople > 0 && Characters.Length > 0)
        {
            for (int i = 0; i < MaxPeople; i++)
            {
                int index = Random.Range(0, Characters.Length);
                Vector3 position = new Vector3
                {
                    x = Random.Range(RangeX_Min, RangeX_Max),
                    y = 0.0f,
                    z = Random.Range(RangeZ_Min, RangeZ_Max)
                };
                GameObject agent = (GameObject)Instantiate(Characters[index], position, Quaternion.identity);
                agent.tag = "Crowd";
                if (Random.Range(0.0f, 100.0f) < 90.0f)
                {
                    agent.GetComponent<NavMeshAgent>().speed = Random.Range(1.0f, 2.5f);
                }         
                else
                {
                    agent.GetComponent<NavMeshAgent>().speed = 10.0f;
                }
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
