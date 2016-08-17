﻿using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;

public class Screenshooter : MonoBehaviour {

    Camera[] cameras;
    public int resWidth = 2550;
    public int resHeight = 3300;
    //List<Texture2D> screenshots;
    Dictionary<string, List<Texture2D>> screenshots;
    public bool TakeScreenshots = false;



    void Awake ()
    {
        cameras = GetComponentsInChildren<Camera>();
        screenshots = new Dictionary<string, List<Texture2D>>();
        foreach (var camera in cameras)
        {
            screenshots.Add(camera.gameObject.name, new List<Texture2D>());
        }
        //screenshots = new List<Texture2D>();  
	}
	
	// Update is called once per frame
	void LateUpdate ()
    {
        if (TakeScreenshots)
        {
            //TakeScreenshot(cameras[0]);
            foreach (var camera in cameras)
            {
                StartCoroutine(TakeScreenshot(camera));
            }
        }

    }

    private static int screenshotId = 0;

    public static string ScreenShotName(string cameraName)
    {
        return string.Format("{0}_{1}.png",
                             cameraName,                                                         
                             screenshotId++);     
    }

    private IEnumerator TakeScreenshot(Camera camera)
    {
        yield return new WaitForEndOfFrame();

        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);
        RenderTexture.active = rt;
        camera.Render();
        
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        screenShot.Apply();
        camera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        screenshots[camera.gameObject.name].Add(screenShot);
        DestroyImmediate(rt);
    }

    public void SaveScreenshotsAtDirectory(string directory)
    {
        //string s = string.Format("Session-{0:yyyy-MM-dd_hh-mm-ss-tt}",System.DateTime.Now);
        var outerDirInfo = Directory.CreateDirectory(directory + "/");        
        foreach (KeyValuePair<string, List<Texture2D>> entry in screenshots)
        {
            screenshotId = 0;
            var dirInfo = Directory.CreateDirectory(outerDirInfo.FullName + "/" + entry.Key + "/");
            foreach (var screenshot in entry.Value)
            {
                Save(screenshot, entry.Key, dirInfo.FullName);
            }           
        }

        ClearDictionary();
    }

    private void ClearDictionary()
    {
        foreach (KeyValuePair<string, List<Texture2D>> entry in screenshots)
        {
            entry.Value.Clear();
        }
    }

    private void Save(Texture2D screenshot, string cameraName, string directory)
    {
        byte[] bytes = screenshot.EncodeToPNG();
        string filename = directory + ScreenShotName(cameraName);
        Debug.Log("Saving: " + filename);
        File.WriteAllBytes(filename, bytes);
    }
}