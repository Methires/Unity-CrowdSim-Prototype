using UnityEngine;

public class CreateScenario : MonoBehaviour
{
    GameObject agent;

	void Start ()
    {
        agent = GameObject.FindGameObjectWithTag("ScenarioAgent");
        ScenarioController scenario = agent.AddComponent<ScenarioController>();

        ////Currently: directly from code with no option for diversity. Temporary solution.
        Vector3 point1 = agent.GetComponent<GenerateDestination>().GenerateWaypoint(GetComponent<SpawnCrowd>().RangeX_Min, GetComponent<SpawnCrowd>().RangeX_Max, GetComponent<SpawnCrowd>().RangeZ_Min, GetComponent<SpawnCrowd>().RangeZ_Max);
        float speed1 = 2.5f;
        MovementData movData1 = new MovementData(point1, speed1);
        scenario.AddNewActivity(movData1, null);

        Vector3 point2 = agent.GetComponent<GenerateDestination>().GenerateWaypoint(GetComponent<SpawnCrowd>().RangeX_Min, GetComponent<SpawnCrowd>().RangeX_Max, GetComponent<SpawnCrowd>().RangeZ_Min, GetComponent<SpawnCrowd>().RangeZ_Min);
        float speed2 = 10.0f;
        MovementData movData2 = new MovementData(point2, speed2);
        scenario.AddNewActivity(movData2, null);

        agent.GetComponent<GenerateDestination>().enabled = false;

        scenario.LoadNewActivity();      
    }	
}
