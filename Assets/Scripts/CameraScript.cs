using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour
{
    public float CameraZoomSpeed;
    public float CameraRotationSpeed;

	void Update ()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            GetComponent<Camera>().fieldOfView = Mathf.Clamp(GetComponent<Camera>().fieldOfView - Time.deltaTime * CameraZoomSpeed, 1.0f, 60.0f);
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            GetComponent<Camera>().fieldOfView = Mathf.Clamp(GetComponent<Camera>().fieldOfView + Time.deltaTime * CameraZoomSpeed, 1.0f, 60.0f);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            transform.localEulerAngles = CreateVectorWithEulerAngles('y', -1, 0.0f, 180.0f);
        }

        if (Input.GetKey(KeyCode.RightArrow))
        { 
            transform.localEulerAngles = CreateVectorWithEulerAngles('y', 1, 0.0f, 180.0f);
        }
    }

    private Vector3 CreateVectorWithEulerAngles(char axis, int sign, float bottomRange, float upperRange)
    {
        Vector3 rotation = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, transform.localEulerAngles.z);
        if (axis == 'x' || axis == 'X')
        {
            rotation.x = Mathf.Clamp(transform.localEulerAngles.x + Time.deltaTime * CameraRotationSpeed * sign, bottomRange, upperRange);
        }
        else if (axis == 'y' || axis == 'Y')
        {
            rotation.y = Mathf.Clamp(transform.localEulerAngles.y + Time.deltaTime * CameraRotationSpeed * sign, bottomRange, upperRange);
        }
        else if (axis == 'z' || axis == 'Z')
        {
            rotation.z = Mathf.Clamp(transform.localEulerAngles.z + Time.deltaTime * CameraRotationSpeed * sign, bottomRange, upperRange);
        }
        return rotation;
    }
}
