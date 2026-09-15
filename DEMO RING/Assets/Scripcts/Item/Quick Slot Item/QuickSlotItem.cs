using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickSlotItem : Item
{
    [Header("Item Model")]
    public GameObject itemModel;

    [Header("Animation")]
    public string useItemAnimation;

    public virtual void AttemptToUseItem(PlayerManager player)
    {
        if (!CanIUseThisItem(player))
            return;

        player.playerAnimatorManager.PlayerTargetActionAnimation(useItemAnimation, true, false, true, true);
    }

    public virtual void SuccessfullyUsedItem(PlayerManager player)
    {

    }
    public virtual bool CanIUseThisItem(PlayerManager player)
    {
        return true;
    }
}
