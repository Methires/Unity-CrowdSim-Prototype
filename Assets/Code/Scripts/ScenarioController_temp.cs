using UnityEngine;
using System.Collections.Generic;
using Assets.Code.Non_MonoBehaviour;

[RequireComponent(typeof(Action))]
[RequireComponent(typeof(Movement))]
public class ScenarioController_temp : MonoBehaviour
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
    private Action _actionScript;
    private Movement _movementScript;

    private List<Activity> _scenario;
    private int _currentActivityIndex;
    private bool _isFinished;

    void Start()
    {
        _scenario = new List<Activity>();
        _currentActivityIndex = -1;
        _actionScript = GetComponent<Action>();
        _movementScript = GetComponent<Movement>();
        _isFinished = false;
   
        var _waypoints1 = new List<Vector3>();
        _waypoints1.Add(new Vector3(50.0f, 0.0f, -100.0f));
        _waypoints1.Add(new Vector3(50.0f, 0.0f, 50.0f));
        var _wData1= new MovementData(_waypoints1, 10.0f);

        var _followedObject1 = GameObject.FindGameObjectWithTag("Test1");
        if (_followedObject1 == null)
        {
            Debug.Log("Can't find gameobject");
        }
        var _aData1 = new ActionData(null, _followedObject1);

        var _wActivity1 = new Activity(_wData1, _aData1);
        _scenario.Add(_wActivity1);
     
        LoadNewActivity();
    }

    void Update()
    {
        if (!_isFinished)
        {
            if (_movementScript.IsFinished && _actionScript.IsFinished)
            {
                LoadNewActivity();
            }
        }
        else
        {
            Debug.Log(gameObject.name + " has ended its scenario");
        }
    }

    private void LoadNewActivity()
    {
        if (_currentActivityIndex + 1 < _scenario.Count)
        {
            if (_scenario[_currentActivityIndex + 1].action != null)
            {
                _actionScript.ConsiderAnimation = _scenario[_currentActivityIndex + 1].action.ConsiderAnimation;
                _actionScript.LoopAnimation = _scenario[_currentActivityIndex + 1].action.LoopAnimation;
                _actionScript.AnimationClip = _scenario[_currentActivityIndex + 1].action.AnimClip;
                _actionScript.ConsiderExitTime = _scenario[_currentActivityIndex + 1].action.ConsiderExitTime;
                _actionScript.ExitTime = _scenario[_currentActivityIndex + 1].action.ExitTime;
                _actionScript.ConsiderExitObject = _scenario[_currentActivityIndex + 1].action.ConsiderExitObject;
                _actionScript.ExitObject = _scenario[_currentActivityIndex + 1].action.ExitObject;
            }

            if (_scenario[_currentActivityIndex + 1].movement != null)
            {
                _movementScript.Speed = _scenario[_currentActivityIndex + 1].movement.Speed;
                if (_scenario[_currentActivityIndex + 1].movement.IsFollowing)
                {
                    _movementScript.IsFollowing = true;
                    _movementScript.FollowedGameObject = _scenario[_currentActivityIndex + 1].movement.FollowedObject;
                }
                else
                {
                    _movementScript.WayPoints = _scenario[_currentActivityIndex + 1].movement.Waypoints;
                }
            }
            _currentActivityIndex++;
        }
        else
        {
            _isFinished = true;
        }
    }
}

