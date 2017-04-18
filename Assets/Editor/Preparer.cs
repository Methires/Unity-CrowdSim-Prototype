using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class Preparer
{
    static void PrepareSimulation()
    {
        SceneNameGetter.ParseXmlConfig(Application.dataPath + "/config.xml");
        if (SceneNameGetter.Mode == "generation")
        {
            EditorSceneManager.OpenScene("Assets/_Scenes/main.unity", OpenSceneMode.Single);
            SceneGenerator.Start(SceneNameGetter.MapSize);
            EditorApplication.Exit(0);
        }
        else
	    {
            EditorSceneManager.OpenScene(string.Format("Assets/Scenes/{0}.unity", SceneNameGetter.SceneName), OpenSceneMode.Single);
            var controller = GameObject.FindObjectOfType<SimulationController>();
            controller.LoadFromConfig = true;
            controller.Close = true;

            Lightmapping.bakedGI = false;
            Lightmapping.realtimeGI = false;

            Lightmapping.BakeAsync();
            //LightmapEditorSettings.

            if (Lightmapping.isRunning)
            {
                Lightmapping.completed += RunWhenDone;
            }
            else
            {
                EditorApplication.isPlaying = true;
            }

        }

    }

    private static void RunWhenDone()
    {
        EditorApplication.isPlaying = true;
    }

    static void CloseAfterImporting()
    {
        EditorApplication.Exit(0);
    }
}
