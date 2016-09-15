using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
using System.Collections.Generic;


public class DynamicAnimationState
{
    private Animator _anim;
    public AnimationClip _motion;
    private AnimatorController _animController;
    private AnimatorStateMachine _rootStateMachine;
    private AnimatorState _self;


    private string _fromStateName = "Idle";
    private string _toStateName = "Idle";
    private string _parameterName;
    private float _transitionLenght = 0.2f;
    private bool _originalRootMotionSetting;
    private int _isInDynamicStateHash;

    public float Length
    {
        get { return _motion.length; }
    }

    public DynamicAnimationState(Animator animator, string motionName)
    {
        _anim = animator;
        _animController = _anim.runtimeAnimatorController as AnimatorController;       

        _rootStateMachine = _animController.layers[0].stateMachine;
        _parameterName = motionName + "Parameter";
        _isInDynamicStateHash = Animator.StringToHash("IsInDynamicState");
        LoadMotion(motionName);
        AddToController();
    }

    public void CleanUp()
    {
        _rootStateMachine.RemoveState(_self);
        _animController.RemoveParameter(Animator.StringToHash(_parameterName));
    }

    private void AddToController()
    {
        _animController.AddParameter(_parameterName, AnimatorControllerParameterType.Bool);
        _self = _rootStateMachine.AddState(_motion.name);
        _self.motion = _motion;


        AnimatorState toState = _rootStateMachine.states.FirstOrDefault(x => x.state.name == _toStateName).state;
        AnimatorState fromState = _rootStateMachine.states.FirstOrDefault(x => x.state.name == _fromStateName).state;

        AnimatorStateTransition toTransition = _self.AddTransition(toState);
        AnimatorStateTransition fromTranistion = fromState.AddTransition(_self);

        SetupTransition(_self, toTransition);
        SetupTransition(fromState,fromTranistion);

        toTransition.AddCondition(AnimatorConditionMode.IfNot, 0, _parameterName);
        fromTranistion.AddCondition(AnimatorConditionMode.If, 0, _parameterName);

        _anim.runtimeAnimatorController = _animController;
    }

    private void SetupTransition(AnimatorState sourceState, AnimatorStateTransition transition)
    {
        //TODO
        //transition.hasExitTime = true;
        //transition.exitTime = GetTimeWithMinimumDistance(sourceState, transition.destinationState);
        transition.duration = _transitionLenght;
        transition.hasFixedDuration = false;
    }

    private float GetTimeWithMinimumDistance(AnimatorState from, AnimatorState to)
    {
        AnimationClip fromClip = from.motion as AnimationClip;
        AnimationClip toClip = to.motion as AnimationClip;
        int frameCount = (int)(fromClip.length * fromClip.frameRate);

        float minDistance = float.MaxValue;
        int minFrame = 0;

        for (int i = 0; i < frameCount; i++)
        {
            float distance = DifferenceInFrame(fromClip, toClip, i / frameCount);
            if (distance < minDistance)
            {
                minDistance = distance;
                minFrame = i;
            }
            
        }
        return minFrame / frameCount;
    }

    private float DifferenceInFrame(AnimationClip fromClip, AnimationClip toClip, float time)
    {
        EditorCurveBinding[] fromBindings = AnimationUtility.GetCurveBindings(fromClip);
        EditorCurveBinding[] toBindings = AnimationUtility.GetCurveBindings(toClip);

        //EditorCurveBinding[] = fromBindings.Intersect<EditorCurveBinding[]>(toBindings);
        List<EditorCurveBinding> commonBindings = new List<EditorCurveBinding>();

        foreach (EditorCurveBinding binding in fromBindings)
        {
            if (toBindings .Contains(binding))
            {
                commonBindings.Add(binding);
            }
        }

        float frameDifference = 0.0f;
        foreach (EditorCurveBinding binding in commonBindings)
        {
            frameDifference += CurvesDifference(AnimationUtility.GetEditorCurve(fromClip, binding), AnimationUtility.GetEditorCurve(toClip, binding), time);
        }

        return frameDifference / fromBindings.Length;
    }

    private float CurvesDifference(AnimationCurve from, AnimationCurve to, float time)
    {
        return Mathf.Abs(from.Evaluate(time) - to.Evaluate(0));
    }

    private void LoadMotion(string name)
    {
        string path = "Assets/Resources/Animations/" + name + ".fbx";
        _motion =  AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip)) as AnimationClip;
        
        if (!_motion)
        {
            Debug.LogError("Could not load animation asset: " + path);
        }
    }

    public void EnterState()
    {
        _originalRootMotionSetting = _anim.applyRootMotion;
        _anim.applyRootMotion = true;
        _anim.SetBool(_isInDynamicStateHash, true);
        _anim.SetBool(Animator.StringToHash(_parameterName), true);
    }

    public void ExitState()
    {
        _anim.SetBool(Animator.StringToHash(_parameterName), false);
        _anim.SetBool(_isInDynamicStateHash, false);
        _anim.applyRootMotion = _originalRootMotionSetting;
    }

    public bool HasFinished()
    {        
        var info = _anim.GetCurrentAnimatorStateInfo(0);
        return info.IsName(_self.name);
    }    
    
}
