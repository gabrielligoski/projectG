using Breeze.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class ExplosiveTrap : Room
{
    [SerializeField] private GameObject explosionVFX;
    public List<Effect> effects;

    public override RoomType roomType()
    {
        return name;
    }

    private List<CharacterController> enemies;


    [SerializeField] private AudioClip explodeSound;
    [SerializeField] private float damage;
    [SerializeField] private float countdown;
    [SerializeField] private bool coroutine;


    private void Start()
    {
        enemies = new List<CharacterController>();
        effects = new List<Effect>();
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
    private void applyDebuffs(CharacterController controller)
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
            controller.GetComponent<BreezeSystem>().TakeDamage(damage, gameObject, true);
            Instantiate(explosionVFX, transform.position + new Vector3(0, 1), Quaternion.identity, null);
            applyDebuffs(controller);
        }
    }
    IEnumerator enemyCheck(float countdown)
    {
        yield return new WaitForSeconds(countdown);
        foreach (var enemy in enemies)
        {
            if (enemy)
            {
                dealHit(enemy);
            }
        }
        SFXManager.Instance.playSFXClip(explodeSound,transform,1f);
        destroy();
    }

    public void destroy()
    {
        GameMaster.Instance.swapRoom(gameObject, Room.RoomType.empty);
        Destroy(gameObject, countdown+1);
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
                    StartCoroutine(enemyCheck(countdown));
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
