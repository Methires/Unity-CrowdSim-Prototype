using UnityEngine;

public class DisplayActivityText : MonoBehaviour
{
    private GameObject _textMeshObject;
    private TextMesh _textMesh;

    void Awake()
    {
        _textMeshObject = new GameObject();
        _textMeshObject.name = "Movement or Action text";
        _textMeshObject.transform.parent = transform;
        _textMeshObject.transform.localPosition = Vector3.zero;
        _textMeshObject.transform.localScale = new Vector3(0.1f, 0.1f, 1.0f);
        _textMesh = _textMeshObject.AddComponent<TextMesh>();
        _textMesh.anchor = TextAnchor.UpperCenter;
        _textMesh.alignment = TextAlignment.Center;
        _textMesh.fontSize = 100;
        _textMesh.color = Color.black;
        _textMesh.text = "Pending...";
    }

    void Update()
    {
        Camera cameraToLookAt = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>(); ;
        Vector3 v = cameraToLookAt.transform.position - _textMeshObject.transform.position;
        v.x = v.z = 0.0f;
        _textMeshObject.transform.LookAt(cameraToLookAt.transform.position - v);
        _textMeshObject.transform.rotation = cameraToLookAt.transform.rotation;
    }

    public void ChangeText(string text)
    {
        _textMesh.text = text;
    }

}
