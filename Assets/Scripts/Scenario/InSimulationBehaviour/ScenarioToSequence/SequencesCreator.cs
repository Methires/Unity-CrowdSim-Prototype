using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SequencesCreator : MonoBehaviour
{
    private List<string> _agentsNames;
    private List<GameObject> _agentsGameObjects;
    private List<List<Level>> _scenariosPerAgent;
    private List<List<List<InGameActionInfo>>> _sequencesPerAgentPerInstance;

    public void RawInfoToListPerAgent(List<Level> data)
    {
        _agentsNames = GetListOfActorsNames(data);
        _scenariosPerAgent = new List<List<Level>>();
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
            _scenariosPerAgent.Add(agentData);
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

    public List<SequenceController> GenerateInGameSequences(int simultaneousInstances)
    {
        CreateAgentsFromCrowd(simultaneousInstances, _agentsNames.Count);
        List<List<Action>> actionSequencesPerAgent = CreateSequencesPerAgent();
        ShowSequencesOnConsole(actionSequencesPerAgent);
        List<SequenceController> sequenceControllers = new List<SequenceController>();
        _sequencesPerAgentPerInstance = new List<List<List<InGameActionInfo>>>();
        int agentIndex = 0;

        for (int instanceIndex = 0; instanceIndex < simultaneousInstances; instanceIndex++)
        {
            List<List<InGameActionInfo>> inGameSequencesPerAgent = new List<List<InGameActionInfo>>();
            for (int i = 0; i < actionSequencesPerAgent.Count; i++)
            {
                List<InGameActionInfo> inGameAgentSequence = new List<InGameActionInfo>();
                GameObject agent = _agentsGameObjects[agentIndex];
                SequenceController seqController = agent.AddComponent<SequenceController>();
                for (int j = 0; j < actionSequencesPerAgent[i].Count; j++)
                {
                    bool activityAdded = false;
                    if (i != 0 && j + 1 < actionSequencesPerAgent[i].Count)
                    {
                        if (actionSequencesPerAgent[i][j + 1].Actors.Count > 1)
                        {
                            int otherAgentId = 0;
                            for (int k = i - 1; k != -1; k--)
                            {
                                foreach (Actor actor in actionSequencesPerAgent[i][j + 1].Actors)
                                {
                                    if (actor.Name == _agentsNames[k])
                                    {
                                        otherAgentId = k;
                                        break;
                                    }
                                }
                            }
                            if (otherAgentId >= 0)
                            {
                                inGameAgentSequence.Add(ActionToActivity(actionSequencesPerAgent[i][j], FindLastWaypoint(inGameSequencesPerAgent[otherAgentId], j), instanceIndex));
                                activityAdded = true;
                            }
                        }
                    }
                    if (!activityAdded)
                    {
                        inGameAgentSequence.Add(ActionToActivity(actionSequencesPerAgent[i][j], Vector3.zero, instanceIndex));
                    }
                    seqController.AddNewInGameAction(inGameAgentSequence.Last());
                }
                agentIndex++;
                sequenceControllers.Add(seqController);
                inGameSequencesPerAgent.Add(inGameAgentSequence);
            }
            _sequencesPerAgentPerInstance.Add(inGameSequencesPerAgent);
        }
        foreach (SequenceController controller in sequenceControllers)
        {
            controller.LoadNewActivity();
        }
        return sequenceControllers;
    }

    private void CreateAgentsFromCrowd(int simultaneousInstances, int agents)
    {
        _agentsGameObjects = new List<GameObject>();
        for (int i = 0; i < simultaneousInstances; i++)
        {
            for (int j = 0; j < agents; j++)
            {
                GameObject[] crowd = GameObject.FindGameObjectsWithTag("Crowd");
                int index = Random.Range(0, crowd.Length);
                crowd[index].tag = "ScenarioAgent";
                crowd[index].name = _agentsNames[j] + "_" + i;
                crowd[index].GetComponent<NavMeshAgent>().avoidancePriority = 0;
                crowd[index].GetComponent<GenerateDestination>().enabled = false;
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
        planeMarkup.transform.localPosition = new Vector3(0.0f, 0.1f, 0.0f);
        Destroy(planeMarkup.GetComponent<MeshCollider>());
    }

    private List<List<Action>> CreateSequencesPerAgent()
    {
        List<List<Action>> sequencesForAgents = new List<List<Action>>();
        for (int i = 0; i < _agentsNames.Count; i++)
        {
            List<Action> singularSequence = new List<Action>();
            for (int j = 0; j < _scenariosPerAgent[i].Count; j++)
            {
                Level allPossibleActions = new Level(j);
                for (int k = 0; k < _scenariosPerAgent[i][j].Actions.Count; k++)
                {
                    int actorIndex = -1;
                    for (int l = 0; l < _scenariosPerAgent[i][j].Actions[k].Actors.Count; l++)
                    {
                        if (_scenariosPerAgent[i][j].Actions[k].Actors[l].Name.Equals(_agentsNames[i]))
                        {
                            actorIndex = l;
                            break;
                        }
                    }
                    if (actorIndex >= 0)
                    {
                        if (IsActionTraversable(singularSequence, _scenariosPerAgent[i][j].Actions[k].Actors[actorIndex], j))
                        {
                            allPossibleActions.Actions.Add(_scenariosPerAgent[i][j].Actions[k]);
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
                sequence += sequences[i][j].Name;
                if (sequences[i][j].Blends != null)
                {
                    sequence += " " + sequences[i][j].Blends[0].Name;
                }
                sequence += " (id=" + sequences[i][j].Index + ")";
                if (j != sequences[i].Count - 1)
                {
                    sequence += "->";
                }
            }
            Debug.Log(sequence);
        }
    }

    private InGameActionInfo ActionToActivity(Action action, Vector3 forcedWaypoint, int instanceIndex)
    {
        MovementData mData = null;
        ActivityData aData = null;
        NavMeshPointGenerator generator = new NavMeshPointGenerator(50.0f);
        Vector3 point;
        if (forcedWaypoint != Vector3.zero)
        {
            point = forcedWaypoint;
        }
        else
        {
            //Doesn't work perfectly :(
            point = generator.RandomPointOnNavMesh(transform.position);
        }

        switch (action.Name.ToLower())
        {
            case "walk":
                float speedW = Random.Range(2.5f, 5.0f);
                mData = new MovementData(point, speedW);
                if (action.Blends != null)
                {
                    mData.Blend = action.Blends[0].Name;
                }
                break;
            case "run":
                float speedR = Random.Range(6.0f, 10.0f);
                mData = new MovementData(point, speedR);
                if (action.Blends != null)
                {
                    mData.Blend = action.Blends[0].Name;
                }
                break;
            default:
                aData = new ActivityData(action.Name, 10.0f);
                if (action.Actors.Count > 1)
                {
                    List<GameObject> requiredAgents = new List<GameObject>();
                    for (int i  = 0; i < _agentsNames.Count; i++)
                    {
                        for (int j = 0; j < action.Actors.Count; j++)
                        {
                            if (_agentsNames[i].Equals(action.Actors[j].Name))
                            {
                                GameObject agent = GameObject.Find(_agentsNames[i] + "_" + instanceIndex);
                                requiredAgents.Add(agent);
                            }
                        }
                    }
                    aData.RequiredAgents = requiredAgents;
                }
                if (action.Blends != null)
                {
                    aData.Blend = action.Blends[0].Name;
                }
                break;
        }
        return new InGameActionInfo(mData, aData);
    }

    private Vector3 FindLastWaypoint(List<InGameActionInfo> actions, int level)
    {
        for (int i = level; i != -1; i--)
        {
            if (actions[i].Movement != null)
            {
                return actions[i].Movement.Waypoint;
            }
        }
        return Vector3.zero;
    }
}
