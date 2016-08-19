using UnityEngine;
using UnityEditor;

public class Preparer
{
    static void PrepareSimulation()
    {
        XmlConfigReader.ParseXmlWithConfiguration(Application.dataPath + "/config.xml");
        Debug.Log(XmlConfigReader.Data.SceneName);
        Debug.Log(XmlConfigReader.Data.DayTime);
        Debug.Log(XmlConfigReader.Data.WeatherConditions);
        Debug.Log(XmlConfigReader.Data.Models);
        Debug.Log(XmlConfigReader.Data.MaxPeople);
        Debug.Log(XmlConfigReader.Data.Tracking);
        Debug.Log(XmlConfigReader.Data.ScenarioFilePath);
        Debug.Log(XmlConfigReader.Data.Length);
        Debug.Log(XmlConfigReader.Data.Repeats);
        Debug.Log(XmlConfigReader.Data.Instances);
        Debug.Log(XmlConfigReader.Data.ResultsPath);
        Debug.Log(XmlConfigReader.Data.VisualFileExtension);
        Debug.Log(XmlConfigReader.Data.VisualResultsType);
        Debug.Log(XmlConfigReader.Data.AnnotationsFileExtension);
        Debug.Log(XmlConfigReader.Data.AnnotationsType);
    }
}
