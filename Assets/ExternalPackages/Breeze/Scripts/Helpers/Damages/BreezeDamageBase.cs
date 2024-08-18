using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Breeze.Core
{
    public class BreezeDamageBase : MonoBehaviour, BreezeDamageable
    {
        [Header("SETTINGS")] 
        [Space] 
        [Space] public bool CanReceiveDamage = true;
        [Range(1f, 10f)] 
        [Space] public float DamageMultiplier = 1f;

        public BreezeSystem System { get; set; }
        public void TakeDamage(float Amount, GameObject Sender, bool IsPlayer, bool HitReaction = true)
        {
            if(System == null || !CanReceiveDamage)
                return;
            
            System.TakeDamage(Amount * DamageMultiplier, Sender, IsPlayer, HitReaction);
        }
    }
}