using UnityEngine;

public class MovementData
{
    public Vector3 Waypoint;
    public float Speed;
    public string Blend;

    public MovementData()
    {
        Waypoint = new Vector3();
        Speed = 0.0f;
        Blend = "";
    }

    public MovementData(Vector3 waypoint, float speed)
    {
        Waypoint = waypoint;
        Speed = speed;
        Blend = "";
    }

    public MovementData(Vector3 waypoint, float speed, string blend)
    {
        Waypoint = waypoint;
        Speed = speed;
        Blend = blend;
    }
}

