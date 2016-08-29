using System.IO;
using System.Xml;

public static class SceneNameGetter
{
    public static string SceneName;

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
        }
    }
}
