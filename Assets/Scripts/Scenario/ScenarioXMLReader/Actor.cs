public class Actor
{
    public string Name;
    public int[] PreviousActivitiesIndexes;

    public Actor()
    {
        Name = "";
        PreviousActivitiesIndexes = null;
    }

    public Actor(string name, int previousActivityCount)
    {
        Name = name;
        PreviousActivitiesIndexes = new int[previousActivityCount];
    }
}