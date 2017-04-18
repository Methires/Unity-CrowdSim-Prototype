using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class Preparer
{
    static void PrepareSimulation()
    {
        SceneNameGetter.ParseXmlConfig(Application.dataPath + "/config.xml");
        EditorSceneManager.OpenScene(string.Format("Assets/Scenes/{0}.unity", SceneNameGetter.SceneName), OpenSceneMode.Single);
        var controller = GameObject.FindObjectOfType<SimulationController>();
        controller.LoadFromConfig = true;
        controller.Close = true;

        EditorApplication.isPlaying = true;
    }
}
