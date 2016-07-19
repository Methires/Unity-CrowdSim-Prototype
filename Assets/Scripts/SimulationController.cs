using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpawnCrowd))]
[RequireComponent(typeof(CreateScenario))]
public class SimulationController : MonoBehaviour
{
    public int ScenarioRepeats;
    public int SimultaneousScenarioInstances;
    public string ScenarioFileName;

    private SpawnCrowd _crowdSpawner;
    private CreateScenario _scenarioCreator;
    private int _repeatsCounter;

	void Start()
    {
        _crowdSpawner = GetComponent<SpawnCrowd>();
        _scenarioCreator = GetComponent<CreateScenario>();
        StartInstanceOfSimulation();
	}
	
    public void StartInstanceOfSimulation()
    {
        _crowdSpawner.GenerateCrowd();
        _scenarioCreator.GenerateScenario(SimultaneousScenarioInstances, ScenarioFileName);
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
