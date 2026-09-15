using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeaponDamageCollider : DamageCollider
{
    [Header("Attacking Character")]
    public CharacterManager characterCasuingDamage;

    [Header("Modifiers")]
    public float light_Attack_01_Modifier;
    public float light_Attack_02_Modifier;
    public float heavy_Attack_01_Modifier;
    public float heavy_Attack_02_Modifier;
    public float charged_Attack_01_Modifier;
    public float charged_Attack_02_Modifier;
    public float run_Attack_01_Modifier;
    public float roll_Attack_01_Modifier;
    public float backstep_Attack_01_Modifier;
    public float jump_Light_Attack_01_Modifier;
    public float jump_Heavy_Attack_01_Modifier;

    protected override void Awake()
    {
        base.Awake();

        if (damageCollider == null)
        {
            damageCollider = GetComponent<Collider>();
        }

        // 常态下，碰撞体是关闭的，只有在攻击动画的特定帧才会打开
        damageCollider.enabled = false;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        CharacterManager damageTarget = other.GetComponentInParent<CharacterManager>();

        if (damageTarget != null)
        {
            if (characterCasuingDamage == damageTarget)
            {
                // 如果碰撞到的角色是自己，直接返回，不进行伤害计算
                return;
            }

            //Debug.Log(damageTarget.name);
            contactPoint = other.GetComponent<Collider>().ClosestPointOnBounds(transform.position);

            //友军
            if (!WorldUtilityManager.instance.CanIDamageThisTarget(characterCasuingDamage.characterGroup, damageTarget.characterGroup))
                return;

            //弹反
            CheckForParry(damageTarget);

            //格挡
            CheckForBlocking(damageTarget);

            //伤害
            if (!damageTarget.characterNetworkManager.isInvulnerable.Value)
                DamageTarget(damageTarget);
        }
    }

    protected override void CheckForParry(CharacterManager damageTarget)
    {
        if (characterDamaged.Contains(damageTarget))
            return;

        if (!characterCasuingDamage.characterNetworkManager.isParryable.Value)
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
            damageTarget.characterNetworkManager.NotifyTheServerOfCharacterParriedServerRpc(characterCasuingDamage.NetworkObjectId);
            damageTarget.characterCombatManager.CloseAllDamageColliders();
        }

    }

    protected override void CalculateDirectionToAttacker(CharacterManager damageTarget)
    {
        directionToAttacker = (characterCasuingDamage.transform.position - damageTarget.transform.position).normalized;
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
        damageEffect.angleHitFrom = Vector3.SignedAngle(characterCasuingDamage.transform.forward, damageTarget.transform.forward, Vector3.up);

        switch (characterCasuingDamage.characterCombatManager.currentAttackType)
        {
            case AttackType.LightAttack01:
                ApplyAttackModifier(light_Attack_01_Modifier, damageEffect);
                break;
            case AttackType.LightAttack02:
                ApplyAttackModifier(light_Attack_02_Modifier, damageEffect);
                break;
            case AttackType.HeavyAttack01:
                ApplyAttackModifier(heavy_Attack_01_Modifier, damageEffect);
                break;
            case AttackType.HeavyAttack02:
                ApplyAttackModifier(heavy_Attack_02_Modifier, damageEffect);
                break;
            case AttackType.ChargedAttack01:
                ApplyAttackModifier(charged_Attack_01_Modifier, damageEffect);
                break;
            case AttackType.ChargedAttack02:
                ApplyAttackModifier(charged_Attack_02_Modifier, damageEffect);
                break;
            case AttackType.RunningAttack01:
                ApplyAttackModifier(run_Attack_01_Modifier, damageEffect);
                break;
            case AttackType.RollingAttack01:
                ApplyAttackModifier(roll_Attack_01_Modifier, damageEffect);
                break;
            case AttackType.BackstepAttack01:
                ApplyAttackModifier(backstep_Attack_01_Modifier, damageEffect);
                break;
            case AttackType.JumpLightAttack01:
                ApplyAttackModifier(jump_Light_Attack_01_Modifier, damageEffect);
                break;
            case AttackType.JumpHeavyAttack01:
                ApplyAttackModifier(jump_Heavy_Attack_01_Modifier, damageEffect);
                break;
            default:
                break;
        }

        if (characterCasuingDamage.IsOwner)
        {
            // 只有攻击者的客户端才会处理伤害效果，其他客户端通过网络同步伤害结果
            //发送攻击请求
            damageTarget.characterNetworkManager.NotifyTheServerOfCharacterDamageServerRpc(
                damageTarget.NetworkObjectId, characterCasuingDamage.NetworkObjectId,
                damageEffect.physicalDamage, damageEffect.magicalDamage, damageEffect.fireDamage, damageEffect.holyDamage, damageEffect.lightningDamage,
                damageEffect.angleHitFrom, damageEffect.poiseDamage, damageEffect.contactPoint.x, damageEffect.contactPoint.y, damageEffect.contactPoint.z);
        }

        //damageTarget.characterEffectsManager.ProcessInstantEffect(damageEffect);
    }

    private void ApplyAttackModifier(float modifier, TakeDamageEffect damageEffect)
    {
        damageEffect.physicalDamage *= modifier;
        damageEffect.magicalDamage *= modifier;
        damageEffect.fireDamage *= modifier;
        damageEffect.lightningDamage *= modifier;
        damageEffect.holyDamage *= modifier;
        damageEffect.poiseDamage *= modifier;
    }

}
