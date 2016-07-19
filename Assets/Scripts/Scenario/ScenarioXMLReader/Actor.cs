using System.Collections.Generic;

class Actor
{
    public string Name;
    public List<string> PreviousActivities;

    public Actor()
    {
        Name = "";
        PreviousActivities = new List<string>();
    }

    public Actor(string name)
    {
        Name = name;
        PreviousActivities = new List<string>();
    }
}