using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Breeze.Core
{
    public class BreezeMeleeWeapon : BreezeWeaponInterface
    {
        //Settings
        public BreezeSystem AISystem;
        public float MinWeaponDamage = 20f;
        public float MaxWeaponDamage = 35f;
        public bool OnlyDamageOnce = true;
        public List<impactEffect> ImpactEffects;
        
        //Mesh
        public GameObject InventoryWeaponParent = null;
        public GameObject PrimaryWeaponParent = null;
        
        //Sounds
        public AudioClip SwingSound = null;
        [Range(0.1f, 1f)]
        public float SwingVolume = 1f;
        public AudioClip DamagedSound = null;
        [Range(0.1f, 1f)]
        public float DamagedVolume = 1f;
        public AudioClip BlockedSound = null;
        [Range(0.1f, 1f)]
        public float BlockedVolume = 1f;
        public AudioClip DrawSound = null;
        [Range(0.1f, 1f)]
        public float DrawVolume = 1f;
        public AudioClip HolsterSound = null;
        [Range(0.1f, 1f)]
        public float HolsterVolume = 1f;

        private AudioSource _source;
        
        //Events
        public UnityEvent<float> DamagedEvent = new UnityEvent<float>();
        public UnityEvent BlockedEvent = new UnityEvent();

        private bool Damaged = false;

        //Impact Effect Class
        [Serializable]
        public class impactEffect
        {
            public enum effectType
            {
                Blocked,
                Normal,
            }
            public enum Parent
            {
                Null,
                HitObject,
            }

#if UNITY_EDITOR

            [TagSelector(UseDefaultTagFieldDrawer =  true)]
#endif
            public string ObjectTag = "Untagged";
            
            public effectType EffectType = effectType.Normal;
            public Parent SetParent = Parent.HitObject;
            public GameObject ImpactEffect;
        }

        private void OnValidate()
        {
            if(Application.isPlaying)
                return;
            
            if (AISystem == null)
            {
                Transform ThePar = transform.parent;

                while (ThePar.GetComponent<BreezeSystem>() == null)
                {
                    ThePar = ThePar.parent;
                }

                AISystem = ThePar.GetComponent<BreezeSystem>();
            }

            if (GetComponent<BoxCollider>() == null)
            {
                gameObject.AddComponent<BoxCollider>().isTrigger = true;
            }
        }

        private void Awake()
        {
            AISystem.OnEquipChanged.AddListener(OnEquipChangedAI);
            gameObject.AddComponent<Rigidbody>().isKinematic = true;

            if (_source == null)
            {
                _source = gameObject.AddComponent<AudioSource>();
                _source.playOnAwake = false;
            }
            Transform ThePar = transform.parent;

            while (ThePar.GetComponent<BreezeSystem>() == null)
            {
                ThePar = ThePar.parent;
            }

            AISystem = ThePar.GetComponent<BreezeSystem>();
        }

        private void Start()
        {
            OnEquipChangedAI(false);
        }

        private void LateUpdate()
        {
            if (AISystem.CurrentHealth <= 0f)
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
                
                PrimaryWeaponParent.transform.SetParent(null);
            }
        }

        public void OnEquipChangedAI(bool equipped, bool start = false)
        {
            if(!AISystem.UseEquipSystem)
                return;
            
            Renderer parentRend = InventoryWeaponParent.transform.GetComponent<Renderer>();
            if (parentRend != null)
            {
                parentRend.enabled = !equipped;
            }
            
            for (int i = 0; i < InventoryWeaponParent.transform.childCount; i++)
            {
                Renderer rend = InventoryWeaponParent.transform.GetChild(i).GetComponent<Renderer>();
                
                if(rend != null)
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
                
                if(rend != null)
                {
                    rend.enabled = equipped;
                }
            }
            
            if(!start)
                Playsound(equipped ? "Draw" : "Holster");
        }

        private void Checker()
        {
            if (!AISystem.anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            {
                Damaged = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            InvokeRepeating("Checker", 0, 0.25f);
            if (AISystem.GetTarget() == null || AISystem.Blocking || !AISystem.anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack") ||
                AISystem.CurrentAIState.Equals("Dead") || other.gameObject.Equals(AISystem.gameObject) ||
                AISystem.anim.GetCurrentAnimatorStateInfo(0).IsTag("Hit"))
            {
                return;
            }


            if (OnlyDamageOnce && Damaged)
            {
                return;   
            }

            BreezeDamageable damageable = other.GetComponent<BreezeDamageable>();
            BreezePlayer player = other.GetComponent<BreezePlayer>();
            
            if(damageable == null && player == null)
                return;
            

            float damage = Random.Range(MinWeaponDamage, MaxWeaponDamage + 1f);

            if (other.gameObject.tag.Equals(AISystem.BreezeTag))
            {
                if (!damageable.System.Blocking)
                {
                    Damaged = true;
                    damageable.System.TakeDamage(damage, AISystem.gameObject, false);
                    Playsound("Damaged");
                    DamagedEvent.Invoke(damage);   
                }
            }
            
            if (other.gameObject.tag.Equals(AISystem.PlayerTag))
            {
                Damaged = true;
                other.GetComponent<BreezePlayer>().TakeDamage(damage, AISystem.gameObject);
                Playsound("Damaged");
                DamagedEvent.Invoke(damage);
            }

            if (!AISystem.TargetIsPlayer && AISystem.TargetAIScript.Blocking)
            {
                Playsound("Block");
                BlockedEvent.Invoke();
            }

            if (AISystem != null)
            {
                List<impactEffect> effectsAvailable = new List<impactEffect>();

                foreach (var effect in ImpactEffects)
                {
                    if (AISystem.TargetIsPlayer && effect.EffectType == impactEffect.effectType.Normal)
                    {
                        if (effect.ObjectTag.Equals(other.gameObject.tag))
                        {
                            GameObject Effect = Instantiate(effect.ImpactEffect,
                                transform.position, Quaternion.FromToRotation(Vector3.forward, transform.position));

                            if (effect.SetParent == impactEffect.Parent.Null)
                            {
                                Effect.transform.SetParent(null);
                            }
                            else
                            {
                                Effect.transform.SetParent(other.transform);
                            }
                        }
                    }
                    
                    if(AISystem.TargetIsPlayer)
                        break;
                    
                    if ((AISystem.TargetAIScript.Blocking && effect.EffectType == impactEffect.effectType.Blocked) || (!AISystem.TargetAIScript.Blocking && effect.EffectType == impactEffect.effectType.Normal))
                    {
                        if (effect.ObjectTag.Equals(other.gameObject.tag))
                        {
                            GameObject Effect = Instantiate(effect.ImpactEffect,
                                transform.position, Quaternion.FromToRotation(Vector3.forward, transform.position));

                            if (effect.SetParent == impactEffect.Parent.Null)
                            {
                                Effect.transform.SetParent(null);
                            }
                            else
                            {
                                Effect.transform.SetParent(other.transform);
                            }
                            break;
                        }
                    }
                        
                }
            }
        }

        public void Playsound(string name)
        {
            _source.Stop();
            
            if (name == "Swinged" && SwingSound != null)
            {
                _source.volume = SwingVolume;
                _source.PlayOneShot(SwingSound);
            }
            else if (name == "Damaged" && DamagedSound != null)
            {
                _source.volume = DamagedVolume;
                _source.PlayOneShot(DamagedSound);
            }
            else if (name == "Draw" && DrawSound != null)
            {
                _source.volume = DrawVolume;
                _source.PlayOneShot(DrawSound);
            }
            else if (name == "Holster" && HolsterSound != null)
            {
                _source.volume = HolsterVolume;
                _source.PlayOneShot(HolsterSound);
            }
            else if(BlockedSound != null)
            {
                _source.volume = BlockedVolume;
                _source.PlayOneShot(BlockedSound);
            }
        }
    }
    
    #if UNITY_EDITOR
    
    public class TagSelectorAttribute : PropertyAttribute
    {
        public bool UseDefaultTagFieldDrawer = false;
    }
    
    [CustomPropertyDrawer(typeof(TagSelectorAttribute))]
    public class TagSelectorPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                EditorGUI.BeginProperty(position, label, property);

                TagSelectorAttribute selectorAttribute = attribute as TagSelectorAttribute;

                if (selectorAttribute.UseDefaultTagFieldDrawer)
                {
                    property.stringValue = EditorGUI.TagField(position, label, property.stringValue);
                }
                else
                {
                    List<string> tagList = new List<string> {"<NoTag>"};
                    tagList.AddRange(InternalEditorUtility.tags);
                    string propertyString = property.stringValue;
                    int index = -1;
                    if (propertyString == "")
                    {
                        index = 0;
                    }
                    else
                    {
                        for (int i = 1; i < tagList.Count; i++)
                        {
                            if (tagList[i] != propertyString)
                            {
                                continue;
                            }

                            index = i;
                            break;
                        }
                    }

                    index = EditorGUI.Popup(position, label.text, index, tagList.ToArray());

                    if (index == 0)
                    {
                        property.stringValue = "";
                    }
                    else if (index >= 1)
                    {
                        property.stringValue = tagList[index];
                    }
                    else
                    {
                        property.stringValue = "";
                    }
                }

                EditorGUI.EndProperty();
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
    #endif
}