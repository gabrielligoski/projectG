using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Breeze.Core
{
    [System.Serializable]
    [CustomEditor(typeof(BreezeAnimations))]
    public class BreezeAnimationsEditor : Editor
    {
        private BreezeAnimations system;

        //Toolbar Variables
        private int TabNumber;
        private int WarningNumber;
        private bool TabChanged;
        private bool ErrorsOccur = false;
        GUIContent[] Buttons = { new GUIContent(" Stationary \n Animations"), new GUIContent(" Base \n Movement"), new GUIContent(" Combat \n Movement"), new GUIContent(" Combat \n Animations") };

        private void OnEnable()
        {
            //Get The System
            system = (BreezeAnimations)target;
        }

        public override void OnInspectorGUI()
        {
            if (!TabChanged)
            {
                TabChanged = true;
                if (PlayerPrefs.HasKey(system.gameObject.GetInstanceID() + " tab"))
                    TabNumber = PlayerPrefs.GetInt(system.gameObject.GetInstanceID() + " tab");
            }
            
            List<string> warningMessages = new List<string>();
            var errorAvailable = true;
            var errorId = 0;
            
            //Checker
            EditorGUILayout.Space(4);
            
            //Errors 
            if (system.BaseIdleAnims.Count <= 0 || system.BaseIdleAnims.Where(x => x == null).ToList().Count > 0)
            {
                errorId = 0;
                NormalError("The 'Base Idle Anims' cannot be null, please assign animations.");
            }
            else if (system.CombatIdleAnims.Count <= 0 || system.CombatIdleAnims.Where(x => x == null).ToList().Count > 0)
            {
                errorId = 1;
                NormalError("The 'Combat Idle Anims' cannot be null, please assign animations.");
            }
            else if (system.WalkFwdAnim == null)
            {
                errorId = 2;
                NormalError("The 'Walk Forward Anim' cannot be null, please assign an animation.");
            }
            else if (system.WalkRAnim == null)
            {
                errorId = 3;
                NormalError("The 'Walk Right Anim' cannot be null, please assign an animation.");
            }
            else if (system.WalkLAnim == null)
            {
                errorId = 4;
                NormalError("The 'Walk Left Anim' cannot be null, please assign an animation.");
            }
            else if (system.RunFwdAnim == null)
            {
                errorId = 5;
                NormalError("The 'Run Forward Anim' cannot be null, please assign an animation.");
            }
            else if (system.RunRAnim == null)
            {
                errorId = 6;
                NormalError("The 'Run Right Anim' cannot be null, please assign an animation.");
            }
            else if (system.RunLAnim == null)
            {
                errorId = 6;
                NormalError("The 'Run Left Anim' cannot be null, please assign an animation.");
            }
            else if (system.CWalkFwdAnim == null)
            {
                errorId = 7;
                NormalError("The 'Combat Walk Forward Anim' cannot be null, please assign an animation.");
            }
            else if (system.CWalkRAnim == null)
            {
                errorId = 8;
                NormalError("The 'Combat Walk Right Anim' cannot be null, please assign an animation.");
            }
            else if (system.CWalkLAnim == null)
            {
                errorId = 9;
                NormalError("The 'Combat Walk Left Anim' cannot be null, please assign an animation.");
            }
            else if (system.CRunFwdAnim == null)
            {
                errorId = 10;
                NormalError("The 'Combat Run Forward Anim' cannot be null, please assign an animation.");
            }
            else if (system.CRunRAnim == null)
            {
                errorId = 11;
                NormalError("The 'Combat Run Right Anim' cannot be null, please assign an animation.");
            }
            else if (system.CRunLAnim == null)
            {
                errorId = 12;
                NormalError("The 'Combat Run Left Anim' cannot be null, please assign an animation.");
            }
            else if (system.System.AIConfidence != BreezeEnums.AIConfidence.Coward && (system.AttackAnims.Count <= 0 || system.AttackAnims.Where(x => x.AttackAnim == null).ToList().Count > 0))
            {
                errorId = 13;
                NormalError("The 'Attack Animations' cannot be null. To use the attack system, please assign animations.");
            }
            else if (system.System.HitReactionPossibility > 0 && system.System.AIConfidence != BreezeEnums.AIConfidence.Coward && (system.HitAnims.Count <= 0 || system.HitAnims.Where(x => x == null).ToList().Count > 0))
            {
                errorId = 15;
                NormalError("The 'Hit Reaction Animations' cannot be null. To use the hit reaction system, please assign animations.");
            }
            else if (system.System.UseEquipSystem && system.System.WeaponType != BreezeEnums.WeaponType.Unarmed && (system.EquipAnim == null || system.UnEquipAnim == null))
            {
                errorId = 16;
                NormalError("The 'Equip Animations' cannot be null. To use the equip system, please assign animations.");
            }
            else if (system.System.UseBlockingSystem && (system.BlockAnims.Count <= 0 || system.BlockAnims.Where(x => x == null).ToList().Count > 0))
            {
                errorId = 17;
                NormalError("The 'Block Animations' cannot be null. To use the blocking system, please assign animations.");
            }
            else if (system.System.DeathMethod == BreezeEnums.AIDeathType.Animation && (system.DeathAnims.Count <= 0 || system.DeathAnims.Where(x => x == null).ToList().Count > 0))
            {
                errorId = 18;
                NormalError("The 'Death Animations' cannot be null, please assign animations.");
            }
            else if (!Application.isPlaying && system.System.WeaponType != BreezeEnums.WeaponType.Shooter && system.AttackAnims.Where(x => x.AttackAnim.events.Length <= 0 || x.AttackAnim.events.ToList().Where(y => y.functionName.Equals("BreezeAttackEvent")).ToList().Count <= 0).ToList().Count > 0)
            {
                errorId = 19;
                NormalError("Make sure to add the 'BreezeAttackEvent' animation event to your attack animations, or your AI won't damage.");
            }
            else if (system.System.WeaponType == BreezeEnums.WeaponType.Shooter && system.System.UseHandIK && system.ReloadAnim != null && system.ReloadAnim.events.ToList().Where(y => y.functionName.Equals("EnableHandIK")).ToList().Count <= 0)
            {
                errorId = 20;
                NormalError("Make sure to add the 'EnableHandIK' animation event to your reload animation, or IK system won't work.");
            }
            else if (system.System.UseEquipSystem && system.EquipAnim != null && system.EquipAnim.events.ToList().Where(y => y.functionName.Equals("BreezeEquip")).ToList().Count <= 0)
            {
                errorId = 21;
                NormalError("Make sure to add the 'BreezeEquip' animation event to your equip animation, or your weapon won't work.");
            }
            else if (system.System.UseEquipSystem && system.UnEquipAnim != null && system.UnEquipAnim.events.ToList().Where(y => y.functionName.Equals("BreezeHolster")).ToList().Count <= 0)
            {
                errorId = 22;
                NormalError("Make sure to add the 'BreezeHolster' animation event to your holster animation, or your weapon won't work.");
            }
            else if (system.System.UseEquipSystem && system.EquipAnim != null && system.System.UseHandIK && system.EquipAnim.events.ToList().Where(y => y.functionName.Equals("EnableHandIK")).ToList().Count <= 0)
            {
                errorId = 23;
                NormalError("Make sure to add the 'EnableHandIK' animation event to your equip animation, or IK system won't work.");
            }
            else if (system.System.UseEquipSystem && system.UnEquipAnim != null && system.System.UseHandIK && system.UnEquipAnim.events.ToList().Where(y => y.functionName.Equals("DisableHandIK")).ToList().Count <= 0)
            {
                errorId = 24;
                NormalError("Make sure to add the 'DisableHandIK' animation event to your holster animation, or IK system won't work.");
            }
            else
            {
                GUI.backgroundColor = new Color(0, 1, 0f, 0.19f);
                EditorGUILayout.HelpBox("Everything looks ready. There are no errors on your AI setup.", MessageType.Info);
                errorAvailable = false;
            }
            GUI.backgroundColor = Color.white;

            if (errorAvailable)
            {
                if(errorId < 19)
                    EditorGUI.BeginDisabledGroup(true);
                
                if (GUILayout.Button("FIX NOW", GUILayout.Width(65f), GUILayout.Height(22f)))
                {
                    fixButton(errorId);
                }
                EditorGUI.EndDisabledGroup();
            }
            
            //Warnings
            if (system.WalkBwdAnim == null)
            {
                WarningError("In order to use the 'Back Away' function, please assign an 'Walk Backwards' animation.", warningMessages);   
            }
            if (system.CWalkBwdAnim == null)
            {
                WarningError("In order to use the 'Combat Back Away' function, please assign an 'Combat Walk Backwards' animation.", warningMessages);   
            }
            if (system.System.WeaponType == BreezeEnums.WeaponType.Shooter && system.ReloadAnim == null)
            {
                WarningError("Please assign a reload animation so that your shooter AI can reload.", warningMessages);   
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
                EditorGUI.BeginDisabledGroup(true);
                if (GUILayout.Button("FIX NOW", GUILayout.Width(65f), GUILayout.Height(22f)))
                {
                    fixButton(WarningNumber, false);
                }
                EditorGUI.EndDisabledGroup();
                
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
            GUIStyle ToolbarStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 35,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            TabNumber = GUILayout.SelectionGrid(TabNumber, Buttons, 4, ToolbarStyle, GUILayout.Height(68), GUILayout.Width(EditorGUIUtility.currentViewWidth-50));
            EditorGUILayout.EndVertical();
            PlayerPrefs.SetInt(system.gameObject.GetInstanceID() + " tab", TabNumber);
            
            //[Variables]

            if (TabNumber == 0)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("IDLE ANIMATIONS", EditorStyles.boldLabel);
                EditorGUILayout.Space(12);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BaseIdleAnims"));
                EditorGUILayout.Space(12);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CombatIdleAnims"));
                
                EditorGUILayout.Space(15);
                EditorGUILayout.LabelField("EMOTE ANIMATIONS", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Coming Soon...", EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
            }
            
            if (TabNumber == 1)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("MOVEMENT ANIMATIONS", EditorStyles.boldLabel);
                EditorGUILayout.Space(12);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("WalkFwdAnim"), new GUIContent("Walk Forward"));
                EditorGUILayout.Space(0.25f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MirrorWalkFwdAnim"), new GUIContent("Mirror Walk Forward"));
                EditorGUILayout.Space(2f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("WalkRAnim"), new GUIContent("Walk Right"));
                EditorGUILayout.Space(0.25f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MirrorWalkRAnim"), new GUIContent("Mirror Walk Right"));
                EditorGUILayout.Space(2f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("WalkLAnim"), new GUIContent("Walk Left"));
                EditorGUILayout.Space(0.25f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MirrorWalkLAnim"), new GUIContent("Mirror Walk Left"));
                EditorGUILayout.Space(2f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("WalkBwdAnim"), new GUIContent("Walk Backward"));
                EditorGUILayout.Space(0.25f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MirrorWalkBwdAnim"), new GUIContent("Mirror Walk Backward"));
                EditorGUILayout.Space(12);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("RunFwdAnim"), new GUIContent("Run Forward"));
                EditorGUILayout.Space(0.25f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MirrorRunFwdAnim"), new GUIContent("Mirror Run Forward"));
                EditorGUILayout.Space(2f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("RunRAnim"), new GUIContent("Run Right"));
                EditorGUILayout.Space(0.25f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MirrorRunRAnim"), new GUIContent("Mirror Run Right"));
                EditorGUILayout.Space(2f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("RunLAnim"), new GUIContent("Run Left"));
                EditorGUILayout.Space(0.25f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MirrorRunLAnim"), new GUIContent("Mirror Run Left"));
                EditorGUILayout.EndVertical();
            }

            if (TabNumber == 2)
            {
                EditorGUILayout.Space(10);
                if (GUILayout.Button("APPLY BASE MOVEMENT ANIMATIONS", GUILayout.Height(26.25f)))
                {
                    if (EditorUtility.DisplayDialog("Warning!",
                            "This action will override your current 'Combat Movement' animations.", "Continue",
                            "Cancel"))
                    {
                        Undo.RegisterCompleteObjectUndo(system, "System Changed");
                        EditorUtility.SetDirty(system);
                        system.Copy();
                    }
                }
                
                EditorGUILayout.Space(6);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("COMBAT MOVEMENT ANIMATIONS", EditorStyles.boldLabel);
                EditorGUILayout.Space(14);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CWalkFwdAnim"), new GUIContent("Combat Walk Forward"));
                EditorGUILayout.Space(0.25f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CMirrorWalkFwdAnim"), new GUIContent("Mirror Walk Forward"));
                EditorGUILayout.Space(2f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CWalkRAnim"), new GUIContent("Combat Walk Right"));
                EditorGUILayout.Space(0.25f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CMirrorWalkRAnim"), new GUIContent("Mirror Walk Right"));
                EditorGUILayout.Space(2f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CWalkLAnim"), new GUIContent("Combat Walk Left"));
                EditorGUILayout.Space(0.25f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CMirrorWalkLAnim"), new GUIContent("Mirror Walk Left"));
                EditorGUILayout.Space(2f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CWalkBwdAnim"), new GUIContent("Combat Walk Backwards"));
                EditorGUILayout.Space(0.25f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CMirrorWalkBwdAnim"), new GUIContent("Mirror Walk Backwards"));
                EditorGUILayout.Space(12);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CRunFwdAnim"), new GUIContent("Combat Run Forward"));
                EditorGUILayout.Space(0.25f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CMirrorRunFwdAnim"), new GUIContent("Mirror Run Forward"));
                EditorGUILayout.Space(2f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CRunRAnim"), new GUIContent("Combat Run Right"));
                EditorGUILayout.Space(0.25f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CMirrorRunRAnim"), new GUIContent("Mirror Run Right"));
                EditorGUILayout.Space(2f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CRunLAnim"), new GUIContent("Combat Run Left"));
                EditorGUILayout.Space(0.25f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CMirrorRunLAnim"), new GUIContent("Mirror Run Left"));
                EditorGUILayout.EndVertical();
            }
            
            if (TabNumber == 3)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("COMBAT ANIMATIONS", EditorStyles.boldLabel);
                EditorGUILayout.Space(15);
                
                if (system.System.AIConfidence == BreezeEnums.AIConfidence.Coward)
                {
                    GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                    EditorGUILayout.LabelField("[AI Confidence Is Coward] (Change It On 'General Settings' Tab)", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                    EditorGUI.BeginDisabledGroup(true);
                }
                else
                {
                    GUI.backgroundColor = new Color(0, 1, 0f, 0.19f);
                    EditorGUILayout.LabelField("[Attack System Enabled]", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                }
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AttackAnims"), new GUIContent("Attack Animations"));
                EditorGUILayout.Space(10);
                EditorGUI.EndDisabledGroup();

                if (system.System.HitReactionPossibility <= 0)
                {
                    GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                    EditorGUILayout.LabelField("[Hit Reactions Disabled] (Change It On 'Combat Settings' Tab)", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                    EditorGUI.BeginDisabledGroup(true);
                }
                else
                {
                    GUI.backgroundColor = new Color(0, 1, 0f, 0.19f);
                    EditorGUILayout.LabelField("[Hit Reactions Enabled]", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                }
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("HitAnims"), new GUIContent("Hit Reaction Animations"));
                EditorGUILayout.Space(10);
                EditorGUI.EndDisabledGroup();

                if (!system.System.UseEquipSystem || system.System.WeaponType == BreezeEnums.WeaponType.Unarmed)
                {
                    GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                    EditorGUILayout.LabelField("[Equip System Disabled] (Enable It On 'General Settings' Tab)", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                    EditorGUI.BeginDisabledGroup(true);
                }
                else
                {
                    GUI.backgroundColor = new Color(0, 1, 0f, 0.19f);
                    EditorGUILayout.LabelField("[Equip System Enabled]", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                }
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("EquipAnim"), new GUIContent("Equip Animation"));
                EditorGUILayout.Space(1);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("UnEquipAnim"), new GUIContent("Un Equip Animation"));
                EditorGUILayout.Space(14);
                EditorGUI.EndDisabledGroup();
                
                if (system.System.WeaponType != BreezeEnums.WeaponType.Shooter)
                {
                    GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                    EditorGUILayout.LabelField("[Only For Shooter AI]", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                    EditorGUI.BeginDisabledGroup(true);
                }
                else
                {
                    GUI.backgroundColor = new Color(0, 1, 0f, 0.19f);
                    EditorGUILayout.LabelField("[Shooter AI Type Enabled]", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                }
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ReloadAnim"), new GUIContent("Reload Animation"));
                EditorGUILayout.Space(10);
                EditorGUI.EndDisabledGroup();
                
                if (!system.System.UseBlockingSystem)
                {
                    GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                    EditorGUILayout.LabelField("[Block System Disabled] (Enable It On 'Combat Settings' Tab)", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                    EditorGUI.BeginDisabledGroup(true);
                }
                else
                {
                    GUI.backgroundColor = new Color(0, 1, 0f, 0.19f);
                    EditorGUILayout.LabelField("[Block Animations Are Enabled]", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                }
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BlockAnims"), new GUIContent("Block Animations"));
                EditorGUILayout.Space(10);
                EditorGUI.EndDisabledGroup();

                if (system.System.DeathMethod == BreezeEnums.AIDeathType.Ragdoll)
                {
                    GUI.backgroundColor = new Color(10f, 0.0f, 0.0f, 0.25f);
                    EditorGUILayout.LabelField("[Rag-doll Death Selected] (Change It On 'General Settings' Tab)", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                    EditorGUI.BeginDisabledGroup(true);
                }
                else
                {
                    GUI.backgroundColor = new Color(0, 1, 0f, 0.19f);
                    EditorGUILayout.LabelField("[Death Animations Are Enabled]", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                }
                
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DeathAnims"), new GUIContent("Death Animations"));
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(10);


            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Import", GUILayout.Height(22), GUILayout.Width(57)))
            {
                system.ImportAnimations();
            }
            if (GUILayout.Button("Export", GUILayout.Height(22), GUILayout.Width(57)))
            {
                system.ExportAnimations();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);


            if (GUILayout.Button("SETUP ANIMATIONS", GUILayout.Height(28)))
            {
                if (ErrorsOccur)
                {
                    EditorUtility.DisplayDialog("WARNING!",
                        "There are some problems with your animation setup, please check the inspector for more info.",
                        "Cancel");
                    return;
                }
                string FilePath = EditorUtility.SaveFilePanelInProject("Save as AnimationController",
                     system.name + " Controller",
                    "overrideController", "Please enter a file name to save the file to");
                
                if (FilePath != string.Empty)
                {
                    string UserFilePath = FilePath;
                    string SourceFilePath = AssetDatabase.GetAssetPath(Resources.Load("Controllers/Breeze " + (system.System.WeaponType == BreezeEnums.WeaponType.Unarmed ? "Unarmed" : "Weapon") + " Controller"));
                    AssetDatabase.CopyAsset(SourceFilePath, UserFilePath);
                    RuntimeAnimatorController runtimeAnimatorController = AssetDatabase.LoadAssetAtPath(UserFilePath, typeof(RuntimeAnimatorController)) as RuntimeAnimatorController;
                    serializedObject.Update();
                    EditorUtility.SetDirty(system);
                    system.Done(runtimeAnimatorController);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
        
        private void fixButton(int index, bool error = true)
        {
            if (error)
            {
                if (index == 19)
                {
                    var id = EditorUtility.DisplayDialogComplex("Solution",
                        "Breeze Attack Event is required for the attack animations. Please configure the animation event.",
                        "Okay.", "Close.", "How?");

                    switch (id)
                    {
                        case 2:
                            Application.OpenURL("https://docs.breezeassets.net/general-topics/animations-setup#attack-animation-event");
                            break;
                    }
                }
                else if (index == 20)
                {
                    var id = EditorUtility.DisplayDialogComplex("Solution",
                        "Breeze Attack Event is required for the attack animations. Please configure the animation event.",
                        "Okay.", "Close.", "How?");

                    switch (id)
                    {
                        case 2:
                            Application.OpenURL("https://docs.breezeassets.net/general-topics/animations-setup#attack-animation-event");
                            break;
                    }
                }
                else if (index == 21)
                {
                    var id = EditorUtility.DisplayDialogComplex("Solution",
                        "Breeze Equip animation event is required for the equip animation. This animation event should be configured correctly.",
                        "Okay.", "Close.", "How?");

                    switch (id)
                    {
                        case 2:
                            Application.OpenURL("https://docs.breezeassets.net/general-topics/animations-setup#equip-system-animation-events");
                            break;
                    }
                }
                else if (index == 22)
                {
                    var id = EditorUtility.DisplayDialogComplex("Solution",
                        "Breeze un equip animation event is required for the holster animation. This animation event should be configured correctly.",
                        "Okay.", "Close.", "How?");

                    switch (id)
                    {
                        case 2:
                            Application.OpenURL("https://docs.breezeassets.net/general-topics/animations-setup#equip-system-animation-events");
                            break;
                    }
                }
                else if (index == 23)
                {
                    var id = EditorUtility.DisplayDialogComplex("Solution",
                        "EnableHandIK animation event is required for the equip animation. This animation event should be configured correctly.",
                        "Okay.", "Close.", "How?");

                    switch (id)
                    {
                        case 2:
                            Application.OpenURL("https://docs.breezeassets.net/general-topics/animations-setup#equip-system-animation-events");
                            break;
                    }
                }
                else if (index == 24)
                {
                    var id = EditorUtility.DisplayDialogComplex("Solution",
                        "DisableHandIK animation event is required for the equip animation. This animation event should be configured correctly.",
                        "Okay.", "Close.", "How?");

                    switch (id)
                    {
                        case 2:
                            Application.OpenURL("https://docs.breezeassets.net/general-topics/animations-setup#equip-system-animation-events");
                            break;
                    }
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
            GUI.backgroundColor = new Color(1, 0, 0f, 0.275f);
            EditorGUILayout.HelpBox(st, MessageType.Error);
        }
    }
}