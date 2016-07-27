public class Actor
{
    public string Name;
    public int[] PreviousActivitiesIndexes;

    public Actor()
    {
        Name = "";
        PreviousActivitiesIndexes = null;
    }

    public Actor(string name, int[] previousIndexes)
    {
        Name = name;
        PreviousActivitiesIndexes = previousIndexes;
    }
}