using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Incantation Action")]
public class CastIncantationAction : WeaponItemAction
{
    public override void AttemptToPerformAction(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        base.AttemptToPerformAction(playerPerformingAction, weaponPerformingAction);

        if (!playerPerformingAction.IsOwner)
            return;

        if (playerPerformingAction.playerNetworkManager.currentStamina.Value <= 0)
            return;

        if (!playerPerformingAction.playerLocomotionManager.isGrounded)
            return;

        if (playerPerformingAction.playerInventoryManager.currentSpell == null)
            return;

        if (playerPerformingAction.playerInventoryManager.currentSpell.spellClass != SpellClass.Incantation)
            return;

        if (playerPerformingAction.playerCombatManager.isUsingItem)
            return;

        if (playerPerformingAction.IsOwner)
            playerPerformingAction.playerNetworkManager.isAttacking.Value = true;

        CastIncantation(playerPerformingAction);
    }

    private void CastIncantation(PlayerManager playerPerformingAction)
    {
        playerPerformingAction.playerInventoryManager.currentSpell.AttemptToCastSpell(playerPerformingAction);
    }
}
