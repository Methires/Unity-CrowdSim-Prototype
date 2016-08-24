using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CrowdController))]
[RequireComponent(typeof(SequencesCreator))]
[RequireComponent(typeof(WeatherConditions))]
[RequireComponent(typeof(Screenshooter))]
[RequireComponent(typeof(CamerasController))]
public class SimulationController : MonoBehaviour
{
    public int Repeats;
    [Header("Scenario")]
    public string ScenarioFilePath;
    public int SimultaneousScenarioInstances;
    [Header("Tracking")]
    public bool Tracking;
    public int SessionLength = 1;
    [Header("Results")]
    public string ScreenshotsDirectory = "D:/Screenshots";
    [Header("Testing")]
    public bool LoadFromConfig;
    public bool MarkWithPlanes;
    public bool Close;

    private CrowdController _crowdController;
    private SequencesCreator _sequenceCreator;
    private Screenshooter _screenshooter;
    private List<SequenceController> _sequencesControllers;
    private int _repeatsCounter;
    private float _elapsedTimeCounter;
    private bool _instanceFinished;
    private bool _screnshooterActive;

    void Start()
    {
        _crowdController = GetComponent<CrowdController>();
        _sequenceCreator = GetComponent<SequencesCreator>();
        _screenshooter = FindObjectOfType<Screenshooter>();
        _sequenceCreator.MarkAgents = MarkWithPlanes;
        WeatherConditions weather = GetComponent<WeatherConditions>();
        if (LoadFromConfig)
        {
            XmlConfigParser.ParseXmlWithConfiguration(Application.dataPath + "/config.xml");
            weather.Time = XmlConfigParser.Data.DayTime;
            weather.Conditions = XmlConfigParser.Data.WeatherConditions;
            _crowdController.CreatePrefabs = true;
            _crowdController.LoadAgentsFromResources = true;
            _crowdController.AgentsFilter = XmlConfigParser.Data.Models;
            _crowdController.MaxPeople = XmlConfigParser.Data.MaxPeople;
            Tracking = XmlConfigParser.Data.Tracking;
            ScenarioFilePath = XmlConfigParser.Data.ScenarioFilePath;
            SessionLength = XmlConfigParser.Data.Length > 1 ? XmlConfigParser.Data.Length : 1;
            Repeats = XmlConfigParser.Data.Repeats;
            SimultaneousScenarioInstances = XmlConfigParser.Data.Instances;
            ScreenshotsDirectory = XmlConfigParser.Data.ResultsPath;
            _screenshooter.TakeScreenshots = true;
            switch (XmlConfigParser.Data.VisualResultsType)
            {
                case 1:
                    _screenshooter.MarkAgentsOnScreenshots = false;
                    break;
                case 2:
                    _screenshooter.MarkAgentsOnScreenshots = false;
                    break;
                case 3:
                    _screenshooter.MarkAgentsOnScreenshots = true;
                    break;
                default:
                    _screenshooter.MarkAgentsOnScreenshots = true;
                    break;
            }
            Close = true;
            MarkWithPlanes = false;
            GetComponent<CamerasController>().enabled = false;
        }
        weather.GenerateWeatherConditions();

        if (!Tracking)
        {
            XmlScenarioReader.ParseXmlWithScenario(ScenarioFilePath);
            _sequenceCreator.RawInfoToListPerAgent(XmlScenarioReader.ScenarioData);
            _sequencesControllers = new List<SequenceController>();
        }
        
        _screnshooterActive = _screenshooter.TakeScreenshots;
        if (_screnshooterActive)
        {
            string dir = string.Format("/Session-{0:yyyy-MM-dd_hh-mm-ss-tt}", System.DateTime.Now);
            ScreenshotsDirectory += dir;
        }
        _screenshooter.TakeScreenshots = false;

        Invoke("StartInstanceOfSimulation", 0.1f);
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
        _screenshooter.TakeScreenshots = _screnshooterActive;


        if (!Tracking)
        {
            _sequencesControllers = _sequenceCreator.GenerateInGameSequences(SimultaneousScenarioInstances, out SessionLength);
            _screenshooter.Annotator = new Annotator(_sequenceCreator.Agents);
        }
        else
        {
            _screenshooter.Annotator = new Annotator(_crowdController.Crowd);
        }


        _repeatsCounter++;
        _instanceFinished = false;
        _elapsedTimeCounter = 0.0f;
    }

    private void EndInstanceOfSimulation()
    {
        _instanceFinished = true;

        if (_screnshooterActive)
        {
            _screenshooter.SaveScreenshotsAtDirectory(ScreenshotsDirectory + "/Take_" + _repeatsCounter);
            _screenshooter.TakeScreenshots = false;
        }

        _crowdController.RemoveCrowd();
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
            EditorApplication.isPlaying = false;
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
