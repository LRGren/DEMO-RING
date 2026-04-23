using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Heavy Attack Action")]
public class HeavyAttackWeaponItemAction : WeaponItemAction
{
    [SerializeField] private string heavy_Attack_01 = "Main_Heavy_Attack_01";

    public override void AttemptToPerformAction(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        base.AttemptToPerformAction(playerPerformingAction, weaponPerformingAction);

        if (!playerPerformingAction.IsOwner)
            return;

        //检查停止
        if (playerPerformingAction.playerNetworkManager.currentStamina.Value <= 0)
            return;

        if (!playerPerformingAction.isGrounded)
            return;

        //执行攻击
        PerformHeavyAttack(playerPerformingAction, weaponPerformingAction);
    }

    private void PerformHeavyAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {

        if (playerPerformingAction.playerNetworkManager.isUsingRightHand.Value)
        {
            playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(AttackType.HeavyAttack01, heavy_Attack_01, true);
        }

        if (playerPerformingAction.playerNetworkManager.isUsingLeftHand.Value)
        {

        }
    }
}
