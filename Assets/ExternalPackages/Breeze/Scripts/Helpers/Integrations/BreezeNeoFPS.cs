#if NEOFPS
using UnityEngine;
using NeoFPS;

namespace Breeze.Core
{
    public class BreezeNeoFPS : MonoBehaviour, IDamageHandler
    {
        private BreezeDamageable System;
        public DamageFilter inDamageFilter { get; set; }
        public IHealthManager healthManager { get; }

        private void Awake()
        {
            System = GetComponent<BreezeDamageable>();
            
            if(System == null)
                Destroy(this);
        }

        public DamageResult AddDamage(float damage)
        {
            System.TakeDamage(damage, null, true);
           return DamageResult.Standard;
        }

        public DamageResult AddDamage(float damage, RaycastHit hit)
        {
            System.TakeDamage(damage, null, true);
            return DamageResult.Standard;
        }

        public DamageResult AddDamage(float damage, IDamageSource source)
        {
            System.TakeDamage(damage, source.controller.currentCharacter.gameObject, true);
            return DamageResult.Standard;
        }

        public DamageResult AddDamage(float damage, RaycastHit hit, IDamageSource source)
        {
            System.TakeDamage(damage, source.controller.currentCharacter.gameObject, true);
            return DamageResult.Standard;
        }
    }
}
#endif