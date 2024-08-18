#if INVECTOR_MELEE
using Invector;
using UnityEngine;

namespace Breeze.Core
{
    public class BreezeInvector : MonoBehaviour, vIDamageReceiver
    {
        public OnReceiveDamage onStartReceiveDamage { get; }
        public OnReceiveDamage onReceiveDamage { get; }
        
        private BreezeDamageable System;

        private void Awake()
        {
            System = GetComponent<BreezeDamageable>();

            if (System == null)
                Destroy(this);
        }
        
        public void TakeDamage(vDamage damage)
        {
            System.TakeDamage(damage.damageValue, damage.sender.gameObject, true);
        }
    }   
}
#endif