using System.Collections.Generic;

public class Activity
{
    public string Name;
    public float Probability;
    public int Index;
    public List<Actor> Actors;
    public List<Blend> Blends;

    public Activity()
    {
        Name = "";
        Probability = 0.0f;
        Index = -1;
        Actors = new List<Actor>();
        Blends = new List<Blend>();
    }

    public Activity(string name, float probability, int index)
    {
        Name = name;
        Probability = probability;
        Index = index;
        Actors = new List<Actor>();
        Blends = new List<Blend>();
    }
}
