using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(CrowdController))]
[RequireComponent(typeof(WeatherConditions))]
[RequireComponent(typeof(Screenshooter))]
[RequireComponent(typeof(CamerasController))]
public class SimulationController : MonoBehaviour
{
    public int Repeats;
    [Header("Scenario")]
    public string ScenarioFile;
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
    private List<SequenceController> _actorsSequencesControllers;
    private int _repeatsCounter;
    //private float _elapsedTimeCounter;
    private int _elapsedTimeCounter;
    private bool _instanceFinished;
    private bool _screnshooterActive;
    private bool _screenshotBufferFull = false;
    private string[] _actorsNames;
    private List<GameObject> _actors;

    void Start()
    {
        _crowdController = GetComponent<CrowdController>();
        _sequenceCreator = new SequencesCreator();
        _screenshooter = FindObjectOfType<Screenshooter>();
        WeatherConditions weather = GetComponent<WeatherConditions>();
        if (LoadFromConfig)
        {
            XmlConfigReader.ParseXmlConfig(Application.dataPath + "/config.xml");

            weather.Time = XmlConfigReader.Data.DayTime;
            weather.Conditions = XmlConfigReader.Data.WeatherConditions;

            _crowdController.CreatePrefabs = true;
            _crowdController.LoadAgentsFromResources = true;
            _crowdController.AgentsFilter = XmlConfigReader.Data.Models;
            _crowdController.MaxPeople = XmlConfigReader.Data.MaxPeople;
            _crowdController.ActionsFilter = XmlConfigReader.Data.ActionsFilter;

            Tracking = XmlConfigReader.Data.Tracking;
            ScenarioFile = XmlConfigReader.Data.ScenarioFile;
            SessionLength = XmlConfigReader.Data.Length > 1 ? XmlConfigReader.Data.Length : 1;
           
            Repeats = XmlConfigReader.Data.Repeats > 1 ? XmlConfigReader.Data.Repeats : 1;
            SimultaneousScenarioInstances = XmlConfigReader.Data.Instances > 1 ? XmlConfigReader.Data.Instances : 1;

            ScreenshotsDirectory = XmlConfigReader.Data.ResultsDirectory;
            _screenshooter.TakeScreenshots = true;
            _screenshooter.MarkAgentsOnScreenshots = XmlConfigReader.Data.BoundingBoxes;
            _screenshooter.ResWidth = XmlConfigReader.Data.ResolutionWidth;
            _screenshooter.ResHeight = XmlConfigReader.Data.ResolutionHeight;
            _screenshooter.ChangeFrameRate(XmlConfigReader.Data.FrameRate);

            Close = true;
            MarkWithPlanes = false;
            GetComponent<CamerasController>().enabled = false;
        }
        weather.GenerateWeatherConditions();
        SessionLength *= _screenshooter.FrameRate;
        if (!Tracking)
        {
            XmlScenarioReader.ParseXmlWithScenario(ScenarioFile);
            _actorsNames = GetActorsNames(XmlScenarioReader.ScenarioData);
            _actorsSequencesControllers = new List<SequenceController>();
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
            _elapsedTimeCounter++;

            if (Tracking)
            {
                if (_elapsedTimeCounter >= SessionLength)
                {
                    EndInstanceOfSimulation();
                }
            }
            else
            {
                if (_actorsSequencesControllers.Count > 0)
                {
                    bool endInstance = true;
                    foreach (SequenceController agentScenario in _actorsSequencesControllers)
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
                if (_elapsedTimeCounter >= SessionLength * 5.0f)
                {
                    EndInstanceOfSimulation();
                    Debug.Log("Aborting sequence");
                }
            }

            if (_screenshotBufferFull)
            {
                string path = ScreenshotsDirectory + "/Take_" + _repeatsCounter;

                Debug.Log("Screenshot buffer full! Saving to: " +  path);
                _screenshooter.SaveScreenshotsAtDirectory(path);
                _screenshotBufferFull = false;
            }
        }
    }

    private void StartInstanceOfSimulation()
    {
        _crowdController.GenerateCrowd();       
        if (!Tracking)
        {
            _actors = CreateActorsFromCrowd(SimultaneousScenarioInstances, _actorsNames);
            _sequenceCreator.RawInfoToListPerAgent(XmlScenarioReader.ScenarioData);
            _sequenceCreator.Agents = _actors;
            _sequenceCreator.MarkActions = MarkWithPlanes;
            _sequenceCreator.Crowd = false;
            _sequenceCreator.ShowSequenceOnConsole = true;
            _actorsSequencesControllers = _sequenceCreator.GenerateInGameSequences(SimultaneousScenarioInstances, out SessionLength);
            SessionLength *= _screenshooter.FrameRate;         
        }
        _sequenceCreator.MarkActions = false;
        _sequenceCreator.Crowd = true;
        _sequenceCreator.ShowSequenceOnConsole = false;
        foreach (GameObject agent in _crowdController.Crowd.Where(x => x.tag == "Crowd").ToList())
        {
            _sequenceCreator.RawInfoToListPerAgent(_crowdController.PrepareActions(agent));
            _sequenceCreator.Agents = new List<GameObject> { agent };
            int temp;
            _sequenceCreator.GenerateInGameSequences(1, out temp);
        }

        _screenshooter.Annotator = new Annotator(_crowdController.Crowd);
        _screenshooter.TakeScreenshots = _screnshooterActive;

        _repeatsCounter++;
        _instanceFinished = false;
        _elapsedTimeCounter = 0;
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

    public void NotifyScreenshotBufferFull()
    {
        _screenshotBufferFull = true;
    }

    private List<GameObject> CreateActorsFromCrowd(int simultaneousInstances, string[] actorsNames)
    {
        CrowdController crowdController = GetComponent<CrowdController>();
        if (crowdController.MaxPeople < actorsNames.Length * simultaneousInstances)
        {
            crowdController.RemoveCrowd();
            crowdController.MaxPeople = actorsNames.Length * simultaneousInstances;
            crowdController.GenerateCrowd();
        }
        List<GameObject> actors = new List<GameObject>();
        for (int i = 0; i < simultaneousInstances; i++)
        {
            for (int j = 0; j < actorsNames.Length; j++)
            {
                GameObject[] crowd = GameObject.FindGameObjectsWithTag("Crowd");
                int index = Random.Range(0, crowd.Length);
                crowd[index].tag = "ScenarioAgent";
                crowd[index].name = actorsNames[j] + "_" + i;
                crowd[index].GetComponent<NavMeshAgent>().avoidancePriority = 0;
                crowd[index].GetComponent<NavMeshAgent>().stoppingDistance = 0.02f;
                //crowd[index].GetComponent<GenerateDestination>().enabled = false;
                crowd[index].AddComponent<DisplayActivityText>();
                actors.Add(crowd[index]);
                if (MarkWithPlanes)
                {
                    MarkActorWithPlane(crowd[index]);
                }
            }
        }
        return actors;
    }

    private void MarkActorWithPlane(GameObject actor)
    {
        GameObject planeMarkup = GameObject.CreatePrimitive(PrimitiveType.Plane);
        planeMarkup.transform.localScale = new Vector3(0.1f, 1.0f, 0.1f);
        planeMarkup.transform.parent = actor.transform;
        planeMarkup.transform.localPosition = new Vector3(0.0f, 0.1f, 0.0f);
        Destroy(planeMarkup.GetComponent<MeshCollider>());
    }

    private string[] GetActorsNames(List<Level> data)
    {
        HashSet<string> hashedActors = new HashSet<string>();
        foreach (Level level in data)
        {
            foreach (Action activity in level.Actions)
            {
                foreach (Actor actor in activity.Actors)
                {
                    hashedActors.Add(actor.Name);
                }
            }
        }
        string[] actors = hashedActors.ToArray();
        return actors;
    }
}
