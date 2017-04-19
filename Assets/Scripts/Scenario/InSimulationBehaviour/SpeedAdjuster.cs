using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;

public class SpeedAdjuster : MonoBehaviour
{
    private struct DistanceAndSpeed
    {
        public float _distance;
        public float _speed;

        public DistanceAndSpeed(float distance, float speed)
        {
            _distance = distance;
            _speed = speed;
        }
    }

    private bool _adjust;
    private bool _walking;

    private Movement _movement;
    private NavMeshAgent _nMA;
    private Vector3 _destination;

    private List<GameObject> _otherRequiredAgents;

    public bool Adjust
    {
        get
        {
            return _adjust;
        }
        set
        {
            _adjust = value;
        }
    }
    public bool Walking
    {
        get
        {
            return _walking;
        }
        set
        {
            _walking = value;
        }
    }
    public Vector3 Destination
    {
        get
        {
            return _destination;
        }
        set
        {
            _destination = value;
        }
    }
    public List<GameObject> OtherAgents
    {
        get
        {
            return _otherRequiredAgents;
        }
        set
        {
            _otherRequiredAgents = value;
            if (_otherRequiredAgents != null)
            {
                _otherRequiredAgents.Remove(gameObject);
            }
        }
    }

    private void Awake()
    {
        _nMA = GetComponent<NavMeshAgent>();
        _movement = GetComponent<Movement>();
    }

    private void Update()
    {
        if (_adjust)
        {
            List<DistanceAndSpeed> otherAgentInfo = new List<DistanceAndSpeed>();
            foreach (GameObject otherAgent in OtherAgents)
            {
                float distance = otherAgent.GetComponent<SpeedAdjuster>().GetRemainingDistance();
                float speed = otherAgent.GetComponent<NavMeshAgent>().speed;
                otherAgentInfo.Add(new DistanceAndSpeed(distance, speed));
            }
            AdjustSpeed(otherAgentInfo);
        }
    }

    public float GetRemainingDistance()
    {
        List<Vector3> corners = _nMA.path.corners.ToList();
        if (!corners.Contains(_destination))
        {
            corners.Add(_destination);
        }
        float distance = Vector3.Distance(transform.position, corners[0]);
        for (int i = 0; i < corners.Count - 1; i++)
        {
            distance += Vector3.Distance(corners[i], corners[i + 1]);
        }
        return distance;
    }

    private void AdjustSpeed(List<DistanceAndSpeed> otherAgentinfo)
    {
        float agentDistance = GetRemainingDistance();
        float agentSpeed = _nMA.speed;
        float agentTime = agentDistance / agentSpeed;
        float time = 0.0f;

        foreach (var info in otherAgentinfo)
        {
            time += info._distance / info._speed;
        }
        time /= otherAgentinfo.Count;

        if (agentTime > time)
        {
            if (Walking)
            {
                _movement.Speed = Mathf.Clamp(_nMA.speed + 0.25f, 1.6f, 3.25f);
            }
            else
            {
                _movement.Speed = Mathf.Clamp(_nMA.speed + 0.25f, 3.5f, 5.5f);
            }
        }
        else if (agentTime < time)
        {
            if (Walking)
            {
                _movement.Speed = Mathf.Clamp(_nMA.speed - 0.25f, 1.6f, 3.25f);
            }
            else
            {
                _movement.Speed = Mathf.Clamp(_nMA.speed - 0.25f, 3.5f, 5.5f);
            }
        }
    }
}
