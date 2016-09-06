public class Blend
{
    public string Name;
    public string MocapId;
    public float Probability;

    public Blend()
    {
        Name = "";
        MocapId = "";
        Probability = 0.0f;
    }

    public Blend(string name, string mocapid, float probability)
    {       
        Name = name;
        MocapId = mocapid;
        Probability = probability;
    }
}

