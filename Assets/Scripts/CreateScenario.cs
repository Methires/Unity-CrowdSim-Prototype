using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class CreateScenario : MonoBehaviour
{
    public int Repeats;

    private List<ScenarioController> _scenarios;
    private int _instancesCounter;
    private bool _isFinished;

    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        if (_scenarios.Count != 0)
        {
            bool[] isAgentFinished = new bool[_scenarios.Count];
            for (int i = 0; i < _scenarios.Count; i++)
            {
                isAgentFinished[i] = _scenarios[i].IsFinished; 
            }
            _isFinished = true;
            for (int i = 0; i < isAgentFinished.Length; i++)
            {
                if (!isAgentFinished[i])
                {
                    _isFinished = false;
                    break;
                }
            }
        }

        if (_isFinished)
        {
            if (_instancesCounter < Repeats - 1)
            {
                _instancesCounter++;
                _isFinished = false;
                _scenarios.Clear();
                Scene scene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(scene.name);
            }
        }
    }

    public void GenerateInstanceOfScenario()
    {
        _scenarios = new List<ScenarioController>();
        GameObject agent1 = GameObject.FindGameObjectWithTag("ScenarioAgent1");   
        //agent1.AddComponent<Rigidbody>().isKinematic = true;
        ScenarioController scenario1 = agent1.AddComponent<ScenarioController>();

        GameObject agent2 = GameObject.FindGameObjectWithTag("ScenarioAgent2");
        agent2.AddComponent<Rigidbody>().isKinematic = true;
        ScenarioController scenario2 = agent2.AddComponent<ScenarioController>();

        ////Currently: directly from code with no option for diversity. Temporary solution.

        //Scenario for agent1
        //Visit first randomly genareted point by walking there
        Vector3 point1_1 = agent1.GetComponent<GenerateDestination>().GenerateWaypoint(GetComponent<SpawnCrowd>().RangeX_Min, GetComponent<SpawnCrowd>().RangeX_Max, GetComponent<SpawnCrowd>().RangeZ_Min, GetComponent<SpawnCrowd>().RangeZ_Max);
        float speed1_1 = 2.5f;
        MovementData movData1_1 = new MovementData(point1_1, speed1_1);
        scenario1.AddNewActivity(movData1_1, null);

        //Visit second randomly generated point by running there
        Vector3 point1_2 = agent1.GetComponent<GenerateDestination>().GenerateWaypoint(GetComponent<SpawnCrowd>().RangeX_Min, GetComponent<SpawnCrowd>().RangeX_Max, GetComponent<SpawnCrowd>().RangeZ_Min, GetComponent<SpawnCrowd>().RangeZ_Max);
        float speed1_2 = 10.0f;
        MovementData movData1_2 = new MovementData(point1_2, speed1_2);
        scenario1.AddNewActivity(movData1_2, null);

        //Change animator state with parameter "Sit", self explanatory, for 10 seconds, then return to normal state
        ActionData actionData1_3 = new ActionData("Sit", 10.0f);
        scenario1.AddNewActivity(null, actionData1_3);

        //Once again visit first randomly generated point by running there
        MovementData movData1_4 = new MovementData(point1_1, speed1_2);
        scenario1.AddNewActivity(movData1_4, null);
        //End of scenario for agent1

        //Scenario for agent2
        //Visit second point from agent1's scenario by running there
        scenario2.AddNewActivity(movData1_2, null);

        //Change animator state with parameter "Squat", self explanatory, until agent1 gets near, then return to normal state
        ActionData actionData2_2 = new ActionData("Squat", agent1);
        scenario2.AddNewActivity(null, actionData2_2);

        //Visit point 0,0,0 by walking there slightly faster
        MovementData movData2_3 = new MovementData(Vector3.zero, 3.5f);
        scenario2.AddNewActivity(movData2_3, null);

        //Change animator state with parameter "Squat", self explanatory, until agent1 gets near, then return to normal state
        ActionData actionData2_4 = new ActionData("Wave", 15.0f);
        scenario2.AddNewActivity(null, actionData2_4);

        agent1.GetComponent<GenerateDestination>().enabled = false;
        agent2.GetComponent<GenerateDestination>().enabled = false;

        scenario1.LoadNewActivity();
        scenario2.LoadNewActivity();

        _scenarios.Add(scenario1);
        _scenarios.Add(scenario2);

        Debug.Log("Instance of scenario nr " + (_instancesCounter + 1));
    }	
}
