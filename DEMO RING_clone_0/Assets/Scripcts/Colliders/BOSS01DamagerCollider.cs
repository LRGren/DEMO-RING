using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BOSS01DamageCollider : DamageCollider
{
    [SerializeField] private AIBossCharacterManager bossCharacter;

    protected override void Awake()
    {
        base.Awake();
        damageCollider = GetComponent<Collider>();
        bossCharacter = GetComponentInParent<AIBossCharacterManager>();
    }

    protected override void CalculateDirectionToAttacker(CharacterManager damageTarget)
    {
        directionToAttacker = (bossCharacter.transform.position - damageTarget.transform.position).normalized;
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
        damageEffect.angleHitFrom = Vector3.SignedAngle(bossCharacter.transform.forward, damageTarget.transform.forward, Vector3.up);

        if (damageTarget.IsOwner)
        {
            // 只有攻击者的客户端才会处理伤害效果，其他客户端通过网络同步伤害结果
            //发送攻击请求
            damageTarget.characterNetworkManager.NotifyTheServerOfCharacterDamageServerRpc(
                damageTarget.NetworkObjectId, bossCharacter.NetworkObjectId,
                damageEffect.physicalDamage, damageEffect.magicalDamage, damageEffect.fireDamage, damageEffect.holyDamage, damageEffect.lightningDamage,
                damageEffect.angleHitFrom, damageEffect.poiseDamage, damageEffect.contactPoint.x, damageEffect.contactPoint.y, damageEffect.contactPoint.z);
        }

        //damageTarget.characterEffectsManager.ProcessInstantEffect(damageEffect);
    }

    protected override void CheckForParry(CharacterManager damageTarget)
    {
        if (characterDamaged.Contains(damageTarget))
            return;

        if (!bossCharacter.characterNetworkManager.isParryable.Value)
            return;

        if (!damageTarget.IsOwner)
        {
            // 只有被攻击者的客户端才会处理弹反逻辑，其他客户端通过网络同步弹反结果
            return;
        }

        if (damageTarget.characterNetworkManager.isParrying.Value)
        {
            // 如果被攻击者正在进行弹反，则触发弹反效果
            characterDamaged.Add(damageTarget);

            // 触发弹反效果

            // 通知服务器处理弹反结果
            damageTarget.characterNetworkManager.NotifyTheServerOfCharacterParriedServerRpc(bossCharacter.NetworkObjectId);
            damageTarget.characterCombatManager.CloseAllDamageColliders();
        }

    }
}
