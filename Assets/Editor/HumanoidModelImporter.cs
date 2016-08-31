using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEditor;
using System.Linq;

class HumanoidModelImporter : AssetPostprocessor
{
    private string _mocapActorId;
    private string _htFilepath = "/Resources/HumanTemplateFull.ht";
    private string _referenceAvatarName = "B3010@ReferenceAvatar";
    private bool _isAnimation = false;

    private static SkeletonBone[] skeletonDescription;
    private static bool secondPass = false;
    private static string path;

    void OnPreprocessModel()
    {
        if (assetPath.Contains("@"))
        {
            _mocapActorId = Path.GetFileNameWithoutExtension(assetPath).Split('@')[0];
            _isAnimation = true;
            _htFilepath = "/Resources/MocapTemplate.ht";
        }

        ModelImporter modelImporter = assetImporter as ModelImporter;

        modelImporter.animationType = ModelImporterAnimationType.Human;
        modelImporter.animationCompression = ModelImporterAnimationCompression.Optimal;
        modelImporter.animationPositionError = 0.5f;
        modelImporter.animationRotationError = 0.5f;
        modelImporter.animationScaleError = 0.5f;
        modelImporter.generateAnimations = ModelImporterGenerateAnimations.GenerateAnimations;
        modelImporter.importAnimation = true;
        modelImporter.resampleCurves = true;
        modelImporter.optimizeGameObjects = true;
        modelImporter.humanDescription = ReadHumanDescription();
        modelImporter.sourceAvatar = null;
        modelImporter.extraExposedTransformPaths = new string[] { string.Format("{0}:Solving:Hips", _mocapActorId) };

        if (!secondPass && _isAnimation)
        {
            string[] paths = AssetDatabase.FindAssets(_referenceAvatarName);
            string referenceAvatarPath = AssetDatabase.GUIDToAssetPath(paths[0]);
            Avatar avatar = AssetDatabase.LoadAssetAtPath<Avatar>(referenceAvatarPath);
            modelImporter.sourceAvatar = avatar;
            
        }

        path = assetPath;
    }

    void OnPostprocessModel(GameObject g)
    {
        ModelImporter modelImporter = assetImporter as ModelImporter;
        if (_isAnimation && !secondPass)
        {
            if (skeletonDescription == null)
            {
                PrepareSkeletonDescription(modelImporter);
            }
          
            secondPass = true;
            AssetDatabase.ImportAsset(path);
        }
        else
        {
            secondPass = false;
        }

        //foreach (Transform child in g.transform)
        //{
        //    if (child.name == _mocapActorId)
        //    {
        //        modelImporter.extraExposedTransformPaths = new string[] { _mocapActorId + ":Solving:Hips" };//string.Format("{0}:Solving:Hips", _mocapActorId) };
        //    }
        //}
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (var asset in importedAssets)
        {
            Debug.Log("Imported: " + asset);
            if (asset.Count(x => x == '@') > 1)
            {
                string[] derivedAssetNames = GenerateDerivedNames(Path.GetFileName(asset));

                foreach (var assetName in derivedAssetNames)
                {
                    AssetDatabase.CopyAsset(asset, Path.GetDirectoryName(asset) + "/" + assetName);
                }
                AssetDatabase.DeleteAsset(asset);
                AssetDatabase.Refresh();
            }
        }
    }

    private static string[] GenerateDerivedNames(string primordialName)
    {
        List<string> namesList = primordialName.Split('@').ToList();
        string commonPart = namesList.Last();
        namesList.Remove(commonPart);

        for (int i = 0; i < namesList.Count; i++)
        {
            namesList[i] = string.Format("{0}@{1}", namesList[i], commonPart);
        }
        return namesList.ToArray();
    }

    private void PrepareSkeletonDescription(ModelImporter modelImporter)
    {
        skeletonDescription = modelImporter.humanDescription.skeleton;
        List<SkeletonBone> tempSkeletonBoneList = skeletonDescription.ToList();

        string refereneAvatarId = _referenceAvatarName.Split('@')[0];
        for (int i = 0; i < tempSkeletonBoneList.Count; i++)
        {
            if (!tempSkeletonBoneList[i].name.Contains(refereneAvatarId))
            {
                tempSkeletonBoneList.RemoveAt(i);
            }
        }
        skeletonDescription = tempSkeletonBoneList.ToArray();
    }

    private HumanDescription ReadHumanDescription()
    {
        HumanDescription humanDescription = new HumanDescription();
        List<HumanBone> humanBones = new List<HumanBone>();
        string[] lines = ReadFileLines();
        string[] pair;

        //Description starts in the 10th line
        for (int i = 9; i < lines.Length; i++)
        {
            pair = lines[i].Split(new string[] { ": " }, StringSplitOptions.None);
            pair[0] = pair[0].Replace(" ", string.Empty);
            pair[1] = pair[1].Replace(" ", string.Empty);

            if (_isAnimation)
            {
                pair[1] = string.Format("{0}:{1}", _mocapActorId, pair[1]);
            }

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

        if (secondPass && _isAnimation)
        {
            string refereneAvatarId = _referenceAvatarName.Split('@')[0];

            List<SkeletonBone> skeletonBones = new List<SkeletonBone>();
            for (int i = 0; i < skeletonDescription.Length; i++)
            {
                string oldName = skeletonDescription[i].name;
                string newName = oldName.Replace(refereneAvatarId, _mocapActorId);

                SkeletonBone newSkeletonBone = new SkeletonBone();
                newSkeletonBone.name = newName;
                newSkeletonBone.position = skeletonDescription[i].position;
                newSkeletonBone.rotation = skeletonDescription[i].rotation;
                newSkeletonBone.scale = skeletonDescription[i].scale;
                newSkeletonBone.transformModified = skeletonDescription[i].transformModified;

                skeletonBones.Add(newSkeletonBone);
            }

            humanDescription.skeleton = skeletonBones.ToArray();
        }
        return humanDescription;
    }

    private string[] ReadFileLines()
    {
        string path = Application.dataPath + _htFilepath;
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
