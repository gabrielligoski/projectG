using UnityEngine;

public class Slow : Effect
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public float effectPower = 2;
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
    public override void Apply(CharacterController cc)
    {
        cc.walkSpeed -= 2;
        cc.runSpeed -= 2;
    }
    public override void Remove(CharacterController cc)
    {
        cc.walkSpeed += 2;
        cc.runSpeed += 2;
    }
}
    