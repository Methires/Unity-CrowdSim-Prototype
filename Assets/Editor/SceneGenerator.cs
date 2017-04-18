using UnityEditor;
using UnityEngine;
using ProceduralSceneGenerator;
using UnityEditor.SceneManagement;
using System.IO;

public class SceneGenerator
{
    public static void Start(int mapSize)
    {
        var onSceneGenerator = GameObject.FindObjectOfType<BuildScene>();
        onSceneGenerator.mapSize = mapSize;
        onSceneGenerator.Generate();
        SaveScene();
    }

    private static void SaveScene()
    {
        int index = GetGighestScenePrefabIndex();
        string[] possiblePrefabs = AssetDatabase.FindAssets(string.Format("ScenePrefab#{0}", index), new string[] { "Assets/Scenes" });
        string prefabPath = AssetDatabase.GUIDToAssetPath(possiblePrefabs[0]);
        GameObject scenePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        UnityEngine.SceneManagement.Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        GameObject prefabInstance = GameObject.Instantiate(scenePrefab, Vector3.zero, Quaternion.identity) as GameObject;
        string sceneName = string.Format("Assets/Scenes/Scene#{0}.unity", index);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), sceneName, false);
        prefabInstance = GameObject.Instantiate(scenePrefab, Vector3.zero, Quaternion.identity) as GameObject;

        CreateScenePreview(prefabInstance, sceneName);
        UnityEditor.AI.NavMeshBuilder.BuildNavMesh();

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), sceneName, false);


        GameObject.DestroyImmediate(prefabInstance);



        //foreach (GameObject o in GameObject.FindObjectsOfType<GameObject>())
        //{
        //    GameObject.DestroyImmediate(o);
        //}
    }

    private static void CreateScenePreview(GameObject g, string assetPath)
    {
        GameObject previewCamera = new GameObject();
        Camera camera = previewCamera.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.Color;
        camera.backgroundColor = Color.white;
        camera.farClipPlane = 1000.0f;
        Light light = previewCamera.AddComponent<Light>();
        light.type = LightType.Directional;

        Vector3 cameraOffset = new Vector3(0.0f, 50.0f, 50.0f);
        previewCamera.transform.position = g.transform.position + cameraOffset;
        previewCamera.transform.LookAt(g.transform.position + new Vector3(0.0f, 1.0f, 0.0f));

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
        GameObject.DestroyImmediate(g);
    }

    private static int GetGighestScenePrefabIndex()
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

}
