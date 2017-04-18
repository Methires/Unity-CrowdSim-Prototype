using UnityEngine;

public class MaterialChanger : MonoBehaviour
{
    public Material VisibleMat;
    public Material InvisibleMat;
    public bool visible = false;

    SkinnedMeshRenderer[] _renderers;
	// Use this for initialization
	void Start ()
    {
        _renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        Material toSet;
        if (visible)
        {
            toSet = VisibleMat;
        }
        else
        {
            toSet = InvisibleMat;
        }

        foreach (var r in _renderers)
        {
            r.material = toSet;
        }
    }

    public void SetVisible(bool v)
    {
        visible = v;
    }

}
