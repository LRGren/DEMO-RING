using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Ash Of War/Parry")]
public class ParryAshOfWar : AshOfWar
{
    public override void AttemptToPerformAction(PlayerManager playerPerformingAction)
    {
        base.AttemptToPerformAction(playerPerformingAction);

        if (!CanIUseThisAbility(playerPerformingAction))
            return;

        DeductStamina(playerPerformingAction);
        DeductFocusPoints(playerPerformingAction);

        PerformParryTypeBasedOnWeapon(playerPerformingAction);

    }

    public override bool CanIUseThisAbility(PlayerManager playerPerformingAction)
    {
        if (playerPerformingAction.isPerformingAction)
            return false;

        if (!playerPerformingAction.playerLocomotionManager.isGrounded)
            return false;

        if (playerPerformingAction.playerNetworkManager.isJumping.Value)
            return false;

        if (playerPerformingAction.playerNetworkManager.currentStamina.Value < staminaCost)
            return false;

        return true;
    }

    private void PerformParryTypeBasedOnWeapon(PlayerManager playerPerformingAction)
    {
        WeaponItem currentWeapon = playerPerformingAction.playerCombatManager.currentWeaponBedingUsed;

        switch (currentWeapon.weaponClass)
        {
            case WeaponClass.MediumShield:
                playerPerformingAction.playerAnimatorManager.PlayerTargetActionAnimation("Slow_Parry_01", true);
                break;
            case WeaponClass.LightShield:
                playerPerformingAction.playerAnimatorManager.PlayerTargetActionAnimation("Light_Parry_01", true);
                break;
            default:
                break;
        }
    }


}
