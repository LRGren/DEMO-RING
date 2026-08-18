using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    [Header("Collider")]
    [SerializeField] protected Collider damageCollider;

    [Header("Damage")]
    public float physicalDamage = 0;
    public float magicalDamage = 0;
    public float fireDamage = 0;
    public float lightningDamage = 0;
    public float holyDamage = 0;

    [Header("Poise Damage")]
    public float poiseDamage = 0;

    [Header("Contact Point")]
    public Vector3 contactPoint;

    [Header("Character Damaged")]
    protected List<CharacterManager> characterDamaged = new List<CharacterManager>();

    [Header("Direction To Attacker")]
    protected Vector3 directionToAttacker;
    protected float dotFromDamageTargetToAttacker;

    protected virtual void Awake()
    {

    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        CharacterManager damageTarget = other.GetComponentInParent<CharacterManager>();

        if (damageTarget != null)
        {
            contactPoint = other.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

            //友军

            //格挡
            CheckForBlocking(damageTarget);

            //伤害
            DamageTarget(damageTarget);
        }
    }

    protected virtual void CheckForBlocking(CharacterManager damageTarget)
    {
        //如果已经被击中，直接不执行
        if (characterDamaged.Contains(damageTarget))
            return;

        CalculateDirectionToAttacker(damageTarget);

        if (damageTarget.characterNetworkManager.isBlocking.Value && dotFromDamageTargetToAttacker > 0.5f)
        {
            //如果是格挡状态，且攻击方向在角色前方，则不使用DmageEffect，而是使用BlockEffect
            characterDamaged.Add(damageTarget);

            TakeBlockedDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.instance.takeBlockedDamageEffect);
            damageEffect.physicalDamage = physicalDamage;
            damageEffect.magicalDamage = magicalDamage;
            damageEffect.fireDamage = fireDamage;
            damageEffect.lightningDamage = lightningDamage;
            damageEffect.holyDamage = holyDamage;

            damageEffect.staminaCost = poiseDamage;

            damageEffect.poiseDamage = poiseDamage;

            damageEffect.contactPoint = contactPoint;

            damageTarget.characterEffectsManager.ProcessInstantEffect(damageEffect);
        }
    }

    protected virtual void CalculateDirectionToAttacker(CharacterManager damageTarget)
    {
        directionToAttacker = (transform.position - damageTarget.transform.position).normalized;
        dotFromDamageTargetToAttacker = Vector3.Dot(damageTarget.transform.forward, directionToAttacker);
    }

    protected virtual void DamageTarget(CharacterManager damageTarget)
    {
        if (characterDamaged.Contains(damageTarget))
            return;

        characterDamaged.Add(damageTarget);

        TakeDamageEffect damageEffect = Instantiate(WorldCharacterEffectsManager.instance.takeDamageEffect);
        damageEffect.physicalDamage = physicalDamage;
        damageEffect.magicalDamage = magicalDamage;
        damageEffect.fireDamage = fireDamage;
        damageEffect.lightningDamage = lightningDamage;
        damageEffect.holyDamage = holyDamage;

        damageEffect.poiseDamage = poiseDamage;

        damageEffect.contactPoint = contactPoint;

        damageTarget.characterEffectsManager.ProcessInstantEffect(damageEffect);
    }

    public virtual void EnableDamageCollider()
    {
        damageCollider.enabled = true;
    }

    public virtual void DisableDamageCollider()
    {
        damageCollider.enabled = false;
        characterDamaged.Clear();
    }
}
