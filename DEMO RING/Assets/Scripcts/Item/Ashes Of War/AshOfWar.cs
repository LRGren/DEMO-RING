using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AshOfWar : Item
{
    [Header("Ash Of War Infomation")]
    public WeaponClass[] usableWeaponClasses;

    [Header("Costs")]
    public int focusPointsCost = 20;
    public int staminaCost = 20;

    public virtual void AttemptToPerformAction(PlayerManager playerPerformingAction)
    {
        //Debug.Log("Attempting to perform action with Ash of War: " + itemName);
    }

    public virtual bool CanIUseThisAbility(PlayerManager playerPerformingAction)
    {
        return false;
    }

    protected virtual void DeductFocusPoints(PlayerManager playerPerformingAction)
    {

    }

    protected virtual void DeductStamina(PlayerManager playerPerformingAction)
    {
        playerPerformingAction.playerNetworkManager.currentStamina.Value -= staminaCost;
    }

}
