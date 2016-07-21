using System.Collections.Generic;

public class Level
{
    public int Index;
    public List<Activity> Activites;

    public Level()
    {
        Index = 0;
        Activites = new List<Activity>();
    }

    public Level(int id)
    {
        Index = id;
        Activites = new List<Activity>();
    }
}
