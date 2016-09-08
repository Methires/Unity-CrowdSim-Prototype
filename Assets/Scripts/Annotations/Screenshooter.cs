using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections;

public class Screenshooter : MonoBehaviour
{
    private Dictionary<string, List<AnnotatedFrame>> _screenshots;
    private Camera[] _cameras;
    private Annotator _annotator;
    private AnnotationFileWriter _annotationFileWriter;
    private SimulationController _simulationController;
    private int _framesCounter = 0;
    private static int screenshotId = 0;

    public bool TakeScreenshots = false;
    public bool MarkAgentsOnScreenshots = false;
    public int ResWidth = 1600;
    public int ResHeight = 1200;
    public int ScreenshotLimit = 1000;

    public Annotator Annotator
    {
        get
        {
            return _annotator;
        }

        set
        {
            _annotator = value;
            _annotator.SetResolution(ResWidth, ResHeight);
        }
    }

    void Awake ()
    {

        //Screen.SetResolution(Screen.resolutions[0].width, Screen.resolutions[0].height, Screen.fullScreen);
        //ResWidth = 16000;//Screen.width;
        //ResHeight = 12000;// Screen.height;
           
               
        Debug.Log(Screen.width + "x" + Screen.height);

        _annotationFileWriter = new AnnotationFileWriter();
        _simulationController = FindObjectOfType<SimulationController>();
        _cameras = FindObjectsOfType<Camera>();

        if (TakeScreenshots)
        {
            Time.captureFramerate = 24;
        }

        SetupDictionary();
    }

	void Update ()
    {

        if (TakeScreenshots)
        {
            foreach (var camera in _cameras)
            {
                TakeScreenshot(camera);
            }

            _framesCounter++;

            if (_framesCounter * _cameras.Length > ScreenshotLimit)
            {
                _framesCounter = 0;
                _simulationController.NotifyScreenshotBufferFull();
            }
        }
    }



    private void TakeScreenshot(Camera camera)
    {
        string previousTag = camera.tag;
        camera.tag = "MainCamera";
        RenderTexture rt = new RenderTexture(ResWidth, ResHeight, 24);
        camera.targetTexture = rt;

        Texture2D screenShot = new Texture2D(ResWidth, ResHeight, TextureFormat.RGB24, false);
        RenderTexture.active = camera.targetTexture;
        camera.Render();
        
        screenShot.ReadPixels(new Rect(0, 0, ResWidth, ResHeight), 0, 0);
        screenShot.Apply();

        camera.targetTexture = null;
        RenderTexture.active = null;

        List<Annotation> annotations = _annotator.MarkAgents(camera);

        if (MarkAgentsOnScreenshots)
        {
            DrawAnnotationRectangles(camera, screenShot, annotations);
        }

        _screenshots[camera.gameObject.name].Add(new AnnotatedFrame(screenShot, annotations));

        DestroyImmediate(rt);
        //DestroyImmediate(screenShot);
        camera.tag = previousTag;

        camera.ResetProjectionMatrix();
    }

    private void DrawAnnotationRectangles(Camera camera, Texture2D screenShot, List<Annotation> annotations)
    {       
        foreach (var annotation in annotations)
        {
            if (_simulationController.Tracking || !annotation.isCrowd)
            {
                screenShot.DrawRectangle(annotation.bounds, Color.blue);
            }          
        }
    }

    private void ClearDictionary()
    {
        foreach (KeyValuePair<string, List<AnnotatedFrame>> entry in _screenshots)
        {
            for (int i = 0; i < entry.Value.Count; i++)
            {
                DestroyImmediate(entry.Value[i].frame);
            }
            entry.Value.Clear();
        }
        _screenshots.Clear();
        SetupDictionary();
    }

    private void SetupDictionary()
    {
        _screenshots = new Dictionary<string, List<AnnotatedFrame>>();
        foreach (var camera in _cameras)
        {
            _screenshots.Add(camera.gameObject.name, new List<AnnotatedFrame>());
        }
    }

    public void SaveScreenshotsAtDirectory(string directory)
    {
        var outerDirInfo = Directory.CreateDirectory(directory + "/");

        foreach (KeyValuePair<string, List<AnnotatedFrame>> entry in _screenshots)
        {
            var dirInfo = Directory.CreateDirectory(outerDirInfo.FullName + "/" + entry.Key + "/");
            _annotationFileWriter.SaveAnnotatedFramesAtDirectory(entry.Value, dirInfo.FullName);
        }

        ClearDictionary();
    }
}
