using System.Collections.Generic;
using UnityEngine;


public class AnnotatedFrame
{
    public List<Annotation> annotations;
    public Texture2D frame;

    public AnnotatedFrame(Texture2D frame, List<Annotation> annotations)
    {
        this.frame = frame;
        this.annotations = annotations;
    }
}

