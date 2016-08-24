using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;

class HumanoidModelImporter : AssetPostprocessor
{
    private Avatar a;
    private static SkeletonBone[] skeletonDescription;
    private static bool secondPass = false;
    private static string pathfako;

    void OnPreprocessModel()
    {
        ModelImporter modelImporter = assetImporter as ModelImporter;
        modelImporter.animationType = ModelImporterAnimationType.Human;
        AssetDatabase.WriteImportSettingsIfDirty(this.assetPath);
        modelImporter.humanDescription = ReadHumanDescription();
        pathfako = assetPath;

        if (!secondPass)
        {
            string[] paths = AssetDatabase.FindAssets("ReptiliuszAvatar");
            string referenceAvatarPath = AssetDatabase.GUIDToAssetPath(paths[0]);//fullPath
            Avatar avatar = AssetDatabase.LoadAssetAtPath<Avatar>(referenceAvatarPath);
            modelImporter.sourceAvatar = avatar;
        }

        //EditorUtility.SetDirty(modelImporter);
        //EditorApplication.SaveAssets();
    }

    void OnPostprocessModel(GameObject g)
    {
        if (!secondPass)
        {
            ModelImporter modelImporter = assetImporter as ModelImporter;
            skeletonDescription = modelImporter.humanDescription.skeleton;
            //Avatar avatar = AvatarBuilder.BuildHumanAvatar(g, modelImporter.humanDescription);
            //g.AddComponent<Animator>().avatar = avatar;
            
        }
        
        //avatar.name = g.name + "Avatar";
        //AssetDatabase.CreateAsset(avatar, "Assets/Resources/" + avatar.name + ".avatar");
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if (!secondPass)
        {
            secondPass = true;
            AssetDatabase.ImportAsset(pathfako);
        }
    }

    private HumanDescription ReadHumanDescription()
    {
        HumanDescription humanDescription = new HumanDescription();
        List<HumanBone> humanBones = new List<HumanBone>();
        string[] lines = ReadFileLines();      
        string[] pair;

        //Description starts in 10th line
        for (int i = 9; i < lines.Length; i++)
        {
            lines[i] = lines[i].Replace(" ", string.Empty);
            pair = lines[i].Split(':');
            HumanBone newBone = new HumanBone();
            newBone.humanName = pair[0];
            newBone.boneName = pair[1];
            humanBones.Add(newBone);
        }
        humanDescription.human = humanBones.ToArray();
        humanDescription.upperArmTwist = 0.5f;
        humanDescription.lowerArmTwist = 0.5f;
        humanDescription.upperLegTwist = 0.5f;
        humanDescription.lowerLegTwist = 0.5f;
        humanDescription.armStretch = 0.05f;
        humanDescription.legStretch = 0.05f;
        humanDescription.feetSpacing = 0.0f;


        if (secondPass)
        {
            humanDescription.skeleton = skeletonDescription;
        }

        //foreach (KeyValuePair<string, string> equivalent in boneEquivalents)
        //{
        //    HumanBone newBone = new HumanBone();
        //    newBone.humanName = equivalent.Key;


        //    humanBones.Add(newBone);
        //}
        //HumanBone b = new HumanBone();
        //humanDescription.skeleton

        //SkeletonBone a = new SkeletonBone();
        //HumanPose a = new HumanPose();

        //var b = HumanTrait.BoneName;
        //var c = HumanTrait.MuscleName;
        //var d = HumanTrait.RequiredBoneCount;
        //var e = HumanTrait.MuscleCount;
        return humanDescription;
    }

    private string[] ReadFileLines()
    {
        string path = Application.dataPath + "/Resources/HumanTemplate.ht";
        List<string> readLines = new List<string>();

        try
        {
            using (StreamReader sr = new StreamReader(path))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    readLines.Add(line);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        return readLines.ToArray();
    }
    


}
