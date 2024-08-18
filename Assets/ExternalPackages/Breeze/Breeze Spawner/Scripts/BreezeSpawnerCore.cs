using System;
using System.Collections.Generic;
using System.Linq;
#if Breeze_AI_Pathfinder_Enabled
using Pathfinding;
#else
using UnityEngine.AI;
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Random = UnityEngine.Random;

namespace Breeze.Addons.Spawner
{
    public class BreezeSpawnerCore : MonoBehaviour
    {
        [Serializable]
        public class SpawningObjects
        {
            public GameObject ObjectPrefab;
            [Range(1, 100)] 
            public int SpawnProbability = 50;
        }
        
        [Serializable]
        public class SpawnTextures
        {
            public GameObject ObjectPrefab;
            public List<Texture> SpawnableTextures = new List<Texture>();
        }

        public List<SpawningObjects> ObjectsToSpawn = new List<SpawningObjects>();
        [Space] public int AmountToSpawn = 10;
        [Space] public float MinimumSpawnDistance = 10.0f;
        [Space] public float MaximumSpawnDistance = 30.0f;
        [Space] public float DestroyAIDistance = 40f;

        [Space] 
        public float MaximumFloorAngle = 45;
        [Space] public LayerMask CollisionDetectionLayers = 0;
        
        [Space] 
        public bool UseTextureDetection = true;
        [Space] public List<SpawnTextures> TextureDetectionList = new List<SpawnTextures>();

        [Space] 
        public int amountSpawned = 0;
        [Space] public List<GameObject> systemsSpawned = new List<GameObject>();


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.color = Color.blue;
            Handles.DrawWireDisc(transform.position, Vector3.up, MinimumSpawnDistance);
            Handles.color = Color.red;
            Handles.DrawWireDisc(transform.position, Vector3.up, DestroyAIDistance);
            Handles.color = Color.green;
            Handles.DrawWireDisc(transform.position, Vector3.up, MaximumSpawnDistance);
        }  
#endif

        public void Start()
        {
            for (int i = 0; i < AmountToSpawn; i++)
            {
                SpawnRandomAI();
            }
        }

        private void LateUpdate()
        {
            foreach (var system in systemsSpawned.Where(system => Vector3.Distance(transform.position, system.transform.position) >= DestroyAIDistance))
            {
                systemsSpawned.Remove(system);
                amountSpawned--;
                Destroy(system.gameObject);
                SpawnRandomAI();
                break;
            }
        }

        private int tries;
        private void SpawnRandomAI()
        {
            float[] weights = new float[ObjectsToSpawn.Count];
            for (int i = 0; i < ObjectsToSpawn.Count; i++)
            {
                weights[i] = ObjectsToSpawn[i].SpawnProbability;
            }

            GameObject systemToSpawn = ObjectsToSpawn[GetRandomWeightedIndex(weights)].ObjectPrefab;
#if Breeze_AI_Pathfinder_Enabled
            Vector3 dest = AstarPath.active.GetNearest(PickRandomPoint()).position;

            Collider[] cols = new Collider[100];
            var size = Physics.OverlapSphereNonAlloc(dest + new Vector3(0, 4, 0), 3, cols, CollisionDetectionLayers);
            bool textureFound = false;
            bool angleOkay = false;
            if (Physics.Raycast(dest + new Vector3(0, 4, 0), Vector3.down, out var hit, 10))
            {
                angleOkay = Vector3.Angle(hit.normal, Vector3.up) <= MaximumFloorAngle;
                if (hit.transform.GetComponent<Terrain>() == null)
                {
                    if (hit.transform.GetComponent<Renderer>() != null)
                    {
                        foreach (var Object in TextureDetectionList)
                        {
                            if (Object.ObjectPrefab.Equals(systemToSpawn))
                            {
                                if (Object.SpawnableTextures.Contains(hit.transform.gameObject.GetComponent<Renderer>().material.mainTexture))
                                {
                                    textureFound = true;
                                }
                            }
                        }   
                    }
                }
                else
                {
                    Terrain terrain = hit.transform.GetComponent<Terrain>();
                    foreach (var Object in TextureDetectionList)
                    {
                        if (Object.ObjectPrefab.Equals(systemToSpawn))
                        {
                            foreach (var texture in Object.SpawnableTextures)
                            {
                                if (texture.Equals(returnTerrainTexture(terrain, hit.point)))
                                {
                                    textureFound = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (!UseTextureDetection)
                textureFound = true;

            if ((size > 0 || !textureFound || !angleOkay) && tries < 100)
            {
                tries++;
                SpawnRandomAI();
                return;
            }
            
            if(tries >= 100)
                return;

            tries = 0;
            systemsSpawned.Add(Instantiate(systemToSpawn.gameObject, dest, Quaternion.identity));
            amountSpawned++;

#else
            bool colliderOkay = false;
            bool angleOkay = false;
            bool TextureOkay = false;
            bool PositionOkay = NavMesh.SamplePosition(PickRandomPoint(), out var hit, 3, NavMesh.AllAreas);
            Collider[] cols = new Collider[100];
            var size = 
                Physics.OverlapSphereNonAlloc(hit.position + new Vector3(0, 4, 0), 3, cols, CollisionDetectionLayers);
            colliderOkay = size <= 0;
            
            if (Physics.Raycast(hit.position + new Vector3(0, 4, 0), Vector3.down, out var tex, 10))
            {
                angleOkay = Vector3.Angle(tex.normal, Vector3.up) <= MaximumFloorAngle;
                if (tex.transform.GetComponent<Terrain>() == null)
                {
                    if (tex.transform.GetComponent<Renderer>() != null)
                    {
                        foreach (var Object in TextureDetectionList)
                        {
                            if (Object.ObjectPrefab.Equals(systemToSpawn))
                            {
                                if (Object.SpawnableTextures.Contains(tex.transform.gameObject.GetComponent<Renderer>().material.mainTexture))
                                {
                                    TextureOkay = true;
                                }
                            }
                        }   
                    }
                }
                else
                {
                    Terrain terrain = tex.transform.GetComponent<Terrain>();
                    foreach (var Object in TextureDetectionList)
                    {
                        if (Object.ObjectPrefab.Equals(systemToSpawn))
                        {
                            foreach (var texture in Object.SpawnableTextures)
                            {
                                if (texture.Equals(returnTerrainTexture(terrain, tex.point)))
                                {
                                    TextureOkay = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            if (!UseTextureDetection)
                TextureOkay = true;
            
            if ((!colliderOkay || !TextureOkay || !PositionOkay || !angleOkay) && tries < 100)
            {
                tries++;
                SpawnRandomAI();
                return;
            }
            
            if(tries >= 100)
                return;

            tries = 0;
            systemsSpawned.Add(Instantiate(systemToSpawn.gameObject, hit.position, Quaternion.identity));
            amountSpawned++;
#endif
        }


        public Texture returnTerrainTexture(Terrain terrain, Vector3 hitPoint)
        {
            Vector3 TerrainPos = hitPoint - terrain.transform.position;
            Vector3 SplatMapPosition = new Vector3(TerrainPos.x / terrain.terrainData.size.x, 0,
                TerrainPos.z / terrain.terrainData.size.z);

            int x = Mathf.FloorToInt(SplatMapPosition.x * terrain.terrainData.alphamapWidth);
            int z = Mathf.FloorToInt(SplatMapPosition.z * terrain.terrainData.alphamapHeight);

            float[,,] alphaMap = terrain.terrainData.GetAlphamaps(x, z, 1, 1);

            int primaryIndex = 0;

            for (int i = 1; i < alphaMap.Length; i++)
            {
                if (alphaMap[0, 0, i] > alphaMap[0, 0, primaryIndex])
                {
                    primaryIndex = i;
                }
            }

            return terrain.terrainData.terrainLayers[primaryIndex].diffuseTexture;
        }
        Vector3 PickRandomPoint () {
            var point = Random.insideUnitSphere * MaximumSpawnDistance;

            point.y = 0;
            point += transform.position;

            while (Vector3.Distance(transform.position, point) < MinimumSpawnDistance)
            {
                point = Random.insideUnitSphere * MaximumSpawnDistance;

                point.y = 0;
                point += transform.position;
            }

            return point;
        }

        public int GetRandomWeightedIndex(float[] weights)
        {
            if (weights == null || weights.Length == 0) return -1;
 
            float w;
            float t = 0;
            int i;
            for (i = 0; i < weights.Length; i++)
            {
                w = weights[i];
 
                if (float.IsPositiveInfinity(w))
                {
                    return i;
                }

                if (w >= 0f && !float.IsNaN(w))
                {
                    t += weights[i];
                }
            }
 
            float r = Random.value;
            float s = 0f;
 
            for (i = 0; i < weights.Length; i++)
            {
                w = weights[i];
                if (float.IsNaN(w) || w <= 0f) continue;
 
                s += w / t;
                if (s >= r) return i;
            }
 
            return -1;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(BreezeSpawnerCore))]
    public class BreezeSpawnerEditor : Editor
    {
        // This will contain the <wave> array of the WaveManager. 
        BreezeSpawnerCore spawner;  
    
        //Toolbar Variables
        private int TabNumber = 0;
        private bool TabChanged = false;
        GUIContent[] Buttons = new GUIContent[4] {new GUIContent(" Base \n Settings"), new GUIContent(" Detection \n Settings"), new GUIContent(" Texture \n Settings"), new GUIContent(" Debug \n Spawner")};

        private void OnEnable()
        {
            spawner = (BreezeSpawnerCore)target;
        }

        //This is the function that makes the custom editor work
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (!TabChanged)
            {
                TabChanged = true;
                if (PlayerPrefs.HasKey(spawner.gameObject.GetInstanceID() + " tab"))
                    TabNumber = PlayerPrefs.GetInt(spawner.gameObject.GetInstanceID() + " tab");
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
            PlayerPrefs.SetInt(spawner.gameObject.GetInstanceID() + " tab", TabNumber);

            if (TabNumber == 0)
            {
                //Settings
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("BASE SETTINGS", EditorStyles.boldLabel);
                EditorGUILayout.Space(2.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ObjectsToSpawn"));
                EditorGUILayout.Space(2.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("AmountToSpawn"));
                EditorGUILayout.Space(2.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MinimumSpawnDistance"));
                EditorGUILayout.Space(2.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MaximumSpawnDistance"));
                EditorGUILayout.Space(2.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DestroyAIDistance"));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
            }
            
            if (TabNumber == 1)
            {
                //Settings
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("DETECTION SETTINGS", EditorStyles.boldLabel);
                EditorGUILayout.Space(2.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MaximumFloorAngle"));
                EditorGUILayout.Space(2.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CollisionDetectionLayers"));
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
            }
            
            if (TabNumber == 2)
            {
                //Settings
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("TEXTURE SETTINGS", EditorStyles.boldLabel);
                EditorGUILayout.Space(2.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("UseTextureDetection"));
                EditorGUILayout.Space(2.75f);
                EditorGUI.BeginDisabledGroup(!spawner.UseTextureDetection);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("TextureDetectionList"));
                EditorGUI.EndDisabledGroup();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
            }
            
            if (TabNumber == 3)
            {
                //Settings
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField("DEBUG PANEL", EditorStyles.boldLabel);
                EditorGUILayout.Space(2.75f);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("amountSpawned"));
                EditorGUILayout.Space(2.75f);
                EditorGUI.BeginDisabledGroup(!spawner.UseTextureDetection);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("systemsSpawned"));
                EditorGUI.EndDisabledGroup();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndVertical();
            }
            
            serializedObject.ApplyModifiedProperties();
     
        }
    }
#endif
}
