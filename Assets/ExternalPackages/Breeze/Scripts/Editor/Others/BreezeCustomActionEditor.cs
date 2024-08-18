using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Breeze.Core
{
    [System.Serializable]
    [CustomEditor(typeof(BreezeCustomAction))]
    public class BreezeCustomActionEditor : Editor
    {
        private BreezeCustomAction system = null;
        
        //Toolbar Variables
        private int TabNumber = 0;
        private bool TabChanged = false;

        GUIContent[] Buttons = new GUIContent[2]
        {
            new GUIContent(" Animation \n Action"), new GUIContent(" Command \n Action")
        };
        
        private void OnEnable()
        {
            //Get The System
            system = (BreezeCustomAction)target;
        }

        public override void OnInspectorGUI()
        {
            if (!TabChanged)
            {
                TabChanged = true;
                if (PlayerPrefs.HasKey(system.GetInstanceID() + " tab Action"))
                    TabNumber = PlayerPrefs.GetInt(system.GetInstanceID() + " tab Action");
            }
            
            //Check Errors
            EditorGUILayout.Space(4);
            GUI.backgroundColor = new Color(0, 1, 0f, 0.19f);
            EditorGUILayout.HelpBox("Everything looks ready. There are no errors on your action setup.",
                MessageType.Info);
            GUI.backgroundColor = Color.white;
            
            //Toolbar
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(10);
            GUIStyle ToolbarStyle = new GUIStyle(EditorStyles.miniButton);
            ToolbarStyle.fixedHeight = 35;
            ToolbarStyle.alignment = TextAnchor.MiddleCenter;
            ToolbarStyle.fontStyle = FontStyle.Bold;
            if (!system.animationAction && TabNumber == 0)
            {
                TabNumber = 1;
            }
            
            TabNumber = GUILayout.SelectionGrid(TabNumber, Buttons, 2, ToolbarStyle, GUILayout.Height(68),
                GUILayout.Width(EditorGUIUtility.currentViewWidth - 50));
            EditorGUILayout.EndVertical();
            PlayerPrefs.SetInt(system.GetInstanceID() + " tab Action", TabNumber);
            
            EditorGUILayout.Space(12);
            if (TabNumber == 0)
            {
                system.animationAction = true;
                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = new Color(0.0f, 1.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("[Selected Action]", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.BeginDisabledGroup(TabNumber == 1);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("ANIMATION ACTION", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CustomAnimationType"), new GUIContent("Custom Animation Type"));
            
            if (system.CustomAnimationType == CustomAnimationType.Custom)
            {
                EditorGUILayout.Space(15);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CustomType"), new GUIContent("Animation Parameter Type"));
                EditorGUILayout.Space(4);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ParameterName"), new GUIContent("Animation Parameter Name"));

                if (system.CustomType == CustomType.Bool)
                {
                    EditorGUILayout.Space(4);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ParameterGoalValue"), new GUIContent("Parameter Goal Value"));
                }
                else if (system.CustomType == CustomType.Number)
                {
                    EditorGUILayout.Space(4);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ParameterGoalNumber"), new GUIContent("Parameter Goal Value"));
                }
            }
            EditorGUILayout.Space(8);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ActionLength"), new GUIContent("Animation Play Length"));

            EditorGUILayout.Space(2);
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();
                
            EditorGUILayout.Space(30);

            if (TabNumber == 1)
            {
                system.animationAction = false;
                EditorGUILayout.Space(4);
                EditorGUILayout.BeginHorizontal();
                GUI.backgroundColor = new Color(0.0f, 1.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("[Selected Action]", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.BeginDisabledGroup(TabNumber == 0);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("COMMAND ACTION", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("CommandType"), new GUIContent("Custom Command Type"));
            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ShouldRepeatUntilEnds"), new GUIContent("Loop Until Finished"));

            if (system.CommandType == CommandType.WalkToDestination)
            {
                EditorGUILayout.Space(15);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DestinationType"), new GUIContent("Destination Variable Type"));

                if (system.DestinationType == DestinationType.Transform)
                {
                    EditorGUILayout.Space(4);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("GoalDestinationName"), new GUIContent("Destination Object Name"));
                }
                else
                {
                    EditorGUILayout.Space(4);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("GoalPosition"), new GUIContent("Destination Position"));
                }
                
                EditorGUILayout.Space(6);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("StoppingDistanceOverride"), new GUIContent("Stopping Distance Override"));
                EditorGUILayout.Space(6);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("WaitWhenArrived"), new GUIContent("Wait When Arrived"));
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space(30);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("eventToPlay"), new GUIContent("Event To Play"));
            EditorGUILayout.Space(2);
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndVertical();
        }
    }
}