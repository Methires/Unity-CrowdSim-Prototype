using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;


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
    private float _length;

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

        var toTransition = _self.AddTransition(toState);
        var fromTranistion = fromState.AddTransition(_self);

        toTransition.AddCondition(AnimatorConditionMode.IfNot, 0, _parameterName);
        fromTranistion.AddCondition(AnimatorConditionMode.If, 0, _parameterName);

        _anim.runtimeAnimatorController = _animController;
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
        _anim.SetBool(Animator.StringToHash(_parameterName), true);
    }

    public void ExitState()
    {
        _anim.SetBool(Animator.StringToHash(_parameterName), false);
    }

    public bool HasFinished()
    {        
        var info = _anim.GetCurrentAnimatorStateInfo(0);
        return info.IsName(_self.name);
    }    
    
}
