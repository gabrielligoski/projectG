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

    [SerializeField] private float damage;
    [SerializeField] private float cooldown;
    [SerializeField] private bool coroutine;


    private void Start()
    {
        enemies = new List<CharacterController>();
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
        var e = controller.GetComponent<BreezeSystem>();
        if (e.CurrentHealth > 0)
        {
            _animator.SetTrigger("play");
            controller.GetComponent<BreezeSystem>().TakeDamage(damage, gameObject, true);
            applyDebuffs(controller);
        }
    }
    IEnumerator enemyCheck(float cooldown)
    {
        for (; ; )
        {
            if (enemies.Count > 0)
            {
                foreach (var enemy in enemies)
                {
                    if (enemy)
                    {
                        dealHit(enemy);
                    }
                }
            }
            else
                break;
            yield return new WaitForSeconds(cooldown);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.TryGetComponent(out CharacterController e))
        {
            if (e.type == CharacterController.CharacterType.enemy)
            {
                Debug.Log("trap activated!");
                enemies.Add(e);
                if (!coroutine)
                {
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
