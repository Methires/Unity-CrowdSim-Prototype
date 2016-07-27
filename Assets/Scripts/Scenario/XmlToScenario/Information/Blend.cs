public class Blend
{
    public string Name;
    public float Probability;

    public Blend()
    {
        Name = "";
        Probability = 0.0f;
    }

    public Blend(string name, float probability)
    {
        Probability = probability;
        Name = name;
    }
}

