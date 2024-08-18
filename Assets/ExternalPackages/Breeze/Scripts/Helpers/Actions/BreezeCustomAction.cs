using UnityEngine;
using UnityEngine.Events;

namespace Breeze.Core
{
    public enum CustomAnimationType
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
        Custom,
    }

    public enum CustomType
    {
        Bool,
        Trigger,
        Number,
    }

    public enum CommandType
    {
        WalkToDestination,
        ToAddMoreActionsCheckDocumentation
    }

    public enum DestinationType
    {
        Transform,
        Vector3,
    }
    
    [CreateAssetMenu(fileName = "New Action", menuName = "Breeze/Custom Action", order = 1)]
    public class BreezeCustomAction : ScriptableObject
    {
        public bool animationAction = true;

        public bool ShouldRepeatUntilEnds = false;
        
        
        //-MAIN ANIMATION PARAMETERS-\\
        public CustomAnimationType CustomAnimationType = CustomAnimationType.Custom;
        
        //Custom Animation Parameters
        public CustomType CustomType = CustomType.Trigger;
        public string ParameterName = "Enter Parameter Name..";
        public bool ParameterGoalValue = true;
        public float ParameterGoalNumber = 1f;
        public float ActionLength = 5f;
        
        
        //-MAIN COMMAND PARAMETERS-\\
        public CommandType CommandType = CommandType.WalkToDestination;
        
        //Custom Command Parameters
          
          //- Walk To Destination Example
          public DestinationType DestinationType = DestinationType.Transform;
          public string GoalDestinationName = "Enter Object Name...";
          public Vector3 GoalPosition = new Vector3(0, 0, 0);
          public float StoppingDistanceOverride = 0.1f;
          public float WaitWhenArrived = 0f;


          public UnityEvent eventToPlay = new UnityEvent();
    }
}