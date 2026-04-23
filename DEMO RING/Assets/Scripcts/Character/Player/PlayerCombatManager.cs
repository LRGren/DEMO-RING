using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerCombatManager : CharacterCombatManager
{
    PlayerManager player;
    public WeaponItem currentWeaponBedingUsed;

    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<PlayerManager>();
    }

    public void PerformWeaponBasedAction(WeaponItemAction weaponAction,WeaponItem weaponPerformingAction)
    {
        if (player.IsOwner)
        {
            weaponAction.AttemptToPerformAction(player, weaponPerformingAction);

            //执行对应的动画
            player.playerNetworkManager.NotifyTheServerOfWeaponActionServerRpc(NetworkManager.Singleton.LocalClientId, weaponAction.actionID, weaponPerformingAction.itemID); ;

        }
    }

    public virtual void DrainStaminaBasedOnAttack()
    {
        if (!player.IsOwner)
            return;

        if(currentWeaponBedingUsed == null)
            return;

        float staminaCost = currentWeaponBedingUsed.basicStaminaCost;

        switch (currentAttackType)
        {
            case AttackType.LightAttack01:
                staminaCost *= currentWeaponBedingUsed.lightAttackStaminaModifier;
                break;
            default:
                break;
        }

        player.playerNetworkManager.currentStamina.Value -= staminaCost;
    }

    public override void SetTarget(CharacterManager newTarget)
    {
        base.SetTarget(newTarget);

        if (player.IsOwner)
        {
            PlayerCamera.instance.SetLockOnCameraHeight();
        }
    }
}
