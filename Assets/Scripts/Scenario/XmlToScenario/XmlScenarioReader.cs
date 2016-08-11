using UnityEngine;
using System;
using System.Xml;
using System.Collections.Generic;

public class XmlScenarioReader
{
    private string _scenarioName;
    private List<Level> _levelsData;

    public List<Level> ScenarioData
    {
        get
        {
            return _levelsData;
        }
        private set
        {
            _levelsData = value;
        }
    }
    public string ScenarioName
    {
        get
        {
            return _scenarioName;
        }
        private set
        {
            _scenarioName = value;
        }
    }

    private XmlDocument LoadXmlFromFile(string fileName)
    {
        XmlDocument xml = new XmlDocument();
        TextAsset textAsset = Resources.Load(fileName) as TextAsset;
        xml.LoadXml(textAsset.text);

        return xml;
    }

    public void ParseXmlWithScenario(string fileName)
    {
        XmlDocument scenario = LoadXmlFromFile(fileName);

        XmlElement scenarioElement = scenario.DocumentElement;
        if (scenarioElement.HasAttribute("name") && scenarioElement.Name.ToLower().Equals("scenario".ToLower()))
        {
            ScenarioName = scenarioElement.GetAttribute("name");
            ScenarioData = new List<Level>();
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
                        ScenarioData.Add(levelData);
                    }
                }
            }
        }
    }

    public void ShowOnConsole()
    {
        Debug.Log("Scenario: " + ScenarioName);
        for (int i = 0; i < ScenarioData.Count; i++)
        {
            Debug.Log("Level Id: " + ScenarioData[i].Index);
            for (int j = 0; j < ScenarioData[i].Actions.Count; j++)
            {
                Debug.Log(" Activity Id: " + ScenarioData[i].Actions[j].Index + " Name: '" + ScenarioData[i].Actions[j].Name + "' Probability: " + ScenarioData[i].Actions[j].Probability);
                for (int k = 0; k < ScenarioData[i].Actions[j].Actors.Count; k++)
                {
                    Debug.Log("     Actor Name: '" + ScenarioData[i].Actions[j].Actors[k].Name + "'");
                    foreach (var previousActivity in ScenarioData[i].Actions[j].Actors[k].PreviousActivitiesIndexes)
                    {
                        Debug.Log("         Parent activity Id: " + previousActivity + "");
                    }
                }
                for (int k = 0; k < ScenarioData[i].Actions[j].Blends.Count; k++)
                {
                    Debug.Log("     Blend Name: '" + ScenarioData[i].Actions[j].Blends[k].Name + "' Probability: " + ScenarioData[i].Actions[j].Blends[k].Probability);
                }
            }
        }
    }

    static private bool FindAttributeIndex(XmlAttributeCollection attributes, string attributeName, out int index)
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
