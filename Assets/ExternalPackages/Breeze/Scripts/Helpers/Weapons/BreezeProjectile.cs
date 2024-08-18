using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Breeze.Core
{
    public class BreezeProjectile : MonoBehaviour
    {
        private bool done = false;
        [HideInInspector] public BreezeGun Gun;
        [HideInInspector] public float damage = 0;
        [HideInInspector] public LayerMask hitMask = 0;
        [HideInInspector] public GameObject sender = null;

        private void Update()
        {
            if(done)
                return;

            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 0.9f, hitMask,
                    QueryTriggerInteraction.Ignore))
            {
                GameObject col = hit.transform.gameObject;

                BreezeDamageable damageable = col.GetComponent<BreezeDamageable>();

                if (damageable != null)
                {
                    BreezeSystem system = damageable.System;

                    if (system != null)
                    {
                        // if(system.CurrentAIFaction == Gun.BreezeSystem.CurrentAIFaction)
                        //     return;
                    
                        foreach (var fact in system.AIFactionsList)
                        {
                            if (fact.Factions == Gun.BreezeSystem.CurrentAIFaction)
                            {
                                if(fact.behaviourType == BreezeEnums.BehaviourType.Friendly)
                                    return;
                            }
                        }
                        system.TakeDamage(damage, sender, false);
                        done = true;
                    }
                }
                else
                {
                    BreezePlayer player = col.GetComponent<BreezePlayer>();

                    if (player != null)
                    {
                        player.TakeDamage(damage, Gun.BreezeSystem.gameObject);
                        done = true;
                    }
                }
                
                foreach (var effect in Gun.ImpactEffects)
                {
                    if (effect.ObjectTag.Equals(col.tag))
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
                Destroy(gameObject, 0.075f);
            }
            Destroy(gameObject, 10);
        }
    }
}