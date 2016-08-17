using UnityEngine;

public class FogController : MonoBehaviour
{
	void Update ()
    {
        Vector3 pos = Camera.main.transform.position;
        transform.position = pos;
    }
}
