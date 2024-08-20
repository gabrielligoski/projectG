using Breeze.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class SpikeTrap : Room
{
    private Animator _animator;
    public List<Effect> effects;

    public override RoomType roomType()
    {
        return name;
    }

    private List<CharacterController> enemies;
    [SerializeField] private AudioClip sfxClip;

    [SerializeField] private float damage;
    [SerializeField] private float cooldown;
    [SerializeField] private bool coroutine;


    private void Start()
    {
        enemies = new List<CharacterController>();
        effects = new List<Effect>();
        _animator = GetComponent<Animator>();
    }
    //private void Update()
    //{
    //    effects = new List<Effect>
    //    {
    //        new Slow()
    //    };
    //}
    IEnumerator debuff(CharacterController controller, Effect effect)
    {
        controller.applyEffect(effect);
        yield return new WaitForSeconds(effect.duration());
        controller.removeEffect(effect);
    }
    void applyDebuffs(CharacterController controller)
    {
        effects.ForEach(effect =>
        {
            StartCoroutine(debuff(controller, effect));
        });
    }
    private void dealHit(CharacterController controller)
    {
        if (controller && controller.TryGetComponent(out BreezeSystem bs) && bs.CurrentHealth > 0)
        {
            bs.TakeDamage(damage, gameObject, true, false);
            applyDebuffs(controller);
        }
    }
    IEnumerator enemyCheck(float cooldown)
    {
        while (true)
        {
            if (enemies.Count > 0)
                enemies.ForEach(enemy => dealHit(enemy));
            else
                break;
            yield return new WaitForSeconds(cooldown);
        }
        coroutine = false;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.TryGetComponent(out CharacterController e))
        {
            if (e.type == CharacterController.CharacterType.enemy)
            {
                enemies.Add(e);
                if (!coroutine)
                {
                    coroutine = true;
                    _animator.SetTrigger("play");
                    SFXManager.Instance.playSFXClip(sfxClip, transform, 1f, 0.05f);
                    _animator.ResetTrigger("play");
                    StartCoroutine(enemyCheck(cooldown));
                }
            }
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.TryGetComponent(out CharacterController e))
        {
            if (e.type == CharacterController.CharacterType.enemy)
            {
                if (e)
                {
                    enemies.Remove(e);
                }
            }
        }
    }
}
