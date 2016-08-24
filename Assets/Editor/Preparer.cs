using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class Preparer
{
    static void PrepareSimulation()
    {
        XmlConfigReader.ParseXmlWithConfiguration(Application.dataPath + "/config.xml");
        EditorSceneManager.OpenScene(string.Format("Assets/Scenes/{0}.unity", XmlConfigReader.Data.SceneName), OpenSceneMode.Single);
        //LayoutHack.LoadLayoutHack();
        EditorApplication.isPlaying = true;
    }
}
