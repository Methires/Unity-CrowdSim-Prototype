public class Actor
{
    public string Name;
    public string MocapId;
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