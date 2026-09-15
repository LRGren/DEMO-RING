using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Fire Projectile Action")]
public class FireProjectileAction : WeaponItemAction
{
    [SerializeField] ProjectileSlot projectileSlot;
    public override void AttemptToPerformAction(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        base.AttemptToPerformAction(playerPerformingAction, weaponPerformingAction);

        if (!playerPerformingAction.IsOwner)
            return;

        if (playerPerformingAction.playerNetworkManager.currentStamina.Value <= 0)
            return;

        // 1. Define which projectile we are using (Main projectile slot, Secondary projectile slot)
        RangedProjectileItem projectileToFire = null;
        switch (projectileSlot)
        {
            case ProjectileSlot.MainProjectileSlot:
                projectileToFire = playerPerformingAction.playerInventoryManager.mainProjectile;
                break;
            case ProjectileSlot.SecondaryProjectileSlot:
                projectileToFire = playerPerformingAction.playerInventoryManager.secondaryProjectile;
                break;
        }

        // 2. If that projectile == null, return
        if (projectileToFire == null)
            return;

        // 3. If the player is not two handing the weapon, make them two hand it now (Weapon must be two handed to fire projectile)
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

        // 4.If the player does not have an arrow notched, do so now
        if (!playerPerformingAction.playerNetworkManager.hasArrowNotched.Value)
        {
            playerPerformingAction.playerNetworkManager.hasArrowNotched.Value = true;
            //bool canIDrawAProjectile;
            if (!CanIFireThisProjectile(playerPerformingAction, projectileToFire))
            {
                playerPerformingAction.playerAnimatorManager.PlayerTargetActionAnimation("Out_Of_Ammo_01", true);
                return;
            }

            if (projectileToFire.currentAmmoAmount < 0)
            {
                playerPerformingAction.playerAnimatorManager.PlayerTargetActionAnimation("Out_Of_Ammo_01", true);
                return;
            }

            playerPerformingAction.playerCombatManager.currentProjectileSlotBeingUsed = projectileSlot;
            playerPerformingAction.playerAnimatorManager.PlayerTargetActionAnimation("Bow_Draw_01", true);
            playerPerformingAction.playerNetworkManager.NotifyTheServerOfDrawnProjectileServerRpc(projectileToFire.itemID);
        }

    }

    private bool CanIFireThisProjectile(PlayerManager playerPerformingAction, RangedProjectileItem projectileToFire)
    {
        // 判断武器类型和弹药类型是否匹配

        return true;
    }

}
