using UnityEngine;
using System;
using System.IO;
using System.Xml;

public static class XmlConfigParser
{
    public struct Config
    {
        public int DayTime;
        public int WeatherConditions;
        public string Models;
        public int MaxPeople;
        public bool Tracking;
        public string ScenarioFilePath;
        public int Length;
        public int Repeats;
        public int Instances;
        public string ResultsPath;
        public string VisualFileExtension;
        public int VisualResultsType;
        public string AnnotationsFileExtension;
        public int AnnotationsType;
    }
    public static Config Data;

    private static XmlDocument LoadXmlFromFile(string path)
    {
        string xmlText;
        if (Path.HasExtension(path))
        {
            xmlText = File.ReadAllText(path);
        }
        else
        {
            TextAsset tAsset = Resources.Load(path) as TextAsset;
            xmlText = tAsset.text;
        }
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(xmlText);

        return xml;
    }

    public static void ParseXmlWithConfiguration(string path)
    {
        XmlDocument xml = LoadXmlFromFile(path);
        XmlElement configElement = xml.DocumentElement;
        if (configElement.Name.Equals("config"))
        {
            for (int i = 0; i < configElement.ChildNodes.Count; i++)
            {
                switch (configElement.ChildNodes.Item(i).Name)
                {
                    case "scene":
                        Data.DayTime = int.Parse(configElement.ChildNodes.Item(i).Attributes.Item(1).Value);
                        Data.WeatherConditions = int.Parse(configElement.ChildNodes.Item(i).Attributes.Item(2).Value);
                        break;
                    case "crowd":
                        Data.Models = configElement.ChildNodes.Item(i).Attributes.Item(0).Value;
                        Data.MaxPeople = int.Parse(configElement.ChildNodes.Item(i).Attributes.Item(1).Value);
                        break;
                    case "simulation":
                        Data.Tracking = Convert.ToBoolean(configElement.ChildNodes.Item(i).Attributes.Item(0).Value);
                        Data.ScenarioFilePath = configElement.ChildNodes.Item(i).Attributes.Item(1).Value;
                        Data.Length = int.Parse(configElement.ChildNodes.Item(i).Attributes.Item(2).Value);
                        Data.Repeats = int.Parse(configElement.ChildNodes.Item(i).Attributes.Item(3).Value);
                        Data.Instances = int.Parse(configElement.ChildNodes.Item(i).Attributes.Item(4).Value);
                        break;
                    case "results":
                        Data.ResultsPath = configElement.ChildNodes.Item(i).Attributes.Item(0).Value;
                        Data.VisualFileExtension = configElement.ChildNodes.Item(i).Attributes.Item(1).Value;
                        Data.VisualResultsType = int.Parse(configElement.ChildNodes.Item(i).Attributes.Item(2).Value);
                        Data.AnnotationsFileExtension = configElement.ChildNodes.Item(i).Attributes.Item(3).Value;
                        Data.AnnotationsType = int.Parse(configElement.ChildNodes.Item(i).Attributes.Item(4).Value);
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
