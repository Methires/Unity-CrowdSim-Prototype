using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CreateScenario : MonoBehaviour
{
    public struct ActivityToPerform
    {
        public string Name;
        public string Blend;
        public int Index;

        public ActivityToPerform(string name, int index)
        {
            Name = name;
            Blend = null;
            Index = index;
        }

        public ActivityToPerform(string name, string blend, int index)
        {
            Name = name;
            Blend = blend;
            Index = index;
        }
    }

    private List<GameObject> _agentsGameObjects;
    private List<string> _agentsNames;
    private List<List<Level>> _dataPerAgent;
    private bool _isFinished;

    void Update()
    {
        if (!_isFinished)
        {
            if (_agentsGameObjects.Count != 0)
            {
                bool[] isAgentFinished = new bool[_agentsGameObjects.Count];
                for (int i = 0; i < _agentsGameObjects.Count; i++)
                {
                    isAgentFinished[i] = _agentsGameObjects[i].GetComponent<ScenarioController>().IsFinished;
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
            _agentsGameObjects.Clear();
            _isFinished = false;
            GetComponent<SimulationController>().EndInstanceOfSimulation();
        }
    }

    public void GenerateInGameSequence(int simultaneousInstances)
    {
        CreateAgentsFromCrowd(simultaneousInstances, 2);
        int agentIndex = 0;

        for (int i = 0; i < simultaneousInstances; i++)
        {
            GameObject agent1 = _agentsGameObjects[agentIndex];
            ScenarioController scenario1 = agent1.AddComponent<ScenarioController>();

            GameObject agent2 = _agentsGameObjects[agentIndex + 1];
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

            agentIndex += 2;
        }

        foreach (GameObject agent in _agentsGameObjects)
        {
            agent.GetComponent<ScenarioController>().LoadNewActivity();
        }
    }

    private void CreateAgentsFromCrowd(int simultaneousInstances, int agents)
    {
        _agentsGameObjects = new List<GameObject>();
        for (int i = 0; i < agents * simultaneousInstances; i++)
        {
            GameObject[] crowd = GameObject.FindGameObjectsWithTag("Crowd");
            int index = Random.Range(0, crowd.Length);
            crowd[index].tag = "ScenarioAgent";
            crowd[index].name = "ScenarioAgent_" + i;
            crowd[index].GetComponent<NavMeshAgent>().avoidancePriority = 0;
            crowd[index].GetComponent<GenerateDestination>().enabled = false;
            crowd[index].AddComponent<DisplayActivityText>();
            _agentsGameObjects.Add(crowd[index]);
            MarkAgentWithPlane(crowd[index]);
        }
    }

    public void RawInfoToListPerAgent(List<Level> data)
    {
        _agentsNames = GetListOfActorsNames(data);
        _dataPerAgent = new List<List<Level>>();
        for (int i = 0; i < _agentsNames.Count; i++)
        {
            List<Level> agentData = new List<Level>();
            for (int j = 0; j < data.Count; j++)
            {
                Level agentLevel = new Level();
                agentLevel.Index = data[j].Index;
                for (int k = 0; k < data[j].Activites.Count; k++)
                {
                    for (int l = 0; l < data[j].Activites[k].Actors.Count; l++)
                    {
                        if (data[j].Activites[k].Actors[l].Name.Equals(_agentsNames[i]))
                        {
                            agentLevel.Activites.Add(data[j].Activites[k]);
                            break;
                        }
                    }
                }
                agentData.Add(agentLevel);
            }
            _dataPerAgent.Add(agentData);
        }
    }

    public List<List<ActivityToPerform>> CreateActivitySequencePerAgent()
    {
        List<List<ActivityToPerform>> sequences = new List<List<ActivityToPerform>>();
        sequences.Capacity = _agentsNames.Count;

        for (int i = 0; i < _agentsNames.Count; i++)
        {
            List<ActivityToPerform> sequence = new List<ActivityToPerform>();
            for (int j = 0; j < _dataPerAgent[i].Count; j++)
            {
                Level tempLevel = new Level();
                for (int k = 0; k < _dataPerAgent[i][j].Activites.Count; k++)
                {
                    int actorIndex = -1;
                    for (int l = 0; l < _dataPerAgent[i][j].Activites[k].Actors.Count; l++)
                    {
                        if (_dataPerAgent[i][j].Activites[k].Actors[l].Name.Equals(_agentsNames[i]))
                        {
                            actorIndex = l;
                            break;
                        }
                    }
                    if (CheckActivityFeasibility(sequence, _dataPerAgent[i][j].Activites[k].Actors[actorIndex], j))
                    {
                        tempLevel.Activites.Add(_dataPerAgent[i][j].Activites[k]);
                    }
                }
                if (tempLevel.Activites.Count != 0)
                {
                    sequence.Add(DrawAnActivity(tempLevel));
                }
            }
            sequences.Add(sequence);
        }

        return sequences;
    }

    public void PrintOut(List<List<ActivityToPerform>> abc)
    {
        foreach (var a in abc)
        {
            foreach (var b in a)
            {
                if (b.Blend == null)
                {
                    Debug.Log("Name: " + b.Name + " Id: " + b.Index);
                }
                else
                {
                    Debug.Log("Name: " + b.Name + " Id: " + b.Index + " Blend: " + b.Blend);
                }

            }
        }
    }

    private bool CheckActivityFeasibility(List<ActivityToPerform> sequenceOfPreviousActivities, Actor actorInActivity, int currentLevelIndex)
    {
        if (currentLevelIndex == 0)
        {
            return true;
        }
        for (int i = 0; i < actorInActivity.PreviousActivitiesIndexes.Length; i++)
        {
            if (actorInActivity.PreviousActivitiesIndexes[i] == sequenceOfPreviousActivities[currentLevelIndex - 1].Index)
            {
                return true;
            }
        }
        return false;
    }

    private ActivityToPerform DrawAnActivity(Level level)
    {
        float[] probabilityArray = new float[level.Activites.Count];
        for (int i = 0; i < level.Activites.Count; i++)
        {
            if (i > 0)
            {
                probabilityArray[i] = level.Activites[i].Probability + probabilityArray[i - 1];
            }
            else
            {
                probabilityArray[i] = level.Activites[i].Probability;
            }
        }
        float randomValue = Random.Range(0.0f, 1.0f);
        int index = -1;
        for (int i = 0; i < probabilityArray.Length; i++)
        {
            index = i;
            if (randomValue > probabilityArray[i])
            {
                continue;
            }
            else
            {
                break;
            }
        }
        if (level.Activites[index].Blends.Count != 0)
        {
            float[] blendProbabilityArray = new float[level.Activites[index].Blends.Count + 1];
            for (int i = 0; i < level.Activites[index].Blends.Count; i++)
            {
                if (i > 0)
                {
                    blendProbabilityArray[i] = level.Activites[i].Probability + blendProbabilityArray[i - 1];
                }
                else
                {
                    blendProbabilityArray[i] = level.Activites[i].Probability;
                }
            }
            blendProbabilityArray[blendProbabilityArray.Length - 1] = 1.0f;
            float randomValueForBlend = Random.Range(0.0f, 1.0f);
            int indexBlend = -1;
            for (int i = 0; i < blendProbabilityArray.Length; i++)
            {
                indexBlend = i;
                if (randomValueForBlend > blendProbabilityArray[i])
                {
                    continue;
                }
                else
                {
                    break;
                }
            }
            if (indexBlend < blendProbabilityArray.Length - 1)
            {
                return new ActivityToPerform(level.Activites[index].Name, level.Activites[index].Blends[indexBlend].Name, level.Activites[index].Index);
            }
        }
        return new ActivityToPerform(level.Activites[index].Name, level.Activites[index].Index);
    }

    private List<string> GetListOfActorsNames(List<Level> data)
    {
        HashSet<string> hashedActors = new HashSet<string>();
        foreach (Level level in data)
        {
            foreach (Activity activity in level.Activites)
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
