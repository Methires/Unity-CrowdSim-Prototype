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
                GameObject agent = (GameObject)Instantiate(Characters[index], generator.RandomPointOnNavMesh(transform.position), Quaternion.identity);
                agent.tag = "Crowd";
                if (Random.Range(0.0f, 100.0f) < 90.0f)
                {
                    agent.GetComponent<NavMeshAgent>().speed = Random.Range(1.0f, 2.5f);
                }
                else
                {
                    agent.GetComponent<NavMeshAgent>().speed = 10.0f;
                }
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
                Destroy(_crowd[i].gameObject);
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
        Animator agentAnimator = obj.GetComponent<Animator>();

        //Animator agentAnimator = obj.GetComponent<Animator>();
        //string[] paths = AssetDatabase.FindAssets("Kawai_retardedAvatar.avatar");
        //string referenceAvatarPath = AssetDatabase.GUIDToAssetPath(paths[0]);
        //Avatar avatar = AssetDatabase.LoadAssetAtPath<Avatar>("Assets/Resources/Kawai_retardedAvatar.avatar");

        var c = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Animations/Locomotion.controller");
        agentAnimator.runtimeAnimatorController = c;
        //agentAnimator.avatar = avatar;
        obj.AddComponent<NavMeshAgent>();
        obj.AddComponent<Rigidbody>().isKinematic = true;
        obj.AddComponent<Agent>();
        obj.AddComponent<GenerateDestination>();
        return obj;
    }

    private void LoadAgents()
    {
        string[] agentPaths = Directory.GetFiles(Application.dataPath + "/Resources/Agents","*.prefab",SearchOption.AllDirectories).Select(path => Path.GetFileNameWithoutExtension(path)).ToArray();
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
}
