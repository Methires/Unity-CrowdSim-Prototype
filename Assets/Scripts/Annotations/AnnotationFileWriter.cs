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

    private void CreateAnnotationsFile(string directory, string contents)
    {
        File.AppendAllText(directory, contents);
    }

    public void SaveAnnotatedFramesAtDirectory(List<AnnotatedFrame> annotatedFrames, string directory)
    {
        StringBuilder trackingStringBuilder = new StringBuilder();
        StringBuilder actionRecognitionStringBuilder = new StringBuilder();
        _screenshotId = Directory.GetFiles(directory, string.Format("*.{0}",_imageFormat)).Length;
        foreach (var annotatedFrame in annotatedFrames)
        {           
            foreach (var annotation in annotatedFrame.annotations)
            {
                trackingStringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                                                       _screenshotId,
                                                       annotation.agentId,
                                                       annotation.bounds.x,
                                                       annotation.bounds.y,
                                                       annotation.bounds.width,
                                                       annotation.bounds.height,
                                                       1.0f,
                                                       annotation.worldPosition.x,
                                                       annotation.worldPosition.y,
                                                       annotation.worldPosition.z));

                if (!annotation.isCrowd)
                {
                    actionRecognitionStringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6}",
                                                                _screenshotId,
                                                                annotation.agentId,
                                                                annotation.action,
                                                                annotation.bounds.x,
                                                                annotation.bounds.y,
                                                                annotation.bounds.width,
                                                                annotation.bounds.height));
                }                    
                             
            }           
            Save(annotatedFrame.frame, directory);
        }

        CreateAnnotationsFile(directory + "TrackingAnnotations.txt", trackingStringBuilder.ToString());
        CreateAnnotationsFile(directory + "ActionRecognitionAnnotations.txt", actionRecognitionStringBuilder.ToString());
        //screenshotId = 0;
    }

    private string ScreenShotName()
    {
        return string.Format("{0}.{1}", _screenshotId++, _imageFormat);
    }

    private void Save(Texture2D screenshot, string directory)
    {
        byte[] bytes = screenshot.EncodeToPNG();
        string filename = directory + ScreenShotName();
        File.WriteAllBytes(filename, bytes);     
    }

    //private void Save(byte[] screenshot, string directory)
    //{
    //    //byte[] bytes = screenshot.EncodeToPNG();
    //    string filename = directory + ScreenShotName();
    //    File.WriteAllBytes(filename, screenshot);
    //}
}
