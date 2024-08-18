using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Breeze.Core
{
    public class BreezeGun : BreezeWeaponInterface
    {
        private float FireSequence = 0f;
        private bool Reloading = false;
        private float MagAmmo = 0f;

        public BreezeSystem BreezeSystem;

        //Weapon Settings
        public bool UseHandIK = false;
        public bool OnlyLeftHand = true;
        public bool UseAimIK = false;

        public bool SingleShot = false;
        public BreezeEnums.ShootingType ShootingType = BreezeEnums.ShootingType.Additive;
        public BreezeEnums.BulletType BulletType = BreezeEnums.BulletType.Projectile;
        public float FireRate = 8f;
        public float SingleShotDelay = 1.5f;
        [Range(1, 100)] public int WeaponAccuracy = 90;
        [Range(1, 100)] public int MoveWeaponAccuracy = 70;
        public bool DebugHits = false;

        //Projectile Settings
        public GameObject BulletObject = null;
        public float BulletForce = 20f;

        //Raycast Settings
        public bool DrawBulletRays = true;
        public LayerMask BulletHitLayers = 1;

        //Ammo Settings
        public int MinDamageAmount = 3;
        public int MaxDamageAmount = 5;
        public int AmmoPerClip = 30;
        public int ClipAmount = 3;


        //VFX Settings
        public List<BreezeMeleeWeapon.impactEffect> ImpactEffects = new List<BreezeMeleeWeapon.impactEffect>();
        public List<ParticleSystem> MuzzleParticles = new List<ParticleSystem>();

        //Sound Settings
        public AudioClip FireSoundEffect = null;
        [Range(0f, 1f)] public float FireSoundVolume = 1f;

        public AudioClip ReloadSoundEffect = null;
        [Range(0f, 1f)] public float ReloadSoundVolume = 1f;

        public AudioClip DrawSoundEffect = null;
        [Range(0f, 1f)] public float DrawSoundVolume = 1f;

        public AudioClip HolsterSoundEffect = null;
        [Range(0f, 1f)] public float HolsterSoundVolume = 1f;

        //Mesh Settings
        public GameObject InventoryWeaponParent = null;
        public GameObject PrimaryWeaponParent = null;

        //Event Settings
        //Events
        public UnityEvent<float> DamagedEvent = new UnityEvent<float>();
        public UnityEvent ShotEvent = new UnityEvent();
        public UnityEvent ReloadEvent = new UnityEvent();
        public UnityEvent OnOutOfAmmo = new UnityEvent();
        public UnityEvent DrawEvent = new UnityEvent();
        public UnityEvent HolsterEvent = new UnityEvent();

        //Private Variables
        public GameObject RightHandIK = null;
        public GameObject LeftHandIK = null;
        public GameObject MuzzlePoint = null;
        private bool WeaponEquipped = false;
        private AudioSource _source;
        private bool SingleShootable = true;

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if (BreezeSystem == null)
            {
                Transform ThePar = transform.parent;

                while (ThePar.GetComponent<BreezeSystem>() == null)
                {
                    ThePar = ThePar.parent;
                }

                BreezeSystem = ThePar.GetComponent<BreezeSystem>();
            }

            if (GetComponent<BoxCollider>() == null)
            {
                gameObject.AddComponent<BoxCollider>().enabled = false;
            }
            else
            {
                gameObject.GetComponent<BoxCollider>().enabled = false;
            }

            if (RightHandIK == null && transform.Find("Right Hand IK") == null)
            {
                RightHandIK = new GameObject("Right Hand IK");
                RightHandIK.transform.SetParent(transform);
                RightHandIK.transform.localPosition = Vector3.zero;
                RightHandIK.transform.localRotation = Quaternion.identity;
            }
            else if (RightHandIK == null)
            {
                RightHandIK = transform.Find("Right Hand IK").gameObject;
            }

            if (LeftHandIK == null && RightHandIK != null && RightHandIK.transform.Find("Left Hand IK") == null)
            {
                LeftHandIK = new GameObject("Left Hand IK");
                LeftHandIK.transform.SetParent(RightHandIK.transform);
                LeftHandIK.transform.localPosition = Vector3.zero;
                LeftHandIK.transform.localRotation = Quaternion.identity;
            }
            else if (LeftHandIK == null && RightHandIK != null)
            {
                LeftHandIK = RightHandIK.transform.Find("Left Hand IK").gameObject;
            }

            if (MuzzlePoint == null && transform.Find("Muzzle Point") == null)
            {
                MuzzlePoint = new GameObject("Muzzle Point");
                MuzzlePoint.transform.SetParent(transform);
                MuzzlePoint.transform.localPosition = Vector3.zero;
                MuzzlePoint.transform.localRotation = Quaternion.identity;
            }
            else if (MuzzlePoint == null)
            {
                MuzzlePoint = transform.Find("Muzzle Point").gameObject;
            }
            else if(MuzzlePoint.GetComponent<MuzzleGizmos>() == null)
            {
                MuzzlePoint.AddComponent<MuzzleGizmos>();
            }

            if (BulletObject == null)
            {
                BulletObject = Resources.Load<GameObject>("Prefabs/Example Bullet");
            }

            BreezeSystem.AimTransform = MuzzlePoint.transform;
            BreezeSystem.LeftHandIK = LeftHandIK.transform;
            BreezeSystem.RightHandIK = RightHandIK.transform;
        }

        private void Awake()
        {
            if (RightHandIK == null && transform.Find("Right Hand IK") == null)
            {
                RightHandIK = new GameObject("Right Hand IK");
                RightHandIK.transform.SetParent(transform);
                RightHandIK.transform.localPosition = Vector3.zero;
                RightHandIK.transform.localRotation = Quaternion.identity;
            }
            else if (RightHandIK == null)
            {
                RightHandIK = transform.Find("Right Hand IK").gameObject;
            }

            if (LeftHandIK == null && RightHandIK != null && RightHandIK.transform.Find("Left Hand IK") == null)
            {
                LeftHandIK = new GameObject("Left Hand IK");
                LeftHandIK.transform.SetParent(RightHandIK.transform);
                LeftHandIK.transform.localPosition = Vector3.zero;
                LeftHandIK.transform.localRotation = Quaternion.identity;
            }
            else if (LeftHandIK == null && RightHandIK != null)
            {
                LeftHandIK = RightHandIK.transform.Find("Left Hand IK").gameObject;
            }

            if (MuzzlePoint == null && transform.Find("Muzzle Point") == null)
            {
                MuzzlePoint = new GameObject("Muzzle Point");
                MuzzlePoint.transform.SetParent(transform);
                MuzzlePoint.transform.localPosition = Vector3.zero;
                MuzzlePoint.transform.localRotation = Quaternion.identity;
            }
            else if (MuzzlePoint == null)
            {
                MuzzlePoint = transform.Find("Muzzle Point").gameObject;
            }

            if (BreezeSystem != null && BreezeSystem.BreezeWeaponHub == null)
            {
                BreezeSystem.OnEquipChanged.AddListener(OnEquipChangedAI);
            }

            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;

            MagAmmo = AmmoPerClip;

            OnEquipChangedAI(false, true);

            if (GetComponent<BoxCollider>() == null)
            {
                gameObject.AddComponent<BoxCollider>().enabled = false;
            }
            else
            {
                gameObject.GetComponent<BoxCollider>().enabled = false;
            }

            Transform ThePar = transform.parent;

            while (ThePar.GetComponent<BreezeSystem>() == null)
            {
                ThePar = ThePar.parent;
            }

            BreezeSystem = ThePar.GetComponent<BreezeSystem>();
        }

        private void Update()
        {
            if (!WeaponEquipped || BreezeSystem == null || BreezeSystem.CurrentAIState == "Dead" ||
                BreezeSystem.GetTarget() == null ||
                BreezeSystem.AIConfidence == BreezeEnums.AIConfidence.Coward ||
                BreezeSystem.anim.GetCurrentAnimatorStateInfo(0).IsTag("Hit"))
            {
                return;
            }

            if (BreezeSystem.switchingWeapon)
                return;

            BreezeSystem.SingleCanFire = SingleShootable;
            if (MagAmmo <= 0 && ClipAmount <= 0)
            {
                OnOutOfAmmo.Invoke();
                if (BreezeSystem.BreezeWeaponHub != null)
                {
                    if (!BreezeSystem.BreezeWeaponHub.switchWeaponBackend())
                    {
                        MagAmmo = 0;
                        ClipAmount = 0;
                        BreezeSystem.AIConfidence = BreezeEnums.AIConfidence.Coward;
                        BreezeSystem.Attacking = false;
                        BreezeSystem.PlayAnimation(BreezeEnums.AnimationType.Idle);
                        return;
                    }
                }
                else
                {
                    MagAmmo = 0;
                    ClipAmount = 0;
                    BreezeSystem.AIConfidence = BreezeEnums.AIConfidence.Coward;
                    BreezeSystem.Attacking = false;
                    BreezeSystem.PlayAnimation(BreezeEnums.AnimationType.Idle);
                    return;
                }
            }

            if (MagAmmo <= 0 && !Reloading)
            {
                Reload();
                return;
            }

            if (Reloading && ShootingType == BreezeEnums.ShootingType.Single)
                FireSequence = Random.Range(2, 5);

            if (MagAmmo < 0 || (BreezeSystem.UseAimIK && BreezeSystem.BodyWeight <= 0.21f) ||
                (ShootingType == BreezeEnums.ShootingType.Single && !SingleShootable))
            {
                return;
            }

            if (MagAmmo <= 0) 
                return;
            
            if (ShootingType == BreezeEnums.ShootingType.Single && SingleShootable)
            {
                if (Time.time >= FireSequence)
                {
                    FireSequence = Time.time + 1f / FireRate;
                    BreezeSystem.AttackWithDelay();
                    Shoot();
                }
            }
            else if (ShootingType == BreezeEnums.ShootingType.Additive)
            {
                if (Time.time >= FireSequence)
                {
                    FireSequence = Time.time + 1f / FireRate;
                    Shoot();
                }
            }
        }

        //Fire
        private void Shoot()
        {
            if (!BreezeSystem.anim.GetBool("Attack"))
                BreezeSystem.AttackWithDelay();

            SingleShootable = false;
            float temporaryaccuracy =
                (100 - (BreezeSystem.anim.GetFloat("Speed") >= 0.5f ? MoveWeaponAccuracy : WeaponAccuracy)) / 50f;
            temporaryaccuracy *= 0.85f;

            if (BulletType == BreezeEnums.BulletType.Raycast)
            {
                RaycastHit hit;

                Vector3 rand = new Vector3(Random.Range(-temporaryaccuracy, temporaryaccuracy) * 0.225f,
                    Random.Range(-temporaryaccuracy, temporaryaccuracy) * 0.094f, 0);
                if (DrawBulletRays)
                {
                    Debug.DrawRay(BreezeSystem.AimTransform.transform.position,
                        BreezeSystem.AimTransform.transform.forward * (BreezeSystem.GetTargetDistance() + 1f) +
                        rand,
                        Color.green, 5);
                }

                if (Physics.Raycast(BreezeSystem.AimTransform.transform.position,
                        BreezeSystem.AimTransform.transform.forward + rand, out hit,
                        BreezeSystem.AttackDistance, BulletHitLayers, QueryTriggerInteraction.Ignore))
                {
                    if (hit.transform == BreezeSystem.transform)
                        return;


                    int damage = Random.Range(MinDamageAmount, MaxDamageAmount + 1);
                    if (hit.transform.GetComponent<BreezePlayer>() != null)
                    {
                        hit.transform.GetComponent<BreezePlayer>().TakeDamage(damage, BreezeSystem.gameObject);
                        DamagedEvent.Invoke(damage);
                    }

                    BreezeDamageable damageable = hit.transform.GetComponent<BreezeDamageable>();

                    if (damageable != null)
                    {
                        if (damageable.System != null &&
                            hit.transform.root.gameObject != BreezeSystem.gameObject)
                        {
                            damageable.System.TakeDamage(damage, BreezeSystem.gameObject, false);
                            DamagedEvent.Invoke(damage);
                        }
                    }
                }

                if (BreezeSystem != null && hit.transform != null)
                {
                    List<BreezeMeleeWeapon.impactEffect> effectsAvailable =
                        new List<BreezeMeleeWeapon.impactEffect>();

                    foreach (var effect in ImpactEffects)
                    {
                        if (effect.ObjectTag.Equals(hit.transform.gameObject.tag))
                        {
                            GameObject Effect = Instantiate(effect.ImpactEffect,
                                hit.point, Quaternion.FromToRotation(Vector3.forward, transform.position));

                            if (effect.SetParent == BreezeMeleeWeapon.impactEffect.Parent.Null)
                            {
                                Effect.transform.SetParent(null);
                            }
                            else
                            {
                                Effect.transform.SetParent(hit.transform);
                            }
                        }
                    }
                }
            }
            else
            {
                int damage = Random.Range(MinDamageAmount, MaxDamageAmount + 1);

                Rigidbody Projectile = Instantiate(BulletObject,
                    MuzzlePoint.transform.position,
                    MuzzlePoint.transform.rotation).GetComponent<Rigidbody>();

                Vector3 direction = (BreezeSystem.TargetAIScript == null ? BreezeSystem.PlayerScript.HitPosition.transform.position : BreezeSystem.TargetAIScript.HitPosition.transform.position - MuzzlePoint.transform.position).normalized;
                
                Vector3 velocity = direction.normalized * BulletForce;

                Projectile.linearVelocity = velocity;

                BreezeProjectile bullet = Projectile.GetComponent<BreezeProjectile>();

                if (bullet != null)
                {
                    bullet.Gun = this;
                    bullet.damage = damage;
                    bullet.hitMask = BulletHitLayers;
                    bullet.sender = BreezeSystem.gameObject;
                }
            }

            foreach (var muzzle in MuzzleParticles)
            {
                muzzle.Play();
            }

            ShotEvent.Invoke();
            PlaySound("Fire");

            MagAmmo--;

            if (ShootingType == BreezeEnums.ShootingType.Single)
            {
                Invoke("StopAttack", 0.2f);
                Invoke("SingleDelay", SingleShotDelay);
            }
        }

        private void LateUpdate()
        {
            if (BreezeSystem.CurrentHealth <= 0 && PrimaryWeaponParent != null)
            {
                if (PrimaryWeaponParent.GetComponent<Rigidbody>() == null)
                {
                    Rigidbody rig = PrimaryWeaponParent.AddComponent<Rigidbody>();
                    rig.isKinematic = false;
                    rig.useGravity = true;
                }
                else
                {
                    Rigidbody rig = PrimaryWeaponParent.GetComponent<Rigidbody>();
                    rig.isKinematic = false;
                    rig.useGravity = true;
                }

                PrimaryWeaponParent.GetComponent<BoxCollider>().isTrigger = false;
                PrimaryWeaponParent.GetComponent<BoxCollider>().enabled = true;

                PrimaryWeaponParent.transform.SetParent(null);
            }
        }

        private void StopAttack()
        {
            BreezeSystem.Attacking = false;
            BreezeSystem.ResetAnimParams();
            BreezeSystem.PlayAnimation(BreezeEnums.AnimationType.Idle);
        }

        private void SingleDelay()
        {
            SingleShootable = true;
        }

        //Reload
        private void Reload()
        {
            if (ClipAmount <= 0)
                return;

            BreezeSystem.Attacking = false;
            BreezeSystem.EquippedWeapon = false;
            BreezeSystem.UseIK = false;
            BreezeSystem.elapsedTime = 0;
            Reloading = true;
            ReloadEvent.Invoke();
            PlaySound("Reload");
            BreezeSystem.PlayAnimation(BreezeEnums.AnimationType.Reload);
            InvokeRepeating("ReloadCheck", 0.2f, 0.1f);
        }

        //Check Reload Anim
        private bool ReloadStarted;

        public void ReloadCheck()
        {
            if (BreezeSystem.anim.GetCurrentAnimatorStateInfo(0).IsTag("Reload"))
                ReloadStarted = true;

            if (ReloadStarted)
            {
                if (!BreezeSystem.anim.GetCurrentAnimatorStateInfo(0).IsTag("Reload"))
                {
                    ReloadStarted = false;
                    Reloading = false;
                    BreezeSystem.Reloading = false;

                    if (BreezeSystem.UseAimIK || BreezeSystem.UseHandIK)
                        StartCoroutine(BreezeSystem.FadeInBodyIK());

                    MagAmmo = AmmoPerClip;
                    ClipAmount--;

                    BreezeSystem.Invoke("EnableAttack", 0.6f);

                    CancelInvoke("ReloadCheck");
                }
            }
        }

        //Change equip mesh states
        public void OnEquipChangedAI(bool equipped, bool start = false)
        {
            if (!BreezeSystem.UseEquipSystem)
            {
                WeaponEquipped = true;
                return;
            }
            
            WeaponEquipped = equipped;

            if (!start)
                PlaySound(equipped ? "Draw" : "Holster");

            Renderer parentRend = InventoryWeaponParent.transform.GetComponent<Renderer>();
            if (parentRend != null)
            {
                parentRend.enabled = !equipped;
            }

            for (int i = 0; i < InventoryWeaponParent.transform.childCount; i++)
            {
                Renderer rend = InventoryWeaponParent.transform.GetChild(i).GetComponent<Renderer>();

                if (rend != null)
                {
                    rend.enabled = !equipped;
                }
            }

            parentRend = PrimaryWeaponParent.transform.GetComponent<Renderer>();
            if (parentRend != null)
            {
                parentRend.enabled = equipped;
            }

            for (int i = 0; i < PrimaryWeaponParent.transform.childCount; i++)
            {
                Renderer rend = PrimaryWeaponParent.transform.GetChild(i).GetComponent<Renderer>();

                if (rend != null)
                {
                    rend.enabled = equipped;
                }
            }
        }

        //Plays sound of weapon
        private void PlaySound(string Name)
        {
            if (Name == "Fire")
            {
                _source.volume = FireSoundVolume;
                _source.PlayOneShot(FireSoundEffect);
            }
            else if (Name == "Reload")
            {
                _source.volume = ReloadSoundVolume;
                _source.PlayOneShot(ReloadSoundEffect);
            }
            else if (Name == "Draw")
            {
                _source.volume = DrawSoundVolume;
                _source.PlayOneShot(DrawSoundEffect);
                DrawEvent.Invoke();
            }
            else
            {
                _source.volume = HolsterSoundVolume;
                _source.PlayOneShot(HolsterSoundEffect);
                HolsterEvent.Invoke();
            }
        }
    }
}