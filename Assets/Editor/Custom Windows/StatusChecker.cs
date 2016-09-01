using UnityEngine;
using UnityEditor;
using System;

public class StatusChecker : EditorWindow
{
    bool _importerExtension = false;
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

        GUILayout.Label("Custom importer", EditorStyles.boldLabel);
        _importerExtension = EditorGUILayout.Toggle("Enable:",_importerExtension);
        try
        {
            HumanoidModelImporter.useImporter = _importerExtension;
        }
        catch (Exception e)
        {
            Debug.Log("OMG");
        }
    }

    void Update()
    {
        if (Lightmapping.isRunning || NavMeshBuilder.isRunning || EditorApplication.isCompiling)
        {
            EditorApplication.isPlaying = false;
        }
    }
}
