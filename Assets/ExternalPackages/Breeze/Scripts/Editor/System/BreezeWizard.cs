using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Breeze.Core
{
    public class BreezeWizard : EditorWindow
    {
        //Toolbar Variables
        private int TabNumber = 0;

        private GUIContent[] Buttons =
        {
            new GUIContent(" Component \n Settings"), new GUIContent(" Behaviour \n Settings"), new GUIContent(" Optimization \n Settings")
        };


        //Creator Vars

        //Components
        private static GameObject AIObject;
        private static string AITag = "Untagged";
        private static string PlayerTag = "Untagged";
        private static string AIName = "Enter Name...";
        private static LayerMask AILayer = 0;

        //Behaviour
        private static BreezeEnums.AIBehaviour AIBehaviour = BreezeEnums.AIBehaviour.Enemy;
        private static BreezeEnums.AttackBehaviour AICompanionBehaviour = BreezeEnums.AttackBehaviour.Aggressive;
        private static BreezeEnums.AIConfidence AIConfidence = BreezeEnums.AIConfidence.Brave;
        private static BreezeEnums.AIWanderingType AIWanderingType = BreezeEnums.AIWanderingType.Patrol;
        private static BreezeEnums.YesNo UseRootMotion = BreezeEnums.YesNo.No;
        private static BreezeEnums.WeaponType AIWeaponType = BreezeEnums.WeaponType.Unarmed;
        private static BreezeEnums.AIDeathType AIDeathType = BreezeEnums.AIDeathType.Animation;

        //Optimization
        private static BreezeEnums.YesNo DeactivateWhenNotVisible = BreezeEnums.YesNo.No;
        private static Renderer LastLodRender = null;

        //Others
        Editor gameObjectEditor;

        [MenuItem("GameObject/Breeze/Create Character %g", false, 0)]
        public static void OpenAICreator()
        {
            if (Selection.activeGameObject != null)
            {
                EditorWindow window = GetWindow(typeof(BreezeWizard), false, "Creation Wizard");
                window.minSize = new Vector2(460, 635f);
                AIObject = Selection.activeGameObject;
            }
        }

        //Draw GUI
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical("Box");
            var style = new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Breeze Character Wizard", style, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space(6.5f);
            EditorGUILayout.HelpBox(
                "The Breeze Wizard allows you to create your characters easily.",
                MessageType.Info, true);
            EditorGUILayout.Space(10);
            EditorGUILayout.EndVertical();

            //Toolbar
            EditorGUILayout.BeginVertical();
            GUIStyle ToolbarStyle = EditorStyles.miniButton;
            ToolbarStyle.fixedHeight = 35;
            ToolbarStyle.alignment = TextAnchor.MiddleCenter;
            ToolbarStyle.fontStyle = FontStyle.Bold;
            TabNumber = GUILayout.SelectionGrid(TabNumber, Buttons, 3, ToolbarStyle, GUILayout.Height(68),
                GUILayout.Width(EditorGUIUtility.currentViewWidth - 10));
            EditorGUILayout.EndVertical();

            //Object Settings
            if (TabNumber == 0)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("COMPONENTS", EditorStyles.boldLabel);
                EditorGUILayout.Space(14);

                if (AIObject == null)
                {
                    GUI.backgroundColor = new Color(1.0f, 0.0f, 0.0f, 0.45f);
                    EditorGUILayout.LabelField("[GameObject cannot be null]", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.Space(2);
                }

                AIObject = (GameObject)EditorGUILayout.ObjectField("Character Object", AIObject, typeof(GameObject), true);
                EditorGUILayout.Space(8);

                if (AITag.Equals("Untagged"))
                {
                    GUI.backgroundColor = new Color(1.0f, 0.0f, 0.0f, 0.45f);
                    EditorGUILayout.LabelField("[AI's tag cannot be Untagged]", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.Space(2);
                }
                else
                {
                    GUI.backgroundColor = new Color(0.0f, 1.0f, 0.0f, 0.45f);
                    EditorGUILayout.LabelField("[Make sure that AI Tag is same for every AI]", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.Space(2);
                }

                AITag = EditorGUILayout.TagField(new GUIContent("Object Tag"), AITag);
                EditorGUILayout.Space(8);

                AILayer = EditorGUILayout.LayerField(new GUIContent("Object Layer"), AILayer);
                EditorGUILayout.Space(8);

                AIName = EditorGUILayout.TextField(new GUIContent("Object Name"), AIName);
                EditorGUILayout.Space(10);

                if (AIObject != null)
                {
                    GUILayout.Space(17);
                    GUIStyle bgColor = new GUIStyle
                    {
                        normal =
                        {
                            background = Texture2D.linearGrayTexture
                        }
                    };

                    if (gameObjectEditor == null)
                        gameObjectEditor = Editor.CreateEditor(AIObject);
                    else if (gameObjectEditor.target != AIObject)
                    {
                        gameObjectEditor = Editor.CreateEditor(AIObject);
                    }

                    gameObjectEditor.OnInteractivePreviewGUI(GUILayoutUtility.GetRect(220, 240), bgColor);
                    EditorGUILayout.Space(8);
                }

                EditorGUILayout.EndVertical();
            }

            //Object Settings
            if (TabNumber == 1)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("BEHAVIOUR", EditorStyles.boldLabel);
                EditorGUILayout.Space(14);

                AIBehaviour = (BreezeEnums.AIBehaviour)EditorGUILayout.EnumPopup("Behaviour", AIBehaviour);
                EditorGUILayout.Space(4);

                if (AIBehaviour == BreezeEnums.AIBehaviour.Enemy)
                    EditorGUILayout.Space(4);

                if (AIBehaviour == BreezeEnums.AIBehaviour.Enemy)
                {
                    AIConfidence =
                        (BreezeEnums.AIConfidence)EditorGUILayout.EnumPopup("Confidence", AIConfidence);
                    EditorGUILayout.Space(4);

                    if (AIBehaviour == BreezeEnums.AIBehaviour.Enemy)
                        EditorGUILayout.Space(4);
                }
                else
                {
                    AICompanionBehaviour =
                        (BreezeEnums.AttackBehaviour)EditorGUILayout.EnumPopup("Companion Behaviour",
                            AICompanionBehaviour);
                    EditorGUILayout.Space(4);

                    if (AIBehaviour == BreezeEnums.AIBehaviour.Enemy)
                        EditorGUILayout.Space(4);

                    PlayerTag = EditorGUILayout.TagField(new GUIContent("Player Object Tag"), PlayerTag);
                    EditorGUILayout.Space(4);

                    if (AIBehaviour == BreezeEnums.AIBehaviour.Enemy)
                        EditorGUILayout.Space(4);
                }

                AIWanderingType =
                    (BreezeEnums.AIWanderingType)EditorGUILayout.EnumPopup("Wandering Type", AIWanderingType);
                EditorGUILayout.Space(4);

                if (AIBehaviour == BreezeEnums.AIBehaviour.Enemy)
                    EditorGUILayout.Space(4);

                UseRootMotion = (BreezeEnums.YesNo)EditorGUILayout.EnumPopup("Use Rootmotion", UseRootMotion);
                EditorGUILayout.Space(4);

                if (AIBehaviour == BreezeEnums.AIBehaviour.Enemy)
                    EditorGUILayout.Space(4);

                if (AIConfidence != BreezeEnums.AIConfidence.Coward ||
                    AIBehaviour == BreezeEnums.AIBehaviour.Companion)
                {
                    AIWeaponType = (BreezeEnums.WeaponType)EditorGUILayout.EnumPopup("Weapon Type", AIWeaponType);
                    EditorGUILayout.Space(4);

                    if (AIBehaviour == BreezeEnums.AIBehaviour.Enemy)
                        EditorGUILayout.Space(4);
                }

                AIDeathType = (BreezeEnums.AIDeathType)EditorGUILayout.EnumPopup("Death Type", AIDeathType);
                EditorGUILayout.Space(2);

                if (AIBehaviour == BreezeEnums.AIBehaviour.Enemy)
                    EditorGUILayout.Space(6);

                if (AIConfidence == BreezeEnums.AIConfidence.Coward)
                {
                    EditorGUILayout.Space(26);

                    if (AIBehaviour == BreezeEnums.AIBehaviour.Enemy)
                        EditorGUILayout.Space(4);
                }


                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginHorizontal("box");
                GUILayout.FlexibleSpace();
                var Logostyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
                Texture2D myTexture = Resources.Load("Images/Logo") as Texture2D;

                GUILayout.Label(myTexture, Logostyle, GUILayout.MaxWidth(750f), GUILayout.MaxHeight(220f));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            //Optimization Settings
            if (TabNumber == 2)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("OPTIMIZATION", EditorStyles.boldLabel);
                EditorGUILayout.Space(14);

                GUI.backgroundColor = new Color(0.0f, 1.0f, 0.0f, 0.45f);
                EditorGUILayout.LabelField("Disables Object's Renderer When Not Visible", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space(14);

                DeactivateWhenNotVisible =
                    (BreezeEnums.YesNo)EditorGUILayout.EnumPopup("Deactivate Off-Screen",
                        DeactivateWhenNotVisible);
                EditorGUILayout.Space(20);

                EditorGUI.BeginDisabledGroup(DeactivateWhenNotVisible == BreezeEnums.YesNo.No);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(1);
                EditorGUILayout.BeginVertical();

                EditorGUI.BeginDisabledGroup(AIObject == null);
                if (GUILayout.Button("Auto Find Renderer", GUILayout.Width(140), GUILayout.Height(25)) &&
                    AIObject != null)
                {
                    LODGroup lodGroup = AIObject.GetComponentInChildren<LODGroup>();

                    if (lodGroup == null)
                    {
                        if (EditorUtility.DisplayDialog("Warning!",
                                "Failed to find any available lods under the character.", "Okay."))
                        {
                            DeactivateWhenNotVisible = BreezeEnums.YesNo.No;
                        }
                    }
                    else
                    {
                        LOD[] lods = lodGroup.GetLODs();
                        foreach (var lod in lods)
                        {
                            if (lod.renderers == null)
                            {
                                if (EditorUtility.DisplayDialog("Warning!",
                                        "Failed to find any available lods under the character.", "Okay."))
                                {
                                    DeactivateWhenNotVisible = BreezeEnums.YesNo.No;
                                }   
                            }
                            else
                            {
                                LastLodRender = lod.renderers[0];
                            }
                        }
                    }
                }

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(21);

                if (LastLodRender == null)
                {
                    GUI.backgroundColor = new Color(1.0f, 0.0f, 0.0f, 0.45f);
                    EditorGUILayout.LabelField("[Renderer cannot be null]", EditorStyles.helpBox);
                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.Space(2);
                }
                else
                {
                    EditorGUILayout.Space(5);
                }

                LastLodRender =
                    (Renderer)EditorGUILayout.ObjectField("Character Renderer", LastLodRender, typeof(Renderer), true);
                EditorGUILayout.Space(8);
                
                if (LastLodRender != null)
                    EditorGUILayout.Space(18);
                
                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginHorizontal("box");
                GUILayout.FlexibleSpace();
                var Logostyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
                Texture2D myTexture = Resources.Load("Images/Logo") as Texture2D;

                GUILayout.Label(myTexture, Logostyle, GUILayout.MaxWidth(750f), GUILayout.MaxHeight(220f));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(1);
            EditorGUILayout.BeginVertical();

            if (GUILayout.Button("Finish Setup", GUILayout.Height(30)) && AIObject != null)
            {
                if (EditorUtility.DisplayDialog("Warning",
                        "This action will remove every component on your character and re-assign them for the setup. Are you sure?", "Yes.", "Cancel."))
                {
                    FinishSetup();   
                }
            }

            GUILayout.Space(4);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        
        void OnInspectorUpdate()
        {
            Repaint();
        }

        private void FinishSetup()
        {
            AIObject.name = AIName;
            AIObject.tag = AITag;
            AIObject.layer = AILayer;

            Animator animator = AIObject.GetComponent<Animator>();

            if (animator == null)
            {
                animator = AIObject.AddComponent<Animator>();
            }

            animator.runtimeAnimatorController = null;

            Component[] components = AIObject.GetComponents<Component>();

            foreach (var component in components)
            {
                if (!(component is Animator) && !(component is Transform))
                {
                    DestroyImmediate(component);
                }
            }

            AIObject.AddComponent<NavMeshAgent>();
            AIObject.AddComponent<CapsuleCollider>();

            BreezeSystem system = AIObject.AddComponent<BreezeSystem>();

            system.AIBehaviour = AIBehaviour;
            system.AIConfidence = AIConfidence;
            system.AttackBehaviour = AICompanionBehaviour;
            system.WeaponType = AIWeaponType;
            system.UseRootMotion = UseRootMotion == BreezeEnums.YesNo.Yes;
            system.WanderType = AIWanderingType;
            system.DeathMethod = AIDeathType;
            system.UseLodOptimization = DeactivateWhenNotVisible;
            system.LodRenderer = LastLodRender;
            system.BreezeTag = AITag;

            AIObject.AddComponent<BreezeAnimations>();
            AIObject.AddComponent<BreezeEvents>();
            AIObject.AddComponent<BreezeSounds>();
            AIObject.AddComponent<BreezeCommandsManager>();

            GameObject hitPos = new GameObject("Hit Position");
            hitPos.transform.SetParent(AIObject.transform);
            hitPos.transform.localPosition = new Vector3(0, 0.75f, 0);
            hitPos.transform.localRotation.eulerAngles.Set(0,0,0);
            hitPos.transform.localScale = new Vector3(1, 1, 1);
            hitPos.AddComponent<BreezeHitPosition>();

            system.HitPosition = hitPos.transform;
            
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            system.OnValidate();
            focusedWindow.Close();
        }
    }
}