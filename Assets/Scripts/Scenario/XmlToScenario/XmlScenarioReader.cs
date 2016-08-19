using UnityEngine;
using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

public static class XmlScenarioReader
{
    private static string _scenarioName;
    private static List<Level> _levelsData;

    public static List<Level> ScenarioData
    {
        get
        {
            return _levelsData;
        }
    }
    public static string ScenarioName
    {
        get
        {
            return _scenarioName;
        }
    }

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

    public static void ParseXmlWithScenario(string fileName)
    {
        XmlDocument scenario = LoadXmlFromFile(fileName);

        XmlElement scenarioElement = scenario.DocumentElement;
        if (scenarioElement.HasAttribute("name") && scenarioElement.Name.ToLower().Equals("scenario".ToLower()))
        {
            _scenarioName = scenarioElement.GetAttribute("name");
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
                            XmlNode activityElement = levelElement.ChildNodes.Item(j);
                            if (activityElement.Name.ToLower().Equals("activity".ToLower()))
                            {
                                int probParamIndex, nameParamIndex, idParamIndex;
                                if (FindAttributeIndex(activityElement.Attributes, "prob", out probParamIndex) 
                                    && FindAttributeIndex(activityElement.Attributes, "name", out nameParamIndex) 
                                    && FindAttributeIndex(activityElement.Attributes, "id", out idParamIndex))
                                {
                                    Action activityData = new Action(
                                        activityElement.Attributes.Item(nameParamIndex).Value, 
                                        Convert.ToSingle(activityElement.Attributes.Item(probParamIndex).Value.Replace(",", ".")), 
                                        int.Parse(activityElement.Attributes.Item(idParamIndex).Value)
                                        );
                                    for (int k = 0; k < activityElement.ChildNodes.Count; k++)
                                    {
                                        if (activityElement.ChildNodes.Item(k).Name.ToLower().Equals("actor".ToLower()))
                                        {
                                            XmlNode actorElement = activityElement.ChildNodes.Item(k);
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
                                                Actor actorData = new Actor(actorElement.Attributes.Item(actorNameIndex).Value.ToLower(),previousActionsIndexes );
                                                activityData.Actors.Add(actorData);
                                            }
                                        }
                                        else if (activityElement.ChildNodes.Item(k).Name.ToLower().Equals("blend".ToLower()))
                                        {
                                            XmlNode blendElement = activityElement.ChildNodes.Item(k);
                                            int blendProbParamIndex;
                                            int blendNameParamIndex;
                                            if (FindAttributeIndex(blendElement.Attributes, "prob", out blendProbParamIndex) 
                                                && FindAttributeIndex(blendElement.Attributes, "name", out blendNameParamIndex))
                                            {
                                                Blend blendData = new Blend();
                                                blendData.Name = blendElement.Attributes.Item(nameParamIndex).Value;
                                                blendData.Probability = Convert.ToSingle(blendElement.Attributes.Item(blendProbParamIndex).Value.Replace(",", "."));
                                                activityData.Blends.Add(blendData);
                                            }
                                        }
                                    }
                                    levelData.Actions.Add(activityData);
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
        Debug.Log("Scenario: " + ScenarioName);
        for (int i = 0; i < _levelsData.Count; i++)
        {
            Debug.Log("Level Id: " + _levelsData[i].Index);
            for (int j = 0; j < _levelsData[i].Actions.Count; j++)
            {
                Debug.Log(" Activity Id: " + _levelsData[i].Actions[j].Index + " Name: '" + _levelsData[i].Actions[j].Name + "' Probability: " + _levelsData[i].Actions[j].Probability);
                for (int k = 0; k < _levelsData[i].Actions[j].Actors.Count; k++)
                {
                    Debug.Log("     Actor Name: '" + _levelsData[i].Actions[j].Actors[k].Name + "'");
                    foreach (var previousActivity in _levelsData[i].Actions[j].Actors[k].PreviousActivitiesIndexes)
                    {
                        Debug.Log("         Parent activity Id: " + previousActivity + "");
                    }
                }
                for (int k = 0; k < _levelsData[i].Actions[j].Blends.Count; k++)
                {
                    Debug.Log("     Blend Name: '" + _levelsData[i].Actions[j].Blends[k].Name + "' Probability: " + _levelsData[i].Actions[j].Blends[k].Probability);
                }
            }
        }
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
