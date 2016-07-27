using UnityEngine;

public class ActivityData
{
    public float ExitTime;
    public GameObject ExitObject;
    public string ParameterName;
    public GameObject[] RequiredAgents;

    public ActivityData()
    {
        ExitTime = 0.0f;
        ExitObject = null;
        ParameterName = "";
        RequiredAgents = null;
    }

    public ActivityData(string parameterName, float exitTime)
    {
        ExitTime = exitTime;
        ExitObject = null;
        ParameterName = parameterName;
        RequiredAgents = null;
    }

    public ActivityData(string parameterName, GameObject exitObject)
    {
        ExitTime = 0.0f;
        ExitObject = exitObject;
        ParameterName = parameterName;
        RequiredAgents = null;
    }

    public ActivityData(string parameterName, GameObject[] requiredAgents, float exitTime)
    {
        ExitTime = exitTime;
        ExitObject = null;
        ParameterName = parameterName;
        RequiredAgents = requiredAgents;
    }
}

