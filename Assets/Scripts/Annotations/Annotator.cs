using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Annotator
{
    private List<GameObject> _agents;
    
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

    private bool IsRectValid(Rect rect)
    {
        return rect.x >= 0.0f && 
            rect.y >= 0.0f && 
            rect.x + rect.width <= _resWidth &&
            rect.y + rect.height <= _resHeigth;
    }

    private void AdjustTrackingRect(ref Rect rect)
    {
        if (!IsRectValid(rect))
        {
            //rect.x = rect.x < 0.0f ? 0.0f : rect.x;
            // rect.y = rect.y < 0.0f ? 0.0f : rect.y;

            if (rect.x < 0.0f)
            {
                rect.width += rect.x;
                rect.x = 0.0f;                 
            }

            if (rect.y < 0.0f)
            {
                rect.height += rect.y;
                rect.y = 0.0f;
            }
          
            rect.width = rect.x + rect.width > _resWidth ? _resWidth - rect.x : rect.width;
            rect.height = rect.y + rect.height > _resHeigth ? _resHeigth - rect.y : rect.height;
        }
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
                bool agentIsCrowd = agent.tag == "Crowd";
                string agentAction = agent.gameObject.GetComponent<Movement>().IsFinished ? agent.gameObject.GetComponent<Activity>().ParamName : "Moving";
                Bounds bounds = agent.GetComponentsInChildren<Renderer>().Aggregate((i1, i2) => i1.bounds.extents.magnitude > i2.bounds.extents.magnitude ? i1 : i2).bounds;
                Rect trackingRekt = new Rect();
                Rect actionRecognitionRekt = new Rect();

                trackingRekt = GetRect(bounds, camera);
                //AdjustTrackingRect(ref trackingRekt);

                if (!agentIsCrowd)
                {
                    actionRecognitionRekt = GetFixedSizeRect(bounds, camera);
                }
                
                if (IsRectValid(trackingRekt))
                {                                                         
                    Agent a = agent.GetComponent<Agent>();
                    annotations.Add(new Annotation(agentAction, trackingRekt, actionRecognitionRekt, a.AgentId, 1.0f, agent.transform.position, agentIsCrowd, IsRectValid(actionRecognitionRekt)));
                }
            }           
        }
        return annotations;
    }
    
    //REKT
    private Rect GetRect(Bounds bounds, Camera camera)
    {
        Vector4[] pts = new Vector4[8];
        
        pts[0] = new Vector4(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z, 1.0f);
        pts[1] = new Vector4(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z, 1.0f);
        pts[2] = new Vector4(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z, 1.0f);
        pts[3] = new Vector4(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z, 1.0f);
        pts[4] = new Vector4(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z, 1.0f);
        pts[5] = new Vector4(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z, 1.0f);
        pts[6] = new Vector4(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z, 1.0f);
        pts[7] = new Vector4(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z, 1.0f);
       
        Matrix4x4 scaledPerspective = Matrix4x4.Perspective(camera.fieldOfView, (float)_resWidth/(float)_resHeigth, camera.nearClipPlane, camera.farClipPlane);
        Matrix4x4 VP = scaledPerspective * camera.worldToCameraMatrix;

        for (int i = 0; i < pts.Length; i++)
        {
            pts[i] = VP * pts[i];
            pts[i] /= pts[i].w;
            pts[i].x = (pts[i].x + 1.0f) * (_resWidth / 2.0f);
            pts[i].y = (pts[i].y + 1.0f) * (_resHeigth / 2.0f);
            pts[i].y = _resHeigth - pts[i].y;

        }

        Vector3 min = pts[0];
        Vector3 max = pts[0];
        for (int i = 1; i < pts.Length; i++)
        {
            min = Vector3.Min(min, pts[i]);
            max = Vector3.Max(max, pts[i]);
        }
        
        Rect r = Rect.MinMaxRect((int)min.x, (int)min.y, (int)max.x, (int)max.y);   
        return r;
    }

    private Rect GetFixedSizeRect(Bounds bounds, Camera camera)
    {
        Rect rect = GetRect(bounds, camera);
        int roundUpTarget = Mathf.Max((int)(_resHeigth * 0.2f), (int)(_resWidth * 0.2f));

        int roundedUpWidth = Mathf.RoundToInt(rect.width / roundUpTarget) * roundUpTarget;
        roundedUpWidth = roundedUpWidth < roundUpTarget ? roundUpTarget : roundedUpWidth;

        int roundedUpHeight = Mathf.RoundToInt(rect.height / roundUpTarget) * roundUpTarget;
        roundedUpHeight = roundedUpHeight < roundUpTarget ? roundUpTarget : roundedUpHeight;

        int extents = Mathf.Max(roundedUpWidth, roundedUpHeight) / 2;
        Rect finalRect = Rect.MinMaxRect(rect.center.x - extents, rect.center.y - extents, rect.center.x + extents, rect.center.y + extents);

        return finalRect;
    }


}
