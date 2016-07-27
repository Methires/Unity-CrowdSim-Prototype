using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CrowdController))]
[RequireComponent(typeof(CreateScenario))]
public class SimulationController : MonoBehaviour
{
    public int ScenarioRepeats;
    public int SimultaneousScenarioInstances;
    public string ScenarioFileName;

    private XmlReader _xmlReader;
    private CrowdController _crowdSpawner;
    private CreateScenario _scenarioCreator;
    private int _repeatsCounter;

	void Start()
    {
        _crowdSpawner = GetComponent<CrowdController>();
        _scenarioCreator = GetComponent<CreateScenario>();
        _xmlReader = new XmlReader();
        _xmlReader.LoadXmlScenario(ScenarioFileName);
        _scenarioCreator.RawInfoToListPerAgent(_xmlReader.scenarioData);
        StartInstanceOfSimulation();
	}
	
    public void StartInstanceOfSimulation()
    {
        _crowdSpawner.GenerateCrowd();
        _scenarioCreator.GenerateInGameSequence(SimultaneousScenarioInstances);
        _repeatsCounter++;
    }

    public void EndInstanceOfSimulation()
    {
        _crowdSpawner.RemoveCrowd();
        StartCoroutine(EndInstance());
    }

    private IEnumerator EndInstance()
    {
        yield return new WaitForSeconds(0.5f);
        if (_repeatsCounter < ScenarioRepeats)
        {
            StartInstanceOfSimulation();
        }
    }
}
