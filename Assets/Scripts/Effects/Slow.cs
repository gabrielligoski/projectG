using UnityEngine;

public class Slow : Effect
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public override EffectType type()
    {
        return EffectType.debuff;
    }

    public override string description()
    {
        return "Debuff effect";
    }

    public override string name()
    {
        return "Slow";
    }
    // Update is called once per frame
    public void Apply(CharacterController cc)
    {
        cc.walkSpeed -= 5;
        cc.updateBreeze();
    }
    public void Remove(CharacterController cc)
    {
        cc.walkSpeed += 5;
        cc.updateBreeze();
    }
}
    