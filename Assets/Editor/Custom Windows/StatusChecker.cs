using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class StatusChecker : EditorWindow
{
    bool _importerExtension = true;

    private bool _generated = false;
    private bool _closeAfterSaving = false;
    [MenuItem("Window/Status Checker")]

    static void Init()
    {
        StatusChecker window = (StatusChecker)GetWindow(typeof(StatusChecker));
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("Light Baking", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Baking:", Lightmapping.isRunning? "YES" : "NO");

        GUILayout.Label("NavMesh Baking", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Baking:", UnityEditor.AI.NavMeshBuilder.isRunning ? "YES" : "NO");

        GUILayout.Label("Scripts Compiling", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Compiling:", EditorApplication.isCompiling? "YES" : "NO");

        GUILayout.Label("Custom importer", EditorStyles.boldLabel);
        _importerExtension = EditorGUILayout.Toggle("Enable:",_importerExtension);
        //HumanoidModelImporter.useImporter = _importerExtension;

        //if (GUILayout.Button("Generate Scene"))
        //{
        //    SceneGenerator.Start(10);
        //} 

        //SceneNameGetter.ParseXmlConfig(Application.dataPath + "/config.xml");
        //if (SceneNameGetter.Mode == "generation")
        //{
        //    if (EditorApplication.isPlaying == true)
        //    {
        //        _generated = true;
        //    }

        //    if (_generated && EditorApplication.isPlaying == false)
        //    {
        //        SaveScene();
        //        _generated = false;
        //        EditorApplication.isPlaying = false;

        //        if (_closeAfterSaving)
        //        {
        //            EditorApplication.Exit(0);
        //        }
        //    }
        //}      
    }

    private void SaveScene()
    {
        int index = GetGighestScenePrefabIndex();
        string[] possiblePrefabs = AssetDatabase.FindAssets(string.Format("ScenePrefab#{0}", index), new string[] { "Assets/Scenes" });
        string prefabPath = AssetDatabase.GUIDToAssetPath(possiblePrefabs[0]);
        GameObject scenePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        foreach (GameObject o in Object.FindObjectsOfType<GameObject>())
        {
            DestroyImmediate(o);
        }

        GameObject prefabInstance = Instantiate(scenePrefab, Vector3.zero, Quaternion.identity) as GameObject;

        string sceneName = string.Format("Assets/Scenes/Scene#{0}.unity", index);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), sceneName, true);

        EditorSceneManager.OpenScene(sceneName);
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();

        //CreateScenePreview(prefabInstance, sceneName);
        DestroyImmediate(prefabInstance);
        //EditorSceneManager.OpenScene("Assets/_Scenes/main.unity");
    }

    private int GetGighestScenePrefabIndex()
    {
        string path = string.Format("{0}//{1}", Application.dataPath, "Scenes");
        string[] scenePrefabs = Directory.GetFiles(path, "*.prefab");
        int highestIndex = 0;
        foreach (var scene in scenePrefabs)
        {
            string fileName = Path.GetFileNameWithoutExtension(scene);
            int index = 0;
            int.TryParse(fileName.Split('#')[1], out index);

            if (index > highestIndex)
            {
                highestIndex = index;
            }
        }

        return highestIndex;
    }

    void Update()
    {
        if (Lightmapping.isRunning || UnityEditor.AI.NavMeshBuilder.isRunning || EditorApplication.isCompiling)
        {
            EditorApplication.isPlaying = false;
        }
    }
}
