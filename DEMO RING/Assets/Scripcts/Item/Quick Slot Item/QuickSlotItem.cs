using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickSlotItem : Item
{
    public int itemCount;

    [Header("Item Model")]
    public GameObject itemModel;

    [Header("Animation")]
    public string useItemAnimation;

    [Header("Item Type")]
    public bool isConsumable;

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

    public virtual int GetAmountOfItem(PlayerManager player)
    {
        return itemCount;
    }

    public virtual void SetAmountOfItem(PlayerManager player, int amount)
    {
        if (!player.IsOwner)
            return;
        if (!isConsumable)
            return;

        itemCount = Mathf.Max(amount, 0);
    }
}
