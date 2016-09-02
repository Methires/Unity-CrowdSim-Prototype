﻿using UnityEngine;

[RequireComponent(typeof(NavMeshAgent))]
public class Movement : MonoBehaviour
{
    private NavMeshAgent _nMA;
    private float _speed;
    private Vector3 _destination;
    private Quaternion _finalRotation;
    private bool _isFinished;
    private bool _isInPosition = false;
    private string _blendParam;
    private GameObject hips;

    public bool IsFinished
    {
        get
        {
            return _isFinished;
        }
    }
    public float Speed
    {
        get
        {
            return _speed;
        }
        set
        {
            _speed = Mathf.Clamp(value, 0.0f, float.MaxValue);
            _nMA.speed = _speed;
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
            _nMA.Resume();
            _nMA.destination = value;
            _isFinished = false;
            _isInPosition = false;
        }
    }
    public string BlendParameter
    {
        get
        {
            return _blendParam;
        }

        set
        {
            _blendParam = value;
        }
    }

    public Quaternion FinalRotation
    {
        get
        {
            return _finalRotation;
        }

        set
        {
            _finalRotation = value;
        }
    }

    void Awake()
    {
        _nMA = GetComponent<NavMeshAgent>();
        _isFinished = true;

    }

    void Update()
    {
        if (!IsFinished)
        {
            if (!_isInPosition)
            {
                CheckPosition();
            }
            else
            {
                CheckRotation();
            }
            
        }
    }

    private void CheckPosition()
    {
        if (_nMA.remainingDistance < _nMA.stoppingDistance + Mathf.Epsilon)
        {
            if (Vector3.Distance(_nMA.destination, transform.position) < _nMA.stoppingDistance * 2)
            {
                _isInPosition = true;
                _nMA.Stop();
            }
            else
            {
                _nMA.SetDestination(_destination);
            }
        }
    }

    private void CheckRotation()
    {
        if (Mathf.Abs(Quaternion.Angle(transform.rotation, _finalRotation)) > 1.0f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, _finalRotation, 10 * Time.deltaTime);
        }
        else
        {
            _isFinished = true;
        }

           // transform.rotation = transform.rotation * (Quaternion.Inverse(hips.transform.localRotation) * _finalRotation);

        
    }
}
