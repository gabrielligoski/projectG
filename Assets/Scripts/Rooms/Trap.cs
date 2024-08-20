using System.Collections.Generic;
using UnityEngine;

public class Trap : Room
{
    private List<Effect> effects;

    public override RoomType roomType()
    {
        return name;
    }

    //private void Update()
    //{
    //    effects = new List<Effect>
    //    {
    //        new Slow()
    //    };
    //}

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.TryGetComponent<CharacterController>(out CharacterController e)) {
            if(e.type == CharacterController.CharacterType.enemy)
            {
                //effects.ForEach(effect => e.applyEffect(effect));
            }
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.TryGetComponent<CharacterController>(out CharacterController e))
        {
            if (e.type == CharacterController.CharacterType.enemy)
            {
                //effects.ForEach(effect => e.removeEffect(effect));
            }
        }
    }
}
