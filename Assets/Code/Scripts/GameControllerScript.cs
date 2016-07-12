using UnityEngine;

using System.Collections.Generic;

public class GameControllerScript : MonoBehaviour
{
    public GameObject[] People;
    public int MaxPeople;
    public float[] RangeX;
    public float[] RangeZ;
    public float[] TimeLimits;

    private List<GameObject> _cameraList;
    private int _cameraIndex;

    void Start ()
    {
        _cameraList = new List<GameObject>(GameObject.FindGameObjectsWithTag("CameraPost"));
        foreach (GameObject camera in _cameraList)
        {
            camera.GetComponentInChildren<Camera>().enabled = false;
            camera.GetComponentInChildren<CameraScript>().enabled = false;
        }
        _cameraList.Add(GameObject.FindGameObjectWithTag("MainCamera"));
        if (MaxPeople > 0 && RangeX.Length == 2 && RangeZ.Length == 2 && TimeLimits.Length == 2 && People.Length > 0)
        {
            for (int i = 0; i < MaxPeople; i++)
            {
                int index = Random.Range(0.0f, 10.0f) < 5.0f ? 0 : 1;
                Vector3 position = new Vector3
                {
                    x = Random.Range(RangeX[0], RangeX[1]),
                    y = 0.0f,
                    z = Random.Range(RangeZ[0], RangeZ[1])
                };
                GameObject person = (GameObject)Instantiate(People[index], position, Quaternion.identity);
                person.GetComponent<SimpleWalkScript>().RangeX_Min = RangeX[0];
                person.GetComponent<SimpleWalkScript>().RangeX_Max = RangeX[1];
                person.GetComponent<SimpleWalkScript>().RangeZ_Min = RangeZ[0];
                person.GetComponent<SimpleWalkScript>().RangeZ_Max = RangeZ[1];
                person.GetComponent<SimpleWalkScript>().MaxTimeToDestination = Random.Range(TimeLimits[0], TimeLimits[1]);
                person.GetComponent<NavMeshAgent>().avoidancePriority = (int)Random.Range(0, 99);
            }
        }
        else
        {
            Debug.Log("WRONG STARTING PARAMETERS\nCHECK GAMECONTROLLER");
        }
	}

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            foreach (GameObject camera in _cameraList)
            {
                camera.GetComponentInChildren<Camera>().enabled = false;
                camera.GetComponentInChildren<CameraScript>().enabled = false;
            }
            _cameraList[_cameraIndex].GetComponentInChildren<Camera>().enabled = true;
            _cameraList[_cameraIndex].GetComponentInChildren<CameraScript>().enabled = true;
            _cameraIndex++;
            _cameraIndex = _cameraIndex < _cameraList.Count ? _cameraIndex : 0;
        }
    }
}
