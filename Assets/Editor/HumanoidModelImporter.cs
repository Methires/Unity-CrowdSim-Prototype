using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEditor;
using System.Linq;

class HumanoidModelImporter : AssetPostprocessor
{
    public static bool useImporter = true;
    private string _mocapActorId;
    //private string _htFilepath = "/Resources/MixamoTemplate.ht";//"/Resources/HumanTemplateFull.ht";
    private string _htFile = "AutodeskTemplate";
    private string _referenceAvatarName = "B3010@ReferenceAvatar";
    private bool _isAnimation = false;

    private static SkeletonBone[] skeletonDescription;
    private static bool secondPass = false;
    private static string path;

    void OnPreprocessModel()
    {
        string[] allowedFolders = new string[] { "Characters", "Resources/Animations" };
        bool requirementsMet = false;

        foreach (var folder in allowedFolders)
        {
            requirementsMet = requirementsMet || assetPath.Contains(folder);
        }

        useImporter = requirementsMet;
        //useImporter = false;
        if (useImporter)
        {
            if (assetPath.Contains("@"))
            {
                _mocapActorId = Path.GetFileNameWithoutExtension(assetPath).Split('@')[0];
                _isAnimation = true;
                _htFile = "MocapTemplate";
            }

            if (assetPath.ToLower().Contains("mixamo"))
            {
                _htFile = "MixamoTemplate";
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
            modelImporter.optimizeGameObjects = _isAnimation;
            modelImporter.humanDescription = ReadHumanDescription();
            modelImporter.sourceAvatar = null;
            modelImporter.extraExposedTransformPaths = new string[] { string.Format("{0}:Solving:Hips", _mocapActorId) };
            modelImporter.motionNodeName = "<None>";

            if (!secondPass && _isAnimation)
            {
                string[] paths = AssetDatabase.FindAssets(_referenceAvatarName);
                string referenceAvatarPath = AssetDatabase.GUIDToAssetPath(paths[0]);
                Avatar avatar = AssetDatabase.LoadAssetAtPath<Avatar>(referenceAvatarPath);
                modelImporter.sourceAvatar = avatar;
            }

            path = assetPath;
        }
    }

    private void CreateModelPreview(GameObject g, string assetPath)
    {
        GameObject previewInstance = GameObject.Instantiate(g, new Vector3(1000, 1000, 1000), Quaternion.identity) as GameObject;
        GameObject previewCamera = new GameObject();
        Camera camera = previewCamera.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Color;
        camera.backgroundColor = Color.white;
        camera.farClipPlane = 5.0f;
        Light light = previewCamera.AddComponent<Light>();
        light.type = LightType.Directional;

        Vector3 cameraOffset = new Vector3(0.0f, 1.0f, 2.0f);
        previewCamera.transform.position = previewInstance.transform.position + cameraOffset;
        previewCamera.transform.LookAt(previewInstance.transform.position + new Vector3(0.0f, 1.0f, 0.0f));

        int resWidth = 400;
        int resHeight = 400;

        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        camera.targetTexture = rt;

        Texture2D screenshot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        RenderTexture.active = camera.targetTexture;
        camera.Render();

        screenshot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        screenshot.Apply();

        string dir = Path.GetDirectoryName(assetPath);
        string filename = string.Format("{0}.png", Path.GetFileNameWithoutExtension(assetPath));

        byte[] bytes = screenshot.EncodeToPNG();
        string path = string.Format("{0}/{1}", dir, filename);
        File.WriteAllBytes(path, bytes);

        camera.targetTexture = null;
        RenderTexture.active = null;

        GameObject.DestroyImmediate(screenshot);
        GameObject.DestroyImmediate(rt);
        GameObject.DestroyImmediate(previewCamera);
        GameObject.DestroyImmediate(previewInstance);
    }

    void OnPostprocessModel(GameObject g)
    {
        if (useImporter)
        {
            ModelImporter modelImporter = assetImporter as ModelImporter;
            if (!_isAnimation)
            {
                CreateModelPreview(g, modelImporter.assetPath);
            }


            if (_isAnimation && !secondPass)
            {
                if (skeletonDescription == null)
                {
                    PrepareSkeletonDescription(modelImporter);
                }

                secondPass = true;

                var clipAnimations = modelImporter.defaultClipAnimations;

                foreach (var animation in clipAnimations)
                {
                    animation.lockRootHeightY = true;
                    animation.keepOriginalPositionY = true;
                }

                modelImporter.clipAnimations = clipAnimations;
                AssetDatabase.ImportAsset(path);
            }
            else
            {
                secondPass = false;
            }
        }
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if (useImporter)
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
    }

    private static string[] GenerateDerivedNames(string primordialName)
    {
        if (useImporter)
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
        return null;
    }

    private void PrepareSkeletonDescription(ModelImporter modelImporter)
    {
        if (useImporter)
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
    }

    private HumanDescription ReadHumanDescription()
    {
        HumanDescription humanDescription = new HumanDescription();
        if (useImporter)
        {
            List<HumanBone> humanBones = new List<HumanBone>();
            string boneNameModifier = "";
            if (_isAnimation)
            {
                boneNameModifier = _mocapActorId + ":";
            }

            HumanTemplate template = Resources.Load(_htFile) as HumanTemplate;
            string[] boneNames = HumanTrait.BoneName;

            List<string> mapping = new List<string>();
            foreach (string boneName in boneNames)
            {
                HumanBone newBone = new HumanBone();
                newBone.humanName = boneName;
                newBone.boneName = template.Find(boneName);

                if (newBone.boneName != "")
                {
                    newBone.boneName = boneNameModifier + newBone.boneName;
                    HumanLimit limit = new HumanLimit();
                    limit.useDefaultValues = true;
                    newBone.limit = limit;
                    humanBones.Add(newBone);
                }
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
        }
        return humanDescription;
    }
}
