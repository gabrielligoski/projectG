#if FIRST_PERSON_CONTROLLER || THIRD_PERSON_CONTROLLER
using Opsive.UltimateCharacterController.Traits.Damage;
using UnityEngine;

namespace Breeze.Core
{
    public class BreezeOpsive : MonoBehaviour, IDamageTarget
    {
        private BreezeDamageable System;

        private void Awake()
        {
            System = GetComponent<BreezeDamageable>();
            
            if(System == null)
                Destroy(this);
        }


        public GameObject Owner { get; }
        public GameObject HitGameObject { get; }
        public void Damage(DamageData damageData)
        {
            System.TakeDamage(damageData.Amount, damageData.DamageOriginator.Owner, true);
        }

        public bool IsAlive()
        {
            return System.System.CurrentHealth > 0;
        }
    }
}
#endif