using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class CrowdController : MonoBehaviour
{
    public GameObject[] Characters;
    public int MaxPeople;
    public bool CreatePrefabs = false;
    public bool LoadAgentsFromResources = false;
    public string AgentsFilter = "";
    public string ActionsFilter = "";

    private float _range = 100.0f;
    private List<GameObject> _crowd;

    public List<GameObject> Crowd
    {
        get
        {
            return _crowd;
        }
    }

    void Awake()
    {
        if (CreatePrefabs)
        {
            CreateAgentPrefabs();
        }
    }

    public void GenerateCrowd()
    {
        if (LoadAgentsFromResources)
        {
            LoadAgents();
        }

        _crowd = new List<GameObject>();
        NavMeshPointGenerator generator = new NavMeshPointGenerator(_range);
        if (MaxPeople > 0 && Characters.Length > 0)
        {
            for (int i = 0; i < MaxPeople; i++)
            {
                int index = Random.Range(0, Characters.Length);
                Quaternion rotation = Quaternion.Euler(new Vector3(0.0f, Random.Range(0.0f, 360.0f), 0.0f));
                GameObject agent = (GameObject)Instantiate(Characters[index], generator.RandomPointOnNavMesh(transform.position), rotation);
                agent.tag = "Crowd";
                agent.name = string.Format("{0}{1}", Characters[index].name, i);
                if (Random.Range(0.0f, 100.0f) < 90.0f)
                {
                    agent.GetComponent<NavMeshAgent>().speed = Random.Range(1.0f, 2.5f);
                }
                else
                {
                    agent.GetComponent<NavMeshAgent>().speed = 10.0f;
                }
                agent.GetComponent<NavMeshAgent>().stoppingDistance = 1.0f;
                _crowd.Add(agent);
            }
        }
    }

    public void RemoveCrowd()
    {
        if (_crowd != null)
        {
            for (int i = 0; i < MaxPeople; i++)
            {
                DestroyImmediate(_crowd[i].gameObject);
            }
            _crowd.Clear();
        }
    }

    private void CreateAgentPrefabs()
    {
        string path = Application.dataPath + "/Characters";
        string[] files = Directory.GetFiles(path, "*.fbx", SearchOption.AllDirectories);

        for (int i = 0; i < files.Length; i++)
        {
            files[i] = "Assets" + (files[i].Remove(0, Application.dataPath.Length));
            CreateAgentPrefab(files[i]);
        }
        LoadAgents();
    }

    private void CreateAgentPrefab(string path)
    {
        string prefabPath = "/Resources/Agents/" + Path.GetFileNameWithoutExtension(path) + ".prefab";
        if (!PrefabExistsAtPath(Application.dataPath + prefabPath))
        {
            GameObject objToPrefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            objToPrefab = Instantiate(objToPrefab);
            objToPrefab = EquipAgentPrefab(objToPrefab);
            PrefabUtility.CreatePrefab("Assets" + prefabPath, objToPrefab);
            Destroy(objToPrefab);
        }
    }

    private bool PrefabExistsAtPath(string path)
    {
        return File.Exists(path);
    }

    private GameObject EquipAgentPrefab(GameObject obj)
    {
        ////Uncomment to find avatar when none is generated
        //Animator agentAnimator = obj.AddComponent<Animator>();
        //string[] paths = AssetDatabase.FindAssets("ReptiliuszAvatar");
        //string referenceAvatarPath = AssetDatabase.GUIDToAssetPath(paths[0]);
        //Avatar avatar = AssetDatabase.LoadAssetAtPath<Avatar>(referenceAvatarPath);//("Assets/Resources/Kawai_retardedAvatar.avatar");
        //agentAnimator.avatar = avatar;


        Animator agentAnimator = obj.GetComponent<Animator>();
        var c = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Locomotion.controller");
        agentAnimator.runtimeAnimatorController = c;
        obj.AddComponent<NavMeshAgent>();
        obj.AddComponent<Rigidbody>().isKinematic = true;
        obj.AddComponent<Agent>();
        //obj.AddComponent<GenerateDestination>();
        return obj;
    }

    private void LoadAgents()
    {
        string[] agentPaths = Directory.GetFiles(Application.dataPath + "/Resources/Agents", "*.prefab", SearchOption.AllDirectories).Select(path => Path.GetFileNameWithoutExtension(path)).ToArray();
        string[] agentsToLoad = AgentsFilter.Split('|');
        agentsToLoad = agentPaths.Where(x => agentsToLoad.Contains(x)).ToArray();
        if (agentsToLoad != null && agentsToLoad.Length != 0)
        {
            agentPaths = agentsToLoad;
        }

        List<GameObject> agents = new List<GameObject>();
        foreach (var path in agentPaths)
        {
            agents.Add(Resources.Load<GameObject>("Agents/" + path));
        }
        Characters = agents.ToArray();
    }

    public List<Level> PrepareActions(GameObject agent)
    {
        List<Level> agentScenario = new List<Level>();
        List<string> actionsNames = ActionsFilter.Split('|').ToList();
        actionsNames.RemoveAll(string.IsNullOrEmpty);
        //int[] indexes = Enumerable.Range(0, actionsNames.Count + 2).ToArray();
        //List<GameObject> crowd = _crowd.Where(x => x.tag == "Crowd").ToList();
        int actionIndex = 0;
        int scenarioLenght = 5;
        float movementProbability = 0.9f;
        float actionsProbability = 1.0f - movementProbability;
        for (int i = 0; i < scenarioLenght; i++)
        {
            Level level = new Level(i);
            List<Action> actions = new List<Action>();
            int[] prevIndexes = i > 0 ? Enumerable.Range((i - 1) * (actionsNames.Count + 2), actionsNames.Count + 2).ToArray() : new int[0];
            for (int j = 0; j < actionsNames.Count; j++)
            {
                string[] animation = actionsNames[j].Split('@');
                Action action = new Action(animation[animation.Length - 1], actionsProbability / actionsNames.Count, actionIndex);
                Actor actor = new Actor(agent.name, animation[0]);
                actor.PreviousActivitiesIndexes = prevIndexes;
                action.Actors = new List<Actor> { actor };
                actions.Add(action);
                actionIndex++;
            }
            Action walk = new Action("walk", movementProbability * 3 / 4, actionIndex);
            actionIndex++;
            Action run = new Action("run", movementProbability / 4, actionIndex);
            actionIndex++;
            Actor movementActor = new Actor(agent.name, prevIndexes);
            run.Actors = walk.Actors = new List<Actor> { movementActor };
            actions.Add(walk);
            actions.Add(run);

            level.Actions = actions;
            agentScenario.Add(level);
        }
        return agentScenario;
    }
}
