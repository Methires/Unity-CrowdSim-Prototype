using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Animations
{
    public static class AnimatorControllerExtension
    {
        private static List<AnimatorState> _copiedStates = new List<AnimatorState>();
        private static List<AnimatorStateMachine> _copiedStateMachines = new List<AnimatorStateMachine>();

        public static AnimatorController Clone(this AnimatorController original)
        {
            AnimatorController copy = new AnimatorController();

            copy.hideFlags = original.hideFlags;
            copy.name = original.name;

            foreach (var parameter in CopyParameters(original.parameters))
            {
                copy.AddParameter(parameter);
            }      
            
            int layersCount = original.layers.Length;
            AnimatorControllerLayer[] newLayers = new AnimatorControllerLayer[layersCount];
           
            for (int i = 0; i < layersCount; i++)
            {
                newLayers[i] = CopyLayer(original.layers[i]);
            }

            copy.layers = newLayers;
            _copiedStates.Clear();
            _copiedStateMachines.Clear();
            return copy;
        }

        private static AnimatorControllerParameter[] CopyParameters(AnimatorControllerParameter[] original)
        {
            int length = original.Length;
            AnimatorControllerParameter[] copy = new AnimatorControllerParameter[length];

            for (int i = 0; i < length; i++)
            {
                copy[i] = CopyParameter(original[i]);
            }

            return copy;
        }


        private static AnimatorControllerParameter CopyParameter(AnimatorControllerParameter original)
        {
            AnimatorControllerParameter copy = new AnimatorControllerParameter();

            copy.defaultBool = original.defaultBool;
            copy.defaultFloat = original.defaultFloat;
            copy.defaultInt = original.defaultInt;
            copy.name = original.name;
            copy.type = original.type;
            return copy;
        }

        private static AnimatorControllerLayer CopyLayer(AnimatorControllerLayer original)
        {
            AnimatorControllerLayer copy = new AnimatorControllerLayer();

            copy.avatarMask = original.avatarMask;
            copy.blendingMode = original.blendingMode;
            copy.defaultWeight = original.defaultWeight;
            copy.iKPass = original.iKPass;
            copy.name = original.name;
            copy.stateMachine = CopyStateMachine(original.stateMachine);
            copy.syncedLayerAffectsTiming = original.syncedLayerAffectsTiming;
            copy.syncedLayerIndex = original.syncedLayerIndex;

            _copiedStates.Clear();
            _copiedStateMachines.Clear();

            return copy;
        }

        private static ChildAnimatorStateMachine CopyChildStateMachine(ChildAnimatorStateMachine original)
        {
            ChildAnimatorStateMachine copy = new ChildAnimatorStateMachine();

            copy.position = original.position;
            copy.stateMachine = CopyStateMachine(original.stateMachine);
            return copy;
        }

        private static AnimatorStateMachine CopyStateMachine(AnimatorStateMachine original)
        {
            AnimatorStateMachine copy = new AnimatorStateMachine();

            copy.anyStatePosition = original.anyStatePosition;
            copy.entryPosition = original.entryPosition;
            copy.exitPosition = original.exitPosition;
            copy.parentStateMachinePosition = original.parentStateMachinePosition;            
            copy.anyStateTransitions = original.anyStateTransitions;
            copy.behaviours = original.behaviours;
            
            copy.entryTransitions = original.entryTransitions;
            copy.hideFlags = original.hideFlags;
            copy.name = original.name;

            int childStateMachinesCount = original.stateMachines.Length;
            ChildAnimatorStateMachine[] newStateMachines = new ChildAnimatorStateMachine[childStateMachinesCount];

            for (int i = 0; i < childStateMachinesCount; i++)
            {
                newStateMachines[i] = CopyChildStateMachine(original.stateMachines[i]);
                copy.AddStateMachine(newStateMachines[i].stateMachine, newStateMachines[i].position);
            }
            //copy.stateMachines = newStateMachines;

            int childStateCount = original.states.Length;
            ChildAnimatorState[] newStates = new ChildAnimatorState[childStateCount];
                       
            for (int i = 0; i < childStateCount; i++)
            {
                newStates[i] = CopyChildState(original.states[i]);
                copy.AddState(newStates[i].state, newStates[i].position);
            }

            for (int i = 0; i < childStateCount; i++)
            {
                foreach (var transition in CopyTransitions(original.states[i].state.transitions))
                {
                    newStates[i].state.AddTransition(transition);
                }
            }

            copy.defaultState = _copiedStates.FirstOrDefault(x => x.name == original.defaultState.name);
            //copy.states = newStates;
            
            _copiedStates.Clear();

            _copiedStateMachines.Add(copy);
            return copy;
        }

        private static ChildAnimatorState CopyChildState(ChildAnimatorState original)
        {
            ChildAnimatorState copy = new ChildAnimatorState();

            copy.position = original.position;
            copy.state = CopyState(original.state);
            return copy;
        }

        private static AnimatorState CopyState(AnimatorState original)
        {
            AnimatorState copy = new AnimatorState();

            copy.behaviours = original.behaviours;
            copy.cycleOffset = original.cycleOffset;
            copy.cycleOffsetParameter = original.cycleOffsetParameter;
            copy.cycleOffsetParameterActive = original.cycleOffsetParameterActive;
            copy.hideFlags = original.hideFlags;
            copy.iKOnFeet = original.iKOnFeet;
            copy.mirror = original.mirror;
            copy.mirrorParameter = original.mirrorParameter;
            copy.mirrorParameterActive = original.mirrorParameterActive;
            copy.motion = original.motion;
            copy.name = original.name;
            copy.speed = original.speed;
            copy.speedParameter = original.speedParameter;
            copy.speedParameterActive = original.speedParameterActive;
            copy.tag = original.tag;
            //copy.transitions = original.transitions;

            _copiedStates.Add(copy);
            return copy;
        }

        //Copies single transition
        private static AnimatorStateTransition CopyTransition(AnimatorStateTransition original)
        {
            AnimatorStateTransition copy = new AnimatorStateTransition();

            copy.canTransitionToSelf = original.canTransitionToSelf;
            copy.conditions = original.conditions;
            copy.destinationState = _copiedStates.FirstOrDefault(x => x.name == original.destinationState.name);

            if (original.destinationStateMachine != null)
            {
                copy.destinationStateMachine = _copiedStateMachines.FirstOrDefault(x => x.name == original.destinationStateMachine.name);
            }
            
            copy.duration = original.duration;
            copy.exitTime = original.exitTime;
            copy.hasExitTime = original.hasExitTime;
            copy.hasFixedDuration = original.hasFixedDuration;
            copy.hideFlags = original.hideFlags;
            copy.interruptionSource = original.interruptionSource;
            copy.isExit = original.isExit;
            copy.mute = original.mute;
            copy.name = original.name;
            copy.offset = original.offset;
            copy.orderedInterruption = original.orderedInterruption;
            copy.solo = original.solo;
            return copy;
        }

        //Copies transition array
        private static AnimatorStateTransition[] CopyTransitions(AnimatorStateTransition[] original)
        {
            int length = original.Length;
            AnimatorStateTransition[] copy = new AnimatorStateTransition[length];

            for (int i = 0; i < length; i++)
            {
                copy[i] = CopyTransition(original[i]);
            }

            return copy;
        }

    }
}
