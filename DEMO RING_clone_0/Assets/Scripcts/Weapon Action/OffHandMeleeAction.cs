using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Off Hand Melee Action")]
public class OffHandMeleeAction : WeaponItemAction
{
    public override void AttemptToPerformAction(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        base.AttemptToPerformAction(playerPerformingAction, weaponPerformingAction);

        if (!playerPerformingAction.playerCombatManager.canBlock)
            return;

        if (playerPerformingAction.playerNetworkManager.isAttacking.Value)
        {
            if (playerPerformingAction.IsOwner)
                playerPerformingAction.playerNetworkManager.isBlocking.Value = false;

            return;
        }

        if (playerPerformingAction.playerNetworkManager.isBlocking.Value)
            return;

        if (playerPerformingAction.IsOwner)
            playerPerformingAction.playerNetworkManager.isBlocking.Value = true;
    }
}
