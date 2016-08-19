using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AnnotationCreator
{
    private List<GameObject> _agents;
    private Vector3[] _pts = new Vector3[8];

    public AnnotationCreator(List<GameObject> agents)
    {
        _agents = agents;
    }

    private bool IsVisibleFromCamera(Camera camera, GameObject agent)
    {
        bool visible = false;
        RaycastHit hit;

        Vector3 direction = camera.transform.position - (agent.transform.position + agent.transform.up);
        if (Physics.Raycast(agent.transform.position + agent.transform.up, direction, out hit))
        {
            visible = hit.collider.name == camera.name;
        }

        return visible;
    }

    public List<Annotation> GetAnnotations(Camera camera)
    {
        List<Annotation> annotations = new List<Annotation>();

        foreach (var agent in _agents)
        {
            if (IsVisibleFromCamera(camera, agent))
            {
                Bounds bounds = agent.GetComponentsInChildren<Renderer>().Aggregate((i1, i2) => i1.bounds.extents.magnitude > i2.bounds.extents.magnitude ? i1 : i2).bounds;
                Rect rekt = GetRect(bounds, camera);
                annotations.Add(new Annotation(agent.name, rekt));
            }           
        }
        return annotations;
    }
    
    //REKT
    private Rect GetRect(Bounds bounds, Camera camera)
    {
        //if (camera.WorldToScreenPoint(bounds.center).z < 0) return;

        //All 8 vertices of the bounds
        _pts[0] = camera.WorldToScreenPoint(new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z));
        _pts[1] = camera.WorldToScreenPoint(new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z));
        _pts[2] = camera.WorldToScreenPoint(new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z));
        _pts[3] = camera.WorldToScreenPoint(new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z));
        _pts[4] = camera.WorldToScreenPoint(new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z));
        _pts[5] = camera.WorldToScreenPoint(new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z));
        _pts[6] = camera.WorldToScreenPoint(new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z));
        _pts[7] = camera.WorldToScreenPoint(new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z));

        //Get them in GUI space
        for (int i = 0; i < _pts.Length; i++)
        {
            _pts[i].y = Screen.height - _pts[i].y;
        }
            
        //Calculate the min and max positions
        Vector3 min = _pts[0];
        Vector3 max = _pts[0];
        for (int i = 1; i < _pts.Length; i++)
        {
            min = Vector3.Min(min, _pts[i]);
            max = Vector3.Max(max, _pts[i]);
        }

        //Construct a rect of the min and max positions and apply some margin
        Rect r = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        return r;
    }

}
