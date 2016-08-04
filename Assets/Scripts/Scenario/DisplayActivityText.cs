using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class DisplayActivityText : MonoBehaviour
{
    private string _text;
    private Vector3[] pts = new Vector3[8];
    private Plane[] planes;
    private Camera cam;


    public void OnGUI()
    {
        cam = Camera.main;
        Bounds b = gameObject.GetComponentsInChildren<Renderer>().Aggregate((i1, i2) => i1.bounds.extents.magnitude > i2.bounds.extents.magnitude ? i1 : i2).bounds;

        if (IsVisibleFromCamera())
        {
            RenderLabels(b);
        }
    }

    private bool IsVisibleFromCamera()
    {
        List<bool> visibility = new List<bool>();
        RaycastHit hit;

        foreach (Transform part in transform)
        {
            Vector3 direction = cam.transform.position - part.position;
            if (Physics.Raycast(part.position,direction,out hit))
            {
                visibility.Add(hit.collider.tag == "MainCamera");
            }
        }

        return !visibility.All(x => x == false);
    }

    private void RenderLabels(Bounds b)
    {              
        Texture texture = Resources.Load("redRect") as Texture;
        //The object is behind us
        if (cam.WorldToScreenPoint(b.center).z < 0) return;

        //All 8 vertices of the bounds
        pts[0] = cam.WorldToScreenPoint(new Vector3(b.center.x + b.extents.x, b.center.y + b.extents.y, b.center.z + b.extents.z));
        pts[1] = cam.WorldToScreenPoint(new Vector3(b.center.x + b.extents.x, b.center.y + b.extents.y, b.center.z - b.extents.z));
        pts[2] = cam.WorldToScreenPoint(new Vector3(b.center.x + b.extents.x, b.center.y - b.extents.y, b.center.z + b.extents.z));
        pts[3] = cam.WorldToScreenPoint(new Vector3(b.center.x + b.extents.x, b.center.y - b.extents.y, b.center.z - b.extents.z));
        pts[4] = cam.WorldToScreenPoint(new Vector3(b.center.x - b.extents.x, b.center.y + b.extents.y, b.center.z + b.extents.z));
        pts[5] = cam.WorldToScreenPoint(new Vector3(b.center.x - b.extents.x, b.center.y + b.extents.y, b.center.z - b.extents.z));
        pts[6] = cam.WorldToScreenPoint(new Vector3(b.center.x - b.extents.x, b.center.y - b.extents.y, b.center.z + b.extents.z));
        pts[7] = cam.WorldToScreenPoint(new Vector3(b.center.x - b.extents.x, b.center.y - b.extents.y, b.center.z - b.extents.z));

        //Get them in GUI space
        for (int i = 0; i < pts.Length; i++) pts[i].y = Screen.height - pts[i].y;

        //Calculate the min and max positions
        Vector3 min = pts[0];
        Vector3 max = pts[0];
        for (int i = 1; i < pts.Length; i++)
        {
            min = Vector3.Min(min, pts[i]);
            max = Vector3.Max(max, pts[i]);
        }

        //Construct a rect of the min and max positions and apply some margin
        Rect r = Rect.MinMaxRect(min.x, min.y, max.x, max.y);

        //GUI.Box(r, GUIContent.none);
        GUI.DrawTexture(r, texture);
        GUI.Label(new Rect(min.x, max.y, 200, 20), _text);
    }

    public void ChangeText(string text)
    {
        //_textMesh.text = text;
        _text = text;
    }

}
