using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Breeze.Core
{ 
    [System.Serializable]
    [CustomEditor(typeof(BreezeGun))]
    public class BreezeGunEditor : Editor
    {
        private BreezeGun weapon = null;
        
        //Toolbar Variables
        private int TabNumber = 0;
        private bool TabChanged = false;
        GUIContent[] Buttons = new GUIContent[4] {new GUIContent(" Weapon \n Settings"), new GUIContent(" VFX \n Settings"), new GUIContent(" Mesh \n Settings"), new GUIContent(" Weapon \n Events")};
        
        private void OnEnable()
        {
            //Get The Weapon
            weapon = (BreezeGun)target;
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
            else if (weapon.transform.parent.GetComponent<BreezeWeaponHub>().WeaponClasses.Where(x => x.weaponScript == weapon).ToList().Count <= 0)
            {
                errorId = 2;
                NormalError("The 'Weapon Hub' component does not contain this weapon object, please configure it.");
            }
            else if (weapon.ImpactEffects.Where(effect => effect.ImpactEffect == null).ToList().Count > 0)
            {
                errorId = 3;
                NormalError("There is an impact particle null on your impact effects list, please fix it.");
            }
            else if (weapon.MuzzleParticles.Where(effect => effect == null).ToList().Count > 0)
            {
                errorId = 4;
                NormalError("There is an muzzle particle null on your muzzle particles list, please fix it.");
            }
            else if (!IsInLayerMask(weapon.BreezeSystem.gameObject.layer, weapon.BulletHitLayers))
            {
                errorId = 5;
                NormalError("The AI objects' layer isn't included in the 'Bullet Hit Layers', please include it to get weapon working.");
            }
            else if (weapon.BreezeSystem.UseEquipSystem && (weapon.PrimaryWeaponParent == null || weapon.InventoryWeaponParent == null))
            {
                errorId = 6;
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
                if(errorId != 5 && errorId != 1 && errorId != 2)
                    EditorGUI.BeginDisabledGroup(true);
                
                if (GUILayout.Button("FIX NOW", GUILayout.Width(65f), GUILayout.Height(22f)))
                {
                    switch (errorId)
                    {
                        case 5:
                            weapon.BulletHitLayers |= 1 << weapon.BreezeSystem.gameObject.layer;
                            break;
                        case 1 when weapon.transform.parent != null:
                        {
                            weaponClass weaponClass = new weaponClass(weapon.name, weapon, null, -1, -1, -1, -1);
                            weapon.transform.parent.gameObject.AddComponent<BreezeWeaponHub>().WeaponClasses.Add(weaponClass);
                            Selection.activeGameObject = weapon.transform.parent.gameObject;
                            break;
                        }
                        case 2:
                            Selection.activeGameObject = weapon.transform.parent.gameObject;
                            break;
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

            //WeaponSettings Tab
            if (TabNumber == 0)
            {
                //Settings
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("WEAPON SETTINGS", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BreezeSystem"));
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Space(2.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ShootingType"));
                EditorGUILayout.Space(2.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BulletType"));
                EditorGUILayout.Space(2.75f);
                if (weapon.ShootingType == BreezeEnums.ShootingType.Additive)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("FireRate"));
                    EditorGUILayout.Space(2.75f);   
                }
                else
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("SingleShotDelay"));
                    EditorGUILayout.Space(2.75f);   
                }
                EditorGUILayout.PropertyField(serializedObject.FindProperty("WeaponAccuracy"), new GUIContent("Weapon Accuracy (Idle)"));
                EditorGUILayout.Space(2.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MoveWeaponAccuracy"), new GUIContent("Weapon Accuracy (Non-Idle)"));
                EditorGUILayout.Space(2.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DebugHits"));
                EditorGUILayout.Space(2.75f);
                
                //IK Settings
                EditorGUILayout.Space(15);
                EditorGUILayout.LabelField("IK SETTINGS", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("UseHandIK"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnlyLeftHand"));
                EditorGUILayout.Space(6.5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("UseAimIK"));

                //Ammo Settings
                EditorGUILayout.Space(15);
                EditorGUILayout.LabelField("AMMO SETTINGS", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AmmoPerClip"));
                EditorGUILayout.Space(2.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ClipAmount"));
                EditorGUILayout.Space(2.75f);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MinDamageAmount"), new GUIContent("Damage Amount:                        Min "));
                GUILayout.Label("    Max");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxDamageAmount"), GUIContent.none ,true,GUILayout.Width(59));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                
                if (weapon.BulletType == BreezeEnums.BulletType.Projectile)
                {
                    //Projectile Settings
                    EditorGUILayout.Space(15);
                    EditorGUILayout.LabelField("PROJECTILE SETTINGS", EditorStyles.boldLabel);
                    EditorGUILayout.Space(2);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("BulletObject"));
                    EditorGUILayout.Space(2.75f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("BulletForce"));
                    EditorGUILayout.Space(2.75f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("BulletHitLayers"));
                }
                else
                {
                    //Raycast Settings
                    EditorGUILayout.Space(15);
                    EditorGUILayout.LabelField("RAYCAST SETTINGS", EditorStyles.boldLabel);
                    EditorGUILayout.Space(2);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("DrawBulletRays"));
                    EditorGUILayout.Space(2.75f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("BulletHitLayers"));
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
            }
            
            //VFX Tab
            if (TabNumber == 1)
            {
                //VFX Settings
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("VFX SETTINGS", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ImpactEffects"));
                EditorGUILayout.Space(2.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MuzzleParticles"));
                
                //Sound Settings
                EditorGUILayout.Space(15);
                EditorGUILayout.LabelField("SOUND SETTINGS", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("FireSoundEffect"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("FireSoundVolume"));
                EditorGUILayout.Space(3.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ReloadSoundEffect"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ReloadSoundVolume"));
                EditorGUILayout.Space(3.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DrawSoundEffect"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DrawSoundVolume"));
                EditorGUILayout.Space(3.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("HolsterSoundEffect"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("HolsterSoundVolume"));

                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
            }
            
            //Mesh Tab
            if (TabNumber == 2)
            {
                //Mesh Settings
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("MESH SETTINGS", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PrimaryWeaponParent"));
                EditorGUILayout.Space(1.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("InventoryWeaponParent"));
                EditorGUILayout.EndVertical();
            }
            
            //Events Tab
            if (TabNumber == 3)
            {
                //Events Settings
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("WEAPON EVENTS", EditorStyles.boldLabel);
                EditorGUILayout.Space(3.5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ShotEvent"));
                EditorGUILayout.Space(7.5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DamagedEvent"));
                EditorGUILayout.Space(7.5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ReloadEvent"));
                EditorGUILayout.Space(7.5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DrawEvent"));
                EditorGUILayout.Space(7.5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("HolsterEvent"));
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
        
        public static bool IsInLayerMask(int layer, LayerMask layerMask)
        {
            return layerMask == (layerMask | (1 << layer));
        }
    }
}