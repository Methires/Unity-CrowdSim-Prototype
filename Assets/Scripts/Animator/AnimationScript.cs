using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class DynamicAnimationState_old
{
    private AnimatorStateMachine _rootMachine;
    private AnimatorController _controller;
    private AnimatorState _self;
    private string _parameterName;

    //TODO: listy stanów i przejść
    private List<AnimatorStateTransition> _enteringTransitions = new List<AnimatorStateTransition>();
    private List<AnimatorStateTransition> _exitingTransitions = new List<AnimatorStateTransition>();

    //private List<AnimatorState> _enteringStates = new List<AnimatorState>();
    //private List<AnimatorState> _exitingStates = new List<AnimatorState>();

    public string name = "DAS";
    public Motion motion;
    public AnimatorState to;
    public AnimatorState from;

    public string ParameterName
    { 
        get { return _parameterName; }
    }

    public AnimatorState Self
    {
        get { return _self;}
    }

    public DynamicAnimationState_old(Motion motion, AnimatorState from, AnimatorState to, AnimatorController controller, string name)
    {
        _controller = controller;
        _rootMachine = controller.layers[0].stateMachine;
        this.motion = motion;
        this.from = from;
        this.to = to;
        this.name = name;

        _parameterName = name + "Parameter";
    }

    ~DynamicAnimationState_old()
    {
        RemoveFromController();
    }

    public void AddToController()
    {
        _controller.AddParameter(_parameterName, AnimatorControllerParameterType.Bool);

        _self = _rootMachine.AddState(name);
        _self.motion = motion;

        var toTransition = _self.AddTransition(to);
        var fromTranistion = from.AddTransition(_self);

        toTransition.AddCondition(AnimatorConditionMode.IfNot, 0, _parameterName);
        fromTranistion.AddCondition(AnimatorConditionMode.If, 0, _parameterName);

        GetTransitionLength(from.motion as AnimationClip, _self.motion as AnimationClip);
    }

    public void AddEnteringTransition(AnimatorState to)
    {
        _enteringTransitions.Add(_self.AddTransition(to));       
    }

    public void AddExitingTransition(AnimatorState from)
    {
        _exitingTransitions.Add(from.AddTransition(_self));
    }
    public void RemoveFromController()
    {
        _rootMachine.RemoveState(_self);

        //TODO: usuwanie parametru
        //_controller.RemoveParameter(0);
    }

    private float GetTransitionLength(AnimationClip clip1, AnimationClip clip2)
    {
        float minTime = Mathf.Min(clip1.length, clip2.length);

        Debug.Log(clip1.length);

        //43s dla drzewa?
        Debug.Log(clip2.length);

        List<AnimationCurve> curves1 = new List<AnimationCurve>();
        List<AnimationCurve> curves2 = new List<AnimationCurve>();

        foreach (var binding in AnimationUtility.GetCurveBindings(clip1))
        {
            curves1.Add(AnimationUtility.GetEditorCurve(clip1, binding));
        }

        foreach (var binding in AnimationUtility.GetCurveBindings(clip2))
        {
            curves2.Add(AnimationUtility.GetEditorCurve(clip2, binding));
        }

        if (curves1.Count != curves2.Count)
            return 0.0f;

        for (float t = 0.0f; t < minTime; t += minTime * 0.01f)
        {
            Debug.Log(PrincipalComponentsMetric(curves1, curves2, t));
            //Debug.Log(t);
        }
        
        return 0.0f;
    }

    private float PrincipalComponentsMetric(List<AnimationCurve> curves1, List<AnimationCurve> curves2, float time)
    {
        int nCurves = curves1.Count;
        float sum = 0.0f;

        for (int i = 0; i < nCurves; i++)
        {
            sum += CurvesDistance(curves1[i], curves2[i], time);
        }

        return Mathf.Sqrt(sum);
    }

    private float CurvesDistance(AnimationCurve curve1, AnimationCurve curve2, float time)
    {        
        return (Mathf.Pow(curve1.Evaluate(time),2) - Mathf.Pow(curve2.Evaluate(time),2));
    }


}

public class AnimationScript : MonoBehaviour
{
    public Motion newMotion;
    private Animator _anim;
    private AnimatorController _animController;

    public List<DynamicAnimationState_old> dynamicAnimationsList = new List<DynamicAnimationState_old>();

    void Start ()
    {
        _anim = GetComponent<Animator>();
        _animController = _anim.runtimeAnimatorController as AnimatorController;

        foreach (var binding in AnimationUtility.GetCurveBindings(newMotion as AnimationClip))
        {           
            AnimationCurve curve = AnimationUtility.GetEditorCurve(newMotion as AnimationClip, binding);
            //Debug.Log(binding.propertyName);
        }

        AnimatorStateMachine rootStateMachine = _animController.layers[0].stateMachine;
        AnimatorState squat = rootStateMachine.states.First(x => x.state.name == "SlavicSquat").state;
        AnimatorState movementTree = rootStateMachine.states.First(x => x.state.name == "BranchedMovementTree").state;
        DynamicAnimationState_old d = new DynamicAnimationState_old(newMotion, squat, movementTree, _animController, "fako");

        dynamicAnimationsList.Add(d);
        AddAnimationStates();
    }

    void AddAnimationStates()
    {
        foreach (var state in dynamicAnimationsList)
        {
            state.AddToController();
        }
    }
	
	void Update ()
    {

    }

    void OnApplicationQuit()
    {
        foreach (var d in dynamicAnimationsList)
        {
            d.RemoveFromController();
        }
    }
}
