using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Breeze.Core
{
    [Serializable]
    public class weaponClass
    {
        public string weaponName;
        public BreezeWeaponInterface weaponScript;
        public RuntimeAnimatorController weaponAnimator;
        [Space] public float AttackDistanceOverride;
        [Space] public float ExtendedAttackDistance;
        [Space] public float TooCloseDistanceOverride;
        [Range(1f, 100f)]
        [Space] public float DetectionDistanceOverride;

        public weaponClass(string weaponName, BreezeWeaponInterface weaponScript, RuntimeAnimatorController weaponAnimator, float attackDistanceOverride, float ExtendedAttackDistance, float tooCloseDistanceOverride, float detectionDistanceOverride)
        {
            this.weaponName = weaponName;
            this.weaponScript = weaponScript;
            this.weaponAnimator = weaponAnimator;
            this.ExtendedAttackDistance = ExtendedAttackDistance;
            AttackDistanceOverride = attackDistanceOverride;
            TooCloseDistanceOverride = tooCloseDistanceOverride;
            DetectionDistanceOverride = detectionDistanceOverride;
        }
    }

    [ExecuteAlways]
    public class BreezeWeaponHub : MonoBehaviour
    {
        [Header("WEAPON HUB")] [Space] [Space] [Space]
        public int startIndex;

        [Space] public List<weaponClass> WeaponClasses = new List<weaponClass>();

        private bool switching;
        private bool holstered;
        private BreezeSystem _system;
        private int currentGoalIndex;
        [HideInInspector] public int currentIndex;
        private BreezeGun currentGunWeapon;
        private BreezeMeleeWeapon currentMeleeWeapon;

        private void OnEnable()
        {
            Transform ThePar = transform.parent;

            while (ThePar.GetComponent<BreezeSystem>() == null)
            {
                ThePar = ThePar.parent;
            }

            _system = ThePar.GetComponent<BreezeSystem>();
            
            if (_system  == null)
                return;
            
            _system.BreezeWeaponHub = this;
            _system.weaponHubInitialized = true;
        }

        private void OnDisable()
        {
            _system.BreezeWeaponHub = null;
            _system.weaponHubInitialized = false;
        }

        private void OnValidate()
        {
            if(_system == null)
                return;

            if (startIndex > WeaponClasses.Count - 1)
                startIndex = WeaponClasses.Count - 1;

            if (startIndex < 0)
                startIndex = 0;

            _system.AttackDistance = WeaponClasses[startIndex].AttackDistanceOverride;
            _system.ExtendedAttackDistance = WeaponClasses[startIndex].ExtendedAttackDistance;
            _system.EnemyTooCloseDistance = WeaponClasses[startIndex].TooCloseDistanceOverride;
            _system.DetectionDistance = WeaponClasses[startIndex].DetectionDistanceOverride;
        }

        private void Awake()
        {
            if(_system != null)
                return;
            
            Transform ThePar = transform.parent;

            while (ThePar.GetComponent<BreezeSystem>() == null)
            {
                ThePar = ThePar.parent;
            }

            _system = ThePar.GetComponent<BreezeSystem>();

            if (_system != null)
                _system.BreezeWeaponHub = this;
        }

        private void Start()
        {
            if (_system == null)
                _system = transform.root.GetComponent<BreezeSystem>();

            currentIndex = startIndex;
            foreach (var weapon in WeaponClasses)
            {
                switch (weapon.weaponScript)
                {
                    case BreezeGun gun:
                        gun.OnEquipChangedAI(false, true);
                        break;
                    case BreezeMeleeWeapon meleeWeapon:
                        meleeWeapon.OnEquipChangedAI(false, true);
                        break;
                }

                weapon.weaponScript.enabled = false;
            }

            if (WeaponClasses[startIndex].AttackDistanceOverride > 0)
            {
                _system.AttackDistance = WeaponClasses[startIndex].AttackDistanceOverride;
            }

            if (WeaponClasses[startIndex].ExtendedAttackDistance > 0)
            {
                _system.ExtendedAttackDistance = WeaponClasses[startIndex].ExtendedAttackDistance;
            }

            if (WeaponClasses[startIndex].DetectionDistanceOverride > 0)
            {
                _system.DetectionDistance = WeaponClasses[startIndex].DetectionDistanceOverride;
            }

            if (WeaponClasses[startIndex].TooCloseDistanceOverride > 0)
            {
                _system.EnemyTooCloseDistance = WeaponClasses[startIndex].TooCloseDistanceOverride;
            }

            if (WeaponClasses[startIndex].weaponScript is BreezeGun)
            {
                currentMeleeWeapon = null;
                currentGunWeapon = WeaponClasses[startIndex].weaponScript as BreezeGun;

                if (currentGunWeapon == null)
                    return;

                _system.WeaponType = BreezeEnums.WeaponType.Shooter;
                currentGunWeapon.enabled = true;
                _system.UseHandIK = currentGunWeapon.UseHandIK;
                _system.OnlyLeftHand = currentGunWeapon.OnlyLeftHand;
                _system.UseAimIK = currentGunWeapon.UseAimIK;
                _system.RightHandIK = currentGunWeapon.RightHandIK.transform;
                _system.LeftHandIK = currentGunWeapon.LeftHandIK.transform;
                _system.AimTransform = currentGunWeapon.MuzzlePoint.transform;
            }
            else
            {
                currentGunWeapon = null;
                currentMeleeWeapon = WeaponClasses[startIndex].weaponScript as BreezeMeleeWeapon;

                if (currentMeleeWeapon == null)
                    return;

                _system.WeaponType = BreezeEnums.WeaponType.Melee;
                _system.UseHandIK = false;
                _system.UseAimIK = false;
                currentMeleeWeapon.enabled = true;
            }

            _system.anim.runtimeAnimatorController = WeaponClasses[startIndex].weaponAnimator;

            if (WeaponClasses[startIndex].weaponScript is BreezeGun)
            {
                _system.OnEquipChanged.AddListener(((BreezeGun)WeaponClasses[startIndex].weaponScript)
                    .OnEquipChangedAI);
            }
            else
            {
                _system.OnEquipChanged.AddListener(((BreezeMeleeWeapon)WeaponClasses[startIndex].weaponScript)
                    .OnEquipChangedAI);
            }
        }

        public void switchWeapon(int index)
        {
            currentGoalIndex = index;
            currentIndex = index;
            _system.switchingWeapon = true;
            Debug.Log("test");
            _system.ResetAnimParams();
            _system.EquippedWeapon = false;
            _system.Combating = false;
            _system.anim.SetBool("Combating", false);
            Invoke(nameof(startHolstered), 0.35f);
        }

        public bool switchWeaponBackend()
        {
            currentGoalIndex = currentIndex == WeaponClasses.Count - 1 ? 0 : currentIndex + 1;
            bool found = false;

            while (!currentGoalIndex.Equals(currentIndex))
            {
                if (WeaponClasses[currentGoalIndex].weaponScript is BreezeGun)
                {
                    if (((BreezeGun)WeaponClasses[currentGoalIndex].weaponScript).ClipAmount > 0)
                    {
                        found = true;
                        break;
                    }
                    else
                    {
                        currentGoalIndex = currentGoalIndex == WeaponClasses.Count - 1 ? 0 : currentGoalIndex + 1;
                    }
                }
                else
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                return false;

            currentIndex = currentGoalIndex;
            _system.switchingWeapon = true;
            Debug.Log("test");
            _system.ResetAnimParams();
            _system.EquippedWeapon = false;
            _system.Combating = false;
            _system.anim.SetBool("Combating", false);
            Invoke(nameof(startHolstered), 0.35f);
            return true;
        }

        private void startHolstered()
        {
            switching = true;
            holstered = false;
        }

        private void Update()
        {
            if (!switching)
                return;

            _system.BodyWeight = 0;

            if (_system.anim.GetCurrentAnimatorStateInfo(0).IsTag("Holster") && !holstered)
                Invoke(nameof(disableAnim), _system.anim.GetCurrentAnimatorStateInfo(0).length - 0.2f);


            if (holstered)
            {
                switching = false;
                holstered = false;
                equip();
            }
        }

        private void disableAnim()
        {
            CancelInvoke(nameof(disableAnim));
            holstered = true;
        }

        private void equip()
        {
            if (currentGunWeapon == null)
            {
                _system.OnEquipChanged.RemoveListener(currentMeleeWeapon.OnEquipChangedAI);
            }
            else
            {
                _system.OnEquipChanged.RemoveListener(currentGunWeapon.OnEquipChangedAI);
            }
            _system.BreezeEvents.OnSwappedWeapon.Invoke(WeaponClasses[currentGoalIndex]);

            if (WeaponClasses[currentGoalIndex].AttackDistanceOverride > 0)
            {
                _system.AttackDistance = WeaponClasses[currentGoalIndex].AttackDistanceOverride;
            }

            if (WeaponClasses[currentGoalIndex].ExtendedAttackDistance > 0)
            {
                _system.ExtendedAttackDistance = WeaponClasses[currentGoalIndex].ExtendedAttackDistance;
            }

            if (WeaponClasses[currentGoalIndex].DetectionDistanceOverride > 0)
            {
                _system.DetectionDistance = WeaponClasses[currentGoalIndex].DetectionDistanceOverride;
            }

            if (WeaponClasses[currentGoalIndex].TooCloseDistanceOverride > 0)
            {
                _system.EnemyTooCloseDistance = WeaponClasses[currentGoalIndex].TooCloseDistanceOverride;
            }

            if (currentGunWeapon != null)
            {
                currentGunWeapon.enabled = false;
            }
            else
            {
                currentMeleeWeapon.enabled = false;
            }

            if (WeaponClasses[currentGoalIndex].weaponScript is BreezeGun)
            {
                BreezeGun weapon = WeaponClasses[currentGoalIndex].weaponScript as BreezeGun;

                if (weapon == null)
                    return;

                _system.WeaponType = BreezeEnums.WeaponType.Shooter;
                weapon.BreezeSystem = _system;
                currentGunWeapon = weapon;
                currentMeleeWeapon = null;
                weapon.enabled = true;
                _system.UseHandIK = currentGunWeapon.UseHandIK;
                _system.OnlyLeftHand = currentGunWeapon.OnlyLeftHand;
                _system.UseAimIK = currentGunWeapon.UseAimIK;
                _system.RightHandIK = currentGunWeapon.RightHandIK.transform;
                _system.LeftHandIK = currentGunWeapon.LeftHandIK.transform;
                _system.AimTransform = currentGunWeapon.MuzzlePoint.transform;
                _system.OnEquipChanged.AddListener(weapon.OnEquipChangedAI);
            }
            else
            {
                BreezeMeleeWeapon weapon = WeaponClasses[currentGoalIndex].weaponScript as BreezeMeleeWeapon;

                if (weapon == null)
                    return;


                currentGunWeapon = null;
                currentMeleeWeapon = weapon;
                _system.WeaponType = BreezeEnums.WeaponType.Melee;
                _system.UseHandIK = false;
                _system.UseAimIK = false;
                _system.OnEquipChanged.AddListener(weapon.OnEquipChangedAI);
            }

            _system.anim.runtimeAnimatorController = WeaponClasses[currentGoalIndex].weaponAnimator;
            _system.switchingWeapon = false;
        }
    }

#if UNITY_EDITOR
    [Serializable]
    [CustomEditor(typeof(BreezeWeaponHub))]
    public class BreezeWeaponHubEditor : Editor
    {
        private BreezeWeaponHub system;
        private int WarningNumber;
        private void OnEnable()
        {
            //Get The System
            system = (BreezeWeaponHub)target;
        }

        public override void OnInspectorGUI()
        {
            List<string> warningMessages = new List<string>();
            var errorAvailable = true;

            EditorGUILayout.Space(8);
            if (system.WeaponClasses.Where(weapon => weapon.weaponAnimator == null).ToList().Count > 0)
            {
                NormalError("One of your weapon classes' 'Weapon Animator' component is missing, please assign it.");
            }
            else if (system.WeaponClasses.Where(weapon => weapon.weaponScript == null).ToList().Count > 0)
            {
                NormalError("One of your weapon classes' 'Weapon Script' component is missing, please assign it.");
            }
            else if (system.WeaponClasses.Where(weapon => weapon.AttackDistanceOverride <= 0).ToList().Count > 0)
            {
                NormalError("One of your weapon classes' 'Attack Distance' component is less than 0, please fix it.");
            }
            else if (system.WeaponClasses.Where(weapon => weapon.ExtendedAttackDistance <= 0).ToList().Count > 0)
            {
                NormalError("One of your weapon classes' 'Extended Attack Distance' component is less than 0, please fix it.");
            }
            else if (system.WeaponClasses.Where(weapon => weapon.TooCloseDistanceOverride < 0).ToList().Count > 0)
            {
                NormalError("One of your weapon classes' 'Extended Attack Distance' component is less than 0, please fix it.");
            }
            else if (system.WeaponClasses.Where(weapon => weapon.DetectionDistanceOverride < 0).ToList().Count > 0)
            {
                NormalError("One of your weapon classes' 'Detection Distance' component is less than 0, please fix it.");
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
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("FIX NOW", GUILayout.Width(65f), GUILayout.Height(22f));
                EditorGUI.EndDisabledGroup();
            }
            
            if(system.GetComponentsInChildren<BreezeWeaponInterface>().ToList().Where(weapon => system.WeaponClasses.Where(classes => classes.weaponScript == weapon).ToList().Count <= 0).ToList().Count >0)
            {
                WarningError("There are weapon classes that aren't added to the 'Weapon Hub' script, please make sure to add them in order to make them functioning.", warningMessages);
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
                if (GUILayout.Button("SHOW ME", GUILayout.Width(75f), GUILayout.Height(22f)))
                {
                    Selection.activeGameObject = system.GetComponentsInChildren<BreezeWeaponInterface>().ToList()
                        .Where(weapon =>
                            system.WeaponClasses.Where(classes => classes.weaponScript == weapon).ToList().Count <= 0)
                        .ToList()[0].gameObject;
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
            EditorGUILayout.Space(5);
            DrawPropertiesExcluding(serializedObject, "m_Script");
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("SWITCH TO NEXT WEAPON", GUILayout.Height(24), GUILayout.Width(230)))
            {
                system.switchWeapon(system.currentIndex >= system.WeaponClasses.Count - 1
                    ? 0
                    : system.currentIndex + 1);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
        
        private void NormalError(string st)
        {
            GUI.backgroundColor = new Color(1, 0, 0f, 0.275f);
            EditorGUILayout.HelpBox(st, MessageType.Error);
        }
        
        private void WarningError(string st, List<string> lt)
        {
            if(!lt.Contains(st))
                lt.Add(st);
        }
    }
#endif
}