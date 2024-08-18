#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Breeze.Core
{
    [System.Serializable]
    [CustomEditor(typeof(BreezeSystem))]
    public class BreezeEditor : Editor
    {
        private BreezeSystem system = null;
        
        //Toolbar Variables
        private int TabNumber = 0;
        private int WarningNumber = 0;
        private bool TabChanged = false;
        private GUIContent[] Buttons = {new GUIContent(" Health \n Settings"), new GUIContent(" General \n Settings"), new GUIContent(" Combat \n Settings"), new GUIContent(" Detection \n Settings"), new GUIContent(" Faction \n Settings"), new GUIContent(" Companion \n Settings"), new GUIContent(" Inverse \n Kinematics"), new GUIContent("  Debug \n Panel")};

        private void OnEnable()
        {
            //Get The System
            system = (BreezeSystem)target;
        }

        public override void OnInspectorGUI()
        {
            if (!TabChanged)
            {
                TabChanged = true;
                if (PlayerPrefs.HasKey(system.gameObject.GetInstanceID() + " tab Core"))
                    TabNumber = PlayerPrefs.GetInt(system.gameObject.GetInstanceID() + " tab Core");
            }
            
            List<string> warningMessages = new List<string>();
            var errorAvailable = true;
            var errorId = 0;
            
            //Checker
            EditorGUILayout.Space(4);
            
            //Errors 
            if (system.HeadTransform == null)
            {
                errorId = 0;
                NormalError("The 'Head Transform' of the AI is null, please assign it.");
            }
            else if (system.anim.runtimeAnimatorController == null)
            {
                errorId = 1;
                NormalError("Your animator component is missing an 'Animator Controller'. Your AI won't work properly, please create an animator controller!");
            }
#if !Breeze_AI_Pathfinder_Enabled
            else if (!NavMesh.SamplePosition(system.transform.position, out _, 5f, NavMesh.AllAreas))
            {
                errorId = 2;
                NormalError("Either your scene don't have a valid navmesh area baked, or your AI object isn't placed on a navmesh area. Your AI won't be able to move.");
            }  
#endif
            else if (system.WanderType == BreezeEnums.AIWanderingType.Waypoint && system.Waypoints.Count <= 1)
            {
                errorId = 3;
                NormalError("The AI object doesn't have enough waypoints. Please navigate to the 'Waypoint Editor' to create more.");
            }
            else if (system.DetectionLayers.value.Equals(0))
            {
                errorId = 4;
                NormalError("Detection layers cannot be 'Nothing', please add detection layers.");
            }
            else if (system.UseAimIK && system.BonesToUse.Count <= 0)
            {
                errorId = 5;
                NormalError("To use AIM IK for your AI object, please add 'Bones To Use' in the detection tab.");
            }
            else if (system.DeathMethod == BreezeEnums.AIDeathType.Ragdoll && system.GetComponentsInChildren<Rigidbody>().Length <= 0)
            {
                errorId = 6;
                NormalError("To use ragdoll as AI's death method, you need to create ragdoll bones for your object.");
            }
            else
            {
                system.hasErrors = false;
                GUI.backgroundColor = new Color(0, 1, 0f, 0.19f);
                EditorGUILayout.HelpBox("Everything looks ready. There are no errors on your AI setup.", MessageType.Info);
                errorAvailable = false;
            }
            GUI.backgroundColor = Color.white;

            if (errorAvailable)
            {
                if (GUILayout.Button("FIX NOW", GUILayout.Width(65f), GUILayout.Height(22f)))
                {
                    fixButton(errorId);
                }
            }
            
            //Warnings
            if (system.nav.radius.Equals(0.5f))
            {
                WarningError("Make sure to adjust your navmeshAgent radius to fit your AI object.", warningMessages);
            }
            if (!Application.isPlaying && system.GetComponent<CapsuleCollider>() != null && system.GetComponent<CapsuleCollider>().radius.Equals(0.5f))
            {
                WarningError("Make sure to adjust your collider radius to fit your AI object.", warningMessages);   
            }
            if(system.anim.avatar == null)
            {
                WarningError("Make sure to assign an proper avatar mask to your animator component for the best performance.", warningMessages);
            }


            if (WarningNumber > warningMessages.Count - 1)
                WarningNumber = warningMessages.Count - 1;

            if (WarningNumber < 0)
                WarningNumber = 0;

            if (warningMessages.Count > 0)
            {
                EditorGUILayout.Space(4.5f);
                GUI.backgroundColor = new Color(1, 1, 0f, 0.275f);
                EditorGUILayout.HelpBox(warningMessages[WarningNumber], MessageType.Warning);
                GUI.backgroundColor = Color.white;
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("FIX NOW", GUILayout.Width(65f), GUILayout.Height(22f)))
                {
                    fixButton(WarningNumber, false);
                }
                
                
                GUILayout.FlexibleSpace();
                EditorGUI.BeginDisabledGroup(WarningNumber <= 0);
                if (GUILayout.Button("<", GUILayout.Width(27f), GUILayout.Height(19f)))
                {
                    WarningNumber--;
                }
                EditorGUI.EndDisabledGroup();
                
                EditorGUILayout.Space(2.5f);
                
                EditorGUI.BeginDisabledGroup(WarningNumber == warningMessages.Count - 1);
                if (GUILayout.Button(">", GUILayout.Width(27f), GUILayout.Height(19f)))
                {
                    WarningNumber++;
                }
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Space(0.3f);
                EditorGUILayout.EndHorizontal();
            }
            
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space(6);

            //Toolbar
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(10);
            var ToolbarStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 35,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            TabNumber = GUILayout.SelectionGrid(TabNumber, Buttons, 4, ToolbarStyle, GUILayout.Height(68), GUILayout.Width(EditorGUIUtility.currentViewWidth-50));
            EditorGUILayout.EndVertical();
            PlayerPrefs.SetInt(system.gameObject.GetInstanceID() + " tab Core", TabNumber);
            
            //[Variables]

            //Stats Tab
            if (TabNumber == 0)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("STATS", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("StartingHealth"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MaximumHealth"));
                EditorGUILayout.Space(1.25f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("RegenerateHealth"));
                if (system.RegenerateHealth)
                {
                    EditorGUILayout.Space(0.75f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RegenerateCondition"));
                    EditorGUILayout.Space(0.75f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RegenerateFrequency"));
                    EditorGUILayout.Space(0.75f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RegenerateAmount"));
                    EditorGUILayout.Space(0.75f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RegenerateStartDelay"));
                    EditorGUILayout.Space(0.75f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RegenerateHealthLimit"));
                }
                //Control Maximum Health
                if (system.StartingHealth > system.MaximumHealth)
                {
                    system.StartingHealth = system.MaximumHealth;
                }
                EditorGUILayout.EndVertical();
            }
            
            //General Settings Tab
            if (TabNumber == 1)
            {
                //Settings
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("SETTINGS", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AIBehaviour"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AIConfidence"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("WeaponType"));
                if (system.WeaponType != BreezeEnums.WeaponType.Unarmed)
                {
                    EditorGUILayout.Space(0.75f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("UseEquipSystem"));
                }
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("WanderType"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DeathMethod"));
                EditorGUILayout.Space(1.25f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DestroyAfterDeath"));
                if (system.DestroyAfterDeath)
                {
                    EditorGUILayout.Space(0.75f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("DestroyDelay"));
                }
                EditorGUILayout.Space(24f);
                
                //Movement
                EditorGUILayout.LabelField("MOVEMENT", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("RotationSpeed"));
                EditorGUILayout.Space(1.25f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("UseRootMotion"));
                if (!system.UseRootMotion)
                {
                    EditorGUILayout.Space(0.75f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("WalkSpeed"));
                    EditorGUILayout.Space(0.75f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RunSpeed"));
                    EditorGUILayout.Space(0.75f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("WalkBackwardsSpeed"));
                }
                EditorGUILayout.Space(1.4f);
                EditorGUI.BeginDisabledGroup(system.WanderType != BreezeEnums.AIWanderingType.Patrol);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PatrolRadius"));
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Space(1.25f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("FleeDistance"));
                EditorGUILayout.Space(1.25f);
                
                if (system.weaponHubInitialized)
                {
                    GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                    EditorGUILayout.LabelField("[Edit This On Weapon Hub Component]", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                }

                EditorGUI.BeginDisabledGroup(system.weaponHubInitialized);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("EnemyTooCloseDistance"));
                EditorGUI.EndDisabledGroup();
                
                EditorGUILayout.Space(1.25f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BackawayMultiplier"));
                EditorGUILayout.Space(1.25f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ForceWalkDistance"));
                EditorGUILayout.Space(1.60f);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MinIdleLength"), new GUIContent("Idle Length:                                                Min "));
                GUILayout.Label("    Max");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxIdleLength"), GUIContent.none ,true,GUILayout.Width(59));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(24f);
                
                //Optimization
                EditorGUILayout.LabelField("OPTIMIZATION", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("UseLodOptimization"));
                EditorGUI.BeginDisabledGroup(system.UseLodOptimization == BreezeEnums.YesNo.No);
                EditorGUILayout.Space(1.25f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("LodRenderer"));
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndVertical();
            }
            
            //Combat Tab
            if (TabNumber == 2)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("COMBAT", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                if (system.WeaponType != BreezeEnums.WeaponType.Shooter)
                {
                    system.ShouldChaseTarget = true;
                    EditorGUI.BeginDisabledGroup(true);
                }
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ShouldChaseTarget"));
                EditorGUILayout.Space(0.75f);
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("HitReactionOnAttack"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("HitReactionPossibility"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("GetHitTolerance"));
                EditorGUILayout.Space(0.75f);

                if (system.weaponHubInitialized)
                {
                    GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                    EditorGUILayout.LabelField("[Edit This On Weapon Hub Component]", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                }

                EditorGUI.BeginDisabledGroup(system.weaponHubInitialized);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AttackDistance"));
                EditorGUI.EndDisabledGroup();
                
                EditorGUILayout.Space(0.75f);
                
                if (system.weaponHubInitialized)
                {
                    GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                    EditorGUILayout.LabelField("[Edit This On Weapon Hub Component]", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                }
                
                EditorGUI.BeginDisabledGroup(system.weaponHubInitialized);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ExtendedAttackDistance"));
                EditorGUI.EndDisabledGroup();
                
                EditorGUILayout.Space(1.60f);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MinAttackDelay"), new GUIContent("Attack Delay:                                           Min "));
                GUILayout.Label("    Max");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MaxAttackDelay"), GUIContent.none ,true,GUILayout.Width(59));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(1.60f);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("minAlertLength"), new GUIContent("Stay Alerted Length:                              Min "));
                GUILayout.Label("    Max");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxAlertLength"), GUIContent.none ,true,GUILayout.Width(59));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(1f);
                
                if (system.WeaponType == BreezeEnums.WeaponType.Shooter)
                {
                    system.UseBlockingSystem = false;
                    GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                    EditorGUILayout.LabelField("[Not Available For Shooters]", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                }
                EditorGUI.BeginDisabledGroup(system.WeaponType == BreezeEnums.WeaponType.Shooter);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("UseBlockingSystem"));
                if (system.UseBlockingSystem)
                {
                    EditorGUILayout.Space(0.75f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("BlockAnimationPossibility"));
                    EditorGUILayout.Space(0.75f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("CanBlockWhileAttacking"));
                }
                EditorGUI.EndDisabledGroup();
                
                EditorGUILayout.Space(14);
                EditorGUILayout.LabelField("EFFECTS", EditorStyles.boldLabel);
                EditorGUILayout.Space(7);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ParticlesList"));
                
                EditorGUILayout.Space(14);
                EditorGUILayout.LabelField("OTHERS", EditorStyles.boldLabel);
                EditorGUILayout.Space(7);
                GUI.backgroundColor = new Color(0.0f, 1.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("[The position for enemy AIM IK to lock on]", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("HitPosition"));

                EditorGUILayout.EndVertical();
            }
            
            //Detection Tab
            if (TabNumber == 3)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("DETECTION", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DetectionType"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DetectionLock"), new GUIContent("Target Decision"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DetectionFrequency"));
                EditorGUILayout.Space(0.75f);
                
                if (system.weaponHubInitialized)
                {
                    GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                    EditorGUILayout.LabelField("[Edit This On Weapon Hub Component]", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                }

                EditorGUI.BeginDisabledGroup(system.weaponHubInitialized);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DetectionDistance"));
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.Space(0.75f);
                EditorGUI.BeginDisabledGroup(system.DetectionType != BreezeEnums.DetectionType.LineOfSight);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DetectionAngle"));
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DetectionLayers"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ObstacleLayers"));
                EditorGUILayout.Space(1.5f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("HeadTransform"));
                EditorGUILayout.Space(6f);
                GUILayout.BeginHorizontal();
                int iButtonWidth = 165;
                GUILayout.Space((Screen.width/2 - iButtonWidth /2) + 14.25f);
                if (GUILayout.Button("Auto Find Head Transform", GUILayout.Width(iButtonWidth), GUILayout.Height(23)))
                {
                    foreach (Transform t in system.transform.GetComponentsInChildren<Transform>())
                    {
                        if (t.name.Contains("head") || t.name.Contains("Head") || t.name.Contains("HEAD"))
                        {
                            if (t.GetComponent<MeshRenderer>() == null && t.GetComponent<SkinnedMeshRenderer>() == null)
                            {
                                system.HeadTransform = t;
                            }
                        }
                    }

                    if (system.HeadTransform == null)
                    {
                        Debug.LogError("[Breeze AI] Head Transform Couldn't Be Automatically Found!");
                    }
                    else
                    {
                        Debug.Log("[Breeze AI] Head Transform Was Found! <b> {" + system.HeadTransform.name + "} </b>");
                    }
                }
                GUILayout.EndHorizontal();
                
                EditorGUILayout.Space(14);
                EditorGUILayout.LabelField("MISC", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("NotifyCloseUnits"));
                EditorGUILayout.Space(0.75f);
                EditorGUI.BeginDisabledGroup(system.NotifyCloseUnits == BreezeEnums.YesNo.No);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("NotifyDistanceLimit"));
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("SoundDetectionLimit"));
                EditorGUI.EndDisabledGroup();
                
                EditorGUILayout.EndVertical();
            }
            
            //Factions Tab
            if (TabNumber == 4)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("FACTIONS", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                GUI.backgroundColor = new Color(0f, 1f, 0f, 0.29f);
                EditorGUILayout.LabelField("Player object needs the component: 'BreezePlayer' attached, to be detected!", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space(0.25f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CurrentAIFaction"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AIPlayerBehaviour"));
                EditorGUILayout.Space(2f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AIFactionsList"));
                EditorGUILayout.Space(2f);
                EditorGUI.BeginChangeCheck();
                var newTag = EditorGUILayout.TagField("Breeze AI Tag", serializedObject.FindProperty("BreezeTag").stringValue);
                if (EditorGUI.EndChangeCheck())
                    serializedObject.FindProperty("BreezeTag").stringValue = newTag;
                EditorGUILayout.Space(0.25f);
                GUI.backgroundColor = new Color(1f, 1f, 0f, 0.49f);
                EditorGUILayout.LabelField("This tag should be same for all of your AI objects, It will be used to detect other AI objects!", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space(0.75f);
                var newPlayerTag = EditorGUILayout.TagField("Player Tag", serializedObject.FindProperty("PlayerTag").stringValue);
                if (EditorGUI.EndChangeCheck())
                    serializedObject.FindProperty("PlayerTag").stringValue = newPlayerTag;
                EditorGUILayout.Space(0.25f);
                GUI.backgroundColor = new Color(1f, 1f, 0f, 0.49f);
                EditorGUILayout.LabelField("This tag should be your Player's tag, and it's really important for the detection system!", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                
                EditorGUILayout.EndVertical();
            }
            
            //Companion
            if (TabNumber == 5)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("COMPANION", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AttackBehaviour"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("FriendlyDamageThreshold"), new GUIContent("Friendly Damage Threshold %"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CFollowingDistance"), new GUIContent("Following Distance"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CSprintFollowDistance"), new GUIContent("Sprint Follow Distance"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CTooCloseDistance"), new GUIContent("Too Close Distance"));
                EditorGUILayout.EndVertical();
            }
            
            //IK Tab
            if (TabNumber == 6)
            {
                if (system.WeaponType != BreezeEnums.WeaponType.Shooter)
                {
                    EditorGUILayout.Space(2);
                    GUI.backgroundColor = new Color(1.0f, 0.0f, 0.0f, 0.25f);
                    EditorGUILayout.LabelField("[Inverse Kinematics Is Only Available For Shooter AI]", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                    EditorGUI.BeginDisabledGroup(true);
                }
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("Hand IK", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                GUI.backgroundColor = new Color(0.0f, 1.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("[Enable Hand IK On Your Weapon Script]", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space(0.25f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OnlyLeftHand"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("HandSmoothAmount"));
                EditorGUILayout.Space(14);
                
                EditorGUILayout.LabelField("Aim IK", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                GUI.backgroundColor = new Color(0.0f, 1.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("[Enable Aim IK On Your Weapon Script]", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AimSmoothAmount"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BonesToUse"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AimOffset"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AngleLimit"));
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndVertical();
            }

            //Debug Tab
            if (TabNumber == 7)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("STATS", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CurrentHealthDebug"), new GUIContent("Current Health:"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("RegeneratingHealthDebug"), new GUIContent("Regenerating Health:"));
                EditorGUI.EndDisabledGroup();
                
                EditorGUILayout.Space(12);
                EditorGUILayout.LabelField("COMBAT", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CurrentAIState"), new GUIContent("Current AI State:"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("IsAlertedDebug"), new GUIContent("AI Alerted:"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("IsAttackingDebug"), new GUIContent("AI Attacking:"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("VisibleTargetsDebug"), new GUIContent("Visible Targets"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CurrentTargetDebug"), new GUIContent("Current Target:"));
                if (system.CurrentTargetDebug != null)
                {
                    EditorGUILayout.Space(0.75f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("TargetDistanceDebug"), new GUIContent("Target Distance:"));   
                    EditorGUILayout.Space(0.75f);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("CurrentTargetTypeDebug"), new GUIContent("Current Target Type:"));   
                }
                EditorGUI.EndDisabledGroup();
                
                EditorGUILayout.Space(12);
                EditorGUILayout.LabelField("COMPONENTS", EditorStyles.boldLabel);
                EditorGUILayout.Space(2);
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AnimatorDebug"), new GUIContent("Animator Component:"));
                EditorGUILayout.Space(0.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("NavmeshAgentDebug"), new GUIContent("NavmeshAgent Component:"));
                EditorGUILayout.Space(0.75f);
                if (system.ColliderDebug != null)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ColliderDebug"), new GUIContent("Collider Component:"));
                    EditorGUILayout.Space(0.75f);   
                }
                EditorGUILayout.PropertyField(serializedObject.FindProperty("SystemDebug"), new GUIContent("System Component:"));
                EditorGUILayout.Space(1.25f);

                EditorGUILayout.EndVertical();
            }
            
            //Apply Changes
            serializedObject.ApplyModifiedProperties();
        }

        private void fixButton(int index, bool error = true)
        {
            if (error)
            {
                switch (index)
                {
                    case 0:
                        int id = EditorUtility.DisplayDialogComplex("Solution",
                            "Head Transform is a must for detection system, please assign your AI object's head gameObject to the required field.",
                            "Okay.", "Close.", "How?");

                        switch (id)
                        {
                            case 0:
                                TabNumber = 3;
                                break;
                            case 2:
                                Application.OpenURL("https://docs.breezeassets.net/error-solutions/breeze-system#head-transform-is-null");
                                break;
                        }
                        break;
                    
                    case 1: 
                        id = EditorUtility.DisplayDialogComplex("Solution",
                            "Animator controller can't be null on your animator component. It's required for animations to be played, please create one!",
                            "Okay.", "Close.", "How?");

                        switch (id)
                        {
                            case 2:
                                Application.OpenURL("https://docs.breezeassets.net/error-solutions/breeze-system#animator-controller-is-null");
                                break;
                        }
                        break;
                    
                    case 2: 
                        id = EditorUtility.DisplayDialogComplex("Solution",
                            "AI objects move on navmesh areas, and your AI can't find a proper navmesh area to use. Please make sure that your scene has a valid navmesh area baked.",
                            "Okay.", "Close.", "How?");

                        switch (id)
                        {
                            case 2:
                                Application.OpenURL("https://docs.breezeassets.net/error-solutions/breeze-system#navmesh-area-isnt-available");
                                break;
                        }
                        break;
                    
                    case 3:
                        id = EditorUtility.DisplayDialogComplex("Solution",
                            "To use waypoint as your AI's wandering type, you need to have atleast 2 waypoints. To create more, please navigate to 'Waypoint Editor' window.",
                            "Okay.", "Close.", "How?");
                        
                        switch (id)
                        {
                            case 2:
                                Application.OpenURL("https://docs.breezeassets.net/error-solutions/breeze-system#ai-doesnt-have-enough-waypoints");
                                BreezeWaypointEditor.ShowWindow();
                                break;
                        }
                        break;
                    
                    case 4:
                        id = EditorUtility.DisplayDialogComplex("Solution",
                            "Detection system requires layers to check. Please add AI layers to the detection layers field for the detection system to work properly.",
                            "Okay.", "Close.", "How?");
                        
                        switch (id)
                        {
                            case 0:
                                TabNumber = 3;
                                break;
                            case 2:
                                Application.OpenURL("https://docs.breezeassets.net/error-solutions/breeze-system#detection-layers-cannot-be-nothing");
                                break;
                        }
                        break;
                    
                    case 5:
                        id = EditorUtility.DisplayDialogComplex("Solution",
                            "AIM IK system requires bones to work. Please add 'Bones To Use' in the required field for the aim IK to work properly.",
                            "Okay.", "Close.", "How?");
                        
                        switch (id)
                        {
                            case 0:
                                TabNumber = 6;
                                break;
                            case 2:
                                Application.OpenURL("https://docs.breezeassets.net/error-solutions/breeze-system#bones-to-use-cannot-be-null-for-aim-ik");
                                break;
                        }
                        break;
                    
                    case 6:
                        id = EditorUtility.DisplayDialogComplex("Solution",
                            "Ragdoll is a system built-in for Unity Engine. You must create an ragdoll structure in order to use the 'Ragoll Death Method'.",
                            "Okay.", "Close.", "How?");
                        
                        switch (id)
                        {
                            case 2:
                                Application.OpenURL("https://docs.breezeassets.net/error-solutions/breeze-system#you-need-to-create-ragdoll-to-use-that-death-method");
                                break;
                        }
                        break;
                }   
            }
            else
            {
                switch (index)
                {
                    case 0:
                        int id = EditorUtility.DisplayDialogComplex("Solution",
                            "Adjusting AI navmeshAgent radius for your AI object can help imporving your AI's movement quality.",
                            "Okay.", "Close.", "How?");

                        switch (id)
                        {
                            case 2:
                                Application.OpenURL("https://docs.breezeassets.net/error-solutions/breeze-system#navmeshagent-radius-should-fit-your-ai-object");
                                break;
                        }
                        break;
                    case 1: 
                        id = EditorUtility.DisplayDialogComplex("Solution",
                            "Adjusting AI collider radius for your AI object can help imporving your AI's combat performance.",
                            "Okay.", "Close.", "How?");

                        switch (id)
                        {
                            case 2:
                                Application.OpenURL("https://docs.breezeassets.net/error-solutions/breeze-system#capsule-collider-radius-should-fit-your-ai-object");
                                break;
                        } 
                        break;
                    case 2: 
                        id = EditorUtility.DisplayDialogComplex("Solution",
                            "Assigning an avatar mask to your animator component can improve your AI's animation quality.",
                            "Okay.", "Close.", "How?");

                        switch (id)
                        {
                            case 2:
                                Application.OpenURL("https://docs.breezeassets.net/error-solutions/breeze-system#make-sure-to-have-an-avatar-mask-assigned");
                                break;
                        }
                        break;
                }
            }
        }

        private void WarningError(string st, List<string> lt)
        {
            if(!lt.Contains(st))
                lt.Add(st);
        }

        private void NormalError(string st)
        {
            system.hasErrors = true;
            GUI.backgroundColor = new Color(1, 0, 0f, 0.275f);
            EditorGUILayout.HelpBox(st, MessageType.Error);
        }
    }
}

#endif