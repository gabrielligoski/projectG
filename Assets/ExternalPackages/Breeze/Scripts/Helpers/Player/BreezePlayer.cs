using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if INVECTOR_MELEE
using Invector;
using Invector.vCharacterController;
#endif

#if FIRST_PERSON_CONTROLLER || THIRD_PERSON_CONTROLLER
using Opsive.UltimateCharacterController.Traits;
#endif

#if NEOFPS
using NeoFPS;
#endif

#if HQ_FPS_TEMPLATE
using HQFPSTemplate;
using DamageType = HQFPSTemplate.DamageType;
#endif

#if SURVIVAL_TEMPLATE_PRO
using PolymindGames;
#endif

namespace Breeze.Core
{
    public class BreezePlayer : MonoBehaviour
    {
       //Settings
       public float CurrentHealth = 100f;
       public Transform HitPosition;

       [HideInInspector] public UnityEvent<GameObject> gotAttackedEvent = new UnityEvent<GameObject>();

       private void OnValidate()
       {
           if(Application.isPlaying)
               return;

           if (HitPosition == null)
           {
               GameObject hitPos = new GameObject("Hit Position");
               hitPos.transform.SetParent(transform);
               hitPos.transform.localPosition = new Vector3(0, 0.75f, 0);
               hitPos.transform.localRotation.eulerAngles.Set(0,0,0);
               hitPos.transform.localScale = new Vector3(1, 1, 1);
               hitPos.AddComponent<BreezeHitPosition>();

               HitPosition = hitPos.transform;
           }
       }

       private void LateUpdate()
       {
#if NEOFPS
           if(GetComponent<BasicHealthManager>() != null)
               CurrentHealth = GetComponent<BasicHealthManager>().health;
#endif

#if HQ_FPS_TEMPLATE
           if(GetComponent<Player>() != null)
               CurrentHealth = GetComponent<Player>().Health.Val;
#endif

#if SURVIVAL_TEMPLATE_PRO
           if (GetComponent<Player>() != null)
               CurrentHealth = GetComponent<Player>().HealthManager.Health;
#endif

#if INVECTOR_MELEE
           if (GetComponent<vThirdPersonController>() != null)
           {
              CurrentHealth = GetComponent<vThirdPersonController>().currentHealth;
           }
#endif

#if FIRST_PERSON_CONTROLLER || THIRD_PERSON_CONTROLLER
           if (GetComponent<CharacterHealth>() != null)
           {
               CurrentHealth = GetComponent<CharacterHealth>().HealthValue;
           }
#endif
       }

       public void TakeDamage(float damage, GameObject sender)
       {
           gotAttackedEvent.Invoke(sender);
#if NEOFPS
           if (GetComponent<BasicHealthManager>() != null)
           {
               GetComponent<BasicHealthManager>().AddDamage(damage);
           }
#endif

#if INVECTOR_MELEE
           if (GetComponent<vThirdPersonController>() != null)
           {
               GetComponent<vThirdPersonController>().TakeDamage(new vDamage((int)damage));
           }
#endif

#if HQ_FPS_TEMPLATE
           if (GetComponent<Player>() != null)
           {
               GetComponent<Player>().DealDamage.Try(new DamageInfo(-damage, DamageType.Bullet, GetComponent<Player>()), GetComponentInChildren<Hitbox>());
           }
#endif

#if SURVIVAL_TEMPLATE_PRO
           if (GetComponent<Player>() != null)
           {
               GetComponent<Player>().HealthManager.ReceiveDamage(damage);
           }
#endif

#if FIRST_PERSON_CONTROLLER || THIRD_PERSON_CONTROLLER
           if (GetComponent<CharacterHealth>() != null)
           {
               GetComponent<CharacterHealth>().Damage(damage);
           }
#endif
       }
    }
}