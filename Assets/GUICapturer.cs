using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class GUICapturer : MonoBehaviour {


    private List<Texture2D> screens;
    private Camera _cam;

    void Start ()
    {
        screens = new List<Texture2D>();
        _cam = GetComponent<Camera>();
    }
	
	void LateUpdate ()
    {
        //_cam.tag = "MainCamera";
        
        StartCoroutine(Shoot());       
    }

    private IEnumerator Shoot()
    {
        yield return new WaitForEndOfFrame();

        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
        _cam.targetTexture = rt;
        RenderTexture.active = _cam.targetTexture;
        _cam.Render();

        var width = Screen.width;
        var height = Screen.height;
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();
        screens.Add(tex);
    }

    private void SaveAll()
    {
        int i = 0;
        foreach (var item in screens)
        {
            Save(item, i, "D:/Screenshots/Test");
            i++;
        }
    }

    private void Save(Texture2D screenshot, int number, string directory)
    {
        byte[] bytes = screenshot.EncodeToPNG();
        string filename = directory + "/" + number + _cam.name + ".png";
        //Debug.Log("Saving: " + filename);
        File.WriteAllBytes(filename, bytes);
    }

    void OnApplicationQuit()
    {
        SaveAll();
    }
}
