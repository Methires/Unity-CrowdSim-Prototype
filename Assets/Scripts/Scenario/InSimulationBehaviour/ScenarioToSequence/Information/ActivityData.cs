using UnityEngine;
using System.Collections.Generic;

public class ActivityData
{
    public string ParameterName;
    public float ExitTime;
    public List<GameObject> RequiredAgents;

    public ActivityData()
    {
        ParameterName = "";
        string parameterWithoutMocapActorId = ParameterName.Split('@')[1];
        ExitTime = 0.0f;
        RequiredAgents = null;
    }

    public ActivityData(string parameterName, float exitTime)
    {
        ParameterName = parameterName;
        ExitTime = exitTime;
        RequiredAgents = null;
        string parameterWithoutMocapActorId = ParameterName.Split('@')[1];
    }

    public ActivityData(string parameterName, float exitTime, string blend)
    {
        ParameterName = parameterName;
        ExitTime = exitTime;
        RequiredAgents = null;
        string parameterWithoutMocapActorId = ParameterName.Split('@')[1];
    }

    public ActivityData(string parameterName, float exitTime, List<GameObject> requiredAgents)
    {
        ParameterName = parameterName;
        ExitTime = exitTime;       
        RequiredAgents = requiredAgents;
        string parameterWithoutMocapActorId = ParameterName.Split('@')[1];
    }

    public ActivityData(string parameterName, float exitTime, List<GameObject> requiredAgents, string blend)
    {
        ParameterName = parameterName;
        string parameterWithoutMocapActorId = ParameterName.Split('@')[1];
        ExitTime = exitTime;
        RequiredAgents = requiredAgents;
    }
}

