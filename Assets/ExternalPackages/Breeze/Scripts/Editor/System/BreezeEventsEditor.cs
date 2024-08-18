using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Breeze.Core
{
    [System.Serializable]
    [CustomEditor(typeof(BreezeEvents))]
    public class BreezeEventsEditor : Editor
    {
        private BreezeEvents system = null;

        //Toolbar Variables
        private int TabNumber = 0;
        private bool TabChanged = false;

        GUIContent[] Buttons = {
            new GUIContent(" General \n Events"), new GUIContent(" Stats \n Events"), new GUIContent(" Combat \n Events"),
            new GUIContent(" Detection \n Events")
        };

        private void OnEnable()
        {
            //Get The System
            system = (BreezeEvents)target;
        }

        public override void OnInspectorGUI()
        {
            if (!TabChanged)
            {
                TabChanged = true;
                if (PlayerPrefs.HasKey(system.gameObject.GetInstanceID() + " tab Events"))
                    TabNumber = PlayerPrefs.GetInt(system.gameObject.GetInstanceID() + " tab Events");
            }
            
            //Check Errors
            EditorGUILayout.Space(4);
            if (1 < 0)
            {
               
            }
            else
            {
                GUI.backgroundColor = new Color(0, 1, 0f, 0.19f);
                EditorGUILayout.HelpBox("Everything looks ready. There are no errors on your events setup.",
                    MessageType.Info);
            }
            GUI.backgroundColor = Color.white;
            

            //Toolbar
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(10);
            GUIStyle ToolbarStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 35,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            TabNumber = GUILayout.SelectionGrid(TabNumber, Buttons, 4, ToolbarStyle, GUILayout.Height(68),
                GUILayout.Width(EditorGUIUtility.currentViewWidth - 50));
            EditorGUILayout.EndVertical();
            PlayerPrefs.SetInt(system.gameObject.GetInstanceID() + " tab Events", TabNumber);

            //[Variables]

            //General Events
            if (TabNumber == 0)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("MOVEMENT EVENTS", EditorStyles.boldLabel);
                EditorGUILayout.Space(10);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnMovementChanged"));
                EditorGUILayout.Space(8);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnAgentStoppedState"));
                EditorGUILayout.Space(8);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnFleeAway"));
                EditorGUILayout.Space(8);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnPathUnreachable"));
                
                EditorGUILayout.Space(15);
                
                EditorGUILayout.LabelField("ANIMATION EVENTS", EditorStyles.boldLabel);
                EditorGUILayout.Space(10);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnAnimationPlayed"));
                EditorGUILayout.Space(8);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnAnimationEvent"));
                EditorGUILayout.Space(4);
                
                EditorGUILayout.EndVertical();
            }
            
            //Stats Events
            if (TabNumber == 1)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("HEALTH EVENTS", EditorStyles.boldLabel);
                EditorGUILayout.Space(10);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnTakeDamage"));
                EditorGUILayout.Space(8);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnRegenHealth"));
                EditorGUILayout.Space(8);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDeath"));
                EditorGUILayout.Space(4);
                
                EditorGUILayout.EndVertical();
            }
            
            //Combat Events
            if (TabNumber == 2)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("COMBAT EVENTS", EditorStyles.boldLabel);
                EditorGUILayout.Space(10);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDealDamage"));
                EditorGUILayout.Space(8);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnKilledTarget"));
                EditorGUILayout.Space(8);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnSwappedWeapon"));
                EditorGUILayout.Space(8);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnAlertState"));
                EditorGUILayout.Space(4);
                
                EditorGUILayout.EndVertical();
            }
            
            //Detection Events
            if (TabNumber == 3)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("DETECTION EVENTS", EditorStyles.boldLabel);
                EditorGUILayout.Space(10);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnFoundTarget"));
                EditorGUILayout.Space(8);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnLostTarget"));
                EditorGUILayout.Space(8);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnSoundDetected"));
                EditorGUILayout.Space(4);
                
                EditorGUILayout.EndVertical();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}