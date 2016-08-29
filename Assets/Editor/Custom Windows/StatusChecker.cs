using UnityEngine;
using UnityEditor;

public class StatusChecker : EditorWindow
{ 
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
        EditorGUILayout.LabelField("Baking:", NavMeshBuilder.isRunning ? "YES" : "NO");

        GUILayout.Label("Scripts Compiling", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Compiling:", EditorApplication.isCompiling? "YES" : "NO");
    }

    void Update()
    {
        if (Lightmapping.isRunning || NavMeshBuilder.isRunning || EditorApplication.isCompiling)
        {
            EditorApplication.isPlaying = false;
        }
    }
}
