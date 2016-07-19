using System.Collections.Generic;

class Activity
{
    public struct Blend
    {
        public float Probablity;
        public string Name;

        public Blend(float probability, string name)
        {
            Probablity = probability;
            Name = name;
        }
    }

    public float Probability;
    public string Name;
    public List<Actor> Actors;
    public List<Blend> Blends;

    public Activity()
    {
        Probability = 0.0f;
        Name = "";
        Actors = new List<Actor>();
        Blends = new List<Blend>();
    }

    public Activity(float probability, string name)
    {
        Probability = probability;
        Name = name;
        Actors = new List<Actor>();
        Blends = new List<Blend>();
    }
}
