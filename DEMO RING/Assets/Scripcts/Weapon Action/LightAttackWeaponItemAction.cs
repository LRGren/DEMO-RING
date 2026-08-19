using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Light Attack Action")]
public class LightAttackWeaponItemAction : WeaponItemAction
{

    [Header("Main Hand Animation Settings")]
    [Header("Light Attack Animations")]
    [SerializeField] private string light_Attack_01 = "Main_Light_Attack_01";
    [SerializeField] private string light_Attack_02 = "Main_Light_Attack_02";

    [Header("Running Attack Animations")]
    [SerializeField] private string running_Attack_01 = "Main_Run_Attack_01";

    [Header("Rolling Attack Animations")]
    [SerializeField] private string rolling_Attack_01 = "Main_Roll_Attack_01";

    [Header("Backstep Attack Animations")]
    [SerializeField] private string backstep_Attack_01 = "Main_Backstep_Attack_01";

    [Header("Two Hand Animation Settings")]
    [Header("Light Attack Animations")]
    [SerializeField] private string th_light_Attack_01 = "TH_Light_Attack_01";
    [SerializeField] private string th_light_Attack_02 = "TH_Light_Attack_02";
    [SerializeField] private string th_light_Attack_03 = "TH_Light_Attack_03";

    [Header("Running Attack Animations")]
    [SerializeField] private string th_running_Attack_01 = "TH_Run_Attack_01";

    [Header("Rolling Attack Animations")]
    [SerializeField] private string th_rolling_Attack_01 = "TH_Roll_Attack_01";

    [Header("Backstep Attack Animations")]
    [SerializeField] private string th_backstep_Attack_01 = "TH_Backstep_Attack_01";

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

        //执行攻击
        PerformLightAttack(playerPerformingAction, weaponPerformingAction);
    }

    private void PerformLightAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        if (playerPerformingAction.playerNetworkManager.isTwoHandingWeapon.Value)
        {
            PerformTwoHandLightAttack(playerPerformingAction, weaponPerformingAction);
        }
        else
        {
            PerformMainHandLightAttack(playerPerformingAction, weaponPerformingAction);
        }
    }

    private void PerformMainHandLightAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        if (playerPerformingAction.playerCombatManager.canComboWithMainHandWeapon && playerPerformingAction.isPerformingAction)
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

    private void PerformTwoHandLightAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        if (playerPerformingAction.playerCombatManager.canComboWithMainHandWeapon && playerPerformingAction.isPerformingAction)
        {
            if (playerPerformingAction.playerCombatManager.lastAttackAnimation == th_light_Attack_01)
            {
                playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.LightAttack02, th_light_Attack_02, true);
            }
            else if (playerPerformingAction.playerCombatManager.lastAttackAnimation == th_light_Attack_02)
            {
                playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.LightAttack03, th_light_Attack_03, true);
            }
            else if (playerPerformingAction.playerCombatManager.lastAttackAnimation == th_light_Attack_03)
            {
                playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.LightAttack01, th_light_Attack_01, true);
            }
        }
        else if (!playerPerformingAction.isPerformingAction)
        {
            playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.LightAttack01, th_light_Attack_01, true);
        }
    }

    private void PerformRunningAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        if (playerPerformingAction.playerNetworkManager.isTwoHandingWeapon.Value)
            playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.RunningAttack01, th_running_Attack_01, true);
        else
            playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.RunningAttack01, running_Attack_01, true);
    }

    public void PerformRollingAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        playerPerformingAction.playerCombatManager.canPerformRollingAttack = false;

        if (playerPerformingAction.playerNetworkManager.isTwoHandingWeapon.Value)
            playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.RollingAttack01, th_rolling_Attack_01, true);
        else
            playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.RollingAttack01, rolling_Attack_01, true);
    }

    public void PerformBackstepAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        playerPerformingAction.playerCombatManager.canPerformBackstepAttack = false;

        if (playerPerformingAction.playerNetworkManager.isTwoHandingWeapon.Value)
            playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.BackstepAttack01, th_backstep_Attack_01, true);
        else
            playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(weaponPerformingAction, AttackType.BackstepAttack01, backstep_Attack_01, true);
    }

}
