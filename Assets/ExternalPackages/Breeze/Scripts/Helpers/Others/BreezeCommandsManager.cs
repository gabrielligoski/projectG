using UnityEngine;

namespace Breeze.Core
{
    public class BreezeCommandsManager : MonoBehaviour
    {
        private BreezeSystem _system;

        private void Awake()
        {
            if (GetComponent<BreezeSystem>() == null)
            {
                enabled = false;
                return;
            }

            _system = GetComponent<BreezeSystem>();
        }

        //Walk To A Given Destination
        public bool WalkToADestination(Vector3 destination)
        {
            _system.stopAI = true;
            _system.SetNavMovement(false);
            _system.PlayAnimation(BreezeEnums.AnimationType.Walk);
            _system.SetDestination(destination);
            return !_system.CheckPath(destination).Equals(Vector3.zero);
        }
        
        //Returns Target
        public GameObject getTarget()
        {
            return _system.GetTarget();
        }

        //Kills AI
        public void KillAI()
        {
            _system.Death();
        }
        
        //Refill Health
        public void RefillAIHealth()
        {
            _system.CurrentHealth = _system.MaximumHealth;
        }
    }   
}
