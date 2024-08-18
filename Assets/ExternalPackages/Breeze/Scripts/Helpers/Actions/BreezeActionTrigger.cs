using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.Events;

namespace Breeze.Core
{
    public enum TriggerType
    {
        Manually,
        OnTriggerEnter,
    }
    
    public class BreezeActionTrigger : MonoBehaviour
    {
        [Header("ACTION SETTINGS")] 
        [Space] 
        [Space]

        public BreezeSystem System;
        
        [Space] public TriggerType TriggerType = TriggerType.Manually;

        [Space] public List<BreezeCustomAction> CustomActions = new List<BreezeCustomAction>();


        [Space] 
        [Space] 
        [Header("ACTION EVENTS")] 
        [Space] 
        [Space] public UnityEvent OnActionsStarted = new UnityEvent();
        [Space] public UnityEvent<BreezeCustomAction> OnActionChanged = new UnityEvent<BreezeCustomAction>();
        [Space] public UnityEvent OnActionsEnd = new UnityEvent();
        
        
        private BreezeCustomAction currentAction;
        private int index = 0;
        [HideInInspector] public bool Done = true;
        private bool Working = false;

        public void StartActions()
        {
            OnActionsStarted.Invoke();
            index = 0;
            Working = false;
            currentAction = null;
            System.stopAI = true;
            Done = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (TriggerType == TriggerType.OnTriggerEnter)
            {
                if(other.gameObject.Equals(System.gameObject))
                    StartActions();
            }
        }

        private void Update()
        {
            if(Done)
                return;
            
            if (currentAction == null)
            {
                if (index > CustomActions.Count - 1)
                {
                    Done = true;
                    System.stopAI = false;
                    OnActionsEnd.Invoke();
                    return;
                }

                currentAction = CustomActions[index];
                
                if(index > 0)
                    OnActionChanged.Invoke(currentAction);
            }
            else
            {
                if (!Working)
                {
                    Working = true;
                    currentAction.eventToPlay.Invoke();

                    if (currentAction.animationAction)
                    {
                        PlayCustomAnimation();
                    }
                    else if(currentAction.ShouldRepeatUntilEnds)
                    {
                        InvokeRepeating(currentAction.CommandType.ToString(), 0, 0.1f);
                    }
                    else
                    {
                        Invoke(currentAction.CommandType.ToString(), 0);
                    }
                }
            }
        }
        
        //WalkToDestination
        private void WalkToDestination()
        {
           
        }

        private float prevVal = 0.0f;
        //Custom Animation
        private void PlayCustomAnimation()
        {
            System.SetNavMovement();
            System.ResetNav();
            if (currentAction.CustomAnimationType == CustomAnimationType.Custom)
            {
                if (currentAction.CustomType == CustomType.Bool)
                {
                    System.anim.SetBool(currentAction.ParameterName, currentAction.ParameterGoalValue);
                }
                else if(currentAction.CustomType == CustomType.Trigger)
                {
                    System.anim.SetTrigger(currentAction.ParameterName);
                }
                else
                {
                    prevVal = System.anim.GetFloat(currentAction.ParameterName);
                    System.anim.SetFloat(currentAction.ParameterName, currentAction.ParameterGoalNumber);
                }
            }
            else
            {
                
            }
            Invoke("AnimationEnd", currentAction.ActionLength);
        }

        private void AnimationEnd()
        {
            if (currentAction.CustomType == CustomType.Bool)
            {
                System.anim.SetBool(currentAction.ParameterName, !currentAction.ParameterGoalValue);
            }
            else if(currentAction.CustomType == CustomType.Trigger)
            {
                System.anim.ResetTrigger(currentAction.ParameterName);
            }
            else
            {
                System.anim.SetFloat(currentAction.ParameterName, prevVal);
                prevVal = 0.0f;
            }
            End();
        }

        private void End()
        {
            index++;
            currentAction = null;
            Working = false;   
        }
    }

#if UNITY_EDITOR
    [System.Serializable]
    [CustomEditor(typeof(BreezeActionTrigger))]
    public class BreezeActionTriggerEditor : Editor
    {
        private BreezeActionTrigger system = null;
        
        private void OnEnable()
        {
            //Get The System
            system = (BreezeActionTrigger)target;
        }

        public override void OnInspectorGUI()
        {
            DrawPropertiesExcluding(serializedObject, new string[] { "m_Script" });
            
            EditorGUI.BeginDisabledGroup(!system.Done || system.TriggerType == TriggerType.OnTriggerEnter);
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("START ACTION", GUILayout.Height(25), GUILayout.Width(230)))
            {
                system.StartActions();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}