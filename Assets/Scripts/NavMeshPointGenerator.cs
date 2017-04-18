using UnityEngine;

public class NavMeshPointGenerator
{
    private float _range;
    GameObject[] _actionZones;
    System.Random _rng;
    public NavMeshPointGenerator(float range)
    {
        _range = range;
    }


    public NavMeshPointGenerator()
    {
        _actionZones = GameObject.FindGameObjectsWithTag("ActionZone");
        _rng = new System.Random();
        Bounds navMeshBounds = new Bounds();
        navMeshBounds.center = Vector3.zero;       
        
        Vector3[] navMeshVertices = UnityEngine.AI.NavMesh.CalculateTriangulation().vertices;

        Vector3 min = Vector3.zero;
        Vector3 max = Vector3.zero;


        Vector3 mean = Vector3.zero;
        foreach (var vertex in navMeshVertices)
        {
            min = Vector3.Min(min, vertex);
            max = Vector3.Max(max, vertex);

            mean += vertex;
        }

        mean = mean / navMeshVertices.Length;

        navMeshBounds.SetMinMax(min, max);
        navMeshBounds.Encapsulate(max);

        GameObject bb = new GameObject();
        bb.transform.position = mean;
        var col = bb.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.center = Vector3.zero;
        col.radius = navMeshBounds.extents.magnitude;//navMeshBounds.size;

        _range = navMeshBounds.extents.magnitude;
    }

    public Vector3 RandomPointOnNavMesh(Vector3 center)
    {
        do
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * _range;
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out hit, 0.1f, UnityEngine.AI.NavMesh.AllAreas))
            {
                return hit.position;
            }
        } while (true);
    }

    public Vector3 RandomPointWithinActionZones()
    {
        int randomIndex = _rng.Next(0,_actionZones.Length - 1);
        Bounds bounds = _actionZones[randomIndex].GetComponent<BoxCollider>().bounds;

        do
        {
            Vector3 randomPoint = bounds.center + Random.insideUnitSphere * bounds.extents.magnitude;
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out hit, 0.1f, UnityEngine.AI.NavMesh.AllAreas))
            {
                return hit.position;
            }

        } while (true);


    }


}
