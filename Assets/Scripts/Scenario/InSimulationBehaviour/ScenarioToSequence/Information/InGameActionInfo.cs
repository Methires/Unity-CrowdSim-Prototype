public class InGameActionInfo
{
    public MovementData Movement;
    public ActivityData Activity;

    public InGameActionInfo(MovementData movement)
    {
        Movement = movement;
        Activity = null;
    }

    public InGameActionInfo(ActivityData activity)
    {
        Movement = null;
        Activity = activity;
    }

    public InGameActionInfo(MovementData movement, ActivityData activity)
    {
        Movement = movement;
        Activity = activity;
    }
}
