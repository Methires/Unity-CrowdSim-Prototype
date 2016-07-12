﻿using UnityEngine;

public class MovementData
{
    public Vector3 Waypoint;
    public float Speed;

    public MovementData()
    {
        Waypoint = new Vector3();
        Speed = 0.0f;
    }

    public MovementData(Vector3 waypoint, float speed)
    {
        Waypoint = waypoint;
        Speed = speed;
    }
}

