﻿using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(Action))]
public class ScenarioController : MonoBehaviour
{
    public struct Activity
    {
        public MovementData movement;
        public ActionData action;

        public Activity(MovementData movementData)
        {
            movement = movementData;
            action = null;
        }

        public Activity(ActionData actionData)
        {
            movement = null;
            action = actionData;
        }

        public Activity(MovementData movementData, ActionData actionData)
        {
            movement = movementData;
            action = actionData;
        }
    }

    private Movement _movementScript;
    private Action _actionScript;
    private List<Activity> _scenario;
    private int _currentActivityIndex;
    private bool _isFinished;

    void Awake()
    {
        _scenario = new List<Activity>();
        _currentActivityIndex = -1;
        _movementScript = GetComponent<Movement>();
        _actionScript = GetComponent<Action>();
        _isFinished = true;
    }

    void Update()
    {
        if (!_isFinished)
        {
            if (_scenario[_currentActivityIndex].movement != null && _scenario[_currentActivityIndex].action == null)
            {
                if (_movementScript.IsFinished)
                {
                    LoadNewActivity();
                }
            }
            else if (_scenario[_currentActivityIndex].movement == null && _scenario[_currentActivityIndex].action != null)
            {
                if (_actionScript.IsFinished)
                {
                    LoadNewActivity();
                }
            }
            else if (_scenario[_currentActivityIndex].movement != null && _scenario[_currentActivityIndex].action != null)
            {
                if (_movementScript.IsFinished && _actionScript.IsFinished)
                {
                    LoadNewActivity();
                }
            }
        }
        else
        {
            Debug.Log("End of scenario for: " + this.tag);
        }
    }

    public void LoadNewActivity()
    {
        if (_currentActivityIndex + 1 < _scenario.Count)
        {
            if (_scenario[_currentActivityIndex + 1].movement != null)
            {
                _movementScript.Speed = _scenario[_currentActivityIndex + 1].movement.Speed;
                _movementScript.Destination = _scenario[_currentActivityIndex + 1].movement.Waypoint;
            }
            if (_scenario[_currentActivityIndex + 1].action != null)
            {
                _actionScript.ExitObject = _scenario[_currentActivityIndex + 1].action.ExitObject;
                _actionScript.ExitTime = _scenario[_currentActivityIndex + 1].action.ExitTime;
                _actionScript.ParamName = _scenario[_currentActivityIndex + 1].action.ParameterName;
            }
            _isFinished = false;
            _currentActivityIndex++;
        }
        else
        {
            _isFinished = true;
        }
    }

    public void AddNewActivity(MovementData movementData, ActionData actionData)
    {
        Activity activity;
        if (movementData != null)
        {
            activity = new Activity(movementData);
            _scenario.Add(activity);

            GameObject planeMarkup = GameObject.CreatePrimitive(PrimitiveType.Plane);
            planeMarkup.transform.localScale = new Vector3(0.1f, 1.0f, 0.1f);
            planeMarkup.transform.position = new Vector3(movementData.Waypoint.x, -0.4f, movementData.Waypoint.z);
            planeMarkup.GetComponent<Renderer>().material.color = Color.red;
        }
        if (actionData != null)
        {
            activity = new Activity(actionData);
            _scenario.Add(activity);

            GameObject planeMarkup = GameObject.CreatePrimitive(PrimitiveType.Plane);
            planeMarkup.transform.localScale = new Vector3(0.05f, 1.0f, 0.05f);
            Vector3 position = new Vector3();
            for (int i = _scenario.Count - 1; i>= 0; i--)
            {
                if (_scenario[i].movement != null)
                {
                    position = new Vector3(_scenario[i].movement.Waypoint.x, -0.38f, _scenario[i].movement.Waypoint.z);
                    break;
                }
            }
            if (movementData != null)
            {
                position = new Vector3(movementData.Waypoint.x, -0.38f, movementData.Waypoint.z);
            }
            planeMarkup.transform.position = position;
            planeMarkup.GetComponent<Renderer>().material.color = Color.yellow;
        }
    }
}
