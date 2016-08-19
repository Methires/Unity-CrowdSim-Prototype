using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;

public class Screenshooter : MonoBehaviour {

    private Dictionary<string, List<Texture2D>> screenshots;
    Camera[] cameras;
    private int resWidth = 2550;
    private int resHeight = 3300;

    //public int resWidth = 2550;
    //public int resHeight = 3300;
    public bool TakeScreenshots = false;

    private AnnotationCreator _annotationsCreator;
    public AnnotationCreator AnnotationsCreator
    {
        get
        {
            return _annotationsCreator;
        }

        set
        {
            _annotationsCreator = value;
        }
    }

    void Awake ()
    {

    resWidth = Screen.width;
    resHeight = Screen.height;

    cameras = GetComponentsInChildren<Camera>();
        screenshots = new Dictionary<string, List<Texture2D>>();
        foreach (var camera in cameras)
        {
            screenshots.Add(camera.gameObject.name, new List<Texture2D>());
        }
    }

	void LateUpdate ()
    {
        if (TakeScreenshots)
        {
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
        string previousTag = camera.tag;
        camera.tag = "MainCamera";
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        camera.targetTexture = rt;

        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.ARGB32, false);
        RenderTexture.active = camera.targetTexture;//rt;
        camera.Render();
        
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        screenShot.Apply();
        camera.targetTexture = null;
        RenderTexture.active = null;
       
        List<Annotation> annotations = _annotationsCreator.GetAnnotations(camera);
        foreach (var annotation in annotations)
        {
            screenShot.DrawRectangle(annotation.bounds, Color.blue);
        } 

        screenshots[camera.gameObject.name].Add(screenShot);
        
        DestroyImmediate(rt);
        camera.tag = previousTag;
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
        //Debug.Log("Saving: " + filename);
        File.WriteAllBytes(filename, bytes);
    }
}
