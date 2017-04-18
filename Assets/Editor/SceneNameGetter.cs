using System.IO;
using System.Xml;

public static class SceneNameGetter
{
    public static string SceneName;
    public static string Mode;
    public static int MapSize = 10;

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
            SceneName = configElement.ChildNodes.Item(0).Attributes.Item(0).Value;
            Mode = configElement.ChildNodes.Item(4).Attributes.Item(0).Value;
            int.TryParse(configElement.ChildNodes.Item(5).Attributes.Item(0).Value, out MapSize);
        }
    }
}
