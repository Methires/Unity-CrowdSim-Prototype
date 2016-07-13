using UnityEngine;

public class ActionData
{
    public float ExitTime;
    public GameObject ExitObject;
    public string ParameterName;

    public ActionData()
    {
        ExitTime = 0.0f;
        ExitObject = null;
        ParameterName = "";
    }

    public ActionData(string parameterName, float exitTime)
    {
        ExitTime = exitTime;
        ExitObject = null;
        ParameterName = parameterName;
    }

    public ActionData(string parameterName, GameObject exitObject)
    {
        ExitTime = 0.0f;
        ExitObject = exitObject;
        ParameterName = parameterName;
    }
}

