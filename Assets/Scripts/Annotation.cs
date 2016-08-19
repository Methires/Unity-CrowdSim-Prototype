using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class Annotation
{
    public Rect bounds;
    public string action;

    public Annotation(string action, Rect bounds)
    {
        this.bounds = bounds;
        this.action = action;
    }
}
