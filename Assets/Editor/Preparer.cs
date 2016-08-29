using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class Preparer
{
    static void PrepareSimulation()
    {
        SceneNameGetter.ParseXmlConfig(Application.dataPath + "/config.xml");
        EditorSceneManager.OpenScene(string.Format("Assets/Scenes/{0}.unity", SceneNameGetter.SceneName), OpenSceneMode.Single);
        EditorApplication.isPlaying = true;
    }
}
