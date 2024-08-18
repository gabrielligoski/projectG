
namespace Breeze
{
    public class BreezeEnums
    {
        public enum RegenerateCondition
        {
            NonCombatState,
            CombatState,
            Both,
        }
        
        public enum YesNo
        {
            Yes,
            No,
        }
        public enum AIBehaviour
        {
            Enemy,
            Companion,
        }
        public enum WeaponType
        {
            Unarmed,
            Melee,
            Shooter,
        }
        
        public enum ParticleType
        {
            OnTarget,
            Custom
        }
        public enum AIConfidence
        {
            Brave,
            Neutral,
            Coward,
        }
        public enum AIWanderingType
        {
            Patrol,
            Waypoint,
            Stationary,
        }
        public enum AIDeathType
        {
            Animation,
            Ragdoll,
        }

        public enum DetectionType
        {
            Trigger,
            LineOfSight,
        }
        
        public enum DetectionLock
        {
            Closest,
            OneByOne,
        }
    
        public enum Factions
        {
            Creature,
            Wildlife,
            NPC,
            Soldier,
            Fighter,
            GeneralAI,
        }
    
        public enum ShootingType
        {
            Single,
            Additive,
        }
        
        public enum BulletType
        {
            Projectile,
            Raycast,
        }
        
        public enum BehaviourType
        {
            Enemy, Neutral, Friendly
        }
    
        public enum AttackBehaviour
        {
            Aggressive,
            Passive,
        }

        public enum AnimationType
        {
            Idle,
            Walk,
            BackAway,
            Run,
            Hit,
            Attack,
            Reload,
            Block,
            Death,
        }

        public enum MovementType
        {
            None,
            Walking,
            Backaway,
            Running,
        }
        
        public enum WaypointOrder
        {
            InOrder,
            Random
        }
    }
}