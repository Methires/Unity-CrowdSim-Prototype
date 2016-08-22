using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;


class AnnotationFileWriter
{
    private int screenshotId = 0;

    private void CreateAnnotationsFile(string directory, string contents)
    {
        File.WriteAllText(directory + "annotations.txt", contents);
    }

    public void SaveAnnotatedFramesAtDirectory(List<AnnotatedFrame> annotatedFrames, string directory)
    {

        //int predictedLineSize = 50;
        StringBuilder stringBuilder = new StringBuilder();
        foreach (var annotatedFrame in annotatedFrames)
        {           
            foreach (var annotation in annotatedFrame.annotations)
            {
                stringBuilder.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9}",
                                                       screenshotId,
                                                       annotation.agentId,
                                                       annotation.bounds.x,
                                                       annotation.bounds.y,
                                                       annotation.bounds.width,
                                                       annotation.bounds.height,
                                                       1.0f,
                                                       annotation.worldPosition.x,
                                                       annotation.worldPosition.y,
                                                       annotation.worldPosition.z));
            }           
            Save(annotatedFrame.frame, directory);
        }

        CreateAnnotationsFile(directory, stringBuilder.ToString());
        screenshotId = 0;
    }

    private string ScreenShotName()
    {
        return string.Format("{0}.png", screenshotId++);
    }

    private void Save(Texture2D screenshot,  string directory)
    {
        byte[] bytes = screenshot.EncodeToPNG();
        string filename = directory + ScreenShotName();
        //Debug.Log("Saving: " + filename);
        File.WriteAllBytes(filename, bytes);
    }
}
