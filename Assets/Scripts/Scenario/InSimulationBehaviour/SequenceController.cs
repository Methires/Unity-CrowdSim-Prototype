using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(Activity))]
public class SequenceController : MonoBehaviour
{
    private Movement _movementScript;
    private Activity _actionScript;
    private List<InGameActionInfo> _sequence;
    private int _currentActivityIndex;
    private bool _isFinished;
    private List<GameObject> _planes;

    public bool IsFinished
    {
        get
        {
            return _isFinished;
        }

        set
        {
            _isFinished = value;
        }
    }

    void Awake()
    {
        _sequence = new List<InGameActionInfo>();
        _planes = new List<GameObject>();
        _currentActivityIndex = -1;
        _movementScript = GetComponent<Movement>();
        _actionScript = GetComponent<Activity>();
        IsFinished = true;
    }

    void Update()
    {
        if (!IsFinished)
        {
            if (_sequence[_currentActivityIndex].Movement != null && _sequence[_currentActivityIndex].Activity == null)
            {
                if (_movementScript.IsFinished)
                {
                    LoadNewActivity();
                }
            }
            else if (_sequence[_currentActivityIndex].Movement == null && _sequence[_currentActivityIndex].Activity != null)
            {
                if (_actionScript.IsFinished)
                {
                    LoadNewActivity();
                }
            }
            else if (_sequence[_currentActivityIndex].Movement != null && _sequence[_currentActivityIndex].Activity != null)
            {
                if (_movementScript.IsFinished && _actionScript.IsFinished)
                {
                    LoadNewActivity();
                }
            }
        }
    }

    public void LoadNewActivity()
    {
        if (_currentActivityIndex + 1 < _sequence.Count)
        {
            if (_sequence[_currentActivityIndex + 1].Movement != null)
            {
                _movementScript.Speed = _sequence[_currentActivityIndex + 1].Movement.Speed;
                _movementScript.BlendParameter = _sequence[_currentActivityIndex + 1].Movement.Blend;
                _movementScript.Destination = _sequence[_currentActivityIndex + 1].Movement.Waypoint;
                if (_sequence[_currentActivityIndex + 1].Movement.Speed < 5.0f)
                {
                    GetComponent<DisplayActivityText>().ChangeText("Walking" + " " + _sequence[_currentActivityIndex + 1].Movement.Blend);
                }
                else
                {
                    GetComponent<DisplayActivityText>().ChangeText("Running" + " " + _sequence[_currentActivityIndex + 1].Movement.Blend);
                }
            }
            if (_sequence[_currentActivityIndex + 1].Activity != null)
            {
                _actionScript.ExitTime = _sequence[_currentActivityIndex + 1].Activity.ExitTime;
                _actionScript.BlendParameter = _sequence[_currentActivityIndex + 1].Activity.Blend;
                _actionScript.OtherAgents = _sequence[_currentActivityIndex + 1].Activity.RequiredAgents;
                _actionScript.ParamName = _sequence[_currentActivityIndex + 1].Activity.ParameterName;
                GetComponent<DisplayActivityText>().ChangeText(_sequence[_currentActivityIndex + 1].Activity.ParameterName + " " + _sequence[_currentActivityIndex + 1].Activity.Blend);
            }
            IsFinished = false;
            _currentActivityIndex++;
        }
        else
        {
            GetComponent<DisplayActivityText>().ChangeText("Scenario has ended");
            CleanUpPlanes();
            IsFinished = true;
        }
    }

    public void AddNewInGameAction(InGameActionInfo action)
    {
        _sequence.Add(action);
        GameObject planeMarkup = GameObject.CreatePrimitive(PrimitiveType.Plane);
        if (action.Movement != null)
        {
            planeMarkup.transform.localScale = new Vector3(0.1f, 1.0f, 0.1f);
            planeMarkup.transform.position = new Vector3(action.Movement.Waypoint.x, action.Movement.Waypoint.y + 0.01f, action.Movement.Waypoint.z);
            planeMarkup.GetComponent<Renderer>().material.color = Color.red;
        }
        if (action.Activity != null)
        {
            planeMarkup.transform.localScale = new Vector3(0.05f, 1.0f, 0.05f);
            Vector3 position = new Vector3();
            for (int i = _sequence.Count - 1; i>= 0; i--)
            {
                if (_sequence[i].Movement != null)
                {
                    position = new Vector3(_sequence[i].Movement.Waypoint.x, _sequence[i].Movement.Waypoint.y + 0.015f, _sequence[i].Movement.Waypoint.z);
                    break;
                }
            }
            planeMarkup.transform.position = position;
            planeMarkup.GetComponent<Renderer>().material.color = Color.yellow;
        }
        Destroy(planeMarkup.GetComponent<MeshCollider>());
        _planes.Add(planeMarkup);
    }

    private void CleanUpPlanes()
    {
        foreach (GameObject plane in _planes)
        {
            Destroy(plane.gameObject);
        }
    }
}

