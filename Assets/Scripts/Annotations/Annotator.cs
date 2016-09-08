using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Annotator
{
    private List<GameObject> _agents;
    private Vector4[] _pts = new Vector4[8];
    private int _resWidth = 1600;
    private int _resHeigth = 1200;

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

    public void SetResolution(int width, int height)
    {
        _resWidth = width;
        _resHeigth = height;
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
                annotations.Add(new Annotation(agent.gameObject.GetComponent<Movement>().Speed > 0 ? "Moving" : agent.gameObject.GetComponent<Activity>().ParamName, rekt, a.AgentId,1.0f, agent.transform.position, agent.tag == "Crowd"));
            }           
        }
        return annotations;
    }
    
    //REKT
    private Rect GetRect(Bounds bounds, Camera camera)
    {
        _pts[0] = new Vector4(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z, 1.0f);
        _pts[1] = new Vector4(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z, 1.0f);
        _pts[2] = new Vector4(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z, 1.0f);
        _pts[3] = new Vector4(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z, 1.0f);
        _pts[4] = new Vector4(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z, 1.0f);
        _pts[5] = new Vector4(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z, 1.0f);
        _pts[6] = new Vector4(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z, 1.0f);
        _pts[7] = new Vector4(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z, 1.0f);
       
        Matrix4x4 scaledPerspective = Matrix4x4.Perspective(camera.fieldOfView, (float)_resWidth/(float)_resHeigth, camera.nearClipPlane, camera.farClipPlane);
        Matrix4x4 VP = scaledPerspective * camera.worldToCameraMatrix;

        _pts[0] = VP * _pts[0];
        _pts[1] = VP * _pts[1];
        _pts[2] = VP * _pts[2];
        _pts[3] = VP * _pts[3];
        _pts[4] = VP * _pts[4];
        _pts[5] = VP * _pts[5];
        _pts[6] = VP * _pts[6];
        _pts[7] = VP * _pts[7];

        for (int i = 0; i < _pts.Length; i++)
        {
            _pts[i] /= _pts[i].w;
            _pts[i].x = (_pts[i].x + 1.0f) * (_resWidth / 2.0f);
            _pts[i].y = (_pts[i].y + 1.0f) * (_resHeigth / 2.0f);
           
        }

        for (int i = 0; i < _pts.Length; i++)
        {
            _pts[i].y = _resHeigth - _pts[i].y;
        }

        Vector3 min = _pts[0];
        Vector3 max = _pts[0];
        for (int i = 1; i < _pts.Length; i++)
        {
            min = Vector3.Min(min, _pts[i]);
            max = Vector3.Max(max, _pts[i]);
        }
        
        Rect r = Rect.MinMaxRect((int)min.x, (int)min.y, (int)max.x, (int)max.y);   
        return r;
    }

    private Rect  RectGetFixedSizeRect(Bounds bounds, Camera camera)
    {

        return new Rect();
    }

}
