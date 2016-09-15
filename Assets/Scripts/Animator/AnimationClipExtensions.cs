using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.IO;




namespace UnityEditor.Animations
{
    public static class AnimationClipExtensions
    {
        public static Bounds ComplexActionAnimationArenaBounds(string namePattern)
        {
            Bounds bounds = new Bounds();
            string resourcesPath = Application.dataPath + "/Resources";
            string[] resourceFiles = Directory.GetFiles(resourcesPath,"*.fbx", SearchOption.AllDirectories);
            resourceFiles = resourceFiles.Where(x => x.Contains(namePattern)).ToArray();

            foreach (var animationName in resourceFiles)
            {
                Bounds clipBounds = AnimationArenaBounds(Path.GetFileNameWithoutExtension(animationName));
                bounds.Encapsulate(clipBounds);
            }
            return bounds;
        }

        public static Bounds AnimationArenaBounds(this AnimationClip clip)
        {           
            Bounds bounds = new Bounds();
            var allBindings = AnimationUtility.GetCurveBindings(clip).ToList();

            EditorCurveBinding[] rootXyz = allBindings.Take(3).ToArray();
            AnimationCurve[] curves = new AnimationCurve[3];

            curves[0] = AnimationUtility.GetEditorCurve(clip, rootXyz[0]);
            curves[1] = AnimationUtility.GetEditorCurve(clip, rootXyz[1]);
            curves[2] = AnimationUtility.GetEditorCurve(clip, rootXyz[2]);


            float x = curves[0].Evaluate(0);
            float y = curves[1].Evaluate(0);
            float z = curves[2].Evaluate(0);

            Vector3 startPositon = new Vector3(x, y, -z);

            Vector3 offset = clip.averageDuration * clip.averageSpeed;
            Vector3 endposition = startPositon - offset;

            bounds.Encapsulate(startPositon);
            bounds.Encapsulate(endposition);
            
            return bounds;
        }

        public static Bounds AnimationArenaBounds(string clipName)
        {
            return AnimationArenaBounds(LoadClip(clipName));
        }

        private static AnimationClip LoadClip(string clipName)
        {
            string path = "Assets/Resources/Animations/" + clipName + ".fbx";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip)) as AnimationClip;

            if (!clip)
            {
                Debug.LogError("Could not load animation asset: " + path);
            }

            return clip;
        }

    }
}
