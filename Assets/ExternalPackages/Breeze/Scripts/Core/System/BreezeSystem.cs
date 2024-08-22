using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
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
        public string ObjectTag = "Untagged";

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
        [Range(1, 250)] public float NotifyDistanceLimit = 1;
        public BreezeEnums.DetectionType DetectionType = BreezeEnums.DetectionType.LineOfSight;
        public BreezeEnums.DetectionLock DetectionLock = BreezeEnums.DetectionLock.Closest;
        public float DetectionFrequency = 0.25f;
        [Range(1f, 100f)] public float DetectionDistance = 10;
        [Range(10f, 270f)] public float DetectionAngle = 70;
        public LayerMask DetectionLayers = 0;
        public LayerMask ObstacleLayers = 0;
        public Transform HeadTransform;

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
        public List<FactionsList> AIFactionsList = new List<FactionsList>();
        public string BreezeTag = "AI";

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
        public NavMeshAgent NavmeshAgentDebug;
        public Collider ColliderDebug;
        public BreezeSystem SystemDebug;

        //Optimization Variables
        public BreezeEnums.YesNo UseLodOptimization = BreezeEnums.YesNo.No;
        public Renderer LodRenderer;

        #endregion

        #region System Variables

        public bool stopAI;
        public bool hasErrors;
        private bool disableErrorCheck = true;

        //Components
        public NavMeshAgent nav;
        public Animator anim;
        public Collider col;
        public BreezeSystem TargetAIScript;
        public BreezeEvents BreezeEvents;
        public BreezeSystem System { get; set; }

        //Objects
        public GameObject CurrentTarget;
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
        private bool HitStarted;
        private bool TempBrave;
        private int ExitNeutral;
        private float currentDamageAmount;

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
                nav = GetComponent<NavMeshAgent>() == null
                    ? gameObject.AddComponent<NavMeshAgent>()
                    : GetComponent<NavMeshAgent>();
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
        }

        private void disableErrorChecking()
        {
            disableErrorCheck = true;
        }

        //Init AI
        private void Start()
        {
            System = this;

            InvokeRepeating(nameof(Detection), 0, DetectionFrequency);

            col = GetComponent<Collider>();
            anim = GetComponent<Animator>();
            nav = GetComponent<NavMeshAgent>();
            col = GetComponent<Collider>();

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

            BreezeEvents = GetComponent<BreezeEvents>();
            Invoke(nameof(disableErrorChecking), 3f);
        }

        #endregion

        #region Core Functions

        private void Update()
        {
            //Check Current Target Health & Stop State
            if (CurrentHealth <= 0) return;

            UpdateMovement();

            if (stopAI) return;

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

                if (ExitNeutral > 0)
                {
                    if (AIConfidence == BreezeEnums.AIConfidence.Neutral)
                    {
                        TempBrave = true;
                        AIConfidence = BreezeEnums.AIConfidence.Brave;
                    }
                    TargetAIScript = ExitSender.GetComponent<BreezeSystem>();

                    ExitNeutral = 0;
                    CurrentTarget = ExitSender;
                    ExitSender = null;
                }
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
                UpdateWandering();

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
                if (GetTarget() != null && !anim.GetBool("Combating") && !Combating)
                {
                    BreezeEvents.OnAlertState.Invoke(true);
                    BreezeSounds.PlaySound(SoundType.Alerted);
                    anim.SetBool("Combating", true);
                    Combating = true;

                    if (WeaponType == BreezeEnums.WeaponType.Shooter && !UseEquipSystem)
                    {
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
            if (TargetAIScript == null && GetTarget() != null)
            {
                TargetAIScript = GetTarget().GetComponent<BreezeSystem>();
            }

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
            if (!nav.hasPath)
                RotateToObject(GetTarget());

            //Check Too Close
            if (((GetTargetDistance() <= EnemyTooCloseDistance && !backingAway) ||
                 (GetTargetDistance() <= EnemyTooCloseDistance * BackawayMultiplier && backingAway) && !Blocking &&
                 (!Attacking || WeaponType == BreezeEnums.WeaponType.Shooter) &&
                 !anim.GetCurrentAnimatorStateInfo(0).IsTag("Hit")))
            {
                if (WeaponType != BreezeEnums.WeaponType.Shooter)
                    CancelInvoke(nameof(AttackWithDelay));
            }

            backingAway = false;

            //Approach To Target
            if (ShouldChaseTarget)
            {
                //Patch Checker Variables
                Vector3 pos = GetTarget().transform.position;

                //Check Distance
                if ((Attacking && GetTargetDistance() > ExtendedAttackDistance) ||
                    (Attacking && GetTargetDistance() > AttackDistance) ||
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
                else if (!Blocking)
                {
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
            else if (!Blocking)
            {
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
                        if (!TargetAIScript.Blocking)
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
            if (!TargetAIScript.Blocking)
            {
                TargetAIScript.TakeDamage(currentDamageAmount, gameObject);
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
            else if (StoredTarget != null)
            {
                if (!FleeDone)
                {
                    if (GetDistanceReached())
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

                nav.speed = WalkBackwardsSpeed;
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
        }

        //Back away from Target
        private bool BackAway()
        {
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
                                Waypoints[currentWaypoint].MaxIdleLength));
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
                currentWaypoint = (currentWaypoint + 1) % Waypoints.Count;
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

            WaypointWander();
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
                if (system.Equals(this))
                    return;

                if (!CheckFaction(system.CurrentAIFaction))
                    return;

                TargetAIScript = system;
                BreezeEvents.OnFoundTarget.Invoke(target);
                CurrentTarget = target;
            }
            else
            {
                TargetAIScript = null;
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


        private Collider[] pTargets = new Collider[10000];
        private void Detection()
        {
            if (FocusOnTarget != null) return;

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

                        if (damageable.System.Equals(this))
                            continue;

                        CheckTarget = damageable.System.gameObject;
                    }
                    else
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

            if (AIConfidence == BreezeEnums.AIConfidence.Neutral)
                return;

            foreach (var target in VisibleTargets)
            {
                if (CheckFaction(target.GetComponent<BreezeDamageable>().System.CurrentAIFaction))
                {
                    TargetAIScript = target.GetComponent<BreezeDamageable>().System;
                    BreezeEvents.OnFoundTarget.Invoke(target);
                    CurrentTarget = target;
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

        //Registers The Damage
        private int GotHitAmount;
        private GameObject FocusOnTarget;

        public void TakeDamage(float Amount, GameObject Sender, bool HitReaction = true)
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


            if (CurrentHealth <= 0)
            {
                Death();
                return;
            }


            if (!Sender.gameObject.TryGetComponent(out BreezeSystem bs))
            {
                return;
            }

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

                            TargetAIScript = Sender.GetComponent<BreezeSystem>();


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
                if (GetTarget() == null && CheckDamageSender(Sender))
                {
                    ExitNeutral = 1;
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

                            TargetAIScript = ExitSender.GetComponent<BreezeSystem>();

                            ExitNeutral = 0;
                            CurrentTarget = ExitSender;
                            ExitSender = null;
                        }

                    }
                }
            }
        }

        //AI Death
        public void Death()
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
            if (GetTarget() == null)
                return false;

            if (TargetAIScript.CurrentHealth <= 0f)
                BreezeEvents.OnKilledTarget.Invoke();

            return TargetAIScript.CurrentHealth > 0f;
        }

        //Check Path
        public Vector3 CheckPath(Vector3 dest)
        {
            if (NavMesh.SamplePosition(dest, out var hit, 4, nav.areaMask))
            {
                return hit.position;
            }

            BreezeEvents.OnPathUnreachable.Invoke();
            return Vector3.zero;
        }

        //Checks The Attacker
        private bool CheckDamageSender(GameObject sender)
        {
            if (sender != null)
            {
                if (!sender.TryGetComponent(out BreezeSystem bs))
                {
                    return true;
                }

                if (CheckFaction(sender.GetComponent<BreezeSystem>().CurrentAIFaction))
                {
                    return true;
                }
            }

            return false;
        }


        //Sets a new navmesh agent destination
        public void SetDestination(Vector3 pos)
        {
            if (CurrentHealth <= 0)
                return;

            nav.SetDestination(pos);

        }

        private float StopDistance()
        {
            return nav.stoppingDistance;
        }

        private bool GetDistanceReached()
        {
            return Vector3.Distance(nav.destination, gameObject.transform.position) <= StopDistance();
        }

        public void ResetNav()
        {
            nav.ResetPath();
        }

        private Vector3 GetDestination()
        {
            return nav.destination;
        }

        private bool CalculatePath(Vector3 pos)
        {
            NavMeshPath path = new NavMeshPath();
            return nav.CalculatePath(pos, path);
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
                Vector3 Destination = Waypoints[0].transform.position +
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

                    ResetAnimParams();
                    anim.SetInteger("Hit Index", RandomAnimIndex(type));
                    CurrentMovement = BreezeEnums.MovementType.None;
                    anim.SetTrigger("Hit");
                    Invoke(nameof(ResetTriggerAnim), 0.35f);
                    SetNavMovement();
                    break;

                case BreezeEnums.AnimationType.Reload:
                    ResetAnimParams();
                    CurrentMovement = BreezeEnums.MovementType.None;
                    anim.SetTrigger("Reload");
                    Invoke(nameof(ResetTriggerAnim), 0.3f);
                    SetNavMovement();
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
            if (CheckCurrentStateTag("Hit"))
                return;

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

            if (DestroyAfterDeath)
                Destroy(gameObject);
        }

        #endregion

        #region Debugs

        //Updates Debug Panel
        private void UpdateDebug()
        {
            VisibleTargetsDebug = VisibleTargets;
            CurrentTargetTypeDebug = "AI";
            CurrentTargetDebug = GetTarget();
            TargetDistanceDebug = GetTargetDistance();
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
            else if (anim.GetCurrentAnimatorStateInfo(0).IsTag("Hit"))
            {
                CurrentAIState = "Getting Hit";
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