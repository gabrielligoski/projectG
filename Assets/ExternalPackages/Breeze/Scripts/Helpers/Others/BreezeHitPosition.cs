using System;
using UnityEngine;

namespace Breeze.Core
{
    public class BreezeHitPosition : MonoBehaviour
    {
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1.0f, 0.0f, 0.0f, 0.75f);
            Gizmos.DrawSphere(transform.position, 0.085f);   
        }
    }
}