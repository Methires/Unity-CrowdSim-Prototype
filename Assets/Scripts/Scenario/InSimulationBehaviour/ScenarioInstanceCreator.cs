using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ScenarioInstanceCreator : MonoBehaviour
{
    private List<string> _agentsNames;
    private List<GameObject> _agentsGameObjects;
    private List<ScenarioController> _agentsScenarios;
    private List<List<Level>> _dataPerAgent;

    public List<ScenarioController> AgentsScenarios
    {
        get
        {
            return _agentsScenarios;
        }
        private set
        {
            _agentsScenarios = value;
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

    public void GenerateInGameSequence(int simultaneousInstances)
    {
        CreateAgentsFromCrowd(simultaneousInstances, _agentsNames.Count);
        List<List<Action>> scenarioPerAgent = CreateSequencePerAgent();
        ShowSequencesOnConsole(scenarioPerAgent);
        AgentsScenarios = new List<ScenarioController>();
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
                            case "walk":
                                Vector3 pointW = new Vector3(10.0f, 0.0f, -10.0f);//agent.GetComponent<GenerateNavMeshAgentDestination>().FindNewDestiation();
                                float speedW = 3.0f;
                                MovementData movDataW = new MovementData(pointW, speedW);
                                if (scenarioPerAgent[j][k].Blends == null)
                                {
                                    scenario.AddNewActivity(movDataW, null);
                                }
                                else
                                {
                                    ActivityData blendData = new ActivityData(scenarioPerAgent[j][k].Blends[0].Name, 5.0f);
                                    scenario.AddNewActivity(movDataW, blendData);
                                }
                                break;
                            case "run":
                                Vector3 pointR = new Vector3(-10.0f, 0.0f, 10.0f);//agent.GetComponent<GenerateNavMeshAgentDestination>().FindNewDestiation();
                                float speedR = 10.0f;
                                MovementData movDataR = new MovementData(pointR, speedR);
                                if (scenarioPerAgent[j][k].Blends == null)
                                {
                                    scenario.AddNewActivity(movDataR, null);
                                }
                                else
                                {
                                    ActivityData blendData = new ActivityData(scenarioPerAgent[j][k].Blends[0].Name, 5.0f);
                                    scenario.AddNewActivity(movDataR, blendData);
                                }
                                break;
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
                AgentsScenarios.Add(scenario);
            }
        }

        foreach (GameObject agent in _agentsGameObjects)
        {
            agent.GetComponent<ScenarioController>().LoadNewActivity();
        }
    }

    private void CreateAgentsFromCrowd(int simultaneousInstances, int agents)
    {
        _agentsGameObjects = new List<GameObject>();
        GameObject[] crowd = GameObject.FindGameObjectsWithTag("Crowd");
        for (int i = 0; i < simultaneousInstances; i++)
        {
            for (int j = 0; i < agents; i++)
            {
                int index = Random.Range(0, crowd.Length);
                crowd[index].tag = "ScenarioAgent";
                crowd[index].name = _agentsNames[j] + "_" + i;
                crowd[index].GetComponent<NavMeshAgent>().avoidancePriority = 0;
                crowd[index].GetComponent<GenerateNavMeshAgentDestination>().enabled = false;
                crowd[index].AddComponent<DisplayActivityText>();
                _agentsGameObjects.Add(crowd[index]);
                MarkAgentWithPlane(crowd[index]);
            }
        }
    }

    private void MarkAgentWithPlane(GameObject agent)
    {
        GameObject planeMarkup = GameObject.CreatePrimitive(PrimitiveType.Plane);
        planeMarkup.transform.localScale = new Vector3(0.1f, 1.0f, 0.1f);
        planeMarkup.transform.parent = agent.transform;
        planeMarkup.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        Destroy(planeMarkup.GetComponent<MeshCollider>());
    }

    private List<List<Action>> CreateSequencePerAgent()
    {
        List<List<Action>> sequencesForAgents = new List<List<Action>>();
        for (int i = 0; i < _agentsNames.Count; i++)
        {
            List<Action> singularSequence = new List<Action>();
            for (int j = 0; j < _dataPerAgent[i].Count; j++)
            {
                Level allPossibleActions = new Level(j);
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
                    if (actorIndex >= 0)
                    {
                        if (IsActionTraversable(singularSequence, _dataPerAgent[i][j].Actions[k].Actors[actorIndex], j))
                        {
                            allPossibleActions.Actions.Add(_dataPerAgent[i][j].Actions[k]);
                        }
                    }
                }
                if (allPossibleActions.Actions.Count != 0)
                {
                    if (i != 0)
                    {
                        Action forcedAction;
                        if (ForceComplexAction(sequencesForAgents, i, allPossibleActions, out forcedAction))
                        {
                            singularSequence.Add(forcedAction);
                        }
                        else
                        {
                            RemoveNontraversableComplexAction(sequencesForAgents, i, ref allPossibleActions);
                            singularSequence.Add(DrawAnAction(allPossibleActions, _agentsNames[i]));
                        }
                    }
                    else
                    {
                        singularSequence.Add(DrawAnAction(allPossibleActions, _agentsNames[i]));
                    }
                }
            }
            sequencesForAgents.Add(singularSequence);
        }

        return sequencesForAgents;
    }

    private bool IsActionTraversable(List<Action> sequence, Actor actor, int currentLevelIndex)
    {
        if (currentLevelIndex != 0)
        {
            for (int i = 0; i < actor.PreviousActivitiesIndexes.Length; i++)
            {
                if (actor.PreviousActivitiesIndexes[i] == sequence[currentLevelIndex - 1].Index)
                {
                    return true;
                }
            }
            return false;
        }

        return true;
    }

    private bool ForceComplexAction(List<List<Action>> sequences, int actorIndex, Level allActions, out Action forcedAction)
    {
        for (int i = 0; i < sequences.Count; i++)
        {
            if (i == actorIndex)
            {
                continue;
            }
            foreach (Action activity in allActions.Actions)
            {
                if (sequences[i][allActions.Index].Index == activity.Index)
                {
                    forcedAction = sequences[i][allActions.Index];
                    return true;
                }
            }
        }

        forcedAction = new Action();
        return false;
    }

    private void RemoveNontraversableComplexAction(List<List<Action>> sequences, int actorIndex, ref Level level)
    {
        if (actorIndex != 0)
        {
            List<Action> actionsToRemove = new List<Action>();
            for (int i = actorIndex - 1; i != -1; i--)
            {
                for (int j = 0; j < level.Actions.Count; j++)
                {
                    if (level.Actions[j].Actors.Count > 1)
                    {
                        foreach (Actor actor in level.Actions[j].Actors)
                        {
                            if (actor.Name == _agentsNames[i])
                            {
                                if (level.Actions[j].Index != sequences[i][level.Index].Index)
                                {
                                    actionsToRemove.Add(level.Actions[j]);
                                }
                            }
                        }
                    }
                }
            }
            foreach (Action action in actionsToRemove)
            {
                level.Actions.Remove(action);
            }
        }
    }

    private Action DrawAnAction(Level allFeasibleActions, string actorName)
    {
        float[] probabilityArray = new float[allFeasibleActions.Actions.Count];
        for (int i = 0; i < allFeasibleActions.Actions.Count; i++)
        {
            if (i > 0)
            {
                probabilityArray[i] = allFeasibleActions.Actions[i].Probability + probabilityArray[i - 1];
            }
            else
            {
                probabilityArray[i] = allFeasibleActions.Actions[i].Probability;
            }
        }
        int index = DrawnAnIndex(probabilityArray);
        if (allFeasibleActions.Actions[index].Blends.Count != 0)
        {
            float[] blendProbabilityArray = new float[allFeasibleActions.Actions[index].Blends.Count + 1];
            for (int i = 0; i < allFeasibleActions.Actions[index].Blends.Count; i++)
            {
                if (i > 0)
                {
                    blendProbabilityArray[i] = allFeasibleActions.Actions[index].Blends[i].Probability + blendProbabilityArray[i - 1];
                }
                else
                {
                    blendProbabilityArray[i] = allFeasibleActions.Actions[index].Blends[i].Probability;
                }
            }
            blendProbabilityArray[blendProbabilityArray.Length - 1] = 1.0f;
            int indexBlend = DrawnAnIndex(blendProbabilityArray);
            if (indexBlend < blendProbabilityArray.Length - 1)
            {
                Action tempWithBlend = new Action
                {
                    Name = allFeasibleActions.Actions[index].Name,
                    Index = allFeasibleActions.Actions[index].Index,
                    Actors = allFeasibleActions.Actions[index].Actors,
                    Blends = new List<Blend>()
                };
                tempWithBlend.Blends.Add(allFeasibleActions.Actions[index].Blends[indexBlend]);
                return tempWithBlend;
            }
        }
        Action temp = new Action
        {
            Name = allFeasibleActions.Actions[index].Name,
            Index = allFeasibleActions.Actions[index].Index,
            Actors = allFeasibleActions.Actions[index].Actors,
            Blends = null
        };
        return temp;
    }

    private int DrawnAnIndex(float[] probabilities)
    {
        float randomValue = Random.Range(0.0f, probabilities[probabilities.Length - 1]);
        int index = -1;
        for (int i = 0; i < probabilities.Length; i++)
        {
            index = i;
            if (randomValue > probabilities[i])
            {
                continue;
            }
            else
            {
                break;
            }
        }
        return index;
    }

    private void ShowSequencesOnConsole(List<List<Action>> sequences)
    {
        for (int i = 0; i < sequences.Count; i++)
        {
            Debug.Log("Sequence for actor: " + _agentsNames[i]);
            string sequence = "";
            for (int j = 0; j < sequences[i].Count; j++)
            {               
                sequence += sequences[i][j].Name + "(id=" + sequences[i][j].Index + ")";
                if (j != sequences[i].Count - 1)
                {
                    sequence += "->";
                }
            }
            Debug.Log(sequence);
        }
    }
}
