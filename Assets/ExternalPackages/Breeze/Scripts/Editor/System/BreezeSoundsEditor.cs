using UnityEditor;
using UnityEngine;

namespace Breeze.Core
{
    [System.Serializable]
    [CustomEditor(typeof(BreezeSounds))]
    public class BreezeSoundsEditor : Editor
    {
        private BreezeSounds system = null;

        //Toolbar Variables
        private int TabNumber = 0;
        private bool TabChanged = false;

        GUIContent[] Buttons = new GUIContent[4]
        {
            new GUIContent(" Stationary \n Sounds"), new GUIContent(" Movement \n Sounds"), new GUIContent(" Combat \n Sounds"),
            new GUIContent(" Other \n Sounds")
        };

        private void OnEnable()
        {
            //Get The System
            system = (BreezeSounds)target;
        }

        public override void OnInspectorGUI()
        {
            if (!TabChanged)
            {
                TabChanged = true;
                if (PlayerPrefs.HasKey(system.gameObject.GetInstanceID() + " tab Sounds"))
                    TabNumber = PlayerPrefs.GetInt(system.gameObject.GetInstanceID() + " tab Sounds");
            }

            //Check Errors
            EditorGUILayout.Space(4);
            if (1 < 0)
            {
               
            }
            else
            {
                GUI.backgroundColor = new Color(0, 1, 0f, 0.19f);
                EditorGUILayout.HelpBox("Everything looks ready. There are no errors on your sound setup.",
                    MessageType.Info);
            }

            GUI.backgroundColor = Color.white;

            //Toolbar
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(10);
            GUIStyle ToolbarStyle = new GUIStyle(EditorStyles.miniButton);
            ToolbarStyle.fixedHeight = 35;
            ToolbarStyle.alignment = TextAnchor.MiddleCenter;
            ToolbarStyle.fontStyle = FontStyle.Bold;
            TabNumber = GUILayout.SelectionGrid(TabNumber, Buttons, 4, ToolbarStyle, GUILayout.Height(68),
                GUILayout.Width(EditorGUIUtility.currentViewWidth - 50));
            EditorGUILayout.EndVertical();
            PlayerPrefs.SetInt(system.gameObject.GetInstanceID() + " tab Sounds", TabNumber);

            //[Variables]
            
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("GeneralVolumeMultiplier"));
            EditorGUILayout.EndVertical();
            
            if (TabNumber == 0)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("IDLE SOUNDS", EditorStyles.boldLabel);
                EditorGUILayout.Space(8);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("IdleSounds"));
                
                EditorGUILayout.Space(15);
                EditorGUILayout.LabelField("EMOTE SOUNDS", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Coming Soon...", EditorStyles.miniLabel);
            }
            
            if (TabNumber == 1)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("MOVEMENT SOUNDS", EditorStyles.boldLabel);
                EditorGUILayout.Space(8);
                GUI.backgroundColor = new Color(1.0f, 1.0f, 0.0f, 0.45f);
                EditorGUILayout.LabelField("[Needs Animation Events To Work] (Included In Documentation)", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("WalkFootsteps"));
                EditorGUILayout.Space(8);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("RunFootsteps"));
            }
            
            if (TabNumber == 2)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("COMBAT SOUNDS", EditorStyles.boldLabel);
                EditorGUILayout.Space(8);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnAttackMissed"));
                EditorGUILayout.Space(0.5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnAttackMissedVolume"));
                EditorGUILayout.Space(6);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnAttackSuccessful"));
                EditorGUILayout.Space(0.5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnAttackSuccessfulVolume"));
                EditorGUILayout.Space(6);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnTookDamage"));
                EditorGUILayout.Space(0.5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnTookDamageVolume"));
                EditorGUILayout.Space(6);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDeath"));
                EditorGUILayout.Space(0.5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnDeathVolume"));
            }
            
            if (TabNumber == 3)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("DETECTION SOUNDS", EditorStyles.boldLabel);
                EditorGUILayout.Space(8);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnAlerted"));
                EditorGUILayout.Space(0.5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnAlertedVolume"));
                EditorGUILayout.Space(6);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnLostTarget"));
                EditorGUILayout.Space(0.5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnLostTargetVolume"));
            }

            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndVertical();
        }
    }
}
