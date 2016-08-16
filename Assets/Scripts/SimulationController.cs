using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CrowdController))]
[RequireComponent(typeof(SequencesCreator))]
public class SimulationController : MonoBehaviour
{
    public int ScenarioRepeats;
    public int SimultaneousScenarioInstances;
    public string ScenarioFileName;
    public string ScreenshotsDirectory = "D:/Screenshots";
    private Screenshooter _screenshooter;


    private XmlScenarioReader _xmlReader;
    private CrowdController _crowdController;
    private SequencesCreator _sequenceCreator;
    private List<SequenceController> _sequencesControllers;
    private int _repeatsCounter;
    private bool _instanceFinished;

	void Start()
    {
        _crowdController = GetComponent<CrowdController>();
        _sequenceCreator = GetComponent<SequencesCreator>();
        _xmlReader = new XmlScenarioReader();
        _xmlReader.ParseXmlWithScenario(ScenarioFileName);
        _sequenceCreator.RawInfoToListPerAgent(_xmlReader.ScenarioData);
        _sequencesControllers = new List<SequenceController>();
        _screenshooter = FindObjectOfType<Screenshooter>();

        string dir = string.Format("/Session-{0:yyyy-MM-dd_hh-mm-ss-tt}", System.DateTime.Now);

        ScreenshotsDirectory += dir;
        StartInstanceOfSimulation();
	}

    void Update()
    {
        if (!_instanceFinished)
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
    }
	
    private void StartInstanceOfSimulation()
    {
        _crowdController.GenerateCrowd();
        _sequencesControllers = _sequenceCreator.GenerateInGameSequences(SimultaneousScenarioInstances);
        _repeatsCounter++;
        _instanceFinished = false;
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
        if (_repeatsCounter < ScenarioRepeats)
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
}
