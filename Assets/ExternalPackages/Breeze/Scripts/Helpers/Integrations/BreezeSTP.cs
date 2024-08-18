#if SURVIVAL_TEMPLATE_PRO
using PolymindGames;
using UnityEngine;

namespace Breeze.Core
{
    public class BreezeSTP : MonoBehaviour, IDamageReceiver
    {
        private BreezeDamageable System;

        private void Awake()
        {
            System = GetComponent<BreezeDamageable>();

            if (System == null)
                Destroy(this);
        }
        
        public DamageResult HandleDamage(float damage, DamageContext context = default)
        {
            System.TakeDamage(damage, context.Source.gameObject, true);
            return DamageResult.Default;
        }
    }   
}
#endif