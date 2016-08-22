using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;

public class Screenshooter : MonoBehaviour {

    private Dictionary<string, List<Texture2D>> _screenshots;
    private Camera[] _cameras;
    private int _resWidth;
    private int _resHeight;
    private Annotator _annotator;
    private static int screenshotId = 0;

    public bool TakeScreenshots = false;
    public bool MarkAgentsOnScreenshots = false;

    public Annotator Annotator
    {
        get
        {
            return _annotator;
        }

        set
        {
            _annotator = value;
        }
    }

    void Awake ()
    {

    _resWidth = Screen.width;
    _resHeight = Screen.height;

    _cameras = GetComponentsInChildren<Camera>();
        _screenshots = new Dictionary<string, List<Texture2D>>();
        foreach (var camera in _cameras)
        {
            _screenshots.Add(camera.gameObject.name, new List<Texture2D>());
        }
    }

	void LateUpdate ()
    {
        if (TakeScreenshots)
        {
            foreach (var camera in _cameras)
            {
                StartCoroutine(TakeScreenshot(camera));
            }
        }
    }


    private static string ScreenShotName(string cameraName)
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
        RenderTexture rt = new RenderTexture(_resWidth, _resHeight, 24);
        camera.targetTexture = rt;

        Texture2D screenShot = new Texture2D(_resWidth, _resHeight, TextureFormat.ARGB32, false);
        RenderTexture.active = camera.targetTexture;
        camera.Render();
        
        screenShot.ReadPixels(new Rect(0, 0, _resWidth, _resHeight), 0, 0);
        screenShot.Apply();
        camera.targetTexture = null;
        RenderTexture.active = null;

        if (MarkAgentsOnScreenshots)
        {
            DrawAnnotationRectangles(camera, screenShot);
        }        

        _screenshots[camera.gameObject.name].Add(screenShot);
        
        DestroyImmediate(rt);
        camera.tag = previousTag;
    }

    private void DrawAnnotationRectangles(Camera camera, Texture2D screenShot)
    {
        List<Annotation> annotations = _annotator.MarkAgents(camera);
        foreach (var annotation in annotations)
        {
            screenShot.DrawRectangle(annotation.bounds, Color.blue);
        }
    }

    private void ClearDictionary()
    {
        foreach (KeyValuePair<string, List<Texture2D>> entry in _screenshots)
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

    public void SaveScreenshotsAtDirectory(string directory)
    {
        //string s = string.Format("Session-{0:yyyy-MM-dd_hh-mm-ss-tt}",System.DateTime.Now);
        var outerDirInfo = Directory.CreateDirectory(directory + "/");
        foreach (KeyValuePair<string, List<Texture2D>> entry in _screenshots)
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
}
