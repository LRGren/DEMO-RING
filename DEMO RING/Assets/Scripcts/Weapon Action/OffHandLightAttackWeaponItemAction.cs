using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Off Hand Light Attack Action")]
public class OffHandLightAttackWeaponItemAction : WeaponItemAction
{
    [Header("Main Hand Animation Settings")]
    [Header("Light Attack Animations")]
    [SerializeField] private string light_Attack_01 = "Off_Light_Attack_01";
    [SerializeField] private string light_Attack_02 = "Off_Light_Attack_02";

    [Header("Dual Wielding Light Attack Animations")]
    [SerializeField] private string dual_light_Attack_01 = "DW_Light_Attack_01";
    [SerializeField] private string dual_light_Attack_02 = "DW_Light_Attack_02";
    private const string DualLightAttack03 = "DW_Light_Attack_03";
    private const string StraightSwordDualLightAttack04 = "DW_Light_Attack_04";

    [Header("Running Attack Animations")]
    [SerializeField] private string running_Attack_01 = "DW_Run_Attack_01";

    [Header("Rolling Attack Animations")]
    [SerializeField] private string rolling_Attack_01 = "DW_Roll_Attack_01";

    [Header("Backstep Attack Animations")]
    [SerializeField] private string backstep_Attack_01 = "DW_Backstep_Attack_01";

    [Header("Jump Attack Animations")]
    [SerializeField] private string jump_Attack_01 = "DW_Jump_Attack_01";

    public override void AttemptToPerformAction(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        base.AttemptToPerformAction(playerPerformingAction, weaponPerformingAction);

        if (!playerPerformingAction.IsOwner)
            return;

        if (playerPerformingAction.playerCombatManager.isUsingItem)
            return;

        //检查停止
        if (playerPerformingAction.playerNetworkManager.currentStamina.Value <= 0)
            return;

        if (playerPerformingAction.playerCombatManager.canPerformJumpAttack && !playerPerformingAction.playerLocomotionManager.isGrounded)
            return;

        if (!playerPerformingAction.playerLocomotionManager.isGrounded)
            return;

        if (playerPerformingAction.playerNetworkManager.isJumping.Value)
            return;

        if (playerPerformingAction.playerNetworkManager.isSprinting.Value)
        {
            PerformRunningAttack(playerPerformingAction, weaponPerformingAction);
            return;
        }

        if (playerPerformingAction.playerCombatManager.canPerformRollingAttack)
        {
            PerformRollingAttack(playerPerformingAction, weaponPerformingAction);
            return;
        }

        if (playerPerformingAction.playerCombatManager.canPerformBackstepAttack)
        {
            PerformBackstepAttack(playerPerformingAction, weaponPerformingAction);
            return;
        }

        playerPerformingAction.playerCombatManager.AttemptCriticalAttack();

        //执行攻击
        PerformLightAttack(playerPerformingAction, weaponPerformingAction);
    }
    public virtual void AttemptToPerformDualWieldingAction(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction, bool isDualWielding = false)
    {
        if (!isDualWielding)
            return;

        base.AttemptToPerformAction(playerPerformingAction, weaponPerformingAction);

        if (!playerPerformingAction.IsOwner)
            return;

        if (playerPerformingAction.playerCombatManager.isUsingItem)
            return;

        //检查停止
        if (playerPerformingAction.playerNetworkManager.currentStamina.Value <= 0)
            return;

        if (playerPerformingAction.playerLocomotionManager.CanStartJumpAttack())
        {
            PerformJumpAttack(playerPerformingAction, weaponPerformingAction, isDualWielding);
            return;
        }

        if (!playerPerformingAction.playerLocomotionManager.isGrounded)
            return;

        if (playerPerformingAction.playerNetworkManager.isJumping.Value)
            return;

        if (playerPerformingAction.playerNetworkManager.isSprinting.Value)
        {
            PerformRunningAttack(playerPerformingAction, weaponPerformingAction, isDualWielding);
            return;
        }

        if (playerPerformingAction.playerCombatManager.canPerformRollingAttack)
        {
            PerformRollingAttack(playerPerformingAction, weaponPerformingAction, isDualWielding);
            return;
        }

        if (playerPerformingAction.playerCombatManager.canPerformBackstepAttack)
        {
            PerformBackstepAttack(playerPerformingAction, weaponPerformingAction, isDualWielding);
            return;
        }
        //执行攻击
        PerformLightAttack(playerPerformingAction, weaponPerformingAction, isDualWielding);
    }
    private void PerformLightAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction, bool isDualWielding = false)
    {
        if (!playerPerformingAction.playerNetworkManager.isTwoHandingWeapon.Value)
        {
            PerformOffHandLightAttack(playerPerformingAction, weaponPerformingAction, isDualWielding);
        }
    }

    private void PerformOffHandLightAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction, bool isDualWielding = false)
    {
        if (isDualWielding)
        {
            if (playerPerformingAction.playerCombatManager.canComboWithOffHandWeapon && playerPerformingAction.isPerformingAction)
            {
                if (playerPerformingAction.playerCombatManager.lastAttackAnimation == dual_light_Attack_01)
                {
                    playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.LightAttack02, dual_light_Attack_02, true);
                }
                else if ((weaponPerformingAction.dualWieldGroup == DualWieldGroup.StraightSword ||
                    weaponPerformingAction.dualWieldGroup == DualWieldGroup.GreatAxe) &&
                    playerPerformingAction.playerCombatManager.lastAttackAnimation == dual_light_Attack_02)
                {
                    playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(
                        weaponPerformingAction, AttackType.LightAttack01, DualLightAttack03, true);
                }
                else if (weaponPerformingAction.dualWieldGroup == DualWieldGroup.StraightSword &&
                    playerPerformingAction.playerCombatManager.lastAttackAnimation == DualLightAttack03)
                {
                    playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(
                        weaponPerformingAction, AttackType.LightAttack02, StraightSwordDualLightAttack04, true);
                }
                else if (playerPerformingAction.playerCombatManager.lastAttackAnimation == dual_light_Attack_02 ||
                    playerPerformingAction.playerCombatManager.lastAttackAnimation == DualLightAttack03 ||
                    playerPerformingAction.playerCombatManager.lastAttackAnimation == StraightSwordDualLightAttack04)
                {
                    playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.LightAttack01, dual_light_Attack_01, true);
                }
            }
            else if (!playerPerformingAction.isPerformingAction)
            {
                playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.LightAttack01, dual_light_Attack_01, true);
            }
        }
        else
        {
            if (playerPerformingAction.playerCombatManager.canComboWithOffHandWeapon && playerPerformingAction.isPerformingAction)
            {
                if (playerPerformingAction.playerCombatManager.lastAttackAnimation == light_Attack_01)
                {
                    playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.LightAttack02, light_Attack_02, true);
                }
                else if (playerPerformingAction.playerCombatManager.lastAttackAnimation == light_Attack_02)
                {
                    playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.LightAttack01, light_Attack_01, true);
                }
            }
            else if (!playerPerformingAction.isPerformingAction)
            {
                playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.LightAttack01, light_Attack_01, true);
            }
        }
    }

    private void PerformRunningAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction, bool isDualWielding = false)
    {
        if (isDualWielding)
        {
            playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.RunningAttack01, running_Attack_01, true);
        }
    }

    private void PerformBackstepAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction, bool isDualWielding = false)
    {
        playerPerformingAction.playerCombatManager.canPerformBackstepAttack = false;
        if (isDualWielding)
        {
            playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.BackstepAttack01, backstep_Attack_01, true);
        }
    }

    private void PerformRollingAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction, bool isDualWielding = false)
    {
        playerPerformingAction.playerCombatManager.canPerformRollingAttack = false;
        if (isDualWielding)
        {
            playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.RollingAttack01, rolling_Attack_01, true);
        }
    }

    private void PerformJumpAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction, bool isDualWielding = false)
    {
        if (isDualWielding)
            playerPerformingAction.playerLocomotionManager.PerformJumpAttack(
                weaponPerformingAction, AttackType.JumpLightAttack01, jump_Attack_01);
    }

}
