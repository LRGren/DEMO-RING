using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Character Actions/Weapon Actions/Test Action")]
public class WeaponItemAction : ScriptableObject
{
    public int actionID;

    public virtual void AttemptToPerformAction(PlayerManager playerPerformingAction, WeaponItem weaponPerformingAction)
    {
        //检测玩家手中的武器
        if (playerPerformingAction.IsOwner)
        {
            playerPerformingAction.playerNetworkManager.currentWeaponBeingUsed.Value = weaponPerformingAction.itemID;
        }

        //Debug.Log("Attempt To Perform Action");

        //AFTER YOU PASS ALL THE CHECKS AS THE OWNER, IF YOU ARE THE OWNER SEND THIS RPC IF NEEDED
        // YOU ONLY NEED TO DO THIS IF YOU HAVE SPECIAL LOGIC THAT REQUIRES THE WEAPON ACTION BE PERFORMED ON THE NETWORK
        // (IF YOU ARE FOLLOWING THE TUTORIAL WAY OF DOING THINGS THIS IS NOT NEEDED)
        //player.playerNetworkManager.NotifyTheServer0fWeaponActionServerRpc(NetworkManager.Singleton.LocalClientId, weaponAction.actionID, weaponPerformingAction.itemID);
    }

}
