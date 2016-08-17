using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CrowdController))]
[RequireComponent(typeof(SequencesCreator))]
public class SimulationController : MonoBehaviour
{
    public int Repeats;
    public int SimultaneousScenarioInstances;
    public string ScenarioFileName;
    public string ScreenshotsDirectory = "D:/Screenshots";
    public bool Tracking;
    public float SessionLength;
    [Header("Weather")]
    [Range(1, 3)]
    public int DayTime;
    [Range(1, 5)]
    public int WeatherConditions;

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
        SetDayTimeLight(DayTime);
        SetWeatherConditions(WeatherConditions);
        StartInstanceOfSimulation();
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
            else
            {
                _elapsedTimeCounter += Time.deltaTime;
                if (_elapsedTimeCounter >= SessionLength )
                {
                    EndInstanceOfSimulation();
                }
            }
        }
    }
	
    private void StartInstanceOfSimulation()
    {
        _crowdController.GenerateCrowd();
        if (!Tracking)
        {
            _sequencesControllers = _sequenceCreator.GenerateInGameSequences(SimultaneousScenarioInstances);
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
            #else
                Application.Quit();
            #endif
        }
    }

    private void SetDayTimeLight(int id)
    {
        GameObject light;
        if (FindObjectOfType<Light>() == null)
        {
            light = new GameObject();
            light.name = "Light";
            light.AddComponent<Light>();
            light.transform.position = Vector3.zero;
            light.GetComponent<Light>().type = LightType.Directional;
            light.GetComponent<Light>().shadows = LightShadows.Soft;
        }
        else
        {
            light = FindObjectOfType<Light>().gameObject;
        }
        float value = light.transform.eulerAngles.x;
        switch (id)
        {
            case 1:
                value = 30.0f;
                light.GetComponent<Light>().color = Color.white;
                break;
            case 2:
                value = 90.0f;
                light.GetComponent<Light>().color = Color.white;
                break;
            case 3:
                value = 170.0f;
                light.GetComponent<Light>().color = new Color32(144, 124, 70, 255);
                break;
            default:
                break;
        }
        light.transform.rotation = Quaternion.Euler(value, 0.0f, 0.0f);
    }

    private void SetWeatherConditions(int id)
    {
        Light light = FindObjectOfType<Light>();
        switch (id)
        {
            case 2:
                Instantiate(Resources.Load("Weather/Rain"));
                light.intensity = 0.75f;
                light.shadowStrength = 0.5f;
                break;
            case 3:
                Instantiate(Resources.Load("Weather/Snow"));
                light.intensity = 0.65f;
                light.shadowStrength = 0.5f;
                break;
            case 4:
                light.color = Color.gray;
                light.intensity = 0.5f;
                light.shadowStrength = 0.5f;
                break;
            case 5:
                Instantiate(Resources.Load("Weather/Fog"));
                light.color = Color.gray;
                light.intensity = 0.5f;
                light.shadowStrength = 0.45f;
                break;
            default:
                break;
        }
    }
}
