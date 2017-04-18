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
        int threshold = agent.tag == "Crowd" ? 3 : 1;
        
        
        //float maxDistance = 80.0f;
        // RaycastHit hit;
        //Vector3 direction = camera.transform.position - (agent.transform.position + agent.transform.up);
        //if (Physics.Raycast(agent.transform.position + agent.transform.up, direction, out hit))
        //{
        //    visible = hit.collider.name == camera.name;
        //}
        //if (Vector3.Distance(agent.transform.position, camera.transform.position) < maxDistance)
        //{

        Bounds bounds = agent.GetComponentsInChildren<Renderer>().Aggregate((i1, i2) => i1.bounds.extents.magnitude > i2.bounds.extents.magnitude ? i1 : i2).bounds;
        Plane[] frustum = GeometryUtility.CalculateFrustumPlanes(camera);

        inFrustum = GeometryUtility.TestPlanesAABB(frustum, bounds);

        if (inFrustum)
        {
            int visisbilityCounter = 0;
            RaycastHit hit;
            visible = false;
            Vector3[] pts = GetBoundsPoints(bounds);
            foreach (var p in pts)
            {
                Vector3 direction = camera.transform.position - p;
                if (Physics.Raycast(p, direction, out hit))
                {
                    visible = hit.collider.name == camera.name;//|| visible;
                    if (visible)
                    {
                        visisbilityCounter++;
                    }
                }
            }

            visible = visisbilityCounter >= threshold;
        }
    
        return visible && inFrustum;
    }

    private void SetAgentVisisble(GameObject agent, bool v)
    {
        MaterialChanger mc = agent.GetComponent<MaterialChanger>();
        if (mc != null)
        {
            mc.SetVisible(v);
        }
    }

    private bool IsRectValid(Rect rect)
    {
        return rect.x >= 0.0f && 
            rect.y >= 0.0f && 
            rect.x + rect.width <= _resWidth &&
            rect.y + rect.height <= _resHeigth;
    }

    private bool AdjustTrackingRect(ref Rect rect)
    {
        bool success = IsRectValid(rect);       
        if (!success)
        {
            float previousWidth = rect.width;
            float previousHeight = rect.height;

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
          
            rect.width = rect.x + rect.width >= _resWidth ? _resWidth - (rect.x + 1) : rect.width;
            rect.height = rect.y + rect.height >= _resHeigth ? _resHeigth - (rect.y + 1) : rect.height;

            success = rect.width >= previousWidth / 2.0f && rect.height >= previousHeight / 2.0f;
        }

        return success;
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
                string agentAction = agent.gameObject.GetComponent<Movement>().IsFinished ? agent.gameObject.GetComponent<Activity>().NameToDisplay : agent.gameObject.GetComponent<Movement>().NameToDisplay;
                string agentName = agent.gameObject.GetComponent<Movement>().IsFinished ? agent.gameObject.GetComponent<Activity>().ActorName : agent.gameObject.GetComponent<Movement>().ActorName;//agent.gameObject.GetComponent<Activity>().ActorName;
                string mocapName = agent.gameObject.GetComponent<Movement>().IsFinished ? agent.gameObject.GetComponent<Activity>().MocapId : agent.gameObject.GetComponent<Movement>().MocapId;//agent.gameObject.GetComponent<Activity>().MocapId;
                int levelIndex = agent.gameObject.GetComponent<Movement>().IsFinished ? agent.gameObject.GetComponent<Activity>().LevelIndex : agent.gameObject.GetComponent<Movement>().LevelIndex;//agent.gameObject.GetComponent<Activity>().LevelIndex;
                bool isComplex = agent.gameObject.GetComponent<Movement>().IsFinished ? agent.gameObject.GetComponent<Activity>().IsComplex : false;
                Rect trackingRekt = new Rect();
                Rect actionRecognitionRekt = new Rect();

                trackingRekt = GetRect(agent, camera);
                //AdjustTrackingRect(ref trackingRekt);

                if (!agentIsCrowd)
                {
                    actionRecognitionRekt = GetFixedSizeRect(agent, camera);
                }

                bool rectValid = IsRectValid(trackingRekt);
                if (!rectValid)
                {
                    rectValid = AdjustTrackingRect(ref trackingRekt);
                }

                if (rectValid)
                {                                                         
                    Agent a = agent.GetComponent<Agent>();
                    annotations.Add(new Annotation(agentAction, 
                                                    trackingRekt, 
                                                    actionRecognitionRekt, 
                                                    a.AgentId, 
                                                    1.0f, 
                                                    agent.transform.position, 
                                                    agentIsCrowd, 
                                                    IsRectValid(actionRecognitionRekt),
                                                    levelIndex,
                                                    agentName,
                                                    mocapName,
                                                    isComplex));

                    
                }
                SetAgentVisisble(agent, IsVisibleFromCamera(camera, agent) && rectValid);
            }
            else
            {
                SetAgentVisisble(agent, false);
            }
            
        }
       
        return annotations;
    }

    private Vector3[] GetBoundsPoints(Bounds bounds)
    {
        Vector3[] pts = new Vector3[9];

        pts[0] = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z);// lewy góra przód
        pts[1] = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z);// lewy góra tył
        pts[2] = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z);// lewy dół przód
        pts[3] = new Vector3(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z);// lewy dół tył
        pts[4] = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z);// prawy góra przód
        pts[5] = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z);// prawy góra tył
        pts[6] = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z);// prawy dół przód
        pts[7] = new Vector3(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z);// prawy dół tył
        pts[8] = bounds.center; //środek

        return pts;
    }

    //REKT
    private Rect GetRect(GameObject agent, Camera camera)
    {
        SkinnedMeshRenderer biggest = agent.GetComponentsInChildren<SkinnedMeshRenderer>().Aggregate((i1, i2) => i1.bounds.extents.magnitude > i2.bounds.extents.magnitude ? i1 : i2);
        biggest.sharedMesh.RecalculateBounds();
        biggest.updateWhenOffscreen = true;
        Bounds bounds = biggest.bounds;

        Vector4[] pts = new Vector4[8];

        pts[0] = new Vector4(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z, 1.0f);// lewy góra przód
        pts[1] = new Vector4(bounds.center.x + bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z, 1.0f);// lewy góra tył
        pts[2] = new Vector4(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z, 1.0f);// lewy dół przód
        pts[3] = new Vector4(bounds.center.x + bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z, 1.0f);// lewy dół tył
        pts[4] = new Vector4(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z + bounds.extents.z, 1.0f);// prawy góra przód
        pts[5] = new Vector4(bounds.center.x - bounds.extents.x, bounds.center.y + bounds.extents.y, bounds.center.z - bounds.extents.z, 1.0f);// prawy góra tył
        pts[6] = new Vector4(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z + bounds.extents.z, 1.0f);// prawy dół przód
        pts[7] = new Vector4(bounds.center.x - bounds.extents.x, bounds.center.y - bounds.extents.y, bounds.center.z - bounds.extents.z, 1.0f);// prawy dół tył
       
        Matrix4x4 scaledPerspective = Matrix4x4.Perspective(camera.fieldOfView, (float)_resWidth/(float)_resHeigth, camera.nearClipPlane, camera.farClipPlane);      
        Matrix4x4 PV = scaledPerspective * camera.worldToCameraMatrix;


        //for (int i = 0; i < pts.Length; i++)
        //{
        //    Vector3 temp = pts[i];
        //    pts[i] = Matrix4x4.TRS(temp - bounds.center , Quaternion.identity, Vector3.zero) * pts[i];
        //    //pts[i] = Matrix4x4.TRS(pts[i], agent.transform.rotation, Vector3.zero) * pts[i];
        //    pts[i] = agent.transform.rotation * pts[i];
        //    pts[i] += (Vector4)bounds.center;
        //    //pts[i] = Matrix4x4.TRS((Vector3)pts[i] + bounds.center, Quaternion.identity, Vector3.zero) * pts[i];
        //}

        float time = 0.01f;
        Debug.DrawLine(pts[0], pts[4], Color.red, time);
        Debug.DrawLine(pts[2], pts[6], Color.red, time);
        Debug.DrawLine(pts[1], pts[5], Color.red, time);
        Debug.DrawLine(pts[3], pts[7], Color.red, time);
        Debug.DrawLine(pts[0], pts[1], Color.red, time);
        Debug.DrawLine(pts[4], pts[5], Color.red, time);
        Debug.DrawLine(pts[2], pts[3], Color.red, time);
        Debug.DrawLine(pts[6], pts[7], Color.red, time);
        Debug.DrawLine(pts[0], pts[2], Color.red, time);
        Debug.DrawLine(pts[4], pts[6], Color.red, time);
        Debug.DrawLine(pts[1], pts[3], Color.red, time);
        Debug.DrawLine(pts[5], pts[7], Color.red, time);

        for (int i = 0; i < pts.Length; i++)
        {
            pts[i] = PV  * pts[i];
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

    private Rect GetFixedSizeRect(GameObject agent, Camera camera)//(Bounds bounds, Camera camera)
    {
        Rect rect = GetRect(agent, camera);//(bounds, camera);
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
