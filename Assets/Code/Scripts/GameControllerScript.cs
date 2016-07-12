using UnityEngine;

public class GameControllerScript : MonoBehaviour
{
    public GameObject[] Characters;
    public int MaxPeople;
    public float RangeX_Min;
    public float RangeX_Max;
    public float RangeZ_Min;
    public float RangeZ_Max;

    void Start ()
    {
        if (MaxPeople > 0 && Characters.Length > 0)
        {
            for (int i = 0; i < MaxPeople; i++)
            {
                int index = Random.Range(0, Characters.Length);
                Vector3 position = new Vector3
                {
                    x = Random.Range(RangeX_Min, RangeX_Max),
                    y = -0.5f,
                    z = Random.Range(RangeZ_Min, RangeZ_Max)
                };
                GameObject person = (GameObject)Instantiate(Characters[index], position, Quaternion.identity);
                if (Random.Range(0.0f, 100.0f) < 90.0f)
                {
                    person.GetComponent<NavMeshAgent>().speed = Random.Range(1.0f, 2.5f);
                }         
                else
                {
                    person.GetComponent<NavMeshAgent>().speed = 10.0f;
                }
            }
        }
	}
}
