using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;

public class SequencesCreator
{
    private List<string> _agentsNames;
    private List<GameObject> _agents;
    private List<List<Level>> _scenariosPerAgent;
    private List<List<List<InGameActionInfo>>> _sequencesPerAgentPerInstance;
    private bool _markActions;
    private bool _crowd;
    private bool _debug;
    NavMeshPointGenerator _generator = new NavMeshPointGenerator(25.0f);

    public List<GameObject> Agents
    {
        get
        {
            return _agents;
        }
        set
        {
            _agents = value;
        }
    }

    public bool MarkActions
    {
        get
        {
            return _markActions;
        }
        set
        {
            _markActions = value;
        }
    }

    public bool Crowd
    {
        get
        {
            return _crowd;
        }

        set
        {
            _crowd = value;
        }
    }

    public bool ShowSequenceOnConsole
    {
        get
        {
            return _debug;
        }
        set
        {
            _debug = value;
        }
    }

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

    public List<SequenceController> GenerateInGameSequences(int simultaneousInstances, out int longestSequenceLenght)
    {
        List<List<Action>> actionSequencesPerAgent = CreateSequencesPerAgent();
        if (_debug)
        {
            ShowSequencesOnConsole(actionSequencesPerAgent);
        }
        for (int i = 0; i < actionSequencesPerAgent.Count; i++)
        {
            if (actionSequencesPerAgent[i][0].Actors.Count > 1)
            {
                Action forcedWalked = new Action
                {
                    Name = "walk",
                    Index = 0,
                    Probability = 1.0f,
                    Actors = new List<Actor> { new Actor()},
                    Blends = null,
                };
                actionSequencesPerAgent[i].Insert(0, forcedWalked);
            }
        }
        List<SequenceController> sequenceControllers = new List<SequenceController>();
        _sequencesPerAgentPerInstance = new List<List<List<InGameActionInfo>>>();
        int agentIndex = 0;
        longestSequenceLenght = 0;

        for (int instanceIndex = 0; instanceIndex < simultaneousInstances; instanceIndex++)
        {
            List<List<InGameActionInfo>> inGameSequencesPerAgent = new List<List<InGameActionInfo>>();
            for (int i = 0; i < actionSequencesPerAgent.Count; i++)
            {
                List<InGameActionInfo> inGameAgentSequence = new List<InGameActionInfo>();
                GameObject agent = _agents[agentIndex];
                SequenceController seqController = agent.AddComponent<SequenceController>();
                seqController.IsCrowd = _crowd;
                seqController.MarkActivities = MarkActions;
                for (int j = 0; j < actionSequencesPerAgent[i].Count; j++)
                {
                    bool actionAdded = false;
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
                                inGameAgentSequence.Add(ActionToActivity(actionSequencesPerAgent[i][j], FindLastWaypoint(inGameSequencesPerAgent[otherAgentId], j), instanceIndex, agent));
                                actionAdded = true;
                            }
                        }
                    }
                    if (!actionAdded)
                    {
                        inGameAgentSequence.Add(ActionToActivity(actionSequencesPerAgent[i][j], Vector3.zero, instanceIndex, agent));
                    }
                    seqController.AddNewInGameAction(inGameAgentSequence.Last());
                }
                agentIndex++;
                sequenceControllers.Add(seqController);
                inGameSequencesPerAgent.Add(inGameAgentSequence);
                float sequenceTime = CalculateAnticipatedTimeOfSequence(inGameAgentSequence, agent);
                longestSequenceLenght = (int)sequenceTime > longestSequenceLenght ? (int)sequenceTime : longestSequenceLenght;
            }

            ValidateAndAdjustMeetingPoints(inGameSequencesPerAgent);
            _sequencesPerAgentPerInstance.Add(inGameSequencesPerAgent);
        }
        foreach (SequenceController controller in sequenceControllers)
        {
            controller.LoadNewActivity();
        }
        return sequenceControllers;
    }

    private Vector3 ValidateAndAdjustMeetingPoints(List<List<InGameActionInfo>> inGameSequencesPerAgent)
    {
        List<List<MovementData>> actorsLastMovements = new List<List<MovementData>>();
        List<List<ActivityData>> actorsComplexActionsActivitData = new List<List<ActivityData>>();

        foreach (var actorSequence in inGameSequencesPerAgent)
        {
            InGameActionInfo[] complexActions = GetComplexActions(actorSequence);
            List<ActivityData> extractedComplexActionsActivitData = new List<ActivityData>();

            List<MovementData> lastMovements = new List<MovementData>();
            foreach (var complexAction in complexActions)
            {
                lastMovements.Add(GetLastMovementBeforeComplexAction(actorSequence, complexAction));
                extractedComplexActionsActivitData.Add(complexAction.Activity);
            }
            actorsLastMovements.Add(lastMovements);
            actorsComplexActionsActivitData.Add(extractedComplexActionsActivitData);
        }

        bool lastPointsAreCoherent = true;
        int lastMovementsCount = actorsLastMovements[0].Count;
        for (int i = 0; i < lastMovementsCount; i++)
        {
            bool lastPointAreCoherentOnGivenLevel = true;
            Vector3 lastPoint = Vector3.zero;
            foreach (var lastMovements in actorsLastMovements)
            {
                if (lastPoint == Vector3.zero)
                {
                    lastPoint = lastMovements[i].Waypoint;
                }
                else
                {
                    lastPointAreCoherentOnGivenLevel = lastPointAreCoherentOnGivenLevel && (lastMovements[i].Waypoint == lastPoint);
                }
            }
            lastPointsAreCoherent = lastPointsAreCoherent && lastPointAreCoherentOnGivenLevel;
        }

        if (!lastPointsAreCoherent)
        {
            Debug.Log("Waypoints for complex action are not coherent!");
        }

        int complexActionsCount = actorsComplexActionsActivitData.Count;
        for (int i = 0; i < complexActionsCount; i++)
        {
            int complexActionsPerActorCount = actorsComplexActionsActivitData[i].Count;

            for (int j = 0; j < complexActionsPerActorCount; j++)
            {
                Vector3 commonMeetingPoint = Vector3.zero;
                

                foreach (var lastMovements in actorsLastMovements)
                {
                    if (commonMeetingPoint == Vector3.zero)
                    {
                        commonMeetingPoint = lastMovements[j].Waypoint;
                        Bounds bounds = actorsComplexActionsActivitData[i][j].ComplexActionBounds;
                        bool isMeetingPointCorrect = CheckMeetingPoint(commonMeetingPoint, bounds);

                        if (!isMeetingPointCorrect)
                        {
                            commonMeetingPoint = GenerateCorrectWaypoint(bounds);
                            lastMovements[j].Waypoint = commonMeetingPoint;
                        }
                    }
                    else
                    {
                        lastMovements[j].Waypoint = commonMeetingPoint;
                    }
                }
            }     
        }    

        return Vector3.zero;
    }

    private Vector3 GenerateCorrectWaypoint(Bounds bounds)
    {
        Vector3 waypoint = Vector3.zero;
        bool waypointIsValid = false;
        int counter = 0;

        do
        { 
            waypoint = _generator.RandomPointOnNavMesh(Vector3.zero);
            waypointIsValid = CheckMeetingPoint(waypoint, bounds);
            counter++;

        } while (!waypointIsValid && counter < 100);        
        return waypoint; 
    }

    private MovementData GetLastMovementBeforeComplexAction(List<InGameActionInfo> actorSequence, InGameActionInfo complexAction)
    {
        MovementData lastMovement = null;

        int complexIndex = actorSequence.FindIndex(x => x == complexAction);
        for (int i = complexIndex; i >= 0; i--)
        {
            if (actorSequence[i].Movement != null)
            {
                lastMovement = actorSequence[i].Movement;
                break;
            }
        }
        return lastMovement;
    }

    private InGameActionInfo[] GetComplexActions(List<InGameActionInfo> actorSequence)
    {
        List<InGameActionInfo> complexActions = new List<InGameActionInfo>();

        foreach (var actionInfo in actorSequence)
        {
            if (actionInfo.Activity != null && actionInfo.Activity.RequiredAgents != null)
            {
                complexActions.Add(actionInfo);
            }
        }

        return complexActions.ToArray();
    }



    private bool CheckMeetingPoint(Vector3 point, Bounds bounds)
    {
        bounds.center = point;
        float margin = 0.2f;
        bool isMeetingPointCorrect = true;

        Vector3[] corners = new Vector3[4];
        corners[0] = new Vector3(point.x + bounds.extents.x, point.y, point.z + bounds.extents.z);
        corners[1] = new Vector3(point.x + bounds.extents.x, point.y, point.z - bounds.extents.z);
        corners[2] = new Vector3(point.x - bounds.extents.x, point.y, point.z + bounds.extents.z);
        corners[3] = new Vector3(point.x - bounds.extents.x, point.y, point.z - bounds.extents.z);


        NavMeshHit hit = new NavMeshHit();
        for (int i = 0; i < corners.Length; i++)
        {
            isMeetingPointCorrect = isMeetingPointCorrect && NavMesh.SamplePosition(corners[i],out hit,margin, NavMesh.AllAreas);
        }        
        return isMeetingPointCorrect;
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
            foreach (Action action in allActions.Actions)
            {
                if (sequences[i][allActions.Index].Index == action.Index)
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
        float randomValue = UnityEngine.Random.Range(0.0f, probabilities[probabilities.Length - 1]);
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

    private InGameActionInfo ActionToActivity(Action action, Vector3 forcedWaypoint, int instanceIndex, GameObject agent)
    {
        MovementData mData = null;
        ActivityData aData = null;
        //NavMeshPointGenerator generator = new NavMeshPointGenerator(25.0f);
        Vector3 point;
        if (forcedWaypoint != Vector3.zero)
        {
            point = forcedWaypoint;
        }
        else
        {
            point = _generator.RandomPointOnNavMesh(Vector3.zero);
        }

        switch (action.Name.ToLower())
        {
            case "walk":
                float speedW = UnityEngine.Random.Range(2.5f, 5.0f);
                mData = new MovementData(point, speedW);
                if (action.Blends != null)
                {
                    mData.Blend = string.Format("{0}@{1}", action.Blends[0].MocapId, action.Blends[0].Name);
                }
                break;
            case "run":
                float speedR = UnityEngine.Random.Range(6.0f, 10.0f);
                mData = new MovementData(point, speedR);
                if (action.Blends != null)
                {
                    mData.Blend = string.Format("{0}@{1}", action.Blends[0].MocapId, action.Blends[0].Name);
                }
                break;
            default:
                string[] agentName = agent.name.Split('_');
                int agentIndex = action.Actors.IndexOf(action.Actors.FirstOrDefault(x => x.Name == agentName[0]));

                string animationClip = string.Format("{0}@{1}", action.Actors[agentIndex].MocapId, action.Name);
                aData = new ActivityData(animationClip, 10.0f);

                if (action.Actors.Count > 1)
                {
                    List<GameObject> requiredAgents = new List<GameObject>();
                    for (int i = 0; i < _agentsNames.Count; i++)
                    {
                        for (int j = 0; j < action.Actors.Count; j++)
                        {
                            if (_agentsNames[i].Equals(action.Actors[j].Name))
                            {
                                GameObject requiredAgent = GameObject.Find(_agentsNames[i] + "_" + instanceIndex);
                                requiredAgents.Add(requiredAgent);
                            }
                        }
                    }
                    aData.RequiredAgents = requiredAgents;
                }
                if (action.Blends != null)
                {
                    aData.Blend = string.Format("{0}@{1}", action.Blends[0].MocapId, action.Blends[0].Name);
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

    private float CalculateAnticipatedTimeOfSequence(List<InGameActionInfo> sequence, GameObject agent)
    {
        float time = 0.0f;
        Vector3 lastPos = agent.transform.position;
        for (int i = 0; i < sequence.Count; i++)
        {
            if (sequence[i].Movement != null)
            {
                float distance = Vector3.Distance(sequence[i].Movement.Waypoint, lastPos);
                lastPos = sequence[i].Movement.Waypoint;
                time += (distance / sequence[i].Movement.Speed);
            }
            else if (sequence[i].Activity != null)
            {
                string[] folders = new string[] { "Assets/Resources/Animations" };
                string[] guids = AssetDatabase.FindAssets(sequence[i].Activity.ParameterName, folders);
                AnimationClip anim = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), typeof(AnimationClip)) as AnimationClip;
                time += anim.length;
            }
        }
        return time;
    }
}
