
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

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
    private bool _markActivities;
    private bool _isCrowd;

    public bool MarkActivities
    {
        get
        {
            return _markActivities;
        }
        set
        {
            _markActivities = value;
        }
    }

    public bool IsCrowd
    {
        get
        {
            return _isCrowd;
        }
        set
        {
            _isCrowd = value;
        }
    }

    public bool IsFinished
    {
        get
        {
            return _isFinished;
        }
    }


    void Awake()
    {
        _sequence = new List<InGameActionInfo>();
        _planes = new List<GameObject>();
        _currentActivityIndex = -1;
        _movementScript = GetComponent<Movement>();
        _actionScript = GetComponent<Activity>();
        _isFinished = true;
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

                Vector3 positionOffsetForMultiActorActivity = Vector3.zero;
                Quaternion finalRotation = Quaternion.identity;
                if (_currentActivityIndex + 2 < _sequence.Count && _sequence[_currentActivityIndex + 2].Activity != null)
                {
                    if (_sequence[_currentActivityIndex + 2].Activity.RequiredAgents != null)
                    {
                        string[] paths = AssetDatabase.FindAssets(_sequence[_currentActivityIndex + 2].Activity.ParameterName);
                        string assetPath = AssetDatabase.GUIDToAssetPath(paths[0]);
                        GameObject exactSpotParent = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                        Transform exactSpot = exactSpotParent.transform.GetChild(0).transform;

                        //Vector3 flippedEuler = exactSpot.localEulerAngles; ;
                        //flippedEuler = new Vector3(0, flippedEuler.z , 0);
                        //finalRotation = Quaternion.Euler(flippedEuler);

                        finalRotation = Quaternion.Euler(0, exactSpot.rotation.eulerAngles.y + exactSpot.rotation.eulerAngles.z, 0);

                        if (exactSpot != null)
                        {
                            positionOffsetForMultiActorActivity.x = exactSpot.position.x;
                            positionOffsetForMultiActorActivity.z = exactSpot.position.z;
                        }
                    }
                }

                if (!_isCrowd)
                {
                    _movementScript.Destination = _sequence[_currentActivityIndex + 1].Movement.Waypoint + positionOffsetForMultiActorActivity;
                    _movementScript.FinalRotation = finalRotation;

                    if (_sequence[_currentActivityIndex + 1].Movement.Speed < 5.0f)
                    {
                        GetComponent<DisplayActivityText>().ChangeText("Walking" + " " + _sequence[_currentActivityIndex + 1].Movement.Blend);
                    }
                    else
                    {
                        GetComponent<DisplayActivityText>().ChangeText("Running" + " " + _sequence[_currentActivityIndex + 1].Movement.Blend);
                    }
                }
                else
                {
                    NavMeshPointGenerator generator = new NavMeshPointGenerator(15.0f);
                    _movementScript.Destination = generator.RandomPointOnNavMesh(transform.position);
                }
            }
            if (_sequence[_currentActivityIndex + 1].Activity != null)
            {
                _actionScript.ExitTime = _sequence[_currentActivityIndex + 1].Activity.ExitTime;
                _actionScript.BlendParameter = _sequence[_currentActivityIndex + 1].Activity.Blend;
                _actionScript.OtherAgents = _sequence[_currentActivityIndex + 1].Activity.RequiredAgents;
                _actionScript.ParamName = _sequence[_currentActivityIndex + 1].Activity.ParameterName;
                if (!_isCrowd)
                {
                    GetComponent<DisplayActivityText>().ChangeText(_sequence[_currentActivityIndex + 1].Activity.ParameterName + " " + _sequence[_currentActivityIndex + 1].Activity.Blend);
                }
            }
            _isFinished = false;
            _currentActivityIndex++;
        }
        else
        {
            if (!_isCrowd)
            {
                GetComponent<DisplayActivityText>().ChangeText("Scenario has ended");
                if (_markActivities)
                {
                    CleanUpPlanes();
                }
                _isFinished = true;
            }
            else
            {
                _currentActivityIndex = - 1;
                LoadNewActivity();
            }
        }
    }

    public void AddNewInGameAction(InGameActionInfo action)
    {
        _sequence.Add(action);
        if (_markActivities)
        {
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
                for (int i = _sequence.Count - 1; i >= 0; i--)
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
    }

    private void CleanUpPlanes()
    {
        foreach (GameObject plane in _planes)
        {
            Destroy(plane.gameObject);
        }
    }
}

