using UnityEngine;
using System;
using System.Xml;
using System.Collections.Generic;

public class XmlReader
{
    private string _scenarioName;
    private List<Level> _levelsData;

    public List<Level> LevelsData
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

    public void LoadXmlScenario(string fileName)
    {
        XmlDocument xml = new XmlDocument();
        TextAsset scenarioText = Resources.Load(fileName) as TextAsset;
        xml.LoadXml(scenarioText.text);
        LevelsData = new List<Level>();

        XmlElement scenarioElement = xml.DocumentElement;
        if (scenarioElement.HasAttribute("name") && scenarioElement.Name.ToLower().Equals("scenario".ToLower()))
        {
            ScenarioName = scenarioElement.GetAttribute("name");
            for (int i = 0; i < scenarioElement.ChildNodes.Count; i++)
            {
                XmlNode levelElement = scenarioElement.ChildNodes.Item(i);
                if (levelElement.Name.ToLower().Equals("level".ToLower()))
                {
                    Level levelData = new Level();
                    int levelIndex;
                    if (FindAttributeIndex(levelElement.Attributes, "id", out levelIndex))
                    {
                        levelData.Index = int.Parse(levelElement.Attributes.Item(levelIndex).Value);
                        for (int j = 0; j < levelElement.ChildNodes.Count; j++)
                        {
                            XmlNode activityElement = levelElement.ChildNodes.Item(j);
                            if (activityElement.Name.ToLower().Equals("activity".ToLower()))
                            {
                                Activity activityData = new Activity();
                                int probParamIndex, nameParamIndex, idParamIndex;
                                if (FindAttributeIndex(activityElement.Attributes, "prob", out probParamIndex) && FindAttributeIndex(activityElement.Attributes, "name", out nameParamIndex) && FindAttributeIndex(activityElement.Attributes, "id", out idParamIndex))
                                {
                                    activityData.Probability = Convert.ToSingle(activityElement.Attributes.Item(probParamIndex).Value.Replace(",", "."));
                                    activityData.Name = activityElement.Attributes.Item(nameParamIndex).Value.ToLower();
                                    activityData.Index = int.Parse(activityElement.Attributes.Item(idParamIndex).Value);
                                    for (int k = 0; k < activityElement.ChildNodes.Count; k++)
                                    {
                                        if (activityElement.ChildNodes.Item(k).Name.ToLower().Equals("actor".ToLower()) || activityElement.ChildNodes.Item(k).Name.ToLower().Equals("agent".ToLower()))
                                        {
                                            XmlNode actorElement = activityElement.ChildNodes.Item(k);
                                            int actorNameIndex;
                                            if (FindAttributeIndex(actorElement.Attributes, "name", out actorNameIndex))
                                            {
                                                Actor actorData = new Actor();
                                                actorData.Name = actorElement.Attributes.Item(actorNameIndex).Value.ToLower();
                                                int prevActionCount = 0;
                                                for (int l = 0; l < actorElement.ChildNodes.Count; l++)
                                                {
                                                    if (actorElement.ChildNodes.Item(l).Name.ToLower().Equals("prev".ToLower()))
                                                    {
                                                        prevActionCount++;
                                                    }
                                                }
                                                actorData.PreviousActivitiesIndexes = new int[prevActionCount];
                                                int prevActivitiesTabInterator = 0;
                                                for (int l = 0; l < actorElement.ChildNodes.Count; l++)
                                                {
                                                    XmlNode prevElement = actorElement.ChildNodes.Item(l);
                                                    if (prevElement.Name.ToLower().Equals("prev".ToLower()) || prevElement.Name.ToLower().Contains("prev".ToLower()))
                                                    {
                                                        int prevIdParamIndex;
                                                        if (FindAttributeIndex(prevElement.Attributes, "id", out prevIdParamIndex))
                                                        {
                                                            actorData.PreviousActivitiesIndexes[prevActivitiesTabInterator] = int.Parse(prevElement.Attributes.Item(prevIdParamIndex).Value);
                                                            prevActivitiesTabInterator++;
                                                        }
                                                    }
                                                }
                                                activityData.Actors.Add(actorData);
                                            }
                                        }
                                        else if (activityElement.ChildNodes.Item(k).Name.ToLower().Equals("blend".ToLower()))
                                        {
                                            XmlNode blendElement = activityElement.ChildNodes.Item(k);
                                            int blendProbParamIndex;
                                            int blendNameParamIndex;
                                            if (FindAttributeIndex(blendElement.Attributes, "prob", out blendProbParamIndex) && FindAttributeIndex(blendElement.Attributes, "name", out blendNameParamIndex))
                                            {
                                                Blend blendData = new Blend();
                                                blendData.Name = blendElement.Attributes.Item(nameParamIndex).Value.ToLower();
                                                blendData.Probability = Convert.ToSingle(blendElement.Attributes.Item(blendProbParamIndex).Value.Replace(",", "."));
                                                activityData.Blends.Add(blendData);
                                            }
                                        }
                                    }
                                    levelData.Activites.Add(activityData);
                                }
                            }
                        }
                        LevelsData.Add(levelData);
                    }
                }
            }
        }
    }

    public void ShowOnConsole()
    {
        Debug.Log("Scenario: " + ScenarioName);
        for (int i = 0; i < LevelsData.Count; i++)
        {
            Debug.Log("Level Id: " + LevelsData[i].Index);
            for (int j = 0; j < LevelsData[i].Activites.Count; j++)
            {
                Debug.Log(" Activity Id: " + LevelsData[i].Activites[j].Index + " Name: '" + LevelsData[i].Activites[j].Name + "' Probability: " + LevelsData[i].Activites[j].Probability);
                for (int k = 0; k < LevelsData[i].Activites[j].Actors.Count; k++)
                {
                    Debug.Log("     Actor Name: '" + LevelsData[i].Activites[j].Actors[k].Name + "'");
                    foreach (var previousActivity in LevelsData[i].Activites[j].Actors[k].PreviousActivitiesIndexes)
                    {
                        Debug.Log("         Parent activity Id: " + previousActivity + "");
                    }
                }
                for (int k = 0; k < LevelsData[i].Activites[j].Blends.Count; k++)
                {
                    Debug.Log("     Blend Name: '" + LevelsData[i].Activites[j].Blends[k].Name + "' Probability: " + LevelsData[i].Activites[j].Blends[k].Probability);
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
