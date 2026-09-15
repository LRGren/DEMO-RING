using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Aim Action")]
public class AimAction : WeaponItemAction
{
    public override void AttemptToPerformAction(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        base.AttemptToPerformAction(playerPerformingAction, weaponPerformingAction);

        if (!playerPerformingAction.playerLocomotionManager.isGrounded)
            return;

        if (playerPerformingAction.playerNetworkManager.isJumping.Value)
            return;

        if (playerPerformingAction.playerLocomotionManager.isRolling)
            return;

        if (playerPerformingAction.playerNetworkManager.isLockOn.Value)
            return;

        if (playerPerformingAction.playerCombatManager.isUsingItem)
            return;

        if (playerPerformingAction.IsOwner)
        {
            if (!playerPerformingAction.playerNetworkManager.isTwoHandingWeapon.Value)
            {
                if (playerPerformingAction.playerNetworkManager.isUsingRightHand.Value)
                {
                    playerPerformingAction.playerNetworkManager.isTwoHandingRightWeapon.Value = true;
                }
                else
                {
                    playerPerformingAction.playerNetworkManager.isTwoHandingLeftWeapon.Value = true;
                }
            }

            playerPerformingAction.playerNetworkManager.isAiming.Value = true;
        }

    }
}
