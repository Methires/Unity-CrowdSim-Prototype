using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class DisplayActivityText : MonoBehaviour
{
    private string _text;
    private Vector3[] pts = new Vector3[8];
    private Plane[] planes;
    private Camera cam;
    private Texture2D _borderTexture;

    void Awake()
    {
        _borderTexture = Resources.Load("redRect") as Texture2D;

        //Color[] colors =_borderTexture.GetPixels();
        //Color borderColor = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));

        //for (int i = 0; i < colors.Length; i++)
        //{
        //    colors[i] = colors[i].a == 1.0f ? colors[i] : borderColor;
        //}

        //Texture2D coloredTexture = new Texture2D(_borderTexture.width, _borderTexture.height);
        //coloredTexture.SetPixels(colors);
        //_borderTexture = coloredTexture;
    }

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
        bool visible = false;
        RaycastHit hit;

        Vector3 direction = cam.transform.position  - (transform.position + transform.up);
        if (Physics.Raycast(transform.position + transform.up, direction, out hit))
        {
            visible = hit.collider.tag == "MainCamera";
        }

        return visible;
    }

    private void RenderLabels(Bounds b)
    {              
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
        GUI.DrawTexture(r, _borderTexture);
        GUI.Label(new Rect(min.x, max.y, 200, 20), _text);
    }

    public void ChangeText(string text)
    {
        _text = text;
    }

}
