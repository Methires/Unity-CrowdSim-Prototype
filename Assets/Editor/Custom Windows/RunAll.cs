using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

public class RunAll : EditorWindow
{
    public static int[] crowd = { 50, 100, 200 };
    private bool readyToGen = false;
    private int index = 0;
    private static List<AllConfigs> listOfAllConfigs = new List<AllConfigs>();
    [MenuItem("Window/Run All")]
    static void Init()
    {
        RunAll window = GetWindow<RunAll>();
        window.Show();
    }

    public struct AllConfigs
    {
        public int time;
        public int conditions;
        public int crowd;
        public bool boundingBox;

        public AllConfigs(int t, int c, int ppl, bool bb)
        {
            time = t;
            conditions = c;
            crowd = ppl;
            boundingBox = bb;
        }
    }
    void Awake()
    {
        readyToGen = false;
    }
    void OnGUI()
    {
        GUILayout.Label("Scripts Compiling", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Compiling:", EditorApplication.isCompiling ? "YES" : "NO");
        if (GUILayout.Button("run"))
        {
            readyToGen = true;
            BuildStruct();
        }
        if (GUILayout.Button("clear"))
        {
            readyToGen = false;
            index = 0;
        }
        if (readyToGen && index < 90 && !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            BuildStruct();
            GameObject simOvs = GameObject.FindGameObjectWithTag("GameController");
            var wc = simOvs.GetComponent<WeatherConditions>();
            var cc = simOvs.GetComponent<CrowdController>();
            var ss = simOvs.GetComponent<Screenshooter>();
            wc.Time = listOfAllConfigs[index].time;
            wc.Conditions = listOfAllConfigs[index].conditions;
            cc.MaxPeople = listOfAllConfigs[index].crowd;
            //ss.MarkAgentsOnScreenshots = listOfAllConfigs[index].boundingBox;
            index++;
            UnityEngine.Debug.Log(index);
            EditorApplication.isPlaying = true;
        }
        else if (index == 90 && !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            /* var psi = new ProcessStartInfo("shutdown", "/s /t 0");
             psi.CreateNoWindow = true;
             psi.UseShellExecute = false;
             Process.Start(psi);
             */
        }
    }
    private static void BuildStruct()
    {
        int maxTime = 4;
        int maxConditions = 6;

        for (int j = 1; j < maxConditions; j++)
        {
            foreach (int k in crowd)
            {
                for (int i = 1; i < maxTime; i++)
                {
                    listOfAllConfigs.Add(new AllConfigs(i, j, k, true));
                    listOfAllConfigs.Add(new AllConfigs(i, j, k, false));
                }
            }
        }
    }

}