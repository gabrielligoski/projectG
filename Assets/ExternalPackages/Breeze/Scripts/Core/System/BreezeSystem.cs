using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if !Breeze_AI_Pathfinder_Enabled
using UnityEngine.AI;
#else
using Pathfinding;
#endif
#if FIRST_PERSON_CONTROLLER || THIRD_PERSON_CONTROLLER
using Opsive.UltimateCharacterController.Traits;
#endif
#if SURVIVAL_TEMPLATE_PRO
using PolymindGames;
#endif
#if INVECTOR_MELEE
using Invector;
using Invector.vCharacterController;
#endif
#if HQ_FPS_TEMPLATE
using HQFPSTemplate;
#endif
#if NEOFPS
using NeoFPS;
#endif
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Breeze.Core
{
    #region System Classes

    [Serializable]
    public class attackDamages
    {
        public float minDamage;
        public float MaxDamage;
    }

    [Serializable]
    public class particles
    {
#if UNITY_EDITOR
        [TagSelector(UseDefaultTagFieldDrawer = true)]
#endif
        public string ObjectTag = "Untagged";

        public BreezeMeleeWeapon.impactEffect.effectType EffectType =
            BreezeMeleeWeapon.impactEffect.effectType.Normal;

        public Transform CustomTransform;
        public Vector3 SpawnOffset = new Vector3(0, 1, 0);
        public GameObject EffectObject;
        public float DestroyAfter = 5f;
    }

    #endregion
    
    public class BreezeSystem : MonoBehaviour, BreezeDamageable
    {
        #region Public Variables

        public BreezeSounds BreezeSounds;

        //Stats Variables
        [Range(1, 999)] public int StartingHealth = 100;
        [Range(1, 999)] public int MaximumHealth = 100;
        public bool RegenerateHealth;

        public BreezeEnums.RegenerateCondition RegenerateCondition =
            BreezeEnums.RegenerateCondition.NonCombatState;

        public float RegenerateFrequency = 1.0f;
        public float RegenerateAmount = 3f;
        public float RegenerateStartDelay = 5.0f;
        [Range(0, 999)] public int RegenerateHealthLimit = 70;

        //Settings Variables
        public BreezeEnums.AIBehaviour AIBehaviour = BreezeEnums.AIBehaviour.Enemy;
        public BreezeEnums.AIConfidence AIConfidence = BreezeEnums.AIConfidence.Brave;
        public BreezeEnums.WeaponType WeaponType = BreezeEnums.WeaponType.Unarmed;
        public bool UseEquipSystem = true;
        public BreezeEnums.AIWanderingType WanderType = BreezeEnums.AIWanderingType.Patrol;
        public BreezeEnums.WaypointOrder WaypointOrder = BreezeEnums.WaypointOrder.InOrder;
        public BreezeEnums.AIDeathType DeathMethod = BreezeEnums.AIDeathType.Animation;
        public bool DestroyAfterDeath;
        public float DestroyDelay = 3.0f;

        //Movement Variables
        public int ForceWalkDistance;
        public int PatrolRadius = 25;
        public float FleeDistance = 8.5f;
        public float MinIdleLength = 4;
        public float MaxIdleLength = 8;
        public float RotationSpeed = 1.4f;
        public bool UseRootMotion = true;
        public float WalkSpeed = 1f;
        public float RunSpeed = 1.5f;
        public float WalkBackwardsSpeed = 0.7f;

        //Combat Variables
        public int minAlertLength = 3;
        public int maxAlertLength = 10;
        public bool ShouldChaseTarget = true;
        public bool HitReactionOnAttack = true;
        public int GetHitTolerance = 3;
        private GameObject LastAtacker;
        public float AttackDistance = 2f;
        [FormerlySerializedAs("DamageRange")] public float ExtendedAttackDistance = 4f;
        public float EnemyTooCloseDistance = 1f;
        public float BackawayMultiplier = 1.2f;
        [Range(0, 100)] public int HitReactionPossibility = 100;
        public float MinAttackDelay = 0.4f;
        public float MaxAttackDelay = 1f;
        public bool UseBlockingSystem = true;
        [Range(1, 100)] public int BlockAnimationPossibility = 40;
        public bool CanBlockWhileAttacking = true;
        public List<particles> ParticlesList = new List<particles>();
        public Transform HitPosition;

        //Detection Variables
        public BreezeEnums.YesNo NotifyCloseUnits = BreezeEnums.YesNo.Yes;
        [Range(5, 250)] public float NotifyDistanceLimit = 30;
        [Range(3, 999)] public float SoundDetectionLimit = 35;
        public BreezeEnums.DetectionType DetectionType = BreezeEnums.DetectionType.LineOfSight;
        public BreezeEnums.DetectionLock DetectionLock = BreezeEnums.DetectionLock.Closest;
        public float DetectionFrequency = 0.25f;
        [Range(1f, 100f)] public float DetectionDistance = 10;
        [Range(10f, 270f)] public float DetectionAngle = 70;
        public LayerMask DetectionLayers = 0;
        public LayerMask ObstacleLayers = 0;
        public Transform HeadTransform;

        //Inverse Kinematics Variables
        public bool UseHandIK;
        public bool OnlyLeftHand = true;
        public float HandSmoothAmount = 0.3f;
        public bool UseAimIK;
        public float AimSmoothAmount = 0.2f;
        public List<HumanBodyBones> BonesToUse = new List<HumanBodyBones>();
        public Vector3 AimOffset = new Vector3(0, 0.5f, 0);
        [Range(30, 135)] public float AngleLimit = 75f;

        //Factions Variables
        [Serializable]
        public class FactionsList
        {
            public BreezeEnums.Factions Factions;
            public BreezeEnums.BehaviourType behaviourType;

            public FactionsList()
            {
                Factions = BreezeEnums.Factions.Creature;
                behaviourType = BreezeEnums.BehaviourType.Enemy;
            }
        }

        public BreezeEnums.Factions CurrentAIFaction = BreezeEnums.Factions.Creature;
        public BreezeEnums.BehaviourType AIPlayerBehaviour = BreezeEnums.BehaviourType.Enemy;
        public List<FactionsList> AIFactionsList = new List<FactionsList>();
        public string BreezeTag = "Untagged";
        public string PlayerTag = "Player";

        //Companion Variables
        public BreezeEnums.AttackBehaviour AttackBehaviour = BreezeEnums.AttackBehaviour.Aggressive;
        [Range(5,90)]
        public int FriendlyDamageThreshold = 40;
        public float CFollowingDistance = 4.5f;
        public float CSprintFollowDistance = 12f;
        public float CTooCloseDistance = 1.5f;

        //Debug Variables
        public float CurrentHealthDebug;
        public bool RegeneratingHealthDebug;
        public string CurrentAIState = "Idle";
        public bool IsAlertedDebug;
        public bool IsAttackingDebug;
        public bool IsTargetVisible;
        public List<GameObject> VisibleTargetsDebug = new List<GameObject>();
        public GameObject CurrentTargetDebug;
        public string CurrentTargetTypeDebug = "AI";
        public float TargetDistanceDebug;
        public Animator AnimatorDebug;
#if !Breeze_AI_Pathfinder_Enabled
        public NavMeshAgent NavmeshAgentDebug;
#else
        public AIPath NavmeshAgentDebug;
#endif
        public Collider ColliderDebug;
        public BreezeSystem SystemDebug;

        //Optimization Variables
        public BreezeEnums.YesNo UseLodOptimization = BreezeEnums.YesNo.No;
        public Renderer LodRenderer;

        #endregion

        #region System Variables

        public bool weaponHubInitialized;
        public bool stopAI;
        public bool hasErrors;
        private bool disableErrorCheck = true;

        //Ik
        private Transform[] boneTransforms;
        public Transform AimTransform;
        public bool UseIK;
        public float state;
        public float elapsedTime;
        private float DistanceFixLerp;
        public float BodyWeight;
        private bool FirstIKDetect = true;
        private bool WeightDown;
        private float YOffset;
        private Vector3 TheVel = Vector3.zero;
        private Vector3 FixedLookPosition;
        private Vector3 lastone;
        public Transform RightHandIK;
        public Transform LeftHandIK;

        //Components
#if !Breeze_AI_Pathfinder_Enabled
        public NavMeshAgent nav;
#else
        public AIPath nav;
#endif
        public Animator anim;
        public Collider col;
        public BreezePlayer PlayerScript;
        public BreezeSystem TargetAIScript;
        public BreezeWeaponHub BreezeWeaponHub;
        public BreezeEvents BreezeEvents;
        public BreezeSystem System { get; set; }

        //Objects
        public GameObject CurrentTarget;
        public Transform playerObject;
        public bool TargetIsPlayer;
        private List<GameObject> IgnoredObjects = new List<GameObject>();
        private List<GameObject> VisibleTargets = new List<GameObject>();
        public List<BreezeWaypoint> Waypoints = new List<BreezeWaypoint>();
        private GameObject ExitSender;

        //States
        public BreezeEnums.MovementType CurrentMovement = BreezeEnums.MovementType.None;
        private BreezeEnums.MovementType savedMovement = BreezeEnums.MovementType.None;
        private bool isRegenHealth;
        private bool backingAway;
        public bool Combating;
        private bool IsPatrolIdle;
        private bool FirstWander = true;
        private bool FirstIdlePlay = true;
        private bool Patrolling;
        private bool Fleeing;
        public bool Attacking;
        private bool FirstAttack;
        private bool InvokedAlert;
        private int currentWaypoint;
        private bool FirstWaypoint = true;
        public bool Blocking;
        public bool GettingHit;
        public bool EquippedWeapon;
        public bool EquipEventCalled;
        private bool HitStarted;
        public bool Reloading;
        private bool TempBrave;
        private int ExitNeutral;
        public bool SingleCanFire = true;
        private float currentDamageAmount;
        public bool switchingWeapon = false;
        private bool UseCompanionLookIK;

        //Stats
        public float CurrentHealth = 100;
        public List<attackDamages> AttackDamagesList = new List<attackDamages>();

        //Positions
        private Vector3 LastKnownTargetPosition = Vector3.zero;

        //Others
        public UnityEvent<bool, bool> OnEquipChanged = new UnityEvent<bool, bool>();

        #endregion

        #region Initial System Methods

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            //Detection Gizmos
            if (AIBehaviour == BreezeEnums.AIBehaviour.Enemy)
            {
                Handles.color = new Color(1, 0.92f, 0.016f, 0.025f);
                Handles.DrawSolidDisc(transform.position, transform.up,
                    DetectionDistance);

                if (AIConfidence != BreezeEnums.AIConfidence.Coward)
                {
                    Handles.color = new Color(1, 0f, 0f, 0.020f);
                    Handles.DrawSolidDisc(transform.position, transform.up,
                        AttackDistance);
                    Handles.color = new Color(0, 0f, 1f, 0.020f);
                    Handles.DrawSolidDisc(transform.position, transform.up,
                        ExtendedAttackDistance);
                }

                if (DetectionType == BreezeEnums.DetectionType.LineOfSight)
                {
                    float totalFOV = DetectionAngle;
                    float rayRange = DetectionDistance;
                    float halfFOV = totalFOV / 2.0f;
                    Gizmos.color = Color.green;
                    Quaternion leftRayRotation = Quaternion.AngleAxis(-halfFOV, Vector3.up);
                    Quaternion rightRayRotation = Quaternion.AngleAxis(halfFOV, Vector3.up);
                    Vector3 leftRayDirection = leftRayRotation * transform.forward;
                    Vector3 rightRayDirection = rightRayRotation * transform.forward;
                    Gizmos.DrawRay(transform.position, leftRayDirection * rayRange);
                    Gizmos.DrawRay(transform.position, rightRayDirection * rayRange);
                }
            }
        }
#endif
        public void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if (FindObjectOfType<BreezeSoundManager>() == null && BreezeSoundManager.Instance == null)
            {
                GameObject obj = new GameObject("Sound PAI");
                obj.AddComponent<BreezeSoundManager>();
                obj.hideFlags = HideFlags.HideInHierarchy;
            }

            //Initialize Hit Position
            if (HitPosition == null)
                HitPosition = transform;
            
            //Check & Set Waypoints
            for (int i = 0; i < Waypoints.Count; i++)
            {
                if (Waypoints[i] == null)
                    continue;

                Waypoints[i].OnValidate();
                if (i != Waypoints.Count - 1)
                {
                    Waypoints[i].GizmosColor = i.Equals(0) ? Color.blue : Color.yellow;

                    Waypoints[i].NextWaypoint = Waypoints[i + 1].gameObject;
                }
                else
                {
                    Waypoints[i].NextWaypoint = Waypoints[0].gameObject;
                    Waypoints[i].GizmosColor = Color.red;
                }
            }

            //Check & Add Required Components
            if (anim == null)
            {
                anim = GetComponent<Animator>() == null
                    ? gameObject.AddComponent<Animator>()
                    : GetComponent<Animator>();
            }
            
            if (nav == null)
            {
#if !Breeze_AI_Pathfinder_Enabled
                nav = GetComponent<NavMeshAgent>() == null
                    ? gameObject.AddComponent<NavMeshAgent>()
                    : GetComponent<NavMeshAgent>();    
#else
                nav = GetComponent<AIPath>() == null
                    ? gameObject.AddComponent<AIPath>()
                    : GetComponent<AIPath>();        
#endif
            }

            if (col == null)
            {
                col = GetComponent<Collider>() == null
                    ? null
                    : GetComponent<Collider>();
            }

            NavmeshAgentDebug = nav;
            AnimatorDebug = anim;
            ColliderDebug = col;
            SystemDebug = this;

            //Check Values
            CurrentHealthDebug = StartingHealth;
        }

        private void Awake()
        {
            System = this;
            
            if (BreezeSoundManager.Instance == null)
            {
                GameObject obj = new GameObject("Sound PAI");
                obj.AddComponent<BreezeSoundManager>();
                obj.hideFlags = HideFlags.HideInHierarchy;
            }
        }

        private void disableErrorChecking()
        {
            disableErrorCheck = true;
        }

        //Init AI
        private void Start()
        {
            System = this;
            if (anim.avatar.isHuman)
            {
                boneTransforms = new Transform[BonesToUse.Count];
                for (int b = 0; b < boneTransforms.Length; b++)
                {
                    if (BonesToUse[b] is HumanBodyBones.Head)
                    {
                        continue;
                    }

                    boneTransforms[b] = anim.GetBoneTransform(BonesToUse[b]);
                }
            }

            InvokeRepeating(nameof(Detection), 0, DetectionFrequency);

            if (col == null && GetComponent<Collider>() != null)
            {
                col = GetComponent<Collider>();
            }
            
            //Check & Add Required Components
            if (anim == null)
            {
                anim = GetComponent<Animator>() == null
                    ? gameObject.AddComponent<Animator>()
                    : GetComponent<Animator>();
            }
            
            if (nav == null)
            {
#if !Breeze_AI_Pathfinder_Enabled
                nav = GetComponent<NavMeshAgent>() == null
                    ? gameObject.AddComponent<NavMeshAgent>()
                    : GetComponent<NavMeshAgent>();    
#else
                nav = GetComponent<AIPath>() == null
                    ? gameObject.AddComponent<AIPath>()
                    : GetComponent<AIPath>();        
#endif
            }

            if (col == null)
            {
                col = GetComponent<Collider>() == null
                    ? null
                    : GetComponent<Collider>();
            }

            Collider[] colliders = GetComponentsInChildren<Collider>();
            Rigidbody[] rigidbodys = GetComponentsInChildren<Rigidbody>();

            if (col != null)
            {
                foreach (var collider1 in colliders)
                {
                    if (collider1 != this.col)
                    {
                        Physics.IgnoreCollision(collider1, this.col);
                    }
                }
            }

            foreach (var rig in rigidbodys)
            {
                rig.isKinematic = true;

                if (rig.GetComponent<BreezeDamageBase>() != null)
                {
                    rig.tag = BreezeTag;
                    rig.GetComponent<BreezeDamageBase>().System = this;
                }
            }

            CurrentHealth = StartingHealth;

            anim.SetBool("Equips", UseEquipSystem);

            if (!PlayerTag.Equals("Untagged"))
            {
                GameObject player = GameObject.FindWithTag(PlayerTag);

                if (player != null && player.GetComponent<BreezePlayer>() != null)
                {
                    playerObject = player.transform;
                    if (AIBehaviour == BreezeEnums.AIBehaviour.Companion)
                    {
                        PlayerScript = player.GetComponent<BreezePlayer>();
                        PlayerScript.gotAttackedEvent.AddListener(playerWasAttacked);
                    }
                }
            }

            if (ExtendedAttackDistance <= AttackDistance)
                ExtendedAttackDistance = AttackDistance + 3f;

            if (AttackDistance > 25f)
            {
                AttackDistance = 25f;
                ExtendedAttackDistance = 30f;
            }

            if (DetectionDistance <= AttackDistance)
                DetectionDistance = AttackDistance + 3f;

            foreach (var particle in ParticlesList.Where(particle => particle.EffectObject == null))
            {
                ParticlesList.Remove(particle);
            }

            EquipEventCalled = !UseEquipSystem;

            BreezeEvents = GetComponent<BreezeEvents>();
            Invoke(nameof(disableErrorChecking), 3f);
            initializeIntegrations();
        }
        
        //Check For Automatic Integrations
        private void initializeIntegrations()
        {
#if NEOFPS
            if (FindObjectOfType<BasicHealthManager>())
            {
                gameObject.AddComponent<BreezeNeoFPS>();
                
                BreezeDamageBase[] bones = GetComponentsInChildren<BreezeDamageBase>();

                foreach (var bone in bones)
                {
                    if (bone.GetComponent<BreezeNeoFPS>() == null)
                    {
                        bone.gameObject.AddComponent<BreezeNeoFPS>();
                    }
                }
            }
#endif
            
#if HQ_FPS_TEMPLATE
            if (FindObjectOfType<Player>())
            {
                gameObject.AddComponent<BreezeHQFPS>();
                
                BreezeDamageBase[] bones = GetComponentsInChildren<BreezeDamageBase>();

                foreach (var bone in bones)
                {
                    if (bone.GetComponent<BreezeHQFPS>() == null)
                    {
                        bone.gameObject.AddComponent<BreezeHQFPS>();
                    }
                }
            }
#endif
            
#if SURVIVAL_TEMPLATE_PRO
            if (FindObjectOfType<Player>())
            {
                gameObject.AddComponent<BreezeSTP>();
                
                BreezeDamageBase[] bones = GetComponentsInChildren<BreezeDamageBase>();

                foreach (var bone in bones)
                {
                    if (bone.GetComponent<BreezeSTP>() == null)
                    {
                        bone.gameObject.AddComponent<BreezeSTP>();
                    }
                }
            }
#endif
            
#if INVECTOR_MELEE
            if (FindObjectOfType<vThirdPersonController>())
            {
                gameObject.AddComponent<BreezeInvector>();
                
                BreezeDamageBase[] bones = GetComponentsInChildren<BreezeDamageBase>();

                foreach (var bone in bones)
                {
                    if (bone.GetComponent<BreezeInvector>() == null)
                    {
                        bone.gameObject.AddComponent<BreezeInvector>();
                    }
                }
            }
#endif
            
#if FIRST_PERSON_CONTROLLER || THIRD_PERSON_CONTROLLER
            if (FindObjectOfType<CharacterHealth>())
            {
                gameObject.AddComponent<BreezeOpsive>();
                
                BreezeDamageBase[] bones = GetComponentsInChildren<BreezeDamageBase>();

                foreach (var bone in bones)
                {
                    if (bone.GetComponent<BreezeOpsive>() == null)
                    {
                        bone.gameObject.AddComponent<BreezeOpsive>();
                    }
                }
            }
#endif
        }

        #endregion

        #region Core Functions

        private void Update()
        {
            //Check editor errors
            if (hasErrors && !disableErrorCheck)
            {
                if (PlayerPrefs.HasKey("DontShowAgainPAIError") &&
                    PlayerPrefs.GetInt("DontShowAgainPAIError").Equals(1))
                {
                    nav.enabled = false;
                    anim.enabled = false;
                    stopAI = true;
                    gameObject.SetActive(false);
                    enabled = false;
                    return;
                }
#if UNITY_EDITOR
                if (EditorUtility.DisplayDialog("Warning",
                        "Your AI object [" + name +
                        "] has errors on it's setup. Please check the errors on the inspector and fix them. Your AI object will be disabled now.",
                        "Okay.", "Don't show again."))
                {
                    nav.enabled = false;
                    anim.enabled = false;
                    stopAI = true;
                    gameObject.SetActive(false);
                    enabled = false;
                }
                else
                {
                    PlayerPrefs.SetInt("DontShowAgainPAIError", 1);
                    nav.enabled = false;
                    anim.enabled = false;
                    stopAI = true;
                    gameObject.SetActive(false);
                    enabled = false;
                }
#else
                PlayerPrefs.SetInt("DontShowAgainPAIError", 1);
                nav.enabled = false;
                anim.enabled = false;
                stopAI = true;
                gameObject.SetActive(false);
                enabled = false;
#endif
            }
            
            //Check Current Target Health & Stop State
            if (CurrentHealth <= 0)
            {
                return;
            }
            
            //Check Sound
            if (searchingSound)
            {
                SetNavMovement(false);
                SetDestination(soundSource.position);
                PlayAnimation(BreezeEnums.AnimationType.Walk);
                // RotateToObject(soundSource.gameObject);

                if ((GetDistanceReached() && anim.GetFloat("Speed") > 0.1f) || GetTarget() != null)
                {
                    ResetNav();
                    searchingSound = false;
                    stopAI = false;
                    soundSource = null;
                }
            }
            
            UpdateMovement();

            if(stopAI)
                return;

            //Check If Movement Changed
            if (savedMovement != CurrentMovement)
            {
                BreezeEvents.OnMovementChanged.Invoke(CurrentMovement);
                savedMovement = CurrentMovement;
            }

            if (FocusOnTarget != null)
            {
                if (GetTarget() == null)
                {
                    FocusOnTarget = null;   
                }
                else if (!GetTarget().Equals(FocusOnTarget))
                {
                    FocusOnTarget = null;
                }
            }

            TargetIsPlayer = PlayerScript != null && TargetAIScript == null;


            //Check If The AI Target Really Lost
            if (UseAimIK && AIConfidence == BreezeEnums.AIConfidence.Brave && BodyWeight > 0.25f &&
                GetTarget() == null)
            {
                CancelInvoke(nameof(AttackWithDelay));
                ResetAnimParams();
                CheckNotTargetRightHelper();
            }

            //Check Getting Hit State Of The AI
            if (CheckCurrentStateTag("Hit") && !HitStarted)
            {
                HitStarted = true;
                GettingHit = true;
            }

            if (!CheckCurrentStateTag("Hit") && HitStarted)
            {
                HitStarted = false;
                GettingHit = false;
                Attacking = false;
                SingleCanFire = true;

                if (ExitNeutral > 0)
                {
                    if (AIConfidence == BreezeEnums.AIConfidence.Neutral)
                    {
                        TempBrave = true;
                        AIConfidence = BreezeEnums.AIConfidence.Brave;
                    }

                    if (ExitNeutral == 2)
                    {
                        TargetAIScript = null;
                        PlayerScript = ExitSender.GetComponent<BreezePlayer>();
                    }
                    else
                    {
                        PlayerScript = null;
                        TargetAIScript = ExitSender.GetComponent<BreezeSystem>();
                    }
                    
                    ExitNeutral = 0;
                    CurrentTarget = ExitSender;
                    ExitSender = null;
                }
                
                if(!EquipEventCalled)
                    return;

                if (UseAimIK)
                {
                    //Enables AIM IK
                    if (GetTarget() != null && UseAimIK && BodyWeight <= 0.25f && (EquippedWeapon || !UseEquipSystem))
                        StartCoroutine(FadeInBodyIK());
                }

                if (UseHandIK)
                {
                    EnableHandIKCommand();
                }
            }

            float layerGoal = WeaponType switch
            {
                //Adjust Animation Layer Weight
                BreezeEnums.WeaponType.Shooter => (anim.GetCurrentAnimatorStateInfo(0).IsTag("Layer") ||
                                                      anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack") ||
                                                      anim.GetCurrentAnimatorStateInfo(0).IsTag("Reload")) &&
                                                     !GettingHit
                    ? 1
                    : 0,
                BreezeEnums.WeaponType.Melee => 0,
                _ => 0
            };

            if (WeaponType != BreezeEnums.WeaponType.Unarmed)
            {
                anim.SetLayerWeight(1, Mathf.Lerp(anim.GetLayerWeight(1), layerGoal, (GettingHit ? 3f : 1.5f) * Time.deltaTime));
            }

            //Update Core Movement & Debugs
            UpdateMovement();
            UpdateDebug();
            UpdateStateDebug();

            if (AIConfidence == BreezeEnums.AIConfidence.Coward)
                LastKnownTargetPosition = Vector3.zero;

            //Check AI Patrol Availability
            if (GetTarget() == null && LastKnownTargetPosition == Vector3.zero)
            {
                Fleeing = false;
                if (AIBehaviour == BreezeEnums.AIBehaviour.Companion)
                {
                    CompanionFunctions();
                }
                else
                {
                    UpdateWandering();
                }
            }
            else
            {
                setCompanionLook(false);
            }

            //Brave AI Functions
            if (AIConfidence == BreezeEnums.AIConfidence.Brave)
            {
                //Check Target Availability
                if (GetTarget() != null && LastKnownTargetPosition == Vector3.zero)
                {
                    //Check If Target Is Reachable
                    ChaseTarget();
                }
                else if (LastKnownTargetPosition != Vector3.zero)
                {
                    //Search For Player
                    FirstWaypoint = true;
                    FirstWander = true;
                    PlayAnimation(BreezeEnums.AnimationType.Walk);
                    SetDestination(LastKnownTargetPosition);

                    if (GetDistanceReached())
                    {
                        LastKnownTargetPosition = Vector3.zero;
                    }
                }

                //Check If Target Is Alive
                if (!CheckTargetHealth())
                {
                    CurrentTarget = null;
                    CancelInvoke(nameof(AttackWithDelay));
                    ResetAnimParams();
                    CheckNotTargetRightHelper();
                }

                //Enter Combat State
                if (GetTarget() != null && !anim.GetBool("Combating") && !Combating && !switchingWeapon)
                {
                    BreezeEvents.OnAlertState.Invoke(true);
                    BreezeSounds.PlaySound(SoundType.Alerted);
                    anim.SetBool("Combating", true);
                    Combating = true;

                    if (WeaponType == BreezeEnums.WeaponType.Shooter && !UseEquipSystem)
                    {
                        EnableIK();
                        OnEquipChanged.Invoke(true, false);
                    }
                }
            }
            //Coward Functions
            else if (AIConfidence == BreezeEnums.AIConfidence.Coward)
            {
                //Flee Away From Target
                if (GetTarget() != null || Fleeing)
                {
                    Flee();
                }

                //Enter Combat State
                if (GetTarget() != null && !Combating)
                {
                    Combating = true;
                }
            }

            //Return To Neutral Confidence If Needed
            if (TempBrave && GetTarget() == null)
            {
                TempBrave = false;
                AIConfidence = BreezeEnums.AIConfidence.Neutral;
            }


            //Check The Availability For Exiting Combat State
            if (GetTarget() == null && anim.GetBool("Combating") && !InvokedAlert)
            {
                CheckNotTargetRight();
            }


            //Health Regeneration
            if (RegenerateHealth && !isRegenHealth && CurrentHealth < RegenerateHealthLimit)
            {
                if (RegenerateCondition == BreezeEnums.RegenerateCondition.NonCombatState && !Combating)
                {
                    Invoke(nameof(startRegen), RegenerateStartDelay);
                }
                else if (RegenerateCondition == BreezeEnums.RegenerateCondition.CombatState && Combating)
                {
                    Invoke(nameof(startRegen), RegenerateStartDelay);
                }
                else if (RegenerateCondition == BreezeEnums.RegenerateCondition.Both)
                {
                    Invoke(nameof(startRegen), RegenerateStartDelay);
                }
                else
                {
                    Invoke(nameof(startRegen), RegenerateStartDelay);
                }
            }

            //Attach Target Scripts
            if (TargetAIScript == null && PlayerScript == null && GetTarget() != null)
            {
                if (TargetIsPlayer)
                {
                    PlayerScript = GetTarget().GetComponent<BreezePlayer>();
                }
                else
                {
                    TargetAIScript = GetTarget().GetComponent<BreezeSystem>();
                }
            }

            PlayerScript = CurrentTarget == null ? null : PlayerScript;
            TargetAIScript = CurrentTarget == null ? null : TargetAIScript;
        }

        private void startRegen()
        {
            CancelInvoke(nameof(startRegen));
            StartCoroutine(RegainHealthOverTime());
        }

        #endregion

        #region Brave AI Functions

        //Chase Target
        private void ChaseTarget()
        {
            //Rotate To Target
            if(!nav.hasPath)
                RotateToObject(GetTarget());

            //Check Too Close
            if (((GetTargetDistance() <= EnemyTooCloseDistance && !backingAway) ||
                 (GetTargetDistance() <= EnemyTooCloseDistance * BackawayMultiplier && backingAway) && !Blocking &&
                 (!Attacking || WeaponType == BreezeEnums.WeaponType.Shooter) &&
                 !anim.GetCurrentAnimatorStateInfo(0).IsTag("Hit")))
            {
                if (WeaponType != BreezeEnums.WeaponType.Shooter)
                    CancelInvoke(nameof(AttackWithDelay));

                if (BackAway())
                {
                    if (WeaponType != BreezeEnums.WeaponType.Shooter)
                        return;
                }
            }

            backingAway = false;

            //Approach To Target
            if (ShouldChaseTarget)
            {
                //Patch Checker Variables
                Vector3 pos = GetTarget().transform.position;

                //Check Distance
                if ((Attacking && GetTargetDistance() > ExtendedAttackDistance &&
                     WeaponType != BreezeEnums.WeaponType.Shooter) ||
                    (Attacking && GetTargetDistance() > AttackDistance &&
                     WeaponType == BreezeEnums.WeaponType.Shooter) ||
                    (!Attacking && GetTargetDistance() > AttackDistance))
                {
                    if (WeaponType != BreezeEnums.WeaponType.Shooter)
                        CancelInvoke(nameof(AttackWithDelay));

                    //Check Path
                    Vector3 newPos = CheckPath(pos);
                    if (!newPos.Equals(Vector3.zero))
                    {
                        pos = newPos;
                    }
                    else
                    {
                        ResetNav();
                        SetNavMovement();
                        PlayAnimation(BreezeEnums.AnimationType.Idle);
                        return;
                    }

                    if (Vector3.Distance(pos, transform.position) <= StopDistance())
                    {
                        Attacking = false;
                        PlayAnimation(BreezeEnums.AnimationType.Idle);
                        SetNavMovement();
                        ResetNav();
                    }
                    else
                    {
                        Attacking = false;

                        PlayAnimation(GetTargetDistance() <= ForceWalkDistance
                            ? BreezeEnums.AnimationType.Walk
                            : BreezeEnums.AnimationType.Run);

                        SetDestination(pos);
                    }
                }
                //Check If Ready To Attack
                else if (!Blocking && (SingleCanFire || WeaponType != BreezeEnums.WeaponType.Shooter))
                {
                    //Check If AI Is Switching Weapon
                    if (switchingWeapon)
                    {
                        Attacking = false;
                        SingleCanFire = true;
                        CancelInvoke(nameof(AttackWithDelay));
                        return;
                    }

                    //Check If Weapon Is Equipped
                    if (WeaponType != BreezeEnums.WeaponType.Unarmed && !EquippedWeapon)
                    {
                        if (UseEquipSystem)
                            return;
                    }

                    //Check If It's Currently Attacking
                    if (!Attacking && !anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
                    {
                        // if(WeaponType != BreezeEnums.WeaponType.Shooter && TargetAIScript != null && TargetAIScript.Attacking)
                        //     return;
                        
                        //Attack!
                        if (FirstAttack && GetTargetDistance() <= ExtendedAttackDistance)
                        {
                            PlayAnimation(BreezeEnums.AnimationType.Attack);
                        }
                        else
                        {
                            PlayAnimation(BreezeEnums.AnimationType.Idle);
                            Invoke(nameof(AttackWithDelay), Random.Range(MinAttackDelay, MaxAttackDelay));
                        }
                    }
                }
            }
            else if (!Blocking && SingleCanFire)
            {
                //Check If AI Is Switching Weapon
                if (switchingWeapon)
                {
                    Attacking = false;
                    SingleCanFire = true;
                    CancelInvoke(nameof(AttackWithDelay));
                    return;
                }

                //Check If Weapon Is Equipped
                if (WeaponType != BreezeEnums.WeaponType.Unarmed && !EquippedWeapon)
                {
                    if (UseEquipSystem)
                        return;
                }
                
                
                RotateToObject(GetTarget());
                
                //Check If It's Currently Attacking
                if (!Attacking && !anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
                {
                    //Attack!
                    if (FirstAttack && GetTargetDistance() <= ExtendedAttackDistance)
                    {
                        PlayAnimation(BreezeEnums.AnimationType.Attack);
                    }
                    else
                    {
                        PlayAnimation(BreezeEnums.AnimationType.Idle);
                        Invoke(nameof(AttackWithDelay), Random.Range(MinAttackDelay, MaxAttackDelay));
                    }
                }
            }
        }

        public void AttackWithDelay()
        {
            //Cancel Previous Attack Attempts
            CancelInvoke(nameof(AttackWithDelay));

            //Play Attack Animation
            PlayAnimation(BreezeEnums.AnimationType.Attack);
        }

        //Initialize Attack From Animation Event
        public void BreezeAttackEvent()
        {
            if (CurrentHealth <= 0)
                return;
            
            BreezeEvents.OnAnimationEvent.Invoke("BreezeAttackEvent");

            //Check If Target Alive
            if (!CheckTargetHealth())
            {
                CurrentTarget = null;
                CancelInvoke(nameof(AttackWithDelay));
                ResetAnimParams();
                CheckNotTargetRightHelper();
                return;
            }

            if (GetTarget() == null || GetTargetDistance() > ExtendedAttackDistance)
                return;

            Attacking = false;
            PlayAnimation(BreezeEnums.AnimationType.Idle);

            if (WeaponType != BreezeEnums.WeaponType.Unarmed)
                return;

            //Spawn Particle Effect
            if (ParticlesList.Count > 0)
            {
                foreach (var effect in ParticlesList)
                {
                    if (effect.ObjectTag.Equals(GetTarget().tag))
                    {
                        if (!TargetIsPlayer)
                        {
                            if ((effect.EffectType == BreezeMeleeWeapon.impactEffect.effectType.Blocked &&
                                 TargetAIScript.Blocking) ||
                                effect.EffectType == BreezeMeleeWeapon.impactEffect.effectType.Normal &&
                                !TargetAIScript.Blocking)
                            {
                                Destroy(Instantiate(effect.EffectObject,
                                    (effect.CustomTransform == null
                                        ? GetTarget().transform.position + effect.SpawnOffset
                                        : effect.CustomTransform.position), Quaternion.identity), effect.DestroyAfter);
                            }
                        }
                        else
                        {
                            Destroy(Instantiate(effect.EffectObject,
                                (effect.CustomTransform == null
                                    ? GetTarget().transform.position + effect.SpawnOffset
                                    : effect.CustomTransform.position), Quaternion.identity), effect.DestroyAfter);
                        }
                    }
                }
            }

            //Initialize Damage
            if (TargetIsPlayer)
            {
                PlayerScript.TakeDamage(currentDamageAmount, gameObject);
                BreezeEvents.OnDealDamage.Invoke(currentDamageAmount);
                BreezeSounds.PlaySound(SoundType.AttackSuccessful);
            }
            else if (!TargetAIScript.Blocking)
            {
                TargetAIScript.TakeDamage(currentDamageAmount, gameObject, false);
                BreezeEvents.OnDealDamage.Invoke(currentDamageAmount);
                BreezeSounds.PlaySound(SoundType.AttackSuccessful);
            }
            else
            {
                BreezeSounds.PlaySound(SoundType.AttackMissed);
            }
        }

        #endregion

        #region Coward AI Functions

        //Flee Away System
        private bool FleeDone = true;
        private GameObject StoredTarget;
        private void Flee()
        {
            //Stop AI Movement
            SetNavMovement(false);

            //Flee Away
            if (!Fleeing)
            {
                BreezeEvents.OnFleeAway.Invoke();
                PlayAnimation(BreezeEnums.AnimationType.Run);
                SetDestination(GetFleePosition());
                StoredTarget = GetTarget();
                FleeDone = false;
                Fleeing = true;
            }
            else if(StoredTarget != null)
            {
                if (!FleeDone)
                {
                    if(GetDistanceReached())
                    {
                        if (Vector3.Distance(transform.position, StoredTarget.transform.position) >=
                            DetectionDistance * 2)
                        {
                            SetNavMovement();
                            ResetNav();
                            ResetAnimParams();
                            Fleeing = false;
                            FleeDone = true;
                        }
                        else
                        {
                            PlayAnimation(BreezeEnums.AnimationType.Run);
                            SetDestination(GetFleePosition());
                        }
                    }
                }
            }
            else
            {
                SetNavMovement();
                ResetNav();
                ResetAnimParams();
                Fleeing = false;
                FleeDone = true;
            }
        }

        #endregion

        #region Companion Functions

        //Check Player Attack
        private void playerWasAttacked(GameObject sender)
        {
            if (GetTarget() != null || AIBehaviour != BreezeEnums.AIBehaviour.Companion)
                return;
            
            BreezeEvents.OnAnimationEvent.Invoke("PlayerWasAttacked");

            TargetIsPlayer = false;
            TargetAIScript = sender.GetComponent<BreezeSystem>();
            if (TargetAIScript == null)
                return;
            CurrentTarget = sender;
        }

        private bool GetPlayerAngle()
        {
            Vector3 direction = (new Vector3(playerObject.transform.position.x,
                playerObject.transform.position.y + playerObject.transform.position.y / 2,
                playerObject.transform.position.z)) - HeadTransform.position;
            float angle = Vector3.Angle(new Vector3(direction.x, 0, direction.z), transform.forward);

            return angle <= 55;
        }

        private void setCompanionLook(bool on)
        {
            if (on)
            {
                if (!GetPlayerAngle())
                {
                    UseCompanionLookIK = false;
                    return;
                }

                UseCompanionLookIK = true;
            }
            else
            {
                UseCompanionLookIK = false;
            }
        }

        //Companion Functions
        private void CompanionFunctions()
        {
            //Check Too Close Distance
            if ((GetPlayerDistance() <= CTooCloseDistance && !backingAway) ||
                (GetPlayerDistance() <= CTooCloseDistance * BackawayMultiplier && backingAway))
            {
                if (BackAwayCompanion())
                {
                    setCompanionLook(true);
                    return;
                }
            }

            //Variables For Path Checking
            Vector3 pos = playerObject.position;

            //Check Distance
            if (GetPlayerDistance() > CSprintFollowDistance)
            {
                setCompanionLook(false);
                //Check Path
                Vector3 newPos = CheckPath(pos);
                if (!newPos.Equals(Vector3.zero))
                {
                    pos = newPos;
                }
                else
                {
                    ResetNav();
                    SetNavMovement();
                    PlayAnimation(BreezeEnums.AnimationType.Idle);
                    return;
                }

                // RotateToObject(playerObject.gameObject);
                PlayAnimation(BreezeEnums.AnimationType.Run);
                SetDestination(pos);
            }
            else if (GetPlayerDistance() > CFollowingDistance)
            {
                setCompanionLook(false);
                //Check Path
                Vector3 newPos = CheckPath(pos);
                if (!newPos.Equals(Vector3.zero))
                {
                    pos = newPos;
                }
                else
                {
                    ResetNav();
                    SetNavMovement();
                    PlayAnimation(BreezeEnums.AnimationType.Idle);
                    return;
                }
                
                // RotateToObject(playerObject.gameObject);
                PlayAnimation(BreezeEnums.AnimationType.Walk);
                SetDestination(pos);
            }
            else
            {
                setCompanionLook(true);
                ResetNav();
                SetNavMovement();
                PlayAnimation(BreezeEnums.AnimationType.Idle);
            }
        }

        #endregion

        #region Movement Functions

        //Patrol Wandering Method
        private void PatrolAI()
        {
            //Play Animation
            if (Patrolling && !CheckCurrentStateTag("Hit") && !CheckCurrentStateTag("Attack") && !Combating &&
                CurrentMovement != BreezeEnums.MovementType.Walking)
            {
                PlayAnimation(BreezeEnums.AnimationType.Walk);
            }

            //Check If The Path End
            if (GetDistanceReached())
            {
                if (!IsPatrolIdle)
                {
                    SetNavMovement();
                    PlayAnimation(BreezeEnums.AnimationType.Idle);
                    IsPatrolIdle = true;
                    Patrolling = false;
                    Invoke(nameof(StartPatrolling), FirstWander ? 0 : Random.Range(MinIdleLength, MaxIdleLength + 1f));
                }
            }
        }

        //Gets A New Patrol Point
        private void StartPatrolling()
        {
            FirstWander = false;
            IsPatrolIdle = false;
            SetNavMovement(false);
            SetDestination(GetPatrolPosition());

            if (!GetDestination().Equals(Vector3.zero))
            {
                PlayAnimation(BreezeEnums.AnimationType.Walk);
                Patrolling = true;
            }
            else
            {
                StartPatrolling();
            }
        }

        //Updates the speed of the agent & animator
        private void UpdateMovement()
        {
            anim.applyRootMotion = UseRootMotion;

            if (stopAI && nav.hasPath)
            {
                if (GetDistanceReached())
                {
                    ResetNav();
                    SetNavMovement();
                    stopAI = false;
                }
            }

            //Smooth transition between movement animations
            if (CurrentMovement == BreezeEnums.MovementType.Running)
            {
                if (anim.GetFloat("Speed") >= 1.95f)
                {
                    anim.SetFloat("Speed", 2f);
                }
                else
                {
                    anim.SetFloat("Speed", 2, 0.15f, Time.deltaTime);
                }

                if (!UseRootMotion)
                {
#if !Breeze_AI_Pathfinder_Enabled
                    nav.speed = RunSpeed;
#else
                    nav.maxSpeed = RunSpeed;
#endif
                }
            }
            else if (CurrentMovement == BreezeEnums.MovementType.Walking)
            {
                if (anim.GetFloat("Speed") >= 0.95f && anim.GetFloat("Speed") <= 1f)
                {
                    anim.SetFloat("Speed", 1f);
                }
                else
                {
                    anim.SetFloat("Speed", 1, 0.15f, Time.deltaTime);
                }

                if (!UseRootMotion)
                {
#if !Breeze_AI_Pathfinder_Enabled
                    nav.speed = WalkSpeed;
#else
                    nav.maxSpeed = WalkSpeed;
#endif
                }
            }
            else if (CurrentMovement == BreezeEnums.MovementType.Backaway)
            {
                if (GetTargetDistance() > EnemyTooCloseDistance)
                {
                    CurrentMovement = BreezeEnums.MovementType.None;
                }

                if (anim.GetFloat("Speed") <= -0.95f)
                {
                    anim.SetFloat("Speed", -1f);
                }
                else
                {
                    anim.SetFloat("Speed", -1, 0.15f, Time.deltaTime);
                }

                if (!UseRootMotion)
                {
#if !Breeze_AI_Pathfinder_Enabled
                    nav.speed = WalkBackwardsSpeed;
#else
                    nav.maxSpeed = WalkBackwardsSpeed;
#endif
                }
            }
            else
            {
                if (anim.GetFloat("Speed") <= 0.05f && anim.GetFloat("Speed") > -0.05f)
                {
                    anim.SetFloat("Speed", 0f);
                }
                else
                {
                    anim.SetFloat("Speed", 0, 0.15f, Time.deltaTime);
                }

                SetNavMovement();
                ResetNav();
            }

            //Update Direction
            if (CurrentMovement != BreezeEnums.MovementType.None)
            {
                Vector3 velocity = Quaternion.Inverse(transform.rotation) * nav.desiredVelocity;
                float angle = Mathf.Atan2(velocity.x, velocity.z) * 180.0f / (float)Math.PI;
                anim.SetFloat("Direction", -angle, 0.15f, Time.deltaTime);
            }
            else
            {
                anim.SetFloat("Direction", 0f);
            }

            //Update Root-Motion
            if (UseRootMotion)
            {
#if !Breeze_AI_Pathfinder_Enabled
                nav.speed = 0.1f;
#else
                nav.maxSpeed = 0.1f;
#endif
            }

#if !Breeze_AI_Pathfinder_Enabled
            nav.nextPosition = transform.position;
#endif
        }

        //Back away from Target
        private bool BackAway()
        {
            if (switchingWeapon)
                return false;

            //Get New Position Point
            Vector3 difftest = transform.position - GetTarget().transform.position;
            difftest.y = 0;
            Vector3 BackupDestinationtest =
                GetTarget().transform.position + difftest.normalized * (2f * EnemyTooCloseDistance);

            //Check If It's reachable
            if (!CalculatePath(BackupDestinationtest))
            {
                return false;
            }

            if (Physics.Raycast(transform.position,
                    difftest.normalized, out _,
                    ((int)Vector3.Distance(BackupDestinationtest, transform.position) + 1f), ObstacleLayers,
                    QueryTriggerInteraction.Ignore))
            {
                return false;
            }
            RotateToObject(GetTarget());

            SetDestination(BackupDestinationtest);
            PlayAnimation(BreezeEnums.AnimationType.BackAway);
            backingAway = true;
            return true;
        }

        //Back away from Companion Player
        private bool BackAwayCompanion()
        {
            if (switchingWeapon)
                return false;

            //Get New Position Point
            Vector3 difftest = transform.position - playerObject.transform.position;
            difftest.y = 0;
            Vector3 BackupDestinationtest =
                playerObject.transform.position + difftest.normalized * (2f * CTooCloseDistance);

            //Check If It's reachable
            if (!CalculatePath(BackupDestinationtest))
            {
                return false;
            }

            if (Physics.Raycast(transform.position,
                    difftest.normalized, out _,
                    ((int)Vector3.Distance(BackupDestinationtest, transform.position) + 1f), ObstacleLayers,
                    QueryTriggerInteraction.Ignore))
            {
                return false;
            }


            RotateToObject(playerObject.gameObject);
            SetDestination(BackupDestinationtest);
            PlayAnimation(BreezeEnums.AnimationType.BackAway);
            backingAway = true;
            return true;
        }

        //Updates AI Wandering Methods
        private void UpdateWandering()
        {
            FirstAttack = true;

            //Check the wandering type
            if (WanderType == BreezeEnums.AIWanderingType.Patrol)
            {
                PatrolAI();
            }
            else if (WanderType == BreezeEnums.AIWanderingType.Waypoint)
            {
                WaypointWander();
            }
            else
            {
                SetNavMovement();
                PlayAnimation(BreezeEnums.AnimationType.Idle);
            }
        }

        //Update Waypoint Wandering
        private void WaypointWander()
        {
            if (WaypointOrder == BreezeEnums.WaypointOrder.InOrder)
            {
                if (NavHasPath() && GetDistanceReached())
                {
                    ResetNav();
                    if (Waypoints[currentWaypoint].WaitOnWaypoint)
                    {
                        PlayAnimation(BreezeEnums.AnimationType.Idle);
                        Invoke(nameof(EndWaypoint),
                            Random.Range(Waypoints[currentWaypoint].MinIdleLength,
                                Waypoints[currentWaypoint].MaxIdleLength + 1f));
                    }
                    else
                    {
                        EndWaypoint();
                    }
                }

                if (FirstWaypoint)
                {
                    FirstWaypoint = false;
                    currentWaypoint = 0;
                    PlayAnimation(BreezeEnums.AnimationType.Walk);
                    SetDestination(Waypoints[currentWaypoint].transform.position);
                }
            }

            if (WaypointOrder == BreezeEnums.WaypointOrder.Random)
            {
                if (NavHasPath() && GetDistanceReached())
                {
                    ResetNav();
                    if (Waypoints[currentWaypoint].WaitOnWaypoint)
                    {
                        PlayAnimation(BreezeEnums.AnimationType.Idle);
                        Invoke(nameof(EndWaypoint),
                            Random.Range(Waypoints[currentWaypoint].MinIdleLength,
                                Waypoints[currentWaypoint].MaxIdleLength + 1f));
                    }
                    else
                    {
                        EndWaypoint();
                    }
                }

                if (FirstWaypoint)
                {
                    FirstWaypoint = false;
                    currentWaypoint = 0;
                    GoToCheckpoint();
                }
            }
        }

        private void GoToCheckpoint()
        {
            //Select Waypoint Point
            bool Valid = false;
            while (!Valid)
            {
                if (!CheckPath(Waypoints[currentWaypoint].transform.position).Equals(Vector3.zero))
                {
                    if (currentWaypoint == (Waypoints.Count - 1))
                    {
                        break;
                    }

                    currentWaypoint++;
                }
                else
                {
                    Valid = true;
                }
            }

            if (!Valid)
            {
                WanderType = BreezeEnums.AIWanderingType.Patrol;
                return;
            }

            PlayAnimation(BreezeEnums.AnimationType.Walk);
            SetDestination(Waypoints[currentWaypoint].transform.position);
        }

        public void EndWaypoint()
        {
            //End Waypoint Movement
            if (WaypointOrder == BreezeEnums.WaypointOrder.InOrder)
            {
                if (currentWaypoint == (Waypoints.Count - 1))
                {
                    currentWaypoint = 0;
                }
                else
                {
                    currentWaypoint++;
                }
            }
            else
            {
                int oldCurrentWaypoint = currentWaypoint;

                while (currentWaypoint.Equals(oldCurrentWaypoint))
                {
                    currentWaypoint = Random.Range(0, Waypoints.Count);
                }
            }

            if (!NavHasPath())
            {
                GoToCheckpoint();
            }
        }

        #endregion

        #region Core Helper Methods

        private void CheckNotTargetRight()
        {
            if (GetTarget() != null || VisibleTargets.Count > 0)
                return;

            InvokedAlert = true;
            float rand = Random.Range(minAlertLength, maxAlertLength);
            Invoke(nameof(DisableAlertMode), rand);
        }

        private void CheckNotTargetRightHelper()
        {
            if (VisibleTargets.Count > 0)
                return;

            StartCoroutine(FadeOutBodyIK());
            BreezeSounds.PlaySound(SoundType.LostTarget);
        }

        //Closes Alert Mode
        public void DisableAlertMode()
        {
            if (GetTarget() != null)
                return;

            InvokedAlert = false;
            BreezeEvents.OnAlertState.Invoke(false);
            anim.SetBool("Combating", false);
            Combating = false;

            DetectionAngle /= 1.4f;
            DetectionDistance /= 1.4f;
        }
        
        //Check Incoming Sound
        private bool searchingSound;
        private Transform soundSource;
        public void NotifySound(GameObject source, bool IsPlayer = false)
        {
            if(source == null || searchingSound || GetTarget() != null)
                return;
            
            if(Vector3.Distance(transform.position, source.transform.position) >= SoundDetectionLimit)
                return;
            
            bool soundConfirmed = false;
            
            if (IsPlayer)
            {
                BreezePlayer player = source.GetComponent<BreezePlayer>();
                
                if(player == null)
                    return;

                if (AIPlayerBehaviour != BreezeEnums.BehaviourType.Friendly)
                {
                    soundConfirmed = true;
                }
            }
            else if(source.GetComponent<BreezeSystem>() != null)
            {
                if (CheckFaction(source.GetComponent<BreezeSystem>().CurrentAIFaction))
                {
                    soundConfirmed = true;
                }
            }
            else
            {
                soundConfirmed = true;
            }

            if (soundConfirmed)
            {
                tempSource = source;
                Invoke(nameof(checkHitTakenFirst), 0.3f);
            }
        }

        private GameObject tempSource;
        private void checkHitTakenFirst()
        {
            if(CheckCurrentStateTag("Hit") || GetTarget() != null)
                return;

            stopAI = true;
            soundSource = tempSource.transform;
            searchingSound = true;
            tempSource = null;
        }
        //Regen Health Overtime
        private IEnumerator RegainHealthOverTime()
        {
            if (!isRegenHealth)
            {
                BreezeEvents.OnRegenHealth.Invoke(true);
                isRegenHealth = true;
            }

            while (CurrentHealth < RegenerateHealthLimit)
            {
                if (CurrentHealth <= 0f)
                    break;

                CurrentHealth += RegenerateAmount;
                yield return new WaitForSeconds(RegenerateFrequency);
            }

            if (CurrentHealth <= 0f)
                yield break;

            CurrentHealth = RegenerateHealthLimit;
            if (isRegenHealth)
            {
                BreezeEvents.OnRegenHealth.Invoke(false);
                isRegenHealth = false;
            }
        }

        //Check If Notified Is An Enemy
        private void CheckNotify(GameObject target)
        {
            BreezeSystem system = target.GetComponent<BreezeSystem>();

            if (system != null)
            {
                if(system.Equals(this))
                    return;

                if (!CheckFaction(system.CurrentAIFaction)) 
                    return;
                
                TargetAIScript = system;
                PlayerScript = null;
                BreezeEvents.OnFoundTarget.Invoke(target);
                CurrentTarget = target;
            }
            else
            {
                BreezePlayer player = target.GetComponent<BreezePlayer>();

                if (player == null || AIBehaviour == BreezeEnums.AIBehaviour.Companion) 
                    return;

                if (AIPlayerBehaviour != BreezeEnums.BehaviourType.Enemy) 
                    return;
                
                TargetAIScript = null;
                PlayerScript = player;
                BreezeEvents.OnFoundTarget.Invoke(target);
                CurrentTarget = target;
            }
        }

        //Optimization
        private bool LodDisabled;
        private void checkLastLod()
        {
            if (CurrentHealth <= 0)
                return;

            if (UseLodOptimization == BreezeEnums.YesNo.No || LodRenderer == null)
                return;

            if (AIBehaviour == BreezeEnums.AIBehaviour.Companion)
                return;

            if (!LodRenderer.isVisible && !LodDisabled)
            {
                stopAI = true;
                if (col != null)
                    col.enabled = false;
                LodDisabled = true;
                BreezeEvents.OnPauseAIState.Invoke(stopAI);
                BreezeEvents.OnVisibleState.Invoke(false);
            }

            if (LodRenderer.isVisible && LodDisabled)
            {
                stopAI = false;
                if (col != null)
                    col.enabled = true;
                LodDisabled = false;
                BreezeEvents.OnPauseAIState.Invoke(stopAI);
                BreezeEvents.OnVisibleState.Invoke(true);
            }
        }

        //Companion Functions

        private float GetPlayerDistance()
        {
            if (playerObject == null)
                return -1;

            return Vector3.Distance(transform.position, playerObject.position);
        }

        private void Detection()
        {
            if (FocusOnTarget != null)
            {
                return;
            }
                
            
            Collider[] pTargets = new Collider[1000];
            Physics.OverlapSphereNonAlloc(transform.position, DetectionDistance, pTargets, DetectionLayers);

            if (DetectionLock == BreezeEnums.DetectionLock.Closest && pTargets[0] != null)
            {
                try
                {
                    pTargets = pTargets.OrderBy(c => (transform.position - c.transform.position).sqrMagnitude)
                        .ToArray();
                }
                catch
                {
                    // ignored
                }
            }


            //Check If The Current Target Is Visible
            if (GetTarget() != null)
            {
                bool TargetVisible = false;

                foreach (var tar in pTargets)
                {
                    if (tar == null)
                        continue;

                    BreezeDamageable damageBase = tar.gameObject.GetComponent<BreezeDamageable>();

                    if (damageBase == null)
                    {
                        BreezePlayer player = tar.gameObject.GetComponent<BreezePlayer>();

                        if (player == null)
                            continue;

                        if (player.gameObject.Equals(GetTarget()))
                        {
                            if (Physics.Linecast(transform.position + new Vector3(0, 1, 0),
                                    GetTarget().transform.position + new Vector3(0, 1, 0), out _,
                                    ObstacleLayers, QueryTriggerInteraction.Ignore))
                            {
                                break;
                            }

                            TargetVisible = true;
                            break;
                        }
                    }
                    else
                    {
                        if (damageBase.System.gameObject.Equals(GetTarget()))
                        {
                            if (Physics.Linecast(transform.position + new Vector3(0, 1, 0),
                                    GetTarget().transform.position + new Vector3(0, 1, 0), out _,
                                    ObstacleLayers, QueryTriggerInteraction.Ignore))
                            {
                                break;
                            }

                            TargetVisible = true;
                            break;
                        }
                    }
                }

                if (!TargetVisible)
                {
                    LastKnownTargetPosition = GetTarget().transform.position;
                    BreezeEvents.OnLostTarget.Invoke(CurrentTarget);
                    CurrentTarget = null;
                    CancelInvoke(nameof(AttackWithDelay));
                    ResetAnimParams();
                    CheckNotTargetRightHelper();
                    return;
                }

                LastKnownTargetPosition = Vector3.zero;

                if (DetectionLock == BreezeEnums.DetectionLock.OneByOne)
                    return;
            }

            VisibleTargets.Clear();

            //Check If The Targets Are Valid
            foreach (var Object in pTargets)
            {
                if (Object == null)
                    continue;
                
                GameObject CheckTarget = Object.gameObject;
                if (CheckTarget.tag.Equals(BreezeTag) || (CheckTarget.transform.root != null &&
                                                             CheckTarget.transform.root.tag.Equals(BreezeTag)))
                {
                    CheckTarget = CheckTarget.tag.Equals(BreezeTag)
                        ? CheckTarget
                        : CheckTarget.transform.root.gameObject;

                    BreezeDamageable damageable = CheckTarget.GetComponent<BreezeDamageable>();

                    if (damageable != null)
                    {
                        if (damageable.System == null || damageable.System.CurrentHealth <= 0f)
                        {
                            continue;
                        }

                        if(damageable.System.Equals(this))
                            continue;
                        
                        CheckTarget = damageable.System.gameObject;
                    }
                    else
                    {
                        continue;
                    }
                }
                else if (CheckTarget.tag.Equals(PlayerTag) || (CheckTarget.transform.root != null && CheckTarget.transform.root.tag.Equals(PlayerTag)))
                {
                    if (AIBehaviour == BreezeEnums.AIBehaviour.Companion)
                        continue;
                    
                    CheckTarget = CheckTarget.tag.Equals(PlayerTag)
                        ? CheckTarget
                        : CheckTarget.transform.root.gameObject;
                    

                    BreezePlayer TargetComponent = CheckTarget.GetComponent<BreezePlayer>();
                    if (TargetComponent == null || TargetComponent.CurrentHealth <= 0f)
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }

                if (Physics.Linecast(transform.position + new Vector3(0, 1, 0),
                        CheckTarget.transform.position + new Vector3(0, 1, 0), out var Hit,
                        ObstacleLayers, QueryTriggerInteraction.Ignore))
                {
                    if (Hit.transform.gameObject != gameObject)
                    {
                        continue;
                    }
                }


                if (CheckTarget.gameObject.Equals(gameObject))
                {
                    continue;
                }

                if (DetectionType == BreezeEnums.DetectionType.LineOfSight)
                {
                    Vector3 direction = (new Vector3(CheckTarget.transform.position.x,
                        CheckTarget.transform.position.y + CheckTarget.transform.localScale.y / 2,
                        CheckTarget.transform.position.z)) - HeadTransform.position;
                    float angle = Vector3.Angle(new Vector3(direction.x, 0, direction.z), transform.forward);
                    if (angle < (DetectionAngle / 2f))
                    {
                        if (!VisibleTargets.Contains(CheckTarget))
                            VisibleTargets.Add(CheckTarget);
                    }
                }
                else if (!IgnoredObjects.Contains(CheckTarget))
                {
                    if (!VisibleTargets.Contains(CheckTarget))
                        VisibleTargets.Add(CheckTarget);
                }
            }

            if (AIConfidence == BreezeEnums.AIConfidence.Neutral ||
                (AIBehaviour == BreezeEnums.AIBehaviour.Companion &&
                 AttackBehaviour == BreezeEnums.AttackBehaviour.Passive))
                return;

            foreach (var target in VisibleTargets)
            {
                TargetIsPlayer = target.tag.Equals(PlayerTag) || (target.transform.parent != null &&
                                                                  target.transform.parent.tag.Equals(PlayerTag));

                switch (TargetIsPlayer)
                {
                    case true when AIBehaviour == BreezeEnums.AIBehaviour.Companion:
                        continue;
                    
                    case true:
                    {
                        if (AIPlayerBehaviour == BreezeEnums.BehaviourType.Enemy)
                        {
                            TargetAIScript = null;
                            BreezeEvents.OnFoundTarget.Invoke(target);
                            CurrentTarget = target;
                            PlayerScript = target.GetComponent<BreezePlayer>();
                        }

                        break;
                    }
                    
                    default:
                    {
                        if (CheckFaction(target.GetComponent<BreezeDamageable>().System.CurrentAIFaction))
                        {
                            PlayerScript = null;
                            TargetAIScript = target.GetComponent<BreezeDamageable>().System;
                            BreezeEvents.OnFoundTarget.Invoke(target);
                            CurrentTarget = target;
                        }

                        break;
                    }
                }


                if (!Combating && NotifyCloseUnits == BreezeEnums.YesNo.Yes)
                    notifyUnits();
            }
        }

        //Notify Close Units
        private void notifyUnits()
        {
            if (GetTarget() == null)
                return;

            BreezeSystem[] allAISystems = FindObjectsOfType<BreezeSystem>();

            foreach (var system in allAISystems)
            {
                if (system.CurrentAIFaction == CurrentAIFaction && system.GetTarget() == null)
                {
                    if (Vector3.Distance(system.transform.position, transform.position) <= NotifyDistanceLimit)
                    {
                        system.CheckNotify(GetTarget());
                    }
                }
            }
        }

        #region Inverse Kinematics

        //Enables General IK Settings
        private void EnableIK()
        {
            if (switchingWeapon)
                return;

            if (Reloading || CurrentHealth <= 0)
            {
                return;
            }

            //Enables Hand IK
            if (!UseIK && (AIConfidence != BreezeEnums.AIConfidence.Coward || !UseEquipSystem))
            {
                UseIK = true;
                elapsedTime = 0;
            }

            //Enables AIM IK
            if (GetTarget() != null && UseAimIK && BodyWeight <= 0.25f && (EquippedWeapon || !UseEquipSystem))
                StartCoroutine(FadeInBodyIK());
        }

        //Called from animator
        public void DisableHandIK()
        {
            BreezeEvents.OnAnimationEvent.Invoke("DisableHandIK");
            UseIK = false;
            elapsedTime = 0;
        }

        public void EnableHandIK()
        {
            BreezeEvents.OnAnimationEvent.Invoke("EnableHandIK");
            UseIK = true;
            elapsedTime = 0;
        }
        
        //System Command
        private void EnableHandIKCommand()
        {
            Invoke(nameof(EnableHandIK), 0f);
        }

        //Disables General IK Settings
        private void DisableIK()
        {
            //Disables Hand IK
            UseIK = false;
            elapsedTime = 0;

            StopCoroutine(FadeInBodyIK());
            //Disables AIM IK
            if (BodyWeight > 0)
                StartCoroutine(FadeOutBodyIK());
        }

        //Checks General IK Settings
        private void CheckIK()
        {
            if (switchingWeapon)
                return;

            //Check If AIM Ik Finished
            if (BodyWeight >= 0.49f)
                StopCoroutine(FadeOutBodyIK());

            //Check If AIM Ik Is Ready
            if (UseAimIK && WeaponType == BreezeEnums.WeaponType.Shooter && BodyWeight < 0.35f &&
                AIConfidence == BreezeEnums.AIConfidence.Brave && !GettingHit && !Reloading && GetTarget() != null)
            {
                if (UseEquipSystem && !EquippedWeapon)
                    return;

                StartCoroutine(FadeInBodyIK());
            }
        }

        //Main AIM Ik Functions
        private Quaternion HeadLookQuaternion;
        private void LateUpdate()
        {
            checkLastLod();

            if (stopAI)
                return;

            if (anim.avatar != null && !anim.avatar.isHuman && AIBehaviour == BreezeEnums.AIBehaviour.Companion)
            {
                Quaternion newRot =
                    Quaternion.LookRotation((playerObject.transform.position - transform.position).normalized,
                        Vector3.up);
                
                newRot.eulerAngles = new Vector3(newRot.eulerAngles.x,
                    newRot.eulerAngles.y - transform.rotation.eulerAngles.y, newRot.eulerAngles.z);   
                
                HeadLookQuaternion = Quaternion.Lerp(HeadLookQuaternion, UseCompanionLookIK ? newRot : Quaternion.Euler(Vector3.zero), 8 * Time.deltaTime);

                if (!HeadLookQuaternion.eulerAngles.Equals(Vector3.zero))
                    HeadTransform.localRotation = HeadLookQuaternion;
            }

            if (AIConfidence == BreezeEnums.AIConfidence.Coward && (EquippedWeapon && UseEquipSystem) && Combating)
            {
                BreezeEvents.OnAlertState.Invoke(false);
                anim.SetBool("Combating", false);
                Combating = false;
                DisableIK();
            }

            if (AIConfidence == BreezeEnums.AIConfidence.Coward && !WeightDown && BodyWeight <= 0.15f)
            {
                WeightDown = true;
            }
            else if (AIConfidence == BreezeEnums.AIConfidence.Coward && WeightDown)
            {
                BodyWeight = 0;

                if (!Combating)
                {
                    UseIK = false;
                    elapsedTime = 0;
                }
            }

            if (AIConfidence == BreezeEnums.AIConfidence.Coward)
            {
                UseIK = false;
                elapsedTime = 0;
            }

            if (GetTarget() == null)
            {
                FirstIKDetect = true;
            }

            CheckIK();

            if (Reloading || GettingHit)
            {
                BodyWeight = 0;
                return;
            }

            DistanceFixLerp = Mathf.Lerp(DistanceFixLerp, 0, Time.deltaTime * 6);

            if (WeaponType == BreezeEnums.WeaponType.Shooter && UseAimIK && !Reloading &&
                !GettingHit)
            {
                for (int i = 0; i < 10; i++)
                {
                    foreach (var bone in boneTransforms)
                    {
                        if (GetTarget() == null)
                        {
                            AimAtTarget(bone, FixedLookPosition, BodyWeight);
                        }
                        else
                        {
                            if (CheckAngle(GetTargetPosition()))
                            {
                                if (FirstIKDetect)
                                {
                                    FixedLookPosition = GetTargetPosition();
                                    FirstIKDetect = false;
                                }

                                FixedLookPosition = Vector3.SmoothDamp(FixedLookPosition, GetTargetPosition(),
                                    ref TheVel,
                                    AimSmoothAmount, 20);
                                AimAtTarget(bone, FixedLookPosition, BodyWeight);
                            }
                        }
                    }
                }
            }
        }

        private float HeadWeight;

        //Main Hand IK Function
        private void OnAnimatorIK(int layerIndex)
        {
            if (stopAI)
                return;

            if (AIBehaviour == BreezeEnums.AIBehaviour.Companion)
            {
                if (playerObject != null)
                    anim.SetLookAtPosition(playerObject.transform.position);

                if (UseCompanionLookIK)
                {
                    HeadWeight = Mathf.Lerp(HeadWeight, 1, Time.deltaTime * 4f);
                    anim.SetLookAtWeight(HeadWeight);
                }
                else
                {
                    HeadWeight = Mathf.Lerp(HeadWeight, 0, Time.deltaTime * 4f);
                    anim.SetLookAtWeight(HeadWeight);
                }
            }

            if (UseIK)
            {
                if (UseHandIK)
                {
                    if (elapsedTime < HandSmoothAmount)
                        elapsedTime += Time.deltaTime;

                    state = Mathf.Lerp(0, 1, elapsedTime / HandSmoothAmount);

                    if (!OnlyLeftHand)
                    {
                        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, state);
                        anim.SetIKRotationWeight(AvatarIKGoal.RightHand, state);

                        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, state);
                        anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, state);
                    }
                    else
                    {
                        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                        anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);

                        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, state);
                        anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, state);
                    }

                    anim.SetIKPosition(AvatarIKGoal.RightHand, RightHandIK.position);
                    anim.SetIKRotation(AvatarIKGoal.RightHand, RightHandIK.rotation);

                    anim.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandIK.position);
                    anim.SetIKRotation(AvatarIKGoal.LeftHand, LeftHandIK.rotation);
                }
            }
            else
            {
                if (UseHandIK)
                {
                    if (elapsedTime < HandSmoothAmount)
                        elapsedTime += Time.deltaTime;

                    state = Mathf.Lerp(0, 1, elapsedTime / HandSmoothAmount);
                    state = 1 - state;

                    if ((UseEquipSystem && !anim.GetBool("Combating")) || GettingHit)
                        state = 0;


                    if (!OnlyLeftHand)
                    {
                        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, state);
                        anim.SetIKRotationWeight(AvatarIKGoal.RightHand, state);

                        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, state);
                        anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, state);
                    }
                    else
                    {
                        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                        anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);

                        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, state);
                        anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, state);
                    }

                    anim.SetIKPosition(AvatarIKGoal.RightHand, RightHandIK.position);
                    anim.SetIKRotation(AvatarIKGoal.RightHand, RightHandIK.rotation);

                    anim.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandIK.position);
                    anim.SetIKRotation(AvatarIKGoal.LeftHand, LeftHandIK.rotation);
                }
            }
        }

        //Returns Adjusted Target Position
        Vector3 GetTargetPosition()
        {
            Vector3 targetdir;

            if (PlayerScript != null && PlayerScript.HitPosition != null)
            {
                targetdir = PlayerScript.HitPosition.position - AimTransform.position;
            }
            else if (TargetAIScript != null && TargetAIScript.HitPosition != null)
            {
                targetdir = TargetAIScript.HitPosition.position - AimTransform.position;
            }
            else
            {
                targetdir = GetTarget().transform.position - AimTransform.position;
            }

            Vector3 aimdirection = AimTransform.forward;

            YOffset = 0;

            targetdir.Set(targetdir.x, targetdir.y + YOffset, targetdir.z);

            Vector3 dir = Vector3.Slerp(targetdir, aimdirection, 0);

            lastone = AimTransform.position + dir + AimOffset;

            float yPos;
            
            if (PlayerScript != null && PlayerScript.HitPosition != null)
            {
                yPos = PlayerScript.HitPosition.position.y;
            }
            else if (TargetAIScript != null && TargetAIScript.HitPosition != null)
            {
                yPos = TargetAIScript.HitPosition.position.y;
            }
            else
            {
                yPos = GetTarget().transform.position.y;
            }

            if (Math.Abs(GetTarget().transform.position.y - transform.position.y) < 1.5f)
                lastone.y -= (yPos - transform.position.y) / 1.4f;
            else
            {
                lastone.y -= (yPos - transform.position.y) / 4f;
            }

            return lastone;
        }

        //Smoothly Aims At Target
        private void AimAtTarget(Transform bone, Vector3 TargetPos, float weight)
        {
            Vector3 RifleDirection = Vector3.Lerp(AimTransform.forward, HeadTransform.forward, DistanceFixLerp);
            Vector3 AimTransf = (Vector3.Lerp(AimTransform.position, HeadTransform.position, DistanceFixLerp)) +
                                RifleDirection * -3;
            Vector3 TargetDirection = TargetPos - AimTransf;
            Quaternion AimDirect = Quaternion.FromToRotation(RifleDirection * 4, TargetDirection);
            Quaternion BlendAmount = Quaternion.Slerp(Quaternion.identity, AimDirect, weight);
            bone.rotation = BlendAmount * bone.rotation;
        }

        //Returns If Angle Available
        private bool CheckAngle(Vector3 pos)
        {
            Vector3 direction = (new Vector3(pos.x, pos.y + pos.y / 2, pos.z)) - HeadTransform.position;
            float angle = Vector3.Angle(new Vector3(direction.x, 0, direction.z), transform.forward);

            return angle <= AngleLimit / 2;
        }

        //Returns Angle
        private float GetAngle(Vector3 pos)
        {
            Vector3 direction = (new Vector3(pos.x, pos.y + pos.y / 2, pos.z)) - HeadTransform.position;
            float angle = Vector3.Angle(new Vector3(direction.x, 0, direction.z), transform.forward);

            return angle;
        }

        //Enables AIM Ik
        public IEnumerator FadeInBodyIK()
        {
            if (Reloading || GettingHit || (!EquippedWeapon && UseEquipSystem) || !Combating || switchingWeapon)
            {
                BodyWeight = 0;
                yield return null;
            }
            
            float T = 0;
            float StartingBodyWeight = BodyWeight;

            while (T < 1f)
            {
                if (GetTarget() == null || anim.GetCurrentAnimatorStateInfo(0).IsTag("Hit"))
                    break;
                
                if (switchingWeapon)
                {
                    BodyWeight = 0;
                    break;
                }

                T += Time.deltaTime * 0.5f;
                float Reference = Mathf.LerpAngle(StartingBodyWeight, 0.5f, T);
                BodyWeight = Reference;
                yield return null;
            }
        }

        //Disables AIM Ik
        IEnumerator FadeOutBodyIK()
        {
            float T = 0f;
            float StartingBodyWeight = BodyWeight;


            while (T < 1f && !Reloading)
            {
                if (BodyWeight <= 0.0005f)
                {
                    BodyWeight = 0;
                    break;
                }

                T += Time.deltaTime * 1.5f;
                BodyWeight = Mathf.LerpAngle(StartingBodyWeight, 0f, T);
                yield return null;
            }
        }

        #endregion

        //Registers The Damage
        private int GotHitAmount;
        private float GotCompanionDamageAmount;
        private GameObject FocusOnTarget;
        
        public void TakeDamage(float Amount, GameObject Sender, bool IsPlayer, bool HitReaction = true)
        {
            if (CurrentHealth <= 0)
            {
                Death();
                return;
            }

            if (Sender.gameObject.Equals(gameObject))
            {
                return;
            }

            if (anim.GetCurrentAnimatorStateInfo(0).IsTag("Block"))
            {
                return;
            }
            
            CurrentHealth -= Amount;

            BreezeEvents.OnTakeDamage.Invoke(Amount);

            if (AIBehaviour == BreezeEnums.AIBehaviour.Companion && IsPlayer)
            {
                GotCompanionDamageAmount += Amount;

                if ((GotCompanionDamageAmount / MaximumHealth) * 100 > FriendlyDamageThreshold)
                {
                    AIPlayerBehaviour = BreezeEnums.BehaviourType.Enemy;
                    DetectionLayers |= 1 << playerObject.gameObject.layer;
                    AIBehaviour = BreezeEnums.AIBehaviour.Enemy;
                    CurrentTarget = playerObject.gameObject;
                    PlayerScript = playerObject.GetComponent<BreezePlayer>();
                    TargetAIScript = null;
                    TargetIsPlayer = true;
                }
            }

            if (CurrentHealth <= 0)
            {
                Death();
                return;
            }

            if (switchingWeapon)
                return;

            if (GetHitTolerance > 0 && GetTarget() != null)
            {
                if (LastAtacker != null && LastAtacker == Sender)
                {
                    GotHitAmount++;

                    if (GotHitAmount >= GetHitTolerance)
                    {
                        if (Vector3.Distance(transform.position, Sender.transform.position) <= DetectionDistance)
                        {
                            FocusOnTarget = Sender;
                            CurrentTarget = Sender;

                            if (IsPlayer)
                            {
                                TargetAIScript = null;
                                PlayerScript = Sender.GetComponent<BreezePlayer>();
                            }
                            else
                            {
                                PlayerScript = null;
                                TargetAIScript = Sender.GetComponent<BreezeSystem>();
                            }

                            GotHitAmount = 0;
                            LastAtacker = null;   
                        }
                        else
                        {
                            GotHitAmount = 0;
                            LastAtacker = null;   
                        }
                    }
                }
                else
                {
                    GotHitAmount = 1;
                }
            }
            LastAtacker = Sender;

            bool HitWillPlay = false;

            if (HitReaction && !CheckCurrentStateTag("Hit") && !CheckCurrentStateTag("Draw") &&
                (Random.Range(1, 100) <= HitReactionPossibility || AIConfidence == BreezeEnums.AIConfidence.Neutral))
            {
                if (anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack") && !HitReactionOnAttack)
                    return;

                HitWillPlay = true;
                GettingHit = true;
                BreezeSounds.PlaySound(SoundType.TookDamage);
                PlayAnimation(BreezeEnums.AnimationType.Hit);
            }
            
            if (Sender != null)
            {
                if (GetTarget() == null && CheckDamageSender(Sender, IsPlayer))
                {
                    ExitNeutral = IsPlayer ? 2 : 1;
                    ExitSender = Sender;

                    if (!HitWillPlay)
                    {
                        if (ExitNeutral > 0)
                        {
                            if (AIConfidence == BreezeEnums.AIConfidence.Neutral)
                            {
                                TempBrave = true;
                                AIConfidence = BreezeEnums.AIConfidence.Brave;
                            }

                            if (ExitNeutral == 2)
                            {
                                TargetAIScript = null;
                                PlayerScript = ExitSender.GetComponent<BreezePlayer>();
                            }
                            else
                            {
                                PlayerScript = null;
                                TargetAIScript = ExitSender.GetComponent<BreezeSystem>();
                            }
                    
                            ExitNeutral = 0;
                            CurrentTarget = ExitSender;
                            ExitSender = null;
                        }

                        if (UseAimIK)
                        {
                            //Enables AIM IK
                            if (GetTarget() != null && UseAimIK && BodyWeight <= 0.25f && (EquippedWeapon || !UseEquipSystem))
                                StartCoroutine(FadeInBodyIK());
                        }
                    }
                }   
            }
        }

        //AI Death
        public void  Death()
        {
            BreezeSounds.PlaySound(SoundType.Death);

            if (nav.enabled)
                ResetNav();

            if (col != null)
            {
                col.enabled = false;
            }

            BreezeEvents.OnDeath.Invoke();

            if (DeathMethod == BreezeEnums.AIDeathType.Animation)
            {
                PlayAnimation(BreezeEnums.AnimationType.Death);
            }
            else
            {
                anim.enabled = false;

                Collider[] colliders = GetComponentsInChildren<Collider>();
                Rigidbody[] rigidbodys = GetComponentsInChildren<Rigidbody>();

                foreach (var rig in rigidbodys)
                {
                    rig.isKinematic = false;
                }

                foreach (var collider1 in colliders)
                {
                    if (col == null)
                    {
                        collider1.enabled = true;
                        continue;
                    }

                    if (collider1 != this.col)
                    {
                        collider1.enabled = true;
                    }
                }
            }

            if (WeaponType != BreezeEnums.WeaponType.Unarmed)
                anim.SetLayerWeight(1, 0);

            DisableIK();
            StopCoroutine(RegainHealthOverTime());
            CurrentHealth = 0f;
            CurrentTarget = null;
            nav.enabled = false;

            UpdateDebug();
            UpdateStateDebug();
            Invoke(nameof(DisableScript), DestroyDelay == 0 ? 5f : DestroyDelay);
        }

        #endregion

        #region Other Helper Methods

        //Check Target Health
        private bool CheckTargetHealth()
        {
            if (GetTarget() == null || (PlayerScript == null && TargetAIScript == null))
                return false;

            if (PlayerScript != null)
            {
                if (PlayerScript.CurrentHealth <= 0f)
                    BreezeEvents.OnKilledTarget.Invoke();

                return PlayerScript.CurrentHealth > 0f;
            }

            if (TargetAIScript.CurrentHealth <= 0f)
                BreezeEvents.OnKilledTarget.Invoke();

            return TargetAIScript.CurrentHealth > 0f;
        }

        //Check Path
        public Vector3 CheckPath(Vector3 dest)
        {
#if !Breeze_AI_Pathfinder_Enabled
            if (NavMesh.SamplePosition(dest, out var hit, 4, nav.areaMask))
            {
                return hit.position;
            }
#else
            GraphNode currentNode = AstarPath.active.GetNearest(transform.position, NNConstraint.Default).node;
            GraphNode goalNode = AstarPath.active.GetNearest(dest, NNConstraint.Default).node;
            if (PathUtilities.IsPathPossible(currentNode, goalNode))
            {
                return dest;
            }
#endif
            

            BreezeEvents.OnPathUnreachable.Invoke();
            return Vector3.zero;
        }

        //Checks The Attacker
        private bool CheckDamageSender(GameObject sender, bool isPlayer)
        {
            if (sender != null)
            {
                if (isPlayer && AIPlayerBehaviour != BreezeEnums.BehaviourType.Friendly)
                {
                    return true;
                }

                if (!isPlayer && CheckFaction(sender.GetComponent<BreezeSystem>().CurrentAIFaction))
                {
                    return true;
                }
            }

            return false;
        }

        //Equip & Un Equip Methods
        public void BreezeEquip()
        {
            OnEquipChanged.Invoke(true, false);
            
            BreezeEvents.OnAnimationEvent.Invoke("BreezeEquip");

            EquipEventCalled = true;

            foreach (var t in anim.runtimeAnimatorController.animationClips)
            {
                foreach (var Event in t.events)
                {
                    if (Event.functionName == "BreezeEquip")
                    {
                        Invoke(nameof(EnableAttack), (t.length - Event.time) - 0.2f);
                        break;
                    }
                }
            }
        }

        public void BreezeHolster()
        {
            EquipEventCalled = false;
            EquippedWeapon = false;
            BreezeEvents.OnAnimationEvent.Invoke("BreezeHolster");

            if (switchingWeapon)
                BodyWeight = 0;

            OnEquipChanged.Invoke(false, false);
        }

        private void EnableAttack()
        {
            EquippedWeapon = true;
            EnableIK();
        }

        //Sets a new navmesh agent destination
        public void SetDestination(Vector3 pos)
        {
            if (CurrentHealth <= 0)
                return;

#if !Breeze_AI_Pathfinder_Enabled
            nav.SetDestination(pos);
#else
            nav.destination = pos;
            nav.SearchPath();
#endif

        }

        //Returns the NavmeshAgent's stopping distance + offset
        private float StopDistance()
        {
#if !Breeze_AI_Pathfinder_Enabled
            return nav.stoppingDistance + 0.76f;
#else
            return nav.endReachedDistance + 0.76f;
#endif
        }

        private bool GetDistanceReached()
        {
            return nav.remainingDistance <= StopDistance();
        }

        public void ResetNav()
        {
#if !Breeze_AI_Pathfinder_Enabled
            nav.ResetPath();
#else
            nav.SetPath(null);
#endif
        }

        private Vector3 GetDestination()
        {
            return nav.destination;
        }

        private bool CalculatePath(Vector3 pos)
        {
#if !Breeze_AI_Pathfinder_Enabled
            NavMeshPath path = new NavMeshPath();
            return nav.CalculatePath(pos, path);
#else
            GraphNode currentNode = AstarPath.active.GetNearest(transform.position, NNConstraint.Default).node;
            GraphNode goalNode = AstarPath.active.GetNearest(pos, NNConstraint.Default).node;
            return PathUtilities.IsPathPossible(currentNode, goalNode);
#endif
        }

        private bool NavHasPath()
        {
            return nav.hasPath;
        }

        //Checks The Faction Relations
        private bool CheckFaction(BreezeEnums.Factions factionToCheck)
        {
            foreach (var fact in AIFactionsList)
            {
                if (fact.Factions.Equals(factionToCheck))
                {
                    return fact.behaviourType != BreezeEnums.BehaviourType.Friendly;
                }
            }

            return false;
        }

        //Notices & Starts Block State
        private void NoticeBlock()
        {
            if (!UseBlockingSystem || anim.GetCurrentAnimatorStateInfo(0).IsTag("Hit") ||
                (!CanBlockWhileAttacking && anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack")))
            {
                return;
            }

            if (Random.Range(1, 101) > BlockAnimationPossibility)
            {
                return;
            }

            PlayAnimation(BreezeEnums.AnimationType.Block);
        }

        //Returns a random valid patrol position with the given settings
        private Vector3 GetPatrolPosition()
        {
            int Tries = 0;

            while (GetDistanceReached())
            {
                //Stop after 8 tries to prevent crashing
                if (Tries > 8)
                {
                    break;
                }

                //Create a new destination position
                Vector3 Destination = transform.position +
                                      new Vector3(Random.insideUnitSphere.y, 0, Random.insideUnitSphere.z) *
                                      PatrolRadius;

                //Check if the position is valid & reachable
                if (CheckPath(Destination) != Vector3.zero)
                {
                    return Destination;
                }

                Tries++;
            }

            return Vector3.zero;
        }


        //Get Flee Position
        private Vector3 GetFleePosition()
        {
            int Tries = 0;

            while (true)
            {
                //Stop after 8 tries to prevent crashing
                if (Tries > 8)
                {
                    break;
                }

                //Get Target Direction
                Vector3 direction = (GetTarget().transform.position - transform.position).normalized;
                //Create a new destination position
                Vector3 Destination = transform.position - direction +
                                      new Vector3(Random.insideUnitSphere.y, 0, Random.insideUnitSphere.z) *
                                      FleeDistance;

                //Check if the position is valid & reachable
                if (!CheckPath(Destination).Equals(Vector3.zero))
                {
                    return Destination;
                }

                Tries++;
            }

            return Vector3.zero;
        }

        //Disables & enables the NavmeshAgent movement
        public void SetNavMovement(bool stopped = true)
        {
            if (CurrentHealth <= 0)
                return;

            BreezeEvents.OnAgentStoppedState.Invoke(stopped);

            nav.isStopped = stopped;
            nav.updatePosition = !stopped;

            if (GetTarget() != null && CurrentAIState == "Backing Away")
            {
                nav.updateRotation = false;
            }
            else
            {
                nav.updateRotation = !stopped;
            }
        }

        //Returns the current target
        public GameObject GetTarget()
        {
            return CurrentTarget;
        }

        //Returns the current target distance if available
        public float GetTargetDistance()
        {
            if (GetTarget() == null)
            {
                return -1;
            }

            return Vector3.Distance(GetTarget().transform.position, transform.position);
        }

        //Plays the given animation on the AI
        public void PlayAnimation(BreezeEnums.AnimationType type)
        {
            if (type != BreezeEnums.AnimationType.Idle && !Attacking)
            {
                FirstIdlePlay = true;
            }

            BreezeEvents.OnAnimationPlayed.Invoke(type);

            switch (type)
            {
                case BreezeEnums.AnimationType.Idle:
                    if (CurrentAIState == "Idle" || Blocking)
                        break;

                    ResetAnimParams();
                    if (FirstIdlePlay)
                    {
                        BreezeSounds.PlaySound(SoundType.Idle);
                        anim.SetFloat("Idle Index", RandomAnimIndex(type));
                        FirstIdlePlay = false;
                    }

                    CurrentMovement = BreezeEnums.MovementType.None;
                    SetNavMovement();
                    break;

                case BreezeEnums.AnimationType.Walk:
                    if (WeaponType != BreezeEnums.WeaponType.Shooter || !Attacking)
                        ResetAnimParams();

                    CurrentMovement = BreezeEnums.MovementType.Walking;
                    SetNavMovement(false);
                    break;

                case BreezeEnums.AnimationType.BackAway:
                    if (CurrentAIState == "Backing Away")
                        break;

                    if (WeaponType != BreezeEnums.WeaponType.Shooter || !Attacking)
                        ResetAnimParams();

                    CurrentMovement = BreezeEnums.MovementType.Backaway;
                    nav.isStopped = false;
                    nav.updatePosition = true;
                    nav.updateRotation = false;
                    break;

                case BreezeEnums.AnimationType.Run:

                    ResetAnimParams();
                    CurrentMovement = BreezeEnums.MovementType.Running;
                    SetNavMovement(false);
                    break;
                case BreezeEnums.AnimationType.Attack:
                    if (CurrentAIState == "Attacking" || anim.GetCurrentAnimatorStateInfo(0).IsTag("Block") ||
                        GetTarget() == null || GetTargetDistance() > ExtendedAttackDistance)
                        break;

                    ResetAnimParams();

                    if (WeaponType == BreezeEnums.WeaponType.Melee)
                    {
                        BreezeMeleeWeapon weapon =
                            BreezeWeaponHub.WeaponClasses[BreezeWeaponHub.currentIndex].weaponScript as
                                BreezeMeleeWeapon;
                        
                        if(weapon != null)
                            weapon.Playsound("Swinged");
                    }
                    
                    FirstAttack = false;
                    Attacking = true;
                    int rand = RandomAnimIndex(type);
                    anim.SetInteger("Attack Index", rand);
                    rand--;

                    CurrentMovement = BreezeEnums.MovementType.None;
                    currentDamageAmount = Random.Range(AttackDamagesList[rand].minDamage,
                        (AttackDamagesList[rand].MaxDamage + 1));

                    anim.SetBool("Attack", true);
                    SetNavMovement();
                    if (TargetAIScript != null && WeaponType != BreezeEnums.WeaponType.Shooter &&
                        WeaponType == TargetAIScript.WeaponType)
                    {
                        if (TargetAIScript.UseBlockingSystem)
                            TargetAIScript.NoticeBlock();
                    }

                    break;

                case BreezeEnums.AnimationType.Hit:
                    if (CurrentAIState == "Getting Hit")
                        break;

                    BodyWeight = 0;
                    DisableIK();
                    ResetAnimParams();
                    anim.SetInteger("Hit Index", RandomAnimIndex(type));
                    CurrentMovement = BreezeEnums.MovementType.None;
                    anim.SetTrigger("Hit");
                    Invoke(nameof(ResetTriggerAnim), 0.35f);
                    SetNavMovement();
                    break;

                case BreezeEnums.AnimationType.Reload:
                    if (Reloading)
                        break;

                    ResetAnimParams();
                    CurrentMovement = BreezeEnums.MovementType.None;
                    anim.SetTrigger("Reload");
                    Invoke(nameof(ResetTriggerAnim), 0.3f);
                    SetNavMovement();
                    Reloading = true;
                    break;

                case BreezeEnums.AnimationType.Block:
                    if (CurrentAIState == "Blocking" || Blocking)
                        break;

                    ResetAnimParams();
                    anim.SetInteger("Block Index", RandomAnimIndex(type));
                    CurrentMovement = BreezeEnums.MovementType.None;
                    anim.SetTrigger("Block");
                    Invoke(nameof(ResetTriggerAnim), 0.3f);
                    SetNavMovement();
                    Blocking = true;
                    InvokeRepeating(nameof(CheckBlockState), 0.3f, 0.15f);
                    break;

                case BreezeEnums.AnimationType.Death:
                    if (CurrentAIState == "Dead")
                        break;

                    ResetAnimParams();
                    anim.SetInteger("Death Index", RandomAnimIndex(type));
                    CurrentMovement = BreezeEnums.MovementType.None;
                    anim.SetTrigger("Death");
                    SetNavMovement();
                    break;
            }
        }

        //Rotates To Object
        private void RotateToObject(GameObject obj)
        {
            if(CheckCurrentStateTag("Hit"))
                return;
            
            if (UseAimIK && BodyWeight >= 0.4f && GetTarget() != null)
            {
                if ((GetAngle(GetTargetPosition()) * 2) <= AngleLimit - 25f)
                    return;
            }

            Vector3 targetRotation = obj.transform.position - transform.position;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetRotation),
                RotationSpeed * Time.deltaTime);

            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }

        //Returns The Animator State Tag Equality
        private bool CheckCurrentStateTag(string ToCheck)
        {
            return anim.GetCurrentAnimatorStateInfo(0).IsTag(ToCheck);
        }

        public int IdleAnimCount;
        public int CIdleAnimCount;
        public int HitAnimCount;
        public int AttackAnimCount;
        public int DeathAnimCount;
        public int BlockAnimCount;

        //Returns A Random Animator Index
        private int RandomAnimIndex(BreezeEnums.AnimationType type)
        {
            int limit = 3;

            switch (type)
            {
                case BreezeEnums.AnimationType.Idle:
                    limit = Combating ? CIdleAnimCount : IdleAnimCount;
                    break;
                case BreezeEnums.AnimationType.Hit:
                    limit = HitAnimCount;
                    break;
                case BreezeEnums.AnimationType.Attack:
                    limit = AttackAnimCount;
                    break;
                case BreezeEnums.AnimationType.Death:
                    limit = DeathAnimCount;
                    break;
                case BreezeEnums.AnimationType.Block:
                    limit = BlockAnimCount;
                    break;
            }

            limit++;

            return Random.Range(1, limit);
        }

        //Resets all animator parameters
        public void ResetAnimParams()
        {
            anim.SetBool("Attack", false);
            anim.SetInteger("Death Index", 0);
            anim.SetInteger("Hit Index", 0);
            anim.SetInteger("Attack Index", 0);
            anim.SetFloat("Idle Index", 0f);
        }

        public void CheckBlockState()
        {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsTag("Block"))
            {
                Blocking = false;
                CancelInvoke(nameof(CheckBlockState));
            }
        }

        public void ResetTriggerAnim()
        {
            anim.ResetTrigger("Block");
            anim.ResetTrigger("Hit");

            if (WeaponType == BreezeEnums.WeaponType.Shooter)
                anim.ResetTrigger("Reload");
        }

        public void DisableScript()
        {
            anim.enabled = false;
            enabled = false;
            
            if(DestroyAfterDeath)
                Destroy(gameObject);
        }

        #endregion

        #region Debugs

        //Updates Debug Panel
        private void UpdateDebug()
        {
            VisibleTargetsDebug = VisibleTargets;
            CurrentTargetTypeDebug = TargetIsPlayer ? "Player" : "AI";
            CurrentTargetDebug =
                AIBehaviour == BreezeEnums.AIBehaviour.Enemy ? GetTarget() : playerObject.gameObject;
            TargetDistanceDebug = AIBehaviour == BreezeEnums.AIBehaviour.Enemy
                ? GetTargetDistance()
                : GetPlayerDistance();
            CurrentHealthDebug = CurrentHealth;
            RegeneratingHealthDebug = isRegenHealth;
            IsAlertedDebug = anim.GetBool("Combating");
            IsAttackingDebug = anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
        }

        //Updates AI State
        private void UpdateStateDebug()
        {
            if (CurrentHealth <= 0)
            {
                CurrentAIState = "Dead";
            }
            else if (switchingWeapon)
            {
                CurrentAIState = "Switching Weapon";
            }
            else if (anim.GetCurrentAnimatorStateInfo(0).IsTag("Hit"))
            {
                CurrentAIState = "Getting Hit";
            }
            else if (Reloading)
            {
                CurrentAIState = "Reloading";
            }
            else if (IsAttacking())
            {
                CurrentAIState = "Attacking";
            }
            else if (Fleeing)
            {
                CurrentAIState = "Fleeing";
            }
            else if (GetTarget() != null && GetDestination().Equals(GetTarget().transform.position))
            {
                CurrentAIState = "Chasing Target";
            }
            else if (Patrolling)
            {
                CurrentAIState = "Patrolling";
            }
            else
            {
                CurrentAIState = CurrentMovement switch
                {
                    BreezeEnums.MovementType.Backaway => "Backing Away",
                    BreezeEnums.MovementType.Running => "Running",
                    BreezeEnums.MovementType.Walking => "Walking",
                    BreezeEnums.MovementType.None => "Idle",
                    _ => CurrentAIState
                };   
            }
        }

        private bool IsAttacking()
        {
            return anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
        }

        #endregion
    }
}