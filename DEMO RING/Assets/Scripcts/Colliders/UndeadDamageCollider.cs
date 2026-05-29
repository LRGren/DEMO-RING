using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndeadDamageCollider : DamageCollider
{
    [SerializeField] private AICharacterManager undeadCharacter;

    protected override void Awake()
    {
        base.Awake();
        damageCollider = GetComponent<Collider>();
        undeadCharacter = GetComponentInParent<AICharacterManager>();
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
        damageEffect.contactPoint = contactPoint;
        damageEffect.angleHitFrom = Vector3.SignedAngle(undeadCharacter.transform.forward, damageTarget.transform.forward, Vector3.up);

        if (damageTarget.IsOwner)
        {
            // 只有攻击者的客户端才会处理伤害效果，其他客户端通过网络同步伤害结果
            //发送攻击请求
            damageTarget.characterNetworkManager.NotifyTheServerOfCharacterDamageServerRpc(
                damageTarget.NetworkObjectId, undeadCharacter.NetworkObjectId,
                damageEffect.physicalDamage, damageEffect.magicalDamage, damageEffect.fireDamage, damageEffect.holyDamage, damageEffect.lightningDamage,
                damageEffect.angleHitFrom, damageEffect.contactPoint.x, damageEffect.contactPoint.y, damageEffect.contactPoint.z);
        }

        //damageTarget.characterEffectsManager.ProcessInstantEffect(damageEffect);
    }
}
