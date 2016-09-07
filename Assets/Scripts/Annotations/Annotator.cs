using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Annotator
{
    private List<GameObject> _agents;
    private Vector3[] _pts = new Vector3[8];

    public Annotator(List<GameObject> agents)
    {
        _agents = agents;
    }

    private bool IsVisibleFromCamera(Camera camera, GameObject agent)
    {
        bool visible = false;
        bool inFrustum = false;
        RaycastHit hit;

        Vector3 direction = camera.transform.position - (agent.transform.position + agent.transform.up);
        if (Physics.Raycast(agent.transform.position + agent.transform.up, direction, out hit))
        {
            visible = hit.collider.name == camera.name;
        }

        Bounds bounds = agent.GetComponentsInChildren<Renderer>().Aggregate((i1, i2) => i1.bounds.extents.magnitude > i2.bounds.extents.magnitude ? i1 : i2).bounds;

        Plane[] frustum = GeometryUtility.CalculateFrustumPlanes(camera);
        inFrustum = GeometryUtility.TestPlanesAABB(frustum,bounds);

        return visible && inFrustum;
    }

    public List<Annotation> MarkAgents(Camera camera)
    {
        List<Annotation> annotations = new List<Annotation>();

        foreach (var agent in _agents)
        {
            if (IsVisibleFromCamera(camera, agent))
            {
                Bounds bounds = agent.GetComponentsInChildren<Renderer>().Aggregate((i1, i2) => i1.bounds.extents.magnitude > i2.bounds.extents.magnitude ? i1 : i2).bounds;
                Rect rekt = GetRect(bounds, camera);
                Agent a = agent.GetComponent<Agent>();            
                annotations.Add(new Annotation(agent.gameObject.GetComponent<Activity>().ParamName, rekt, a.AgentId,1.0f, agent.transform.position));
            }           
        }
        return annotations;
    }
    
    //REKT
    private Rect GetRect(Bounds bounds, Camera camera)
    {
        //if (camera.WorldToScreenPoint(bounds.center).z < 0) return;

        _pts[0] = camera.WorldToScreenPoint(new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z));
        _pts[1] = camera.WorldToScreenPoint(new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z));
        _pts[2] = camera.WorldToScreenPoint(new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z));
        _pts[3] = camera.WorldToScreenPoint(new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z));
        _pts[4] = camera.WorldToScreenPoint(new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z));
        _pts[5] = camera.WorldToScreenPoint(new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z));
        _pts[6] = camera.WorldToScreenPoint(new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z));
        _pts[7] = camera.WorldToScreenPoint(new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z));

        float widthMultiplier = (float)1920 / (float)camera.pixelWidth;
        float heightMultiplier = (float)1080 / (float)camera.pixelHeight;


        //for (int i = 0; i < _pts.Length; i++)
        //{
        //    _pts[i].y = _pts[i].y * heightMultiplier;
        //    _pts[i].x = _pts[i].x * widthMultiplier;
        //}


        for (int i = 0; i < _pts.Length; i++)
        {
            _pts[i].y = Screen.height - _pts[i].y;
        }

        Vector3 min = _pts[0];
        Vector3 max = _pts[0];
        for (int i = 1; i < _pts.Length; i++)
        {
            min = Vector3.Min(min, _pts[i]);
            max = Vector3.Max(max, _pts[i]);
        }

        //Matrix4x4 scaling = Matrix4x4.Scale(new Vector3(widthMultiplier, heightMultiplier, 1));
        //min = scaling * min;
        //max = scaling * max;

        Rect r = Rect.MinMaxRect(min.x, min.y, max.x, max.y);   
        return r;
    }

}
