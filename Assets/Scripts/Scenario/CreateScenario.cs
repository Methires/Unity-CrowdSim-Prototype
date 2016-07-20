using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class CreateScenario : MonoBehaviour
{
    private struct NameAndBlend
    {
        string Name;
        string Blend;

        private NameAndBlend(string name)
        {
            Name = name;
            Blend = null;
        }

        private NameAndBlend(string name, string blend)
        {
            Name = name;
            Blend = blend;
        }
    }

    private List<GameObject> _scenarioAgents;
    private bool _isFinished;
    private Layer[,] _scenarioForAgent;
    private List<string> _actorsNames;

    void Update()
    {
        if (!_isFinished)
        {
            if (_scenarioAgents.Count != 0)
            {
                bool[] isAgentFinished = new bool[_scenarioAgents.Count];
                for (int i = 0; i < _scenarioAgents.Count; i++)
                {
                    isAgentFinished[i] = _scenarioAgents[i].GetComponent<ScenarioController>().IsFinished;
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
        }

        if (_isFinished)
        {
            _scenarioAgents.Clear();
            _isFinished = false;
            GetComponent<SimulationController>().EndInstanceOfSimulation();
        }
    }

    public void GenerateInGameSequence(int simultaneousInstances)
    {
        CreateScenarioAgents(simultaneousInstances);
        int agentIndex = 0;

        for (int i = 0; i < simultaneousInstances; i++)
        {
            GameObject agent1 = _scenarioAgents[agentIndex];
            ScenarioController scenario1 = agent1.AddComponent<ScenarioController>();

            GameObject agent2 = _scenarioAgents[agentIndex + 1];
            ScenarioController scenario2 = agent2.AddComponent<ScenarioController>();

            //Currently: directly from code with no option for diversity. Temporary solution.

            //Scenario for agent1
            //Visit first randomly genareted point by walking there
            Vector3 point1_1 = agent1.GetComponent<GenerateDestination>().GenerateWaypoint(GetComponent<SpawnCrowd>().RangeX_Min, GetComponent<SpawnCrowd>().RangeX_Max, GetComponent<SpawnCrowd>().RangeZ_Min, GetComponent<SpawnCrowd>().RangeZ_Max);
            float speed1_1 = 3.0f;
            MovementData movData1_1 = new MovementData(point1_1, speed1_1);
            scenario1.AddNewActivity(movData1_1, null);

            //Visit second randomly generated point by running there
            Vector3 point1_2 = agent1.GetComponent<GenerateDestination>().GenerateWaypoint(GetComponent<SpawnCrowd>().RangeX_Min, GetComponent<SpawnCrowd>().RangeX_Max, GetComponent<SpawnCrowd>().RangeZ_Min, GetComponent<SpawnCrowd>().RangeZ_Max);
            float speed1_2 = 6.0f;
            MovementData movData1_2 = new MovementData(point1_2, speed1_2);
            scenario1.AddNewActivity(movData1_2, null);


            ////Change animator state with parameter "Sit", self explanatory, for 10 seconds, then return to normal state
            //ActionData actionData1_3 = new ActionData("Sit", 10.0f);
            //scenario1.AddNewActivity(null, actionData1_3);

            //Complex action for two agents
            //Change animator state with parameter "Sit" together with agent2 for 10 seconds, then return to normal state
            GameObject[] requiredActors = new GameObject[1];
            requiredActors[0] = agent2;
            ActionData actionData1_3 = new ActionData("Sit", requiredActors, 10.0f);
            scenario1.AddNewActivity(null, actionData1_3);

            //Once again visit first randomly generated point by running there
            MovementData movData1_4 = new MovementData(point1_1, speed1_2);
            scenario1.AddNewActivity(movData1_4, null);
            //End of scenario for agent1

            //Scenario for agent2
            //Visit second point from agent1's scenario by running there
            scenario2.AddNewActivity(movData1_2, null);


            ////Change animator state with parameter "Squat", self explanatory, until agent1 gets near, then return to normal state
            //ActionData actionData2_2 = new ActionData("Squat", agent1);
            //scenario2.AddNewActivity(null, actionData2_2);

            GameObject[] requiredActors1 = new GameObject[1];
            requiredActors1[0] = agent1;
            ActionData actionData2_2 = new ActionData("Sit", requiredActors1, 10.0f);
            scenario2.AddNewActivity(null, actionData2_2);

            //Visit point 0,0,0 by walking there slightly faster
            MovementData movData2_3 = new MovementData(Vector3.zero, 3.5f);
            scenario2.AddNewActivity(movData2_3, null);

            ////Change animator state with parameter "Wave", self explanatory, then return to normal state
            //ActionData actionData2_4 = new ActionData("Wave", 15.0f);
            //scenario2.AddNewActivity(null, actionData2_4);

            agentIndex += _scenarioForAgent.GetLength(1);
        }

        foreach (GameObject agent in _scenarioAgents)
        {
            agent.GetComponent<ScenarioController>().LoadNewActivity();
        }
    }

    void CreateScenarioAgents(int simultaneousInstances, int agents = 2)
    {
        _scenarioAgents = new List<GameObject>();
        for (int i = 0; i < agents * simultaneousInstances; i++)
        {
            GameObject[] crowd = GameObject.FindGameObjectsWithTag("Crowd");
            int index = Random.Range(0, crowd.Length);
            crowd[index].tag = "ScenarioAgent";
            crowd[index].name = "ScenarioAgent_" + i;
            crowd[index].GetComponent<NavMeshAgent>().avoidancePriority = 0;
            crowd[index].GetComponent<GenerateDestination>().enabled = false;
            crowd[index].AddComponent<DisplayActivityText>();
            _scenarioAgents.Add(crowd[index]);
            MarkAgentWithPlane(crowd[index]);
        }
    }

    public void LoadScenarioFromXml(string fileName)
    {
        XmlReader reader = new XmlReader();
        List<Layer> layers = reader.LoadXmlScenario(fileName);
        _actorsNames = GetListOfActors(layers);
        _scenarioForAgent = new Layer[layers.Count, _actorsNames.Count];
        for (int i = 0; i < _actorsNames.Count; i++)
        {
            for (int j = 0; j < layers.Count; j++)
            {
                Layer agentSpecificLayer = new Layer();
                for (int k = 0; k < layers[j].Activites.Count; k++)
                {
                    Activity activityWithSpecificAgent;
                    for (int l = 0; l < layers[j].Activites[k].Actors.Count; l++)
                    {
                        if (layers[j].Activites[k].Actors[l].Name.Equals(_actorsNames[i]))
                        {
                            activityWithSpecificAgent = layers[j].Activites[k];
                            agentSpecificLayer.Activites.Add(activityWithSpecificAgent);
                            break;
                        }
                    }
                }
                _scenarioForAgent[j,i] = agentSpecificLayer;
            }
        }
    }

    private void CreateActivitySequence()
    {
        List<NameAndBlend>[] sequences = new List<NameAndBlend>[_scenarioForAgent.GetLength(1)];
        for (int i = 0; i < sequences.Length; i++)            
        {
            sequences[i] = new List<NameAndBlend>();
        }
        for (int i = 0; i < _scenarioForAgent.GetLength(1); i++)
        {
            for (int j = 0; j < _scenarioForAgent.GetLength(0); j++)
            {
                Layer temp = new Layer();
                string previousActivity = "HOW TO FIND ONE?!?";
                for (int k = 0; k < _scenarioForAgent[j, i].Activites.Count; k++)
                {
                    foreach (var actor in _scenarioForAgent[j, i].Activites[k].Actors)
                    {
                        if (actor.Name.ToLower().Equals(_actorsNames[i].ToLower()))
                        {
                            if (actor.PreviousActivities.Contains(previousActivity))
                            {
                                temp.Activites.Add(_scenarioForAgent[j, i].Activites[k]);
                            }
                        }
                    }
                }
                float[] probabilityArray = new float[temp.Activites.Count];
                for (int k = 0; k < temp.Activites.Count; k++)
                {
                    if (k > 0)
                    {
                        probabilityArray[k] = temp.Activites[k].Probability + probabilityArray[k - 1];
                    }
                    else
                    {
                        probabilityArray[k] = temp.Activites[k].Probability;
                    }
                }
                float randomValue = Random.Range(0.0f, probabilityArray[probabilityArray.Length - 1]);
            }
        }
    }

    private List<string> GetListOfActors(List<Layer> layers)
    {
        HashSet<string> hashedActors = new HashSet<string>();
        foreach (Layer layer in layers)
        {
            foreach (Activity activity in layer.Activites)
            {
                foreach (Actor actor in activity.Actors)
                {
                    hashedActors.Add(actor.Name);
                }
            }
        }
        List<string> actors = hashedActors.ToList();
        return actors;
    }

    private void MarkAgentWithPlane(GameObject agent)
    {
        GameObject planeMarkup = GameObject.CreatePrimitive(PrimitiveType.Plane);
        planeMarkup.transform.localScale = new Vector3(0.1f, 1.0f, 0.1f);
        planeMarkup.transform.parent = agent.transform;
        planeMarkup.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        Destroy(planeMarkup.GetComponent<MeshCollider>());
    }
}
