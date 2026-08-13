using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Light Attack Action")]
public class LightAttackWeaponItemAction : WeaponItemAction
{
    [Header("Light Attack Animations")]
    [SerializeField] private string light_Attack_01 = "Main_Light_Attack_01";
    [SerializeField] private string light_Attack_02 = "Main_Light_Attack_02";

    [Header("Running Attack Animations")]
    [SerializeField] private string running_Attack_01 = "Main_Run_Attack_01";

    [Header("Rolling Attack Animations")]
    [SerializeField] private string rolling_Attack_01 = "Main_Roll_Attack_01";

    [Header("Backstep Attack Animations")]
    [SerializeField] private string backstep_Attack_01 = "Main_Backstep_Attack_01";

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

        if (playerPerformingAction.playerCombatManager.canComboWithMainHandWeapon && playerPerformingAction.isPerformingAction)
        {
            if (playerPerformingAction.playerCombatManager.lastAttackAnimation == light_Attack_01)
            {
                playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(AttackType.LightAttack02, light_Attack_02, true);
            }
            else if (playerPerformingAction.playerCombatManager.lastAttackAnimation == light_Attack_02)
            {
                playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(AttackType.LightAttack01, light_Attack_01, true);
            }
        }
        else if (!playerPerformingAction.isPerformingAction)
        {
            playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(AttackType.LightAttack01, light_Attack_01, true);
        }
    }

    private void PerformRunningAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(AttackType.RunningAttack01, running_Attack_01, true);
    }

    public void PerformRollingAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        playerPerformingAction.playerCombatManager.canPerformRollingAttack = false;
        playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(AttackType.RollingAttack01, rolling_Attack_01, true);
    }

    public void PerformBackstepAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        playerPerformingAction.playerCombatManager.canPerformBackstepAttack = false;
        playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(AttackType.BackstepAttack01, backstep_Attack_01, true);
    }

}
