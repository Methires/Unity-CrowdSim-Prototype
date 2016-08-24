using UnityEngine;
using System.Collections.Generic;

public class WeatherConditions : MonoBehaviour
{   
    [Tooltip("1 - Morning, 2 - Noon, 3 - Afternoon")]
    [Range(1, 3)]
    public int Time;
    [Tooltip("1 - Sunny, 2 - Rain, 3 - Snow, 4 - Overcast, 5 - Fog")]
    [Range(1, 5)]
    public int Conditions;

    public void GenerateWeatherConditions()
    {
        SetLight(Time);
        SetConditions(Conditions);
    }

    private void SetLight(int id)
    {
        GameObject light;
        if (FindObjectOfType<Light>() == null)
        {
            light = new GameObject();
            light.name = "Light";
            light.AddComponent<Light>();
            light.transform.position = Vector3.zero;
            light.GetComponent<Light>().type = LightType.Directional;
            light.GetComponent<Light>().shadows = LightShadows.Soft;
        }
        else
        {
            light = FindObjectOfType<Light>().gameObject;
        }
        float value = light.transform.eulerAngles.x;
        switch (id)
        {
            case 1:
                value = 30.0f;
                light.GetComponent<Light>().color = Color.white;
                break;
            case 2:
                value = 90.0f;
                light.GetComponent<Light>().color = Color.white;
                break;
            case 3:
                value = 170.0f;
                light.GetComponent<Light>().color = new Color32(144, 124, 70, 255);
                break;
            default:
                break;
        }
        light.transform.rotation = Quaternion.Euler(value, 0.0f, 0.0f);
    }

    private void SetConditions(int id)
    {
        Light light = FindObjectOfType<Light>();
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
                    GameObject fog = (GameObject)Instantiate(Resources.Load("Weather/Fog"));
                    fog.transform.position = camera.transform.position;
                }
                light.color = Color.gray;
                light.intensity = 0.5f;
                light.shadowStrength = 0.45f;
                break;
            default:
                break;
        }
    }
}
