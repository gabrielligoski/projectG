using Breeze.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Tower : Room
{
    private List<Effect> effects;

    public override RoomType roomType()
    {
        if (Enum.TryParse(name, out RoomType parsed))
            return parsed;
        else
        {
            Debug.LogError("Tower couldn´t find the correct enum type");
            return RoomType.none;
        }

    }

    private void Update()
    {
        effects = new List<Effect>
        {
            new Slow()
        };
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.TryGetComponent<CharacterController>(out CharacterController e)) {
            if(e.type == CharacterController.CharacterType.human)
            {
                effects.ForEach(effect => e.applyEffect(effect));
            }
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.TryGetComponent<CharacterController>(out CharacterController e))
        {
            if (e.type == CharacterController.CharacterType.human)
            {
                effects.ForEach(effect => e.removeEffect(effect));
            }
        }
    }
}
