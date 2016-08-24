using UnityEngine;

public class FogController : MonoBehaviour
{
    [Range(0.0f, 5.0f)]
    public float Scale = 0.6f;
    [Range(0.0f, 1.0f)]
    public float Intensity = 0.8f;
    [Range(0.0f, 2.5f)]
    public float Alpha = 1.96f;
    [Range(0.0f, 1.0f)]
    public float AlphaSub = 0.84f;
    [Range(0.0f, 4.0f)]
    public float Pow = 0.87f;
    public Color32 FogColor = new Color32(255, 255, 255, 255);

    private Material _material;

    void Start()
    {
        _material = GetComponent<Renderer>().material;
    }

	void Update()
    {
        _material.SetFloat("_Scale", Scale);
        _material.SetFloat("_Intensity", Intensity);
        _material.SetFloat("_Alpha", Alpha);
        _material.SetFloat("_AlphaSub", AlphaSub);
        _material.SetFloat("_Pow", Pow);
        _material.SetColor("_Color", FogColor);
    }
}
