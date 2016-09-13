using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;


class AnnotationFileWriter
{
    private int _screenshotId = 0;
    private string _imageFormat = "png";
    private bool _markAgentsOnScreenshots = true;

    public bool MarkAgentsOnScreenshots
    {
        get
        {
            return _markAgentsOnScreenshots;
        }

        set
        {
            _markAgentsOnScreenshots = value;
        }
    }

    private void AppendAnnotationsFile(string directory, string contents)
    {
        File.AppendAllText(directory, contents);
    }

    public void SaveAnnotatedFramesAtDirectory(List<AnnotatedFrame> annotatedFrames, string directory)
    {
        StringBuilder trackingStringBuilder = new StringBuilder();
        StringBuilder actionRecognitionStringBuilder = new StringBuilder();

        StringBuilder trackingTrainingStringBuilder = new StringBuilder();

        _screenshotId = Directory.GetFiles(directory, string.Format("*.{0}",_imageFormat)).Length;
        foreach (var annotatedFrame in annotatedFrames)
        {           
            foreach (var annotation in annotatedFrame.annotations)
            {
                trackingStringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                                                       _screenshotId,
                                                       annotation.agentId,
                                                       annotation.trackingBounds.x,
                                                       annotation.trackingBounds.y,
                                                       annotation.trackingBounds.width,
                                                       annotation.trackingBounds.height,
                                                       1.0f,
                                                       annotation.worldPosition.x,
                                                       annotation.worldPosition.y,
                                                       annotation.worldPosition.z));

                trackingTrainingStringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                                                       _screenshotId,
                                                       -1.0f,
                                                       annotation.trackingBounds.x,
                                                       annotation.trackingBounds.y,
                                                       annotation.trackingBounds.width,
                                                       annotation.trackingBounds.height,
                                                       1.0f,
                                                       annotation.worldPosition.x,
                                                       annotation.worldPosition.y,
                                                       annotation.worldPosition.z));

                if (!annotation.isCrowd && annotation.actionRecognitionBoundsIsValid)
                {
                    actionRecognitionStringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6}",
                                                                _screenshotId,
                                                                annotation.agentId,
                                                                annotation.action,
                                                                annotation.actionRecognitionBounds.x,
                                                                annotation.actionRecognitionBounds.y,
                                                                annotation.actionRecognitionBounds.width,
                                                                annotation.actionRecognitionBounds.height));

                    SaveActorCutout(annotation, annotatedFrame.frame, directory);
                }               
            }

            if (MarkAgentsOnScreenshots)
            {
                DrawAnnotationRectangle(annotatedFrame.frame, annotatedFrame.annotations);
            }

            SaveScreenshot(annotatedFrame.frame, directory);

        }

        AppendAnnotationsFile(directory + "TrackingAnnotations.txt", trackingStringBuilder.ToString());
        AppendAnnotationsFile(directory + "TrackingTraining.txt", trackingTrainingStringBuilder.ToString());
        AppendAnnotationsFile(directory + "ActionRecognitionAnnotations.txt", actionRecognitionStringBuilder.ToString());
    }

    private string ScreenShotName()
    {
        return string.Format("{0}.{1}", _screenshotId++, _imageFormat);
    }

    private void SaveScreenshot(Texture2D screenshot, string directory)
    {
        byte[] bytes = screenshot.EncodeToPNG();
        string filename = directory + ScreenShotName();
        File.WriteAllBytes(filename, bytes);     
    }

    private void SaveActorCutout(Annotation annotation, Texture2D frame, string directory)
    {
        Rect boundingBox = annotation.actionRecognitionBounds;
        int width = (int)boundingBox.width;
        int height = (int)boundingBox.height;
        int x = (int)boundingBox.x;
        int y = frame.height - (int)boundingBox.y - height;
        

        Texture2D cutout = new Texture2D(width, height, TextureFormat.RGB24, false);
        Color[] pixels = frame.GetPixels(x, y, width, height);
        cutout.SetPixels(0,0, width, height, pixels);
        cutout.Apply();

        SaveCutout(cutout, directory, annotation.agentId.ToString() + annotation.action);
        GameObject.Destroy(cutout);
    }

    private void SaveCutout(Texture2D cutout, string directory, string agentActionName)
    {
        DirectoryInfo dirInfo = Directory.CreateDirectory(directory + agentActionName);
        string filename = string.Format("{0}.{1}", Directory.GetFiles(directory, string.Format("*.{0}", _imageFormat)).Length, _imageFormat);
        filename = dirInfo.FullName + "/" + filename;
        byte[] bytes = cutout.EncodeToPNG();
        
        File.WriteAllBytes(filename, bytes);
    }

    private void DrawAnnotationRectangle(Texture2D screenShot, List<Annotation> annotations)
    {
        foreach (var annotation in annotations)
        {
            screenShot.DrawRectangle(annotation.trackingBounds, Color.blue);

            if (annotation.actionRecognitionBoundsIsValid)
            {
                screenShot.DrawRectangle(annotation.actionRecognitionBounds, Color.green);
            }
            
        }     
    }

}
