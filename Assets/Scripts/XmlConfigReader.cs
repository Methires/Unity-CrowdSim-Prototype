using System;
using System.IO;
using System.Xml;

public static class XmlConfigReader
{
    public struct Config
    {
        public int DayTime;
        public int WeatherConditions;
        public string Models;
        public int MaxPeople;
        public string ActionsFilter;
        public bool Tracking;
        public string ScenarioFile;
        public int Length;
        public int Repeats;
        public int Instances;
        public string ResultsDirectory;
        public int BoundingBoxes;
        public int ResolutionWidth;
        public int ResolutionHeight;
        public int FrameRate;
        public int BufferSize;
    }
    public static Config Data;

    private static XmlDocument LoadXmlFromFile(string path)
    {
        string xmlText = File.ReadAllText(path);
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(xmlText);

        return xml;
    }

    public static void ParseXmlConfig(string path)
    {
        XmlDocument xml = LoadXmlFromFile(path);
        XmlElement configElement = xml.DocumentElement;
        if (configElement.Name.Equals("config"))
        {
            Data.DayTime = int.Parse(configElement.ChildNodes.Item(0).Attributes.Item(1).Value);
            Data.WeatherConditions = int.Parse(configElement.ChildNodes.Item(0).Attributes.Item(2).Value);

            Data.Models = configElement.ChildNodes.Item(1).Attributes.Item(0).Value;
            Data.MaxPeople = int.Parse(configElement.ChildNodes.Item(1).Attributes.Item(1).Value);
            Data.ActionsFilter = configElement.ChildNodes.Item(1).Attributes.Item(2).Value;

            Data.Tracking = Convert.ToBoolean(configElement.ChildNodes.Item(2).Attributes.Item(0).Value);
            Data.ScenarioFile = configElement.ChildNodes.Item(2).Attributes.Item(1).Value;
            Data.Length = int.Parse(configElement.ChildNodes.Item(2).Attributes.Item(2).Value);
            Data.Repeats = int.Parse(configElement.ChildNodes.Item(2).Attributes.Item(3).Value);
            Data.Instances = int.Parse(configElement.ChildNodes.Item(2).Attributes.Item(4).Value);

            Data.ResultsDirectory = configElement.ChildNodes.Item(3).Attributes.Item(0).Value;
            Data.BoundingBoxes = int.Parse(configElement.ChildNodes.Item(3).Attributes.Item(1).Value);
            Data.ResolutionWidth = int.Parse(configElement.ChildNodes.Item(3).Attributes.Item(2).Value);
            Data.ResolutionHeight = int.Parse(configElement.ChildNodes.Item(3).Attributes.Item(3).Value);
            Data.FrameRate = int.Parse(configElement.ChildNodes.Item(3).Attributes.Item(4).Value);
            Data.BufferSize = int.Parse(configElement.ChildNodes.Item(3).Attributes.Item(5).Value);
        }
    }
}