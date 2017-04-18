using UnityEngine;
using System.Collections;

public class Lighting : MonoBehaviour
{
    public Material mat;
    private Light[] lights;
    private Light mainLight;

    // Use this for initialization
    public void SetSampleSceneLighting()
    {
        float valX = 0;
        float valY = 0;
        WeatherConditions wc = GetComponent<WeatherConditions>();
        lights = FindObjectsOfType<Light>();
        mainLight = GetComponent<WeatherConditions>().MainLight.GetComponent<Light>();

        switch (wc.Time)
        {
            case 1:
                mat.SetColor("_EmissionColor", Color.black);
                SetActiveLights(false);

                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Sample3_Estate")
                {
                    mainLight.intensity = 0.5f;
                }
                else if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Sample2_Square")
                {
                    mainLight.intensity = 0.7f;
                    valX = 35;
                    valY = -30.0f;
                    mainLight.transform.rotation = Quaternion.Euler(valX, valY, mainLight.transform.rotation.z);
                }

                break;
            case 2:
                mat.SetColor("_EmissionColor", Color.black);
                SetActiveLights(false);

                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Sample3_Estate")
                {

                }
                else if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Sample2_Square")
                {
                    mainLight.intensity = 0.7f;
                    valX = 72f;
                    valY = -45.0f;
                    mainLight.transform.rotation = Quaternion.Euler(valX, valY, mainLight.transform.rotation.z);
                }
                break;

            case 3:

                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Sample3_Estate")
                {
                    //  mainLight.intensity = 0f;
                    mainLight.gameObject.SetActive(false);
                    mat.SetColor("_EmissionColor", Color.white);
                    SetActiveLights(true);
                    valX = mainLight.transform.rotation.x;
                    valY = mainLight.transform.rotation.y;
                }
                else if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Sample2_Square")
                {
                    mainLight.intensity = 0f;
                    mainLight.color = new Color(231, 229, 219);
                    valX = -4;
                    valY = -45.0f;
                    SetActiveLights(true);
                }
                else if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Sample1_Crossroad")
                {
                    mainLight.intensity = 0f;
                    mat.SetColor("_EmissionColor", Color.white);
                    SetActiveLights(true);
                }
                mainLight.transform.rotation = Quaternion.Euler(valX, valY, mainLight.transform.rotation.z);
                break;
            default:
                break;
        }

        var cameras = FindObjectsOfType<Camera>();
        foreach (Camera camera in cameras)
        {
            switch (wc.Conditions)
            {
                case 5:
                    var fog = camera.GetComponent<UnityStandardAssets.ImageEffects.GlobalFog>();
                    fog.heightDensity = 10;
                    fog.height = 2;
                    fog.startDistance = 4;
                    break;
                default:
                    break;
            }
        }
    }

    void SetActiveLights(bool truefalse)
    {
        foreach (Light light in lights)
        {
            if (light.type != LightType.Directional)
            {
                light.gameObject.SetActive(truefalse);
                light.shadows = LightShadows.Hard;
            }
        }

    }
}
