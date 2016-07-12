using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class SimpleWalkScript : MonoBehaviour
{
    private NavMeshAgent _navAgent;

    private float _rangeX_Min;
    private float _rangeX_Max;
    private float _rangeZ_Min;
    private float _rangeZ_Max;

    private float _maxTimeToDestination;
    private float _walkingToDestCounter;

    /* UNCOMMENT FOR BASIC WAITING FOR OBJECTS
    private bool _isWaiting;
    private float _waitingTime;
    private float _waitingTimeCounter;
    private float _waitingProbabilty;
    */

    public float RangeX_Min
    {
        get
        {
            return _rangeX_Min;
        }

        set
        {
            _rangeX_Min = value;
        }
    }

    public float RangeX_Max
    {
        get
        {
            return _rangeX_Max;
        }

        set
        {
            _rangeX_Max = value;
        }
    }

    public float RangeZ_Min
    {
        get
        {
            return _rangeZ_Min;
        }

        set
        {
            _rangeZ_Min = value;
        }
    }

    public float RangeZ_Max
    {
        get
        {
            return _rangeZ_Max;
        }

        set
        {
            _rangeZ_Max = value;
        }
    }

    public float MaxTimeToDestination
    {
        get
        {
            return _maxTimeToDestination;
        }

        set
        {
            _maxTimeToDestination = value;
        }
    }

    void Start ()
    {
        //UNCOMMENT FOR BASIC WAITING FOR OBJECTS
        //_waitingProbabilty = Mathf.Clamp(Random.Range(-2.0f, 10.0f), 0.0f, 10.0f);
        //_waitingTime = Random.Range(2.0f, 10.0f);
        _navAgent = GetComponent<NavMeshAgent>();
        _navAgent.destination = GenerateWaypoint(RangeX_Min, RangeX_Max, RangeZ_Min, RangeZ_Max);
	}
	
	void Update ()
    {
        if ((Mathf.Abs(_navAgent.destination.x - transform.position.x) < 1 && Mathf.Abs(_navAgent.destination.z - transform.position.z) < 1) || _walkingToDestCounter >= MaxTimeToDestination)
        {
            _navAgent.destination = GenerateWaypoint(RangeX_Min, RangeX_Max, RangeZ_Min, RangeZ_Max);
            _walkingToDestCounter = 0.0f;
        }
        _walkingToDestCounter += Time.deltaTime;
            /*UNCOMMENT FOR BASIC WAITING FOR OBJECTS
            if (Random.Range(0.0f, 10.0f) > _waitingProbabilty)
            {
                _isWaiting = true;
                _waitingTimeCounter = 0.0f;
                _navAgent.Stop();
                GetComponent<Animator>().enabled = false;
           }
        }

        if (_isWaiting)
        {
            _waitingTimeCounter += Time.deltaTime;
            if (_waitingTimeCounter > _waitingTime)
            {
                _isWaiting = false;
                _navAgent.Resume();
                GetComponent<Animator>().enabled = true;
            }
        }
        else
        {
            _walkingToDestCounter += Time.deltaTime;
        }*/
        
    }

    private Vector3 GenerateWaypoint(float RangeDownX, float RangeUpX, float RangeDownZ, float RangeUpZ)
    {
        Vector3 waypoint = new Vector3
        {
            x = Random.Range(RangeDownX, RangeUpX),
            y = 0.0f,
            z = Random.Range(RangeDownZ, RangeUpZ)
    };
        return waypoint;
    }

    /*APPLY ROOT MOTION HANDLED BY SCRIPT - [TO DO] LOOK THIS ONE UP
    void OnAnimatorMove()
    {

    }
    */
}
