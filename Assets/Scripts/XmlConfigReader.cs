using System;
using System.IO;
using System.Xml;

public static class XmlConfigReader
{
    public struct Config
    {
        public int MaxPeople;
        public string ActionsFilter;
        public bool Tracking;
        public string ScenarioFile;
        public int Length;
        public int Repeats;
        public int Instances;
    }
    public static Config Data;

    private static XmlDocument LoadConfigXML(string path)
    {
        string xmlText = File.ReadAllText(path);
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(xmlText);

        return xml;
    }

    public static void ParseConfigXML(string path)
    {
        XmlDocument xml = LoadConfigXML(path);
        XmlElement configElement = xml.DocumentElement;
        if (configElement.Name.Equals("config"))
        {
            Data.MaxPeople = int.Parse(configElement.ChildNodes.Item(1).Attributes.Item(1).Value);
            Data.ActionsFilter = configElement.ChildNodes.Item(1).Attributes.Item(2).Value;

            Data.Tracking = Convert.ToBoolean(configElement.ChildNodes.Item(2).Attributes.Item(0).Value);
            Data.ScenarioFile = configElement.ChildNodes.Item(2).Attributes.Item(1).Value;
            Data.Length = int.Parse(configElement.ChildNodes.Item(2).Attributes.Item(2).Value);
            Data.Repeats = int.Parse(configElement.ChildNodes.Item(2).Attributes.Item(3).Value);
            Data.Instances = int.Parse(configElement.ChildNodes.Item(2).Attributes.Item(4).Value);
        }
    }
}