using UnityEngine;

public class RainSnowController : MonoBehaviour
{
    public ParticleSystem RainFallParticleSystem;
    public ParticleSystem RainMistParticleSystem;

    private Material rainMaterial;
    private Material rainMistMaterial;

    void Start()
    {
        if (RainFallParticleSystem != null)
        {
            ParticleSystem.EmissionModule e = RainFallParticleSystem.emission;
            e.enabled = true;
            Renderer rainRenderer = RainFallParticleSystem.GetComponent<Renderer>();
            rainRenderer.enabled = true;
            rainMaterial = new Material(rainRenderer.material);
            rainMaterial.EnableKeyword("SOFTPARTICLES_OFF");
            rainRenderer.material = rainMaterial;
            RainFallParticleSystem.Play();
            ParticleSystem.MinMaxCurve rate = e.rate;
            rate.mode = ParticleSystemCurveMode.Constant;
            rate.constantMin = rate.constantMax = RainFallEmissionRate();
            e.rate = rate;
        }
        if (RainMistParticleSystem != null)
        {
            ParticleSystem.EmissionModule e = RainMistParticleSystem.emission;
            e.enabled = true;
            Renderer rainRenderer = RainMistParticleSystem.GetComponent<Renderer>();
            rainRenderer.enabled = true;
            rainMistMaterial = new Material(rainRenderer.material);
            rainMistMaterial.EnableKeyword("SOFTPARTICLES_ON");
            rainRenderer.material = rainMistMaterial;
            RainMistParticleSystem.Play();
            float emissionRate = MistEmissionRate();
            ParticleSystem.MinMaxCurve rate = e.rate;
            rate.mode = ParticleSystemCurveMode.Constant;
            rate.constantMin = rate.constantMax = emissionRate;
            e.rate = rate;
        }
    }

    void Update()
    {
        UpdateRain();
    }

    private void UpdateRain()
    {
        // keep rain and mist above the camera
        if (RainFallParticleSystem != null)
        {
            RainFallParticleSystem.transform.position = Camera.main.transform.position;
            RainFallParticleSystem.transform.Translate(0.0f, 50.0f, 0.0f);
            RainFallParticleSystem.transform.rotation = Quaternion.Euler(0.0f, Camera.main.transform.rotation.eulerAngles.y, 0.0f);
        }
        if (RainMistParticleSystem != null)
        {
            Vector3 pos = Camera.main.transform.position;
            pos.y += 10.0f;
            RainMistParticleSystem.transform.position = pos;
        }
    }

    private float RainFallEmissionRate()
    {
        return (RainFallParticleSystem.maxParticles / RainFallParticleSystem.startLifetime);
    }

    private float MistEmissionRate()
    {
        return (RainMistParticleSystem.maxParticles / RainMistParticleSystem.startLifetime);
    }
}