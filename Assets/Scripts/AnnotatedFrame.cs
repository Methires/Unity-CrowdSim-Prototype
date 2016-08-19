using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

