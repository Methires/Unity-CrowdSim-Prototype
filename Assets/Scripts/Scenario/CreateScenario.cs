using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CreateScenario : MonoBehaviour
{
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
        CreateAgentsFromCrowd(simultaneousInstances, _agentsNames.Count);
        List<List<Action>> scenarioPerAgent = CreateActivitySequencePerAgent();
        PrintOut(scenarioPerAgent);
        int agentIndex = 0;

        for (int i = 0; i < simultaneousInstances; i++)
        {
            for (int j = 0; j < scenarioPerAgent.Count; j++)
            {
                GameObject agent = _agentsGameObjects[agentIndex];
                ScenarioController scenario = agent.AddComponent<ScenarioController>();

                for (int k = 0; k < scenarioPerAgent[j].Count; k++)
                {
                    if (scenarioPerAgent[j][k].Actors.Count == 1)
                    {
                        switch (scenarioPerAgent[j][k].Name.ToLower())
                        {
                            //case "walk":
                            //    Vector3 pointW = agent.GetComponent<GenerateDestination>().GenerateVector3WithinLimits();
                            //    float speedW = 3.0f;
                            //    MovementData movDataW = new MovementData(pointW, speedW);
                            //    if (scenarioPerAgent[j][k].Blends == null)
                            //    {
                            //        scenario.AddNewActivity(movDataW, null);
                            //    }
                            //    else
                            //    {
                            //        ActionData blendData = new ActionData(scenarioPerAgent[j][k].Blends[0].Name, 5.0f);
                            //        scenario.AddNewActivity(movDataW, blendData);
                            //    }
                            //    break;
                            //case "run":
                            //    Vector3 pointR = agent.GetComponent<GenerateDestination>().GenerateVector3WithinLimits();
                            //    float speedR = 10.0f;
                            //    MovementData movDataR = new MovementData(pointR, speedR);
                            //    if (scenarioPerAgent[j][k].Blends == null)
                            //    {
                            //        scenario.AddNewActivity(movDataR, null);
                            //    }
                            //    else
                            //    {
                            //        ActionData blendData = new ActionData(scenarioPerAgent[j][k].Blends[0].Name, 5.0f);
                            //        scenario.AddNewActivity(movDataR, blendData);
                            //    }
                            //    break;
                            default:
                                ActivityData aData = new ActivityData(scenarioPerAgent[j][k].Name, 10.0f);
                                scenario.AddNewActivity(null, aData);
                                break;
                        }
                    }
                    else
                    {
                        ActivityData complAcData = new ActivityData(scenarioPerAgent[j][k].Name, 10.0f);
                        scenario.AddNewActivity(null, complAcData);
                    }
                }
                agentIndex++;
            }
        }

        foreach(GameObject agent in _agentsGameObjects)
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
            crowd[index].GetComponent<GenerateNavMeshAgentDestination>().enabled = false;
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
                for (int k = 0; k < data[j].Actions.Count; k++)
                {
                    for (int l = 0; l < data[j].Actions[k].Actors.Count; l++)
                    {
                        if (data[j].Actions[k].Actors[l].Name.Equals(_agentsNames[i]))
                        {
                            agentLevel.Actions.Add(data[j].Actions[k]);
                            break;
                        }
                    }
                }
                agentData.Add(agentLevel);
            }
            _dataPerAgent.Add(agentData);
        }
    }

    private List<List<Action>> CreateActivitySequencePerAgent()
    {
        List<List<Action>> sequences = new List<List<Action>>();
        sequences.Capacity = _agentsNames.Count;

        for (int i = 0; i < _agentsNames.Count; i++)
        {
            List<Action> sequence = new List<Action>();
            for (int j = 0; j < _dataPerAgent[i].Count; j++)
            {
                Level tempLevel = new Level();
                tempLevel.Index = j;
                for (int k = 0; k < _dataPerAgent[i][j].Actions.Count; k++)
                {
                    int actorIndex = -1;
                    for (int l = 0; l < _dataPerAgent[i][j].Actions[k].Actors.Count; l++)
                    {
                        if (_dataPerAgent[i][j].Actions[k].Actors[l].Name.Equals(_agentsNames[i]))
                        {
                            actorIndex = l;
                            break;
                        }
                    }
                    if (CheckActivityFeasibility(sequence, _dataPerAgent[i][j].Actions[k].Actors[actorIndex], j))
                    {
                        tempLevel.Actions.Add(_dataPerAgent[i][j].Actions[k]);
                    }
                }
                if (tempLevel.Actions.Count != 0)
                {
                    if (i != 0)
                    {
                        Action forcedActity;
                        if (ForceActivity(sequences, i, tempLevel, out forcedActity))
                        {
                            sequence.Add(forcedActity);
                        }
                        else
                        {
                            RemoveComplexActivity(sequences, i, ref tempLevel);
                            sequence.Add(DrawAnActivity(tempLevel, _agentsNames[i]));
                        }
                    }
                    else
                    {
                        sequence.Add(DrawAnActivity(tempLevel, _agentsNames[i]));
                    }
                }
            }
            sequences.Add(sequence);
        }

        return sequences;
    }

    private void PrintOut(List<List<Action>> abc)
    {
        foreach (var a in abc)
        {
            foreach (var b in a)
            {
                if (b.Blends == null)
                {
                    Debug.Log("Name: " + b.Name + " Id: " + b.Index);
                }
                else
                {
                    string blends = "";
                    foreach (var c in b.Blends)
                    {
                        blends += " " + c.Name;
                    }
                    Debug.Log("Name: " + b.Name + " Id: " + b.Index + " Blend: " + blends);
                }

            }
        }
    }

    private bool CheckActivityFeasibility(List<Action> sequence, Actor actorInActivity, int currentLevelIndex)
    {
        if (currentLevelIndex == 0)
        {
            return true;
        }
        for (int i = 0; i < actorInActivity.PreviousActivitiesIndexes.Length; i++)
        {
            if (actorInActivity.PreviousActivitiesIndexes[i] == sequence[currentLevelIndex - 1].Index)
            {
                return true;
            }
        }
        return false;
    }

    private bool ForceActivity(List<List<Action>> sequences, int actorIndex, Level level, out Action forcedActivity)
    {
        for (int i = 0; i < sequences.Count; i++)
        {
            if (i == actorIndex)
            {
                continue;
            }
            foreach (Action activity in level.Actions)
            {
                if (sequences[i][level.Index].Index == activity.Index)
                {
                    forcedActivity = sequences[i][level.Index];
                    return true;
                }
            }
        }

        forcedActivity = new Action();
        return false;
    }

    //Think a bit more about this one, as it may not work properly with 3 or more agents 
    private void RemoveComplexActivity(List<List<Action>> sequences, int actorIndex, ref Level level)
    {
        List<int> indexes = new List<int>();
        for (int i = 0; i < level.Actions.Count; i++)
        {
            if (level.Actions[i].Actors.Count < 1)
            {
                for (int j = 0; j < sequences.Count; j++)
                {
                    if (sequences[j][level.Index].Index != level.Actions[i].Index)
                    {
                        indexes.Remove(j);
                    } 
                }
            }
        }
        foreach (int index in indexes)
        {
            level.Actions.RemoveAt(index);
        }
    }

    private Action DrawAnActivity(Level level, string actorName)
    {
        float[] probabilityArray = new float[level.Actions.Count];
        for (int i = 0; i < level.Actions.Count; i++)
        {
            if (i > 0)
            {
                probabilityArray[i] = level.Actions[i].Probability + probabilityArray[i - 1];
            }
            else
            {
                probabilityArray[i] = level.Actions[i].Probability;
            }
        }
        float randomValue = Random.Range(0.0f, probabilityArray[probabilityArray.Length - 1]);
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
        if (level.Actions[index].Blends.Count != 0)
        {
            float[] blendProbabilityArray = new float[level.Actions[index].Blends.Count + 1];
            for (int i = 0; i < level.Actions[index].Blends.Count; i++)
            {
                if (i > 0)
                {
                    blendProbabilityArray[i] = level.Actions[index].Blends[i].Probability + blendProbabilityArray[i - 1];
                }
                else
                {
                    blendProbabilityArray[i] = level.Actions[index].Blends[i].Probability;
                }
            }
            blendProbabilityArray[blendProbabilityArray.Length - 1] = 1.0f;
            float randomValueForBlend = Random.Range(0.0f, blendProbabilityArray[blendProbabilityArray.Length - 1]);
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
                Action tempWithBlend = new Action
                {
                    Name = level.Actions[index].Name,
                    Index = level.Actions[index].Index,
                    Actors = level.Actions[index].Actors,
                    Blends = new List<Blend>()
                };
                tempWithBlend.Blends.Add(level.Actions[index].Blends[indexBlend]);
                return tempWithBlend;
            }
        }
        Action temp = new Action
        {
            Name = level.Actions[index].Name,
            Index = level.Actions[index].Index,
            Actors = level.Actions[index].Actors,
            Blends = null
        };
        return temp;
    }

    private List<string> GetListOfActorsNames(List<Level> data)
    {
        HashSet<string> hashedActors = new HashSet<string>();
        foreach (Level level in data)
        {
            foreach (Action activity in level.Actions)
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
