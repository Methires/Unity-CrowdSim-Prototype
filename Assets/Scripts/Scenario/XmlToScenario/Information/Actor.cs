public class Actor
{
    public string Name;
    public string MocapId;
    public int[] PreviousActivitiesIndexes;

    public Actor()
    {
        Name = "";
        MocapId = "";
        PreviousActivitiesIndexes = new int[0];
    }

    public Actor(string name, int[] previousIndexes)
    {
        Name = name;
        MocapId = "";
        PreviousActivitiesIndexes = previousIndexes;
    }

    public Actor(string name, string mocapId)
    {
        Name = name;
        MocapId = mocapId;
        PreviousActivitiesIndexes = new int[0];
    }
}