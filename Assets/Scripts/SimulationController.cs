using UnityEngine;

[RequireComponent(typeof(SpawnCrowd))]
[RequireComponent(typeof(CreateScenario))]
public class SimulationController : MonoBehaviour
{
    public int ScenarioRepeats;
    public int SimultaneousScenarioInstances;

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
        for (int i = 0; i < SimultaneousScenarioInstances; i++)
        {
            _scenarioCreator.GenerateScenario();
        }
        _repeatsCounter++;
    }

    public void EndInstanceOfSimulation()
    {
        _crowdSpawner.RemoveCrowd();

        if (_repeatsCounter < ScenarioRepeats)
        {
            StartInstanceOfSimulation();
        }
    }
}
