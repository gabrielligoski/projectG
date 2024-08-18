using UnityEngine;
using UnityEngine.Events;

namespace Breeze.Core
{
    public class BreezeEvents : MonoBehaviour
    {
        //Others
        private BreezeSystem _system;

        //Movement Events
        public UnityEvent<BreezeEnums.MovementType> OnMovementChanged = new UnityEvent<BreezeEnums.MovementType>();
        public UnityEvent<bool> OnAgentStoppedState = new UnityEvent<bool>();
        public UnityEvent OnFleeAway = new UnityEvent();

        //Animation Events
        public UnityEvent<BreezeEnums.AnimationType> OnAnimationPlayed = new UnityEvent<BreezeEnums.AnimationType>();
        public UnityEvent<string> OnAnimationEvent = new UnityEvent<string>();

        //Path Events
        public UnityEvent OnPathUnreachable = new UnityEvent();

        //Optimization Events
        public UnityEvent<bool> OnPauseAIState = new UnityEvent<bool>();
        public UnityEvent<bool> OnVisibleState = new UnityEvent<bool>();

        //Health Events
        public UnityEvent<float> OnTakeDamage = new UnityEvent<float>();
        public UnityEvent<bool> OnRegenHealth = new UnityEvent<bool>();
        public UnityEvent OnDeath = new UnityEvent();

        //Combat Events
        public UnityEvent<float> OnDealDamage = new UnityEvent<float>();
        public UnityEvent OnKilledTarget = new UnityEvent();
        public UnityEvent<weaponClass> OnSwappedWeapon = new UnityEvent<weaponClass>();
        public UnityEvent<bool> OnAlertState = new UnityEvent<bool>();

        //Detection Events
        public UnityEvent<GameObject> OnFoundTarget = new UnityEvent<GameObject>();
        public UnityEvent<GameObject> OnLostTarget = new UnityEvent<GameObject>();
        public UnityEvent<GameObject> OnSoundDetected = new UnityEvent<GameObject>();
    }
}