using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickSlotItemProjectileDamageCollider : DamageCollider
{
    public CharacterManager user;
    QuickSlotItemProjectileManager _quickSlotItemProjectileManager;

    protected override void Awake()
    {
        base.Awake();

        _quickSlotItemProjectileManager = GetComponentInParent<QuickSlotItemProjectileManager>();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        CharacterManager damageTarget = other.GetComponentInParent<CharacterManager>();

        if (damageTarget != null)
        {
            if (user == damageTarget)
            {
                // 如果碰撞到的角色是自己，直接返回，不进行伤害计算
                return;
            }

            //Debug.Log(damageTarget.name);
            contactPoint = other.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

            //友军
            if (!WorldUtilityManager.instance.CanIDamageThisTarget(user.characterGroup, damageTarget.characterGroup))
                return;

            //弹反
            CheckForParry(damageTarget);

            //格挡
            CheckForBlocking(damageTarget);

            //伤害
            if (!damageTarget.characterNetworkManager.isInvulnerable.Value)
                DamageTarget(damageTarget);

            _quickSlotItemProjectileManager.WaitThenDestroyProjectile(0.4f);
        }
    }

    protected override void CalculateDirectionToAttacker(CharacterManager damageTarget)
    {
        directionToAttacker = (user.transform.position - damageTarget.transform.position).normalized;
        dotFromDamageTargetToAttacker = Vector3.Dot(damageTarget.transform.forward, directionToAttacker);
    }

    protected override void DamageTarget(CharacterManager damageTarget)
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
        damageEffect.angleHitFrom = Vector3.SignedAngle(user.transform.forward, damageTarget.transform.forward, Vector3.up);

        if (user.IsOwner)
        {
            // 只有攻击者的客户端才会处理伤害效果，其他客户端通过网络同步伤害结果
            //发送攻击请求
            damageTarget.characterNetworkManager.NotifyTheServerOfCharacterDamageServerRpc(
                damageTarget.NetworkObjectId, user.NetworkObjectId,
                damageEffect.physicalDamage, damageEffect.magicalDamage, damageEffect.fireDamage, damageEffect.holyDamage, damageEffect.lightningDamage,
                damageEffect.angleHitFrom, damageEffect.poiseDamage, damageEffect.contactPoint.x, damageEffect.contactPoint.y, damageEffect.contactPoint.z);
        }

        //damageTarget.characterEffectsManager.ProcessInstantEffect(damageEffect);
    }
}
