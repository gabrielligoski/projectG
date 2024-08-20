using UnityEditor;
using UnityEngine;

namespace Breeze.Core
{
    public class BreezeWaypointEditor : EditorWindow
    {
        private static BreezeSystem selectedSystem = null;
        private static BreezeWaypoint selectedWaypoint = null;
        private static bool Copying = false;
        private static BreezeSystem ToCopyFrom;
        
        //Show Editor
        [MenuItem("Breeze/Tools/Waypoint Editor %w", false, 200)]
        public static void ShowWindow()
        {
            EditorWindow window = GetWindow(typeof(BreezeWaypointEditor), false, "Waypoint Editor");
            window.minSize = new Vector2(460, 605f);

            if (Selection.activeGameObject != null)
            {
                if (Selection.activeGameObject.GetComponent<BreezeSystem>() != null)
                {
                    selectedSystem = Selection.activeGameObject.GetComponent<BreezeSystem>();
                }   
            }
        }

        //Enable Editor From Button
        public static void EnableEditor(BreezeSystem system)
        {
            selectedSystem = system;
            ShowWindow();
        }
        
        /// <summary>
        /// When we open the window, subscribe to the SceneView's input event
        /// </summary>
        void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        /// <summary>
        /// When we close, unsubscribe from the SceneView
        /// </summary>
        void OnDisable()
        {
            if (selectedSystem != null)
            {
                foreach (var point in selectedSystem.Waypoints)
                {
                    GameObject.Find(selectedSystem.gameObject.name + "'s Waypoints").hideFlags = HideFlags.NotEditable;
                    point.gameObject.hideFlags = HideFlags.NotEditable;
                }   
            }

            if (selectedSystem != null)
            {
                Selection.activeGameObject = selectedSystem.gameObject;
                selectedSystem = null;
            }
            
            selectedWaypoint = null;
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        //Check for mouse click
        void OnSceneGUI(SceneView sceneView)
        {
            Event current = Event.current;

            if (current.type == EventType.MouseDown && current.button == 0 && current.control)
            {
                Done(false);
            }
            else if (current.type == EventType.MouseDown && current.button == 1 && current.control)
            {
                Done(true);
            }
        }

        //Update waypoints
        private void Update()
        {
            if (selectedSystem != null)
            {
                foreach (var point in selectedSystem.Waypoints)
                {
                    GameObject.Find(selectedSystem.gameObject.name + "'s Waypoints").hideFlags = HideFlags.None;
                    point.gameObject.hideFlags = HideFlags.None;
                }
                
                if (selectedWaypoint == null && selectedSystem.Waypoints.Count > 0)
                {
                    selectedWaypoint = selectedSystem.Waypoints[0];
                }
                
                selectedSystem.OnValidate();
                foreach (var point in selectedSystem.Waypoints)
                {
                    point.DrawGizmos = true;
                }
                Selection.activeGameObject =
                    selectedWaypoint == null ? selectedSystem.gameObject : selectedWaypoint.gameObject;
            }
        }

        //Draw GUI
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical("Box");
            var style = new GUIStyle(EditorStyles.boldLabel) {alignment = TextAnchor.MiddleCenter, fontSize = 16};
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Breeze AI Waypoint Editor", style, GUILayout.ExpandWidth(true));
            EditorGUILayout.Space(6.5f);
            EditorGUILayout.HelpBox(
                "The Breeze AI Waypoint Editor allows you to Add/Remove waypoints for your AI.",
                MessageType.Info, true);
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("SYSTEM", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
            selectedSystem = (BreezeSystem)EditorGUILayout.ObjectField("Selected AI System", selectedSystem, typeof(BreezeSystem), true);
            if (selectedSystem != null)
            {
                EditorGUILayout.Space(5);
                selectedSystem.WaypointOrder =
                   (BreezeEnums.WaypointOrder)EditorGUILayout.EnumPopup("AI Waypoint Movement", selectedSystem.WaypointOrder);
            }
            EditorGUILayout.Space(5);
            GUI.backgroundColor = new Color(0, 1, 0f, 0.19f);
            EditorGUILayout.HelpBox(
                "[Hold Control] Left click on your map to create a new waypoint, right click on a waypoint to remove it, left click on a waypoint to edit it's settings.",
                MessageType.Info, true);
            GUI.backgroundColor = Color.white;
            EditorGUILayout.Space(10f);
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("WAYPOINTS", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(8f);
            if (selectedWaypoint == null)
            {
                GUI.backgroundColor = new Color(1f, 1.0f, 0.0f, 0.25f);
                EditorGUILayout.LabelField("Please select a waypoint in your scene first!", EditorStyles.helpBox);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.Space(2f);   
            }
            EditorGUI.BeginDisabledGroup(true);
            selectedWaypoint = (BreezeWaypoint)EditorGUILayout.ObjectField("Selected Waypoint", selectedWaypoint, typeof(BreezeWaypoint), true);
            EditorGUI.EndDisabledGroup();
            if (selectedWaypoint != null)
            {
                EditorGUILayout.Space(4f);
                selectedWaypoint.WaitOnWaypoint =
                    EditorGUILayout.Toggle("Wait On Arrival", selectedWaypoint.WaitOnWaypoint);
                
                if (selectedWaypoint.WaitOnWaypoint)
                {
                    EditorGUILayout.Space(4f);   
                    selectedWaypoint.MinIdleLength =
                        EditorGUILayout.FloatField("Min Wait Length", selectedWaypoint.MinIdleLength);
                    EditorGUILayout.Space(4f);   
                    selectedWaypoint.MaxIdleLength =
                        EditorGUILayout.FloatField("Max Wait Length", selectedWaypoint.MaxIdleLength);
                }
                EditorGUILayout.Space(4f);
            }

            if (selectedSystem != null)
            {
                EditorGUILayout.Space(10f);
                if (GUILayout.Button("Copy Waypoints From Another AI", GUILayout.Width(225), GUILayout.Height(24)))
                {
                    Copying = !Copying;
                }

                if (Copying)
                {
                    EditorGUILayout.Space(4f);
                    ToCopyFrom = (BreezeSystem)EditorGUILayout.ObjectField("System To Copy", ToCopyFrom, typeof(BreezeSystem), true);

                    if (ToCopyFrom != null)
                    {
                        selectedSystem.Waypoints = ToCopyFrom.Waypoints;
                        Copying = false;
                    }
                }   
            }

            EditorGUILayout.EndVertical();
        }

        private Transform parent;
       
        //Check the object user clicked on, and do the process
        public void Done(bool remove)
        {
            if(Application.isPlaying)
                return;
            
            EditorUtility.SetDirty(selectedSystem);


            if (GameObject.Find(selectedSystem.gameObject.name + "'s Waypoints") != null)
            {
                parent = GameObject.Find(selectedSystem.gameObject.name + "'s Waypoints").transform;
            }
            else if(!remove)
            {
                parent = new GameObject(selectedSystem.gameObject.name + "'s Waypoints").transform;
            }
            
#if UNITY_EDITOR

            RaycastHit hit = new RaycastHit();
            if (Camera.main == null)
            {
                Debug.LogError("There must be at least 1 camera in the scene for creating waypoints!");
                return;
            }

            Ray ray = HandleUtility.GUIPointToWorldRay (Event.current.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.GetComponent<BreezeWaypoint>() == null && !remove)
                {
                    GameObject obj = new GameObject("Waypoint #" + (selectedSystem.Waypoints.Count + 1));
                    obj.AddComponent<SphereCollider>().isTrigger = true;
                    obj.transform.position = hit.point + new Vector3(0, 0.4f, 0);
                    selectedWaypoint = obj.AddComponent<BreezeWaypoint>();
                    selectedSystem.Waypoints.Add(selectedWaypoint);
                    Selection.activeGameObject =
                        selectedWaypoint == null ? selectedSystem.gameObject : selectedWaypoint.gameObject;
                    obj.transform.SetParent(parent);
                    Repaint();
                }
                else if(hit.transform.GetComponent<BreezeWaypoint>() != null)
                {
                    if (remove)
                    {
                        BreezeWaypoint waypoint = hit.transform.GetComponent<BreezeWaypoint>();
                        selectedSystem.Waypoints.Remove(waypoint);
                        if (selectedWaypoint == waypoint)
                        {
                            selectedWaypoint = null;
                        }
                        DestroyImmediate(hit.transform.gameObject);

                        if (selectedSystem.Waypoints.Count <= 0 && parent.gameObject != null)
                        {
                            DestroyImmediate(parent.gameObject);
                        }
                    }
                    else
                    {
                        selectedWaypoint = hit.transform.GetComponent<BreezeWaypoint>();   
                    }
                    Selection.activeGameObject =
                        selectedWaypoint == null ? selectedSystem.gameObject : selectedWaypoint.gameObject;
                    Repaint();
                }
            }
#endif
        }
    }
}