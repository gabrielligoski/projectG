using UnityEngine;

public abstract class Effect 
{
    public abstract string name();
    public abstract string description();
    public abstract EffectType type();
    public virtual void Apply(CharacterController entity) { }
    public virtual void Remove(CharacterController entity) { }
    public enum EffectType { 
        buff,
        debuff,
        dot,
    }
}
