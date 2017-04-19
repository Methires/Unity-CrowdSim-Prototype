using UnityEngine;
using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Text;

public static class XmlScenarioReader
{
    private static List<Level> _levelsData;

    public static List<Level> ScenarioData
    {
        get
        {
            return _levelsData;
        }
    }

    private static XmlDocument LoadScenarioXML(string path)
    {
        string xmlText;
        if (Path.HasExtension(path))
        {
            xmlText = File.ReadAllText(path);
        }
        else
        {
            TextAsset tAsset = (TextAsset)Resources.Load(path);
            xmlText = tAsset.text;
        }
        XmlDocument xml = new XmlDocument();
        xml.LoadXml(xmlText);

        return xml;
    }

    public static void ParseScenarioXML(string fileName)
    {
        XmlDocument scenario = LoadScenarioXML(fileName);

        XmlElement scenarioElement = scenario.DocumentElement;
        if (scenarioElement.Name.ToLower().Equals("scenario".ToLower()))
        {
            _levelsData = new List<Level>();
            for (int i = 0; i < scenarioElement.ChildNodes.Count; i++)
            { 
                XmlNode levelElement = scenarioElement.ChildNodes.Item(i);
                if (levelElement.Name.ToLower().Equals("level".ToLower()))
                {
                    int levelIndex;
                    if (FindAttributeIndex(levelElement.Attributes, "id", out levelIndex))
                    {
                        Level levelData = new Level(int.Parse(levelElement.Attributes.Item(levelIndex).Value));
                        for (int j = 0; j < levelElement.ChildNodes.Count; j++)
                        {
                            XmlNode actionElement = levelElement.ChildNodes.Item(j);
                            if (actionElement.Name.ToLower().Equals("action".ToLower()))
                            {
                                int probParamIndex, nameParamIndex, idParamIndex;
                                if (FindAttributeIndex(actionElement.Attributes, "prob", out probParamIndex) 
                                    && FindAttributeIndex(actionElement.Attributes, "name", out nameParamIndex) 
                                    && FindAttributeIndex(actionElement.Attributes, "id", out idParamIndex))
                                {
                                    Action actionData = new Action(
                                        actionElement.Attributes.Item(nameParamIndex).Value, 
                                        Convert.ToSingle(actionElement.Attributes.Item(probParamIndex).Value.Replace(",", ".")), 
                                        int.Parse(actionElement.Attributes.Item(idParamIndex).Value)
                                        );
                                    for (int k = 0; k < actionElement.ChildNodes.Count; k++)
                                    {
                                        if (actionElement.ChildNodes.Item(k).Name.ToLower().Equals("actor".ToLower()))
                                        {
                                            XmlNode actorElement = actionElement.ChildNodes.Item(k);
                                            int actorNameIndex;
                                            if (FindAttributeIndex(actorElement.Attributes, "name", out actorNameIndex))
                                            {
                                                int prevActionCount = 0;
                                                for (int l = 0; l < actorElement.ChildNodes.Count; l++)
                                                {
                                                    if (actorElement.ChildNodes.Item(l).Name.ToLower().Equals("prev".ToLower()))
                                                    {
                                                        prevActionCount++;
                                                    }
                                                }
                                                int[] previousActionsIndexes = new int[prevActionCount];
                                                int prevActionTabInterator = 0;
                                                for (int l = 0; l < actorElement.ChildNodes.Count; l++)
                                                {
                                                    XmlNode prevElement = actorElement.ChildNodes.Item(l);
                                                    if (prevElement.Name.ToLower().Equals("prev".ToLower()))
                                                    {
                                                        int prevIdParamIndex;
                                                        if (FindAttributeIndex(prevElement.Attributes, "id", out prevIdParamIndex))
                                                        {
                                                            previousActionsIndexes[prevActionTabInterator] = int.Parse(prevElement.Attributes.Item(prevIdParamIndex).Value);
                                                            prevActionTabInterator++;
                                                        }
                                                    }
                                                }
                                                Actor actorData = new Actor(actorElement.Attributes.Item(actorNameIndex).Value.ToLower(), previousActionsIndexes);
                                                int actorMocapIdIndex;
                                                if (FindAttributeIndex(actorElement.Attributes, "mocapId", out actorMocapIdIndex))
                                                {
                                                    actorData.MocapId = actorElement.Attributes.Item(actorMocapIdIndex).Value;
                                                }                                             
                                                actionData.Actors.Add(actorData);
                                            }
                                        }
                                    }
                                    levelData.Actions.Add(actionData);
                                }
                            }
                        }
                        _levelsData.Add(levelData);
                    }
                }
            }
        }
    }

    public static void ShowOnConsole()
    {
        StringBuilder msg = new StringBuilder();
        for (int i = 0; i < _levelsData.Count; i++)
        {
            msg.AppendLine(string.Format("Level ID: {0}", _levelsData[i].Index.ToString()));
            for (int j = 0; j < _levelsData[i].Actions.Count; j++)
            {
                msg.AppendLine(string.Format("/tActivity ID: {0}  Name: {1} Probability:", _levelsData[i].Actions[j].Index.ToString(), _levelsData[i].Actions[j].Name, _levelsData[i].Actions[j].Probability.ToString()));
                for (int k = 0; k < _levelsData[i].Actions[j].Actors.Count; k++)
                {
                    msg.AppendLine(string.Format("/t/tActor's Name: {0}",_levelsData[i].Actions[j].Actors[k].Name));
                    foreach (var previousActivity in _levelsData[i].Actions[j].Actors[k].PreviousActivitiesIndexes)
                    {
                        msg.AppendLine(string.Format("/t/t/tParent activity ID: {0}", previousActivity));
                    }
                }
            }
        }

        Debug.Log(msg.ToString());
    }

    private static bool FindAttributeIndex(XmlAttributeCollection attributes, string attributeName, out int index)
    {
        index = -1;
        for (int i = 0; i < attributes.Count; i++)
        {
            if (attributes.Item(i).Name.ToString().ToLower().Equals(attributeName.ToLower()))
            {
                index = i;
                return true;
            }
        }
        return false;
    }
}
