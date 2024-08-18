using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Breeze.Core
{
    [System.Serializable]
    [CustomEditor(typeof(BreezeMeleeWeapon))]
    public class BreezeMeleeEditor : Editor
    {
        private BreezeMeleeWeapon weapon = null;
        
        //Toolbar Variables
        private int TabNumber = 0;
        private bool TabChanged = false;
        GUIContent[] Buttons = new GUIContent[4] {new GUIContent(" Weapon \n Settings"), new GUIContent(" Mesh \n Settings"), new GUIContent(" Sound \n Settings"), new GUIContent(" Weapon \n Events")};

        private void OnEnable()
        {
            //Get The Weapon
            weapon = (BreezeMeleeWeapon)target;
        }

        public override void OnInspectorGUI()
        {
            if (!TabChanged)
            {
                TabChanged = true;
                if (PlayerPrefs.HasKey(weapon.gameObject.GetInstanceID() + " tab"))
                    TabNumber = PlayerPrefs.GetInt(weapon.gameObject.GetInstanceID() + " tab");
            }
            
            var errorAvailable = true;
            var errorId = 0;

            EditorGUILayout.Space(4);
            if (weapon.transform.parent == null || weapon.transform.parent.GetComponent<BreezeWeaponHub>() == null)
            {
                errorId = 1;
                NormalError("There is no weapon hub found on your hand object, please assign one.");
            }
            else if (weapon.ImpactEffects.Where(effect => effect != null && effect.ImpactEffect == null).ToList().Count > 0)
            {
                errorId = 2;
                NormalError("There is an impact particle null on your impact effects list, please fix it.");
            }
            else if (weapon.AISystem.UseEquipSystem && (weapon.PrimaryWeaponParent == null || weapon.InventoryWeaponParent == null))
            {
                errorId = 3;
                NormalError("To use the equip system, please assign weapon meshes under the 'Mesh Settings' tab.");
            }
            else
            {
                GUI.backgroundColor = new Color(0, 1, 0f, 0.19f);
                EditorGUILayout.HelpBox("Everything looks ready. There are no errors on your weapon setup.", MessageType.Info);
                errorAvailable = false;
            }
            GUI.backgroundColor = Color.white;

            if (errorAvailable)
            {
                if(errorId != 1)
                    EditorGUI.BeginDisabledGroup(true);
                
                if (GUILayout.Button("FIX NOW", GUILayout.Width(65f), GUILayout.Height(22f)))
                {
                    if(weapon.transform.parent != null)
                    {
                        weaponClass weaponClass = new weaponClass(weapon.name, weapon, null, -1, -1, -1, -1);
                        weapon.transform.parent.gameObject.AddComponent<BreezeWeaponHub>().WeaponClasses.Add(weaponClass);
                        Selection.activeGameObject = weapon.transform.parent.gameObject;
                    }
                }
                EditorGUI.EndDisabledGroup();
            }

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
            PlayerPrefs.SetInt(weapon.gameObject.GetInstanceID() + " tab", TabNumber);

            //[Variables]
            
            //WeaponSettings Tab
            if (TabNumber == 0)
            {
                //Settings
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("SETTINGS", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AISystem"));
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Space(4.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnlyDamageOnce"));
                EditorGUILayout.Space(4.75f);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MinWeaponDamage"), new GUIContent("Weapon Damage:                       Min "));
                GUILayout.Label("    Max");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxWeaponDamage"), GUIContent.none ,true,GUILayout.Width(59));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(4.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ImpactEffects"));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
            }
            
            //Mesh Settings Tab
            if (TabNumber == 1)
            {
                //Settings
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("OBJECTS", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PrimaryWeaponParent"));
                EditorGUILayout.Space(1.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("InventoryWeaponParent"));
                EditorGUILayout.Space(1.75f);
                EditorGUILayout.EndVertical();
            }
            
            //Sound Settings Tab
            if (TabNumber == 2)
            {
                //Settings
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("OBJECTS", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("SwingSound"));
                EditorGUILayout.Space(0.5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("SwingVolume"));
                EditorGUILayout.Space(11f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DamagedSound"));
                EditorGUILayout.Space(0.5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DamagedVolume"));
                EditorGUILayout.Space(11f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BlockedSound"));
                EditorGUILayout.Space(0.5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BlockedVolume"));
                EditorGUILayout.Space(11f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DrawSound"));
                EditorGUILayout.Space(0.5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DrawVolume"));
                EditorGUILayout.Space(11f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("HolsterSound"));
                EditorGUILayout.Space(0.5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("HolsterVolume"));
                EditorGUILayout.EndVertical();
            }
            
            //Weapon Events Tab
            if (TabNumber == 3)
            {
                //Settings
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("EVENTS", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DamagedEvent"));
                EditorGUILayout.Space(7.5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BlockedEvent"));
                EditorGUILayout.EndVertical();
            }
            
            //Apply Changes
            serializedObject.ApplyModifiedProperties();
        }
        
        private void NormalError(string st)
        {
            GUI.backgroundColor = new Color(1, 0, 0f, 0.275f);
            EditorGUILayout.HelpBox(st, MessageType.Error);
        }
    }   
}
