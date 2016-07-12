using System.Collections.Generic;
using UnityEngine;

namespace Assets.Code.Non_MonoBehaviour
{
    public class MovementData
    {
        public List<Vector3> Waypoints;
        public float Speed;
        public bool IsFollowing;
        public GameObject FollowedObject;

        public MovementData()
        {
            Waypoints = new List<Vector3>();
            Speed = 0.0f;
            IsFollowing = false;
            FollowedObject = null;
        }

        public MovementData(List<Vector3> waypoints, float speed)
        {
            Waypoints = waypoints;
            Speed = speed;
            IsFollowing = false;
            FollowedObject = null;
        }

        public MovementData(bool isFollowing, GameObject followedObject, float speed)
        {
            Waypoints = new List<Vector3>();
            Speed = speed;
            IsFollowing = isFollowing;
            FollowedObject = followedObject;
        }
    }
}
