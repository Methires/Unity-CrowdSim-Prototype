using UnityEngine;

public class WeatherConditions : MonoBehaviour
{
    [Tooltip("1 - Morning, 2 - Noon, 3 - Afternoon")]
    [Range(1, 3)]
    public int Time;
    [Tooltip("1 - Sunny, 2 - Rain, 3 - Snow, 4 - Overcast, 5 - Fog")]
    [Range(1, 5)]
    public int Conditions;

    private GameObject mainLight;

    public GameObject MainLight
    {
        get
        {
            return mainLight;
        }

        set
        {
            mainLight = value;
        }
    }

    public void GenerateWeatherConditions()
    {
        SetLight(Time);
        SetConditions(Conditions);
    }

    void SetLight(int id)
    {
        Light[] lights = FindObjectsOfType<Light>();
        foreach (Light l in lights)
        {
            if (l.type == LightType.Directional)
            {
                l.transform.DetachChildren();
                //Destroy all directional lights
                DestroyImmediate(l.gameObject);
            }
        }
        //Create new directional light - the only one on the scene
        MainLight = new GameObject("Light");
        // mainLight.name = "Light";
        MainLight.AddComponent<Light>();
        MainLight.transform.position = Vector3.zero;
        MainLight.GetComponent<Light>().type = LightType.Directional;
        MainLight.GetComponent<Light>().shadows = LightShadows.Soft;

        float valueX = MainLight.transform.eulerAngles.x;
        float valueY = MainLight.transform.eulerAngles.y;
        valueY = 0f;
        MainLight.GetComponent<Light>().color = Color.white;

        switch (id)
        {
            case 1:
                valueX = 30.0f;
                break;
            case 2:
                valueX = 80.0f;
                break;
            case 3:
                valueX = -0.0f;
                MainLight.GetComponent<Light>().color = new Color32(144, 124, 70, 255);
                break;
            default:
                break;
        }
        MainLight.GetComponent<Light>().intensity = 0.7F;
        MainLight.GetComponent<Light>().transform.rotation = Quaternion.Euler(valueX, valueY, 0.0f);
    }

    void SetConditions(int id)
    {
        Light light = MainLight.GetComponent<Light>();
        Camera[] cameras = FindObjectsOfType<Camera>();
        switch (id)
        {
            case 2:
                foreach (var camera in cameras)
                {
                    GameObject rain = (GameObject)Instantiate(Resources.Load("Weather/Rain"));
                    rain.transform.position = camera.transform.position;
                    rain.transform.GetChild(0).transform.Translate(0.0f, 50.0f, 0.0f);
                    rain.transform.GetChild(0).transform.rotation = Quaternion.Euler(0.0f, camera.transform.rotation.eulerAngles.y, 0.0f);
                    rain.transform.GetChild(1).transform.Translate(0.0f, 10.0f, 0.0f);
                }
                light.intensity = 0.75f;
                light.shadowStrength = 0.5f;
                break;
            case 3:
                foreach (var camera in cameras)
                {
                    GameObject snow = (GameObject)Instantiate(Resources.Load("Weather/Snow"));
                    snow.transform.position = camera.transform.position;
                    snow.transform.GetChild(0).transform.Translate(0.0f, 50.0f, 0.0f);
                    snow.transform.GetChild(0).transform.rotation = Quaternion.Euler(0.0f, camera.transform.rotation.eulerAngles.y, 0.0f);
                    snow.transform.GetChild(1).transform.Translate(0.0f, 10.0f, 0.0f);
                }
                light.intensity = 0.65f;
                light.shadowStrength = 0.5f;
                break;
            case 4:
                light.color = Color.gray;
                light.intensity = 0.5f;
                light.shadowStrength = 0.5f;
                break;
            case 5:
                foreach (var camera in cameras)
                {
                    var fog = camera.GetComponent<UnityStandardAssets.ImageEffects.GlobalFog>();
                    fog.enabled = true;
                }
                light.color = Color.gray;
                light.intensity = 0.5f;
                light.shadowStrength = 0.45f;
                break;
            default:
                break;
        }
    }

    public void RemoveConditions()
    {
        Light light = MainLight.GetComponent<Light>();
        Camera[] cameras = FindObjectsOfType<Camera>();
        RainSnowController[] rains = FindObjectsOfType(typeof(RainSnowController)) as RainSnowController[];
        foreach (var rsc in rains)
        {
            DestroyImmediate(rsc.gameObject);
        }
        foreach (var camera in cameras)
        {
            var fog = camera.GetComponent<UnityStandardAssets.ImageEffects.GlobalFog>();
            if (fog != null)
            {
                Debug.Log("NO FOG in " + camera.name);
            }
            fog.enabled = false;
        }
    }
}
