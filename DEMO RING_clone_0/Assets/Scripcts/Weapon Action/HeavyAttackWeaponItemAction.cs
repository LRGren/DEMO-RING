using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Heavy Attack Action")]
public class HeavyAttackWeaponItemAction : WeaponItemAction
{
    [Header("Main Hand Animation Settings")]
    [SerializeField] private string heavy_Attack_01 = "Main_Heavy_Attack_01";
    [SerializeField] private string heavy_Attack_02 = "Main_Heavy_Attack_02";

    [Header("Two Hand Animation Settings")]
    [SerializeField] private string th_heavy_Attack_01 = "TH_Heavy_Attack_01";
    [SerializeField] private string th_heavy_Attack_02 = "TH_Heavy_Attack_02";

    public override void AttemptToPerformAction(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        base.AttemptToPerformAction(playerPerformingAction, weaponPerformingAction);

        if (!playerPerformingAction.IsOwner)
            return;

        //检查停止
        if (playerPerformingAction.playerNetworkManager.currentStamina.Value <= 0)
            return;

        if (!playerPerformingAction.playerLocomotionManager.isGrounded)
            return;

        playerPerformingAction.playerNetworkManager.isAttacking.Value = true;

        //执行攻击
        PerformHeavyAttack(playerPerformingAction, weaponPerformingAction);
    }

    private void PerformHeavyAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        if (playerPerformingAction.playerNetworkManager.isTwoHandingWeapon.Value)
        {
            PerformTwoHandHeavyAttack(playerPerformingAction, weaponPerformingAction);
        }
        else
        {
            PerformMainHandHeavyAttack(playerPerformingAction, weaponPerformingAction);
        }
    }

    private void PerformMainHandHeavyAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        if (playerPerformingAction.playerCombatManager.canComboWithMainHandWeapon && playerPerformingAction.isPerformingAction)
        {
            if (playerPerformingAction.playerCombatManager.lastAttackAnimation == heavy_Attack_01)
            {
                playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.HeavyAttack02, heavy_Attack_02, true);
            }
            else if (playerPerformingAction.playerCombatManager.lastAttackAnimation == heavy_Attack_02)
            {
                playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.HeavyAttack01, heavy_Attack_01, true);
            }
        }
        else if (!playerPerformingAction.isPerformingAction)
        {
            playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.HeavyAttack01, heavy_Attack_01, true);
        }
    }

    private void PerformTwoHandHeavyAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        if (playerPerformingAction.playerCombatManager.canComboWithMainHandWeapon && playerPerformingAction.isPerformingAction)
        {
            if (playerPerformingAction.playerCombatManager.lastAttackAnimation == th_heavy_Attack_01)
            {
                playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.HeavyAttack02, th_heavy_Attack_02, true);
            }
            else if (playerPerformingAction.playerCombatManager.lastAttackAnimation == th_heavy_Attack_02)
            {
                playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.HeavyAttack01, th_heavy_Attack_01, true);
            }
        }
        else if (!playerPerformingAction.isPerformingAction)
        {
            playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.HeavyAttack01, th_heavy_Attack_01, true);
        }
    }
}
