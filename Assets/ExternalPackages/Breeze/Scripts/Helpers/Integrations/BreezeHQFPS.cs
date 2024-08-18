#if HQ_FPS_TEMPLATE
using HQFPSTemplate;
using UnityEngine;

namespace Breeze.Core
{
    public class BreezeHQFPS : MonoBehaviour, IDamageable
    {
        private BreezeDamageable System;

        private void Awake()
        {
            System = GetComponent<BreezeDamageable>();

            if (System == null)
                Destroy(this);
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            System.TakeDamage(-damageInfo.Delta, damageInfo.Source.gameObject, true);
        }
    }
}
#endif