﻿using UnityEngine;

public class MovementData
{
    public Vector3 Waypoint;
    public float Speed;
    public bool Forced;

    public MovementData()
    {
        Waypoint = new Vector3();
        Speed = 0.0f;
        Forced = false;
    }

    public MovementData(Vector3 waypoint, float speed)
    {
        Waypoint = waypoint;
        Speed = speed;
        Forced = false;
    }

    public MovementData(Vector3 waypoint, float speed, bool forced)
    {
        Waypoint = waypoint;
        Speed = speed;
        Forced = forced;
    }

    public MovementData(Vector3 waypoint, float speed, string blend)
    {
        Waypoint = waypoint;
        Speed = speed;
        Forced = false;
    }

    public MovementData(Vector3 waypoint, float speed, string blend, bool forced)
    {
        Waypoint = waypoint;
        Speed = speed;
        Forced = forced;
    }
}

