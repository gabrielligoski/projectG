using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Breeze.Core
{
    public interface BreezeDamageable
    {
        public BreezeSystem System { get; set; }
        public void TakeDamage(float Amount, GameObject Sender, bool IsPlayer, bool HitReaction = true);
    }
}