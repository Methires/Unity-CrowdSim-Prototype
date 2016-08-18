using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CrowdController))]
[RequireComponent(typeof(SequencesCreator))]
[RequireComponent(typeof(WeatherConditions))]
public class SimulationController : MonoBehaviour
{
    public int Repeats;
    public int SimultaneousScenarioInstances;
    public string ScenarioFileName;
    public string ScreenshotsDirectory = "D:/Screenshots";
    public bool Tracking;
    public float SessionLength;
    [Header("Closing editor")]
    public bool Close;

    private XmlScenarioReader _xmlReader;
    private CrowdController _crowdController;
    private SequencesCreator _sequenceCreator;
    private Screenshooter _screenshooter;
    private List<SequenceController> _sequencesControllers;
    private int _repeatsCounter;
    private float _elapsedTimeCounter;
    private bool _instanceFinished;

    void Start()
    {
        _crowdController = GetComponent<CrowdController>();
        _sequenceCreator = GetComponent<SequencesCreator>();
        if (!Tracking)
        {
            _xmlReader = new XmlScenarioReader();
            _xmlReader.ParseXmlWithScenario(ScenarioFileName);
            _sequenceCreator.RawInfoToListPerAgent(_xmlReader.ScenarioData);
            _sequencesControllers = new List<SequenceController>();
        }
        _screenshooter = FindObjectOfType<Screenshooter>();
        string dir = string.Format("/Session-{0:yyyy-MM-dd_hh-mm-ss-tt}", System.DateTime.Now);
        ScreenshotsDirectory += dir;
        Invoke("StartInstanceOfSimulation", 0.1f);
        //StartInstanceOfSimulation();
    }

    void Update()
    {
        if (!_instanceFinished)
        {
            if (!Tracking)
            {
                if (_sequencesControllers.Count > 0)
                {
                    bool endInstance = true;
                    foreach (SequenceController agentScenario in _sequencesControllers)
                    {
                        if (!agentScenario.IsFinished)
                        {
                            endInstance = false;
                            break;
                        }
                    }
                    if (endInstance)
                    {
                        EndInstanceOfSimulation();
                    }
                }
            }

            _elapsedTimeCounter += Time.deltaTime;
            if (Tracking)
            {
                if (_elapsedTimeCounter >= SessionLength)
                {
                    EndInstanceOfSimulation();
                }
            } 
            else
            {
                if (_elapsedTimeCounter >= SessionLength * 5.0f)
                {
                    EndInstanceOfSimulation();
                    Debug.Log("Aborting sequence");
                }
            }
        }
    }

    private void StartInstanceOfSimulation()
    {
        _crowdController.GenerateCrowd();
        if (!Tracking)
        {
            _sequencesControllers = _sequenceCreator.GenerateInGameSequences(SimultaneousScenarioInstances, out SessionLength);
        }
        _repeatsCounter++;
        _instanceFinished = false;
        _elapsedTimeCounter = 0.0f;
    }

    private void EndInstanceOfSimulation()
    {
        _instanceFinished = true;
        _crowdController.RemoveCrowd();
        _screenshooter.SaveScreenshotsAtDirectory(ScreenshotsDirectory + "/Take_" + _repeatsCounter);
        StartCoroutine(EndInstance());
    }

    private IEnumerator EndInstance()
    {
        yield return new WaitForSeconds(0.5f);
        if (_repeatsCounter < Repeats)
        {
            StartInstanceOfSimulation();
        }
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            if (Close)
            {
                EditorApplication.Exit(0);
            }
#else
                Application.Quit();
#endif
        }
    }
}
