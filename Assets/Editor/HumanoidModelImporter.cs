using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System;

class HumanoidModelImporter : AssetPostprocessor
{
    private Avatar a;
    ModelImporter modelImporter;
    private static SkeletonBone[] skeletonDescription;
    private static bool secondPass = false;
    private static string path;
    private string htFilepath;

    void OnPreprocessModel()
    {
        //htFilepath = "/Resources/Daniel.ht";
        htFilepath = "/Resources/HumanTemplateFull.ht";

        modelImporter = assetImporter as ModelImporter;

        modelImporter.animationType = ModelImporterAnimationType.Human;
        modelImporter.animationCompression = ModelImporterAnimationCompression.Optimal;
        modelImporter.animationPositionError = 0.5f;
        modelImporter.animationRotationError = 0.5f;
        modelImporter.animationScaleError = 0.5f;
        modelImporter.generateAnimations = ModelImporterGenerateAnimations.GenerateAnimations;
        modelImporter.importAnimation = true;
        modelImporter.resampleCurves = true;
        var s = modelImporter.defaultClipAnimations;
        modelImporter.optimizeGameObjects = true;

        modelImporter.humanDescription = ReadHumanDescription();
        modelImporter.sourceAvatar = null;


        //if (!secondPass)
        //{
        //    string[] paths = AssetDatabase.FindAssets("MoCapAvatar");
        //    string referenceAvatarPath = AssetDatabase.GUIDToAssetPath(paths[0]);//fullPath
        //    Avatar avatar = AssetDatabase.LoadAssetAtPath<Avatar>(referenceAvatarPath);
        //    modelImporter.sourceAvatar = avatar;
        //}

        path = assetPath;
        AssetDatabase.WriteImportSettingsIfDirty(this.assetPath);
    }

    void OnPostprocessModel(GameObject g)
    {
        if (!secondPass)
        {
            ModelImporter modelImporter = assetImporter as ModelImporter;
            skeletonDescription = modelImporter.humanDescription.skeleton;
            List<GameObject> children = new List<GameObject>();
            foreach (Transform child in g.transform)
            {
                children.Add(child.gameObject);
            }

            children = new List<GameObject>();

            //g.AddComponent<SkinnedMeshCombiner>();

            //secondPass = true;
            //AssetDatabase.ImportAsset(path);
        }
    }

    void OnPreprocessAnimation()
    {
        var modelImporter = assetImporter as ModelImporter;
        modelImporter.clipAnimations = modelImporter.defaultClipAnimations;
        int i = 0;
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        int i = 0;
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
            pair = lines[i].Split(new string[] { ": " }, StringSplitOptions.None);
            pair[0] = pair[0].Replace(" ", string.Empty);
            pair[1] = pair[1].Replace(" ", string.Empty);

            HumanBone newBone = new HumanBone();
            newBone.humanName = pair[0];
            newBone.boneName = pair[1];
            HumanLimit limit = new HumanLimit();
            limit.useDefaultValues = true;
            newBone.limit = limit;
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
        humanDescription.hasTranslationDoF = true;

        if (secondPass)
        {
            //Uncomment to have avatar based on Reptiliusz
            //humanDescription.skeleton = skeletonDescription;
        }
        return humanDescription;
    }

    private string[] ReadFileLines()
    {
        string path = Application.dataPath + htFilepath;
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
