using System.Collections.Generic;

class Layer
{
    public int Id;
    public List<Activity> Activites;

    public Layer()
    {
        Id = 0;
        Activites = new List<Activity>();
    }

    public Layer(int id)
    {
        Id = id;
        Activites = new List<Activity>();
    }
}
