using UnityEngine;

public class ActionData
{
    public bool ConsiderAnimation;
    public bool LoopAnimation;
    public AnimationClip AnimClip;
    public bool ConsiderExitTime;
    public float ExitTime;
    public bool ConsiderExitObject;
    public GameObject ExitObject;

    public ActionData()
    {
        ConsiderAnimation = false;
        LoopAnimation = false;
        AnimClip = null;
        ConsiderExitTime = false;
        ExitTime = 0.0f;
        ConsiderExitObject = false;
        ExitObject = null;
    }

    public ActionData(bool playAnimationOnce, AnimationClip animClip)
    {
        ConsiderAnimation = playAnimationOnce;
        LoopAnimation = !playAnimationOnce;
        AnimClip = animClip;
        ConsiderExitTime = false;
        ExitTime = 0.0f;
        ConsiderExitObject = false;
        ExitObject = null;
    }

    public ActionData(AnimationClip animClip, float exitTime)
    {
        ConsiderAnimation = false;
        LoopAnimation = true;
        AnimClip = animClip;
        ConsiderExitTime = true;
        ExitTime = exitTime;
        ConsiderExitObject = false;
        ExitObject = null;
    }

    public ActionData(AnimationClip animClip, GameObject exitObject)
    {
        ConsiderAnimation = false;
        LoopAnimation = true;
        AnimClip = animClip;
        ConsiderExitTime = false;
        ExitTime = 0.0f;
        ConsiderExitObject = true;
        ExitObject = exitObject;
    }

    public ActionData(AnimationClip animClip, float exitTime, GameObject exitObject)
    {
        ConsiderAnimation = false;
        LoopAnimation = true;
        AnimClip = animClip;
        ConsiderExitTime = true;
        ExitTime = exitTime;
        ConsiderExitObject = true;
        ExitObject = exitObject;
    }

    public ActionData(bool playAnimationOnce, AnimationClip animClip, float exitTime, GameObject exitObject)
    {
        ConsiderAnimation = playAnimationOnce;
        LoopAnimation = !playAnimationOnce;
        AnimClip = animClip;
        ConsiderExitTime = true;
        ExitTime = exitTime;
        ConsiderExitObject = true;
        ExitObject = exitObject;
    }

    public ActionData(bool playAnimationOnce, AnimationClip animClip, float exitTime)
    {
        ConsiderAnimation = playAnimationOnce;
        LoopAnimation = !playAnimationOnce;
        AnimClip = animClip;
        ConsiderExitTime = true;
        ExitTime = exitTime;
        ConsiderExitObject = false;
        ExitObject = null;
    }

    public ActionData(bool playAnimationOnce, AnimationClip animClip, GameObject exitObject)
    {
        ConsiderAnimation = playAnimationOnce;
        LoopAnimation = !playAnimationOnce;
        AnimClip = animClip;
        ConsiderExitTime = false;
        ExitTime = 0.0f;
        ConsiderExitObject = true;
        ExitObject = exitObject;
    }
}

