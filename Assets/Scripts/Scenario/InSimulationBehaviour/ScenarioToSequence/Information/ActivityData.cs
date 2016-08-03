using UnityEngine;
using System.Collections.Generic;

public class ActivityData
{
    public string ParameterName;
    public float ExitTime;
    public List<GameObject> RequiredAgents;
    public string Blend;

    public ActivityData()
    {
        ParameterName = "";
        ExitTime = 0.0f;
        RequiredAgents = null;
        Blend = "";
    }

    public ActivityData(string parameterName, float exitTime)
    {
        ParameterName = parameterName;
        ExitTime = exitTime;
        RequiredAgents = null;
        Blend = "";
    }

    public ActivityData(string parameterName, float exitTime, string blend)
    {
        ParameterName = parameterName;
        ExitTime = exitTime;
        RequiredAgents = null;
        Blend = blend;
    }

    public ActivityData(string parameterName, float exitTime, List<GameObject> requiredAgents)
    {
        ParameterName = parameterName;
        ExitTime = exitTime;       
        RequiredAgents = requiredAgents;
        Blend = "";
    }

    public ActivityData(string parameterName, float exitTime, List<GameObject> requiredAgents, string blend)
    {
        ParameterName = parameterName;
        ExitTime = exitTime;
        RequiredAgents = requiredAgents;
        Blend = blend;
    }
}

