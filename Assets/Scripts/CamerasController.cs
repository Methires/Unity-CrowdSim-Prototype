using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CamerasController : MonoBehaviour
{
    public List<GameObject> _cameras;
    private int _cameraIndex = 0;

    void Awake()
    {
        _cameras = new List<GameObject>();
    }

    void Start()
    {
        Camera[] camComponents = FindObjectsOfType<Camera>();
        foreach (Camera cam in camComponents)
        {
            _cameras.Add(cam.gameObject);
            cam.gameObject.tag = "Untagged";
        }
        camComponents.FirstOrDefault().tag = "MainCamera";


        //_cameras.Remove(GameObject.FindGameObjectWithTag("MainCamera"));
        //_cameras.Insert(0, GameObject.FindGameObjectWithTag("MainCamera"));
        ActiveCamera();
    }

	void Update ()
    {
        if (Input.GetMouseButtonDown(0))
        {
            ActiveCamera();
        }
	}

    private void ActiveCamera()
    {
        foreach (GameObject camera in _cameras)
        {
            camera.tag = "Untagged";
            camera.GetComponent<Camera>().enabled = false;
        }
        _cameras[_cameraIndex].GetComponent<Camera>().enabled = true;
        _cameras[_cameraIndex].tag = "MainCamera";
        _cameraIndex++;
        _cameraIndex = _cameraIndex < _cameras.Count ? _cameraIndex : 0;
    }
}
