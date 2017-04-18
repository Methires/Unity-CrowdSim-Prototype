using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(CrowdController))]
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
        if (LoadFromConfig)
        {
            XmlConfigReader.ParseXmlConfig(Application.dataPath + "/config.xml");

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

            Close = true;
            MarkWithPlanes = false;
            GetComponent<CamerasController>().enabled = false;
        }

        if (!Tracking)
        {
            XmlScenarioReader.ParseXmlWithScenario(ScenarioFile);
            _actorsNames = GetActorsNames(XmlScenarioReader.ScenarioData);
            _actorsSequencesControllers = new List<SequenceController>();
        }

        if (_screnshooterActive)
        {
            string dir = string.Format("/Session-{0:yyyy-MM-dd_hh-mm-ss-tt}", System.DateTime.Now);
            ScreenshotsDirectory += dir;
        }

        Invoke("StartInstanceOfSimulation", 0.5f);
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

                Debug.Log("Screenshot buffer full! Saving to: " + path);
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

        _repeatsCounter++;
        _instanceFinished = false;
        _elapsedTimeCounter = 0;
    }

    private void EndInstanceOfSimulation()
    {
        _instanceFinished = true;

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
                crowd[index].GetComponent<UnityEngine.AI.NavMeshAgent>().avoidancePriority = 0;
                crowd[index].GetComponent<UnityEngine.AI.NavMeshAgent>().stoppingDistance = 0.02f;
                //crowd[index].GetComponent<GenerateDestination>().enabled = false;
                crowd[index].AddComponent<SpeedAdjuster>();
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


//Uncomment commented and comment ucommented to allow for bulk simulation

//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Diagnostics;
//using UnityEditor;

//[RequireComponent(typeof(CrowdController))]
//[RequireComponent(typeof(WeatherConditions))]
//[RequireComponent(typeof(Screenshooter))]
//[RequireComponent(typeof(CamerasController))]
//public class SimulationController : MonoBehaviour
//{
//    [Header("HAXED VERSION")]
//    public int SessionLength = 1;
//    public string ScreenshotsDirectory = "D:/Screenshots";
//    public bool LoadFromConfig
//    {
//        get
//        { return false; }
//        set { }
//    }
//    public bool Close
//    {
//        get
//        { return false; }
//        set { }
//    }
//    public bool Tracking
//    {
//        get
//        { return true; }
//        set
//        { }
//    }

//    private CrowdController _crowdController;
//    private SequencesCreator _sequenceCreator;
//    private Screenshooter _screenshooter;
//    private WeatherConditions _weatherController;
//    private List<SequenceController> _actorsSequencesControllers;
//    private int _repeatsCounter;
//    private int _elapsedTimeCounter;
//    private bool _instanceFinished;
//    private bool _screnshooterActive;
//    private bool _screenshotBufferFull = false;
//    private string[] _actorsNames;
//    private List<GameObject> _actors;


//    public struct SceneConditions
//    {
//        public int Time;
//        public int Weather;
//        public int Crowd;

//        public SceneConditions(int time, int weather, int crowd)
//        {
//            Time = time;
//            Weather = weather;
//            Crowd = crowd;
//        }
//    }
//    public List<SceneConditions> _conditions;
//    public int CurrentCondtion;

//    void Start()
//    {
//        _crowdController = GetComponent<CrowdController>();
//        _sequenceCreator = new SequencesCreator();
//        _screenshooter = FindObjectOfType<Screenshooter>();
//        _weatherController = GetComponent<WeatherConditions>();
//        _conditions = new List<SceneConditions>();
//        for (int i = 3; i > 0; i--)
//        {
//            for (int k = 0; k < 3; k++)
//            {
//                for (int j = 1; j < 6; j++)
//                {
//                    _conditions.Add(new SceneConditions(i, j, 25 + k * 35));
//                }
//            }
//        }
//        CurrentCondtion = 0;
//        Autoscreens(_conditions[CurrentCondtion]);
//    }

//    void Update()
//    {
//        if (!_instanceFinished)
//        {
//            _elapsedTimeCounter++;
//            if (_elapsedTimeCounter >= SessionLength)
//            {
//                EndInstanceOfSimulation();
//            }

//            if (_screenshotBufferFull)
//            {
//                string path = ScreenshotsDirectory + "/Take_" + _repeatsCounter;

//                UnityEngine.Debug.Log("Screenshot buffer full! Saving to: " + path);
//                _screenshooter.SaveScreenshotsAtDirectory(path);
//                _screenshotBufferFull = false;
//            }
//        }
//    }

//    private void StartInstanceOfSimulation()
//    {
//        _crowdController.GenerateCrowd();
//        _sequenceCreator.MarkActions = false;
//        _sequenceCreator.Crowd = true;
//        _sequenceCreator.ShowSequenceOnConsole = false;
//        foreach (GameObject agent in _crowdController.Crowd.Where(x => x.tag == "Crowd").ToList())
//        {
//            _sequenceCreator.RawInfoToListPerAgent(_crowdController.PrepareActions(agent));
//            _sequenceCreator.Agents = new List<GameObject> { agent };
//            int temp;
//            _sequenceCreator.GenerateInGameSequences(1, out temp);
//        }

//        _screenshooter.Annotator = new Annotator(_crowdController.Crowd);
//        _screenshooter.TakeScreenshots = _screnshooterActive;

//        _instanceFinished = false;
//        _elapsedTimeCounter = 0;
//    }

//    private void EndInstanceOfSimulation()
//    {
//        _instanceFinished = true;

//        if (_screnshooterActive)
//        {
//            _screenshooter.SaveScreenshotsAtDirectory(ScreenshotsDirectory + "/Take_" + _repeatsCounter);
//            _screenshooter.TakeScreenshots = false;
//        }

//        _crowdController.RemoveCrowd();
//        //StartCoroutine(EndInstance());
//        EndInstance();
//    }

//    private void EndInstance()
//    {
//        //yield return new WaitForSeconds(0.5f);
//        _weatherController.RemoveConditions();
//        if (CurrentCondtion < _conditions.Count - 1)
//        {
//            CurrentCondtion++;
//            Autoscreens(_conditions[CurrentCondtion]);
//        }
//        else
//        {
//            //Shuts down computer
//            //var psi = new ProcessStartInfo("shutdown", "/s /t 0");
//            //psi.CreateNoWindow = true;
//            //psi.UseShellExecute = false;
//            //Process.Start(psi);
//            EditorApplication.isPlaying = false;
//            EditorApplication.Exit(0);
//        }
//    }

//    public void NotifyScreenshotBufferFull()
//    {
//        _screenshotBufferFull = true;
//    }

//    private void Autoscreens(SceneConditions conditions)
//    {
//        _weatherController.Time = conditions.Time;
//        _weatherController.Conditions = conditions.Weather;

//        _crowdController.CreatePrefabs = true;
//        _crowdController.LoadAgentsFromResources = true;
//        _crowdController.AgentsFilter = "";
//        _crowdController.MaxPeople = conditions.Crowd;
//        _crowdController.ActionsFilter = "";

//        Tracking = true;
//        SessionLength = 20;

//        string time, con, size;
//        switch (conditions.Time)
//        {
//            case 1:
//                time = "Moring";
//                break;
//            case 2:
//                time = "Noon";
//                break;
//            case 3:
//                time = "Evening";
//                break;
//            default:
//                time = "Time";
//                break;
//        }
//        switch (conditions.Weather)
//        {
//            case 1:
//                con = "Sun";
//                break;
//            case 2:
//                con = "Rain";
//                break;
//            case 3:
//                con = "Snow";
//                break;
//            case 4:
//                con = "Overcast";
//                break;
//            case 5:
//                con = "Fog";
//                break;
//            default:
//                con = "Weather";
//                break;
//        }
//        switch (conditions.Crowd)
//        {
//            case 25:
//                size = "Small";
//                break;
//            case 60:
//                size = "Medium";
//                break;
//            case 95:
//                size = "Large";
//                break;
//            default:
//                size = "Size";
//                break;
//        }
//        ScreenshotsDirectory = string.Format("D:/Screenshots/Sample3_Estate/{0}_{1}_{2}", time, con, size);

//        _screenshooter.SetParams(true, 2);
//        _screenshooter.ResWidth = 800;
//        _screenshooter.ResHeight = 600;
//        _screenshooter.ChangeFrameRate(8);
//        _screenshooter.ScreenshotLimit = 500;

//        GetComponent<CamerasController>().enabled = false;

//        _weatherController.GenerateWeatherConditions();
//        if (GetComponent<Lighting>() != null)
//        {
//            GetComponent<Lighting>().SetSampleSceneLighting();
//        }

//        SessionLength *= _screenshooter.FrameRate;

//        _screnshooterActive = _screenshooter.TakeScreenshots;
//        if (_screnshooterActive)
//        {
//            string dir = string.Format("/Session-{0:yyyy-MM-dd_hh-mm-ss-tt}", System.DateTime.Now);
//            ScreenshotsDirectory += dir;
//        }
//        _screenshooter.TakeScreenshots = false;

//        Invoke("StartInstanceOfSimulation", 0.5f);
//    }
//}
