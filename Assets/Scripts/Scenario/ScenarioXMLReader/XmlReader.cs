using UnityEngine;
using System;
using System.Xml;
using System.Collections.Generic;

public class XmlReader
{
    private List<Layer> layersData = new List<Layer>();

    public List<Layer> LoadXmlScenario(string fileName)
    {
        XmlDocument scenarioDoc = new XmlDocument();
        TextAsset scenarioText = Resources.Load(fileName) as TextAsset;
        scenarioDoc.LoadXml(scenarioText.text);

        XmlElement root = scenarioDoc.DocumentElement;
        layersData = new List<Layer>();
        if (root.HasAttribute("name") && root.Name.ToLower().Equals("scenario".ToLower()))
        {
            string name = root.GetAttribute("name");
            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                XmlNode layer = root.ChildNodes.Item(i);
                if (layer.Name.ToLower().Equals("layer".ToLower()))
                {
                    Layer layerData = new Layer();
                    int layerIndex;
                    if (FindAttributeIndex(layer.Attributes, "id", out layerIndex))
                    {
                        layerData.Id = int.Parse(layer.Attributes.Item(layerIndex).Value);
                        for (int j = 0; j < layer.ChildNodes.Count; j++)
                        {
                            XmlNode activity = layer.ChildNodes.Item(j);
                            if (activity.Name.ToLower().Equals("activity".ToLower()))
                            {
                                Activity activityData = new Activity();
                                int probabilityIndex;
                                int nameIndex;
                                if (FindAttributeIndex(activity.Attributes, "prob", out probabilityIndex) && FindAttributeIndex(activity.Attributes, "name", out nameIndex))
                                {
                                    activityData.Probability = Convert.ToSingle(activity.Attributes.Item(probabilityIndex).Value.Replace(",", "."));
                                    activityData.Name = activity.Attributes.Item(nameIndex).Value;
                                    for (int k = 0; k < activity.ChildNodes.Count; k++)
                                    {
                                        XmlNode actorOrBlend = activity.ChildNodes.Item(k);
                                        if (actorOrBlend.Name.ToLower().Equals("actor".ToLower()) || actorOrBlend.Name.ToLower().Equals("agent".ToLower()))
                                        {
                                            Actor actorData = new Actor();
                                            int actorNameIndex;
                                            if (FindAttributeIndex(actorOrBlend.Attributes, "name", out actorNameIndex))
                                            {
                                                actorData.Name = actorOrBlend.Attributes.Item(actorNameIndex).Value;
                                                for (int l = 0; l < actorOrBlend.ChildNodes.Count; l++)
                                                {
                                                    XmlNode previousActivity = actorOrBlend.ChildNodes.Item(l);
                                                    if (previousActivity.Name.ToLower().Equals("prev".ToLower()))
                                                    {
                                                        actorData.PreviousActivities.Add(previousActivity.InnerText);
                                                    }
                                                }
                                                activityData.Actors.Add(actorData);
                                            }
                                        }
                                        else if (actorOrBlend.Name.ToLower().Equals("blend".ToLower()))
                                        {
                                            int blendProbabilityIndex;
                                            int blendNameIndex;
                                            if (FindAttributeIndex(actorOrBlend.Attributes, "prob", out blendProbabilityIndex) && FindAttributeIndex(actorOrBlend.Attributes, "name", out blendNameIndex))
                                            {
                                                activityData.Blends.Add(new Activity.Blend(Convert.ToSingle(actorOrBlend.Attributes.Item(blendProbabilityIndex).Value.Replace(",",".")), actorOrBlend.Attributes.Item(blendNameIndex).Value));
                                            }
                                        }
                                    }
                                    layerData.Activites.Add(activityData);
                                }
                            }
                        }
                        layersData.Add(layerData);
                    }
                }
            }
        }
        return layersData;
    }

    public void ShowOnConsole()
    {
        for (int i = 0; i < layersData.Count; i++)
        {
            Debug.Log("Layer ID = " + layersData[i].Id);
            for (int j = 0; j < layersData[i].Activites.Count; j++)
            {
                Debug.Log(" Activity Name = '" + layersData[i].Activites[j].Name + "' Probability = " + layersData[i].Activites[j].Probability);
                for (int k = 0; k < layersData[i].Activites[j].Actors.Count; k++)
                {
                    Debug.Log("     Actor Name = '" + layersData[i].Activites[j].Actors[k].Name + "'");
                    foreach (var previousActivity in layersData[i].Activites[j].Actors[k].PreviousActivities)
                    {
                        Debug.Log("         Previous Activity Name = '" + previousActivity + "'");
                    }
                }
                foreach (var blend in layersData[i].Activites[j].Blends)
                {
                    Debug.Log("     Blend with Name = " + blend.Name + "' with Probability = " + blend.Probablity);
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
