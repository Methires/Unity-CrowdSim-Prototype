﻿using System.Collections.Generic;

public class Action
{
    public string Name;
    public float Probability;
    public int Index;
    public List<Actor> Actors;
    public bool Forced;

    public Action()
    {
        Name = "";
        Probability = 0.0f;
        Index = -1;
        Actors = new List<Actor>();
        Forced = false;
    }

    public Action(string name, float probability, int index)
    {
        Name = name;
        Probability = probability;
        Index = index;
        Actors = new List<Actor>();
        Forced = false;
    }
}
