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
}
