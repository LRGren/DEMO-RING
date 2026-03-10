using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Light Attack Action")]
public class LightAttackWeaponItemAction : WeaponItemAction
{
    [SerializeField] private string light_Attack_01 = "Main_Light_Attack_01";

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
        PerformLightAttack(playerPerformingAction, weaponPerformingAction);
    }

    private void PerformLightAttack(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {

        if (playerPerformingAction.playerNetworkManager.isUsingRightHand.Value)
        {
            playerPerformingAction.playerAnimatorManager.PlayerTargetAttackActionAnimation(light_Attack_01, true);
        }

        if (playerPerformingAction.playerNetworkManager.isUsingLeftHand.Value)
        {

        }
    }
}
