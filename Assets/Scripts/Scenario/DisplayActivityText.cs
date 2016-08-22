using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class DisplayActivityText : MonoBehaviour
{
    private string _text;
    private Vector3[] _pts = new Vector3[8];
    private Camera _cam;
    private Texture2D _borderTexture;

    void Awake()
    {
        _borderTexture = Resources.Load("redRect") as Texture2D;
    }

    public void OnGUI()
    {
        _cam = Camera.main;
        Bounds b = gameObject.GetComponentsInChildren<Renderer>().Aggregate((i1, i2) => i1.bounds.extents.magnitude > i2.bounds.extents.magnitude ? i1 : i2).bounds;

        RenderLabels(b);       
    }

    private bool IsVisibleFromCamera()
    {
        bool visible = false;
        RaycastHit hit;

        Vector3 direction = _cam.transform.position  - (transform.position + transform.up);
        if (Physics.Raycast(transform.position + transform.up, direction, out hit))
        {
            visible = hit.collider.tag == "MainCamera";
        }

        return visible;
    }

    private void RenderLabels(Bounds b)
    {              
        if (_cam.WorldToScreenPoint(b.center).z < 0) return;

        //All 8 vertices of the bounds
        _pts[0] = _cam.WorldToScreenPoint(new Vector3(b.center.x + b.extents.x, b.center.y + b.extents.y, b.center.z + b.extents.z));
        _pts[1] = _cam.WorldToScreenPoint(new Vector3(b.center.x + b.extents.x, b.center.y + b.extents.y, b.center.z - b.extents.z));
        _pts[2] = _cam.WorldToScreenPoint(new Vector3(b.center.x + b.extents.x, b.center.y - b.extents.y, b.center.z + b.extents.z));
        _pts[3] = _cam.WorldToScreenPoint(new Vector3(b.center.x + b.extents.x, b.center.y - b.extents.y, b.center.z - b.extents.z));
        _pts[4] = _cam.WorldToScreenPoint(new Vector3(b.center.x - b.extents.x, b.center.y + b.extents.y, b.center.z + b.extents.z));
        _pts[5] = _cam.WorldToScreenPoint(new Vector3(b.center.x - b.extents.x, b.center.y + b.extents.y, b.center.z - b.extents.z));
        _pts[6] = _cam.WorldToScreenPoint(new Vector3(b.center.x - b.extents.x, b.center.y - b.extents.y, b.center.z + b.extents.z));
        _pts[7] = _cam.WorldToScreenPoint(new Vector3(b.center.x - b.extents.x, b.center.y - b.extents.y, b.center.z - b.extents.z));

        //Get them in GUI space
        for (int i = 0; i < _pts.Length; i++) _pts[i].y = Screen.height - _pts[i].y;

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
        if (IsVisibleFromCamera())
        {
            GUI.DrawTexture(r, _borderTexture);
            GUI.Label(new Rect(min.x, max.y, 200, 20), _text);
        }
    }

    public void ChangeText(string text)
    {
        _text = text;
    }

}
