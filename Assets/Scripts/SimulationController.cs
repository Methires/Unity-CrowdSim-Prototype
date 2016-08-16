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
    public bool Tracking;
    public float SessionLength;

    private XmlScenarioReader _xmlReader;
    private CrowdController _crowdController;
    private SequencesCreator _sequenceCreator;
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
