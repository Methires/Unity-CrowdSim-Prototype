using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices;

public class Screenshooter : MonoBehaviour
{
    private Dictionary<string, List<AnnotatedFrame>> _screenshots;
    private Camera[] _cameras;
    private int _resWidth;
    private int _resHeight;
    private Annotator _annotator;
    private AnnotationFileWriter _annotationFileWriter;
    private SimulationController _simulationController;
    private int _framesCounter = 0;
    private static int screenshotId = 0;

    public bool TakeScreenshots = false;
    public bool MarkAgentsOnScreenshots = false;
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
        }
    }

    void Awake ()
    {
        _resWidth = Screen.width;
        _resHeight = Screen.height;
        _annotationFileWriter = new AnnotationFileWriter();
        _simulationController = FindObjectOfType<SimulationController>();
        _cameras = FindObjectsOfType<Camera>();

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
        RenderTexture rt = new RenderTexture(_resWidth, _resHeight, 24);
        camera.targetTexture = rt;

        Texture2D screenShot = new Texture2D(_resWidth, _resHeight, TextureFormat.RGB24, false);
        RenderTexture.active = camera.targetTexture;
        camera.Render();
        
        screenShot.ReadPixels(new Rect(0, 0, _resWidth, _resHeight), 0, 0);
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
    }

    private void DrawAnnotationRectangles(Camera camera, Texture2D screenShot, List<Annotation> annotations)
    {       
        foreach (var annotation in annotations)
        {
            screenShot.DrawRectangle(annotation.bounds, Color.blue);
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
