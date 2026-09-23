using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Consumeables/Flask")]
public class FlaskItem : QuickSlotItem
{
    [Header("Flasks Type")]
    public bool isHealthFlask;

    [Header("Flask Amount")]
    public int healthFlaskAmount;
    public int manaFlaskAmount;

    [Header("Empty Flask")]
    public GameObject emptyFlaskPrefab;
    public string useEmptyFlaskAnimation;

    public override bool CanIUseThisItem(PlayerManager player)
    {
        if (!player.playerCombatManager.isUsingItem && player.isPerformingAction)
            return false;

        if (player.playerNetworkManager.isAttacking.Value)
            return false;

        return true;
    }

    public override void AttemptToUseItem(PlayerManager player)
    {
        if (!CanIUseThisItem(player))
            return;

        if (player.playerNetworkManager.remainingManaFlasks.Value <= 0 && !isHealthFlask)
        {
            if (player.playerCombatManager.isUsingItem)
                return;

            player.playerCombatManager.isUsingItem = true;

            Destroy(player.playerEffectsManager.activeQuickSlotItemFX.gameObject);
            GameObject emptyFlask = Instantiate(emptyFlaskPrefab, player.playerEquipmentManager.rightHandWeaponSlot.transform);
            player.playerEffectsManager.activeQuickSlotItemFX = emptyFlask;

            if (player.IsOwner)
            {
                player.playerAnimatorManager.PlayerTargetActionAnimation(useEmptyFlaskAnimation, false, false, true, true, false);
                player.playerNetworkManager.HideWeaponsServerRpc();
            }
            return;
        }
        else if (player.playerNetworkManager.remainingHealthFlasks.Value <= 0 && isHealthFlask)
        {
            if (player.playerCombatManager.isUsingItem)
                return;

            player.playerCombatManager.isUsingItem = true;

            Destroy(player.playerEffectsManager.activeQuickSlotItemFX.gameObject);
            GameObject emptyFlask = Instantiate(emptyFlaskPrefab, player.playerEquipmentManager.rightHandWeaponSlot.transform);
            player.playerEffectsManager.activeQuickSlotItemFX = emptyFlask;

            if (player.IsOwner)
            {
                player.playerAnimatorManager.PlayerTargetActionAnimation(useEmptyFlaskAnimation, false, false, true, true, false);
                player.playerNetworkManager.HideWeaponsServerRpc();
            }
            return;
        }

        // Check For Chugging
        if (player.playerCombatManager.isUsingItem)
        {
            if (player.IsOwner)
            {
                player.playerNetworkManager.isChugging.Value = true;
            }

            return;
        }

        player.playerCombatManager.isUsingItem = true;

        player.playerEffectsManager.activeQuickSlotItemFX = Instantiate(itemModel, player.playerEquipmentManager.rightHandWeaponSlot.transform);

        if (player.IsOwner)
        {
            player.playerAnimatorManager.PlayerTargetActionAnimation(useItemAnimation, false, false, true, true, false);
            player.playerNetworkManager.HideWeaponsServerRpc();
        }
    }

    public override void SuccessfullyUsedItem(PlayerManager player)
    {
        base.SuccessfullyUsedItem(player);

        if (player.IsOwner)
        {
            if (isHealthFlask)
            {
                player.playerNetworkManager.remainingHealthFlasks.Value--;
                player.playerNetworkManager.currentHealth.Value += healthFlaskAmount;
            }
            else
            {
                player.playerNetworkManager.remainingManaFlasks.Value--;
                player.playerNetworkManager.currentFocusPoints.Value += manaFlaskAmount;
            }

            if (isConsumable)
            {
                PlayerUIManager.instance.playerUIHudManager.SetQuickSlotCountText(GetAmountOfItem(player), true);
            }
        }

        if (isHealthFlask && player.playerNetworkManager.remainingHealthFlasks.Value <= 0)
        {
            Destroy(player.playerEffectsManager.activeQuickSlotItemFX.gameObject);
            GameObject emptyFlask = Instantiate(emptyFlaskPrefab, player.playerEquipmentManager.rightHandWeaponSlot.transform);
            player.playerEffectsManager.activeQuickSlotItemFX = emptyFlask;
        }
        else if (!isHealthFlask && player.playerNetworkManager.remainingManaFlasks.Value <= 0)
        {
            Destroy(player.playerEffectsManager.activeQuickSlotItemFX.gameObject);
            GameObject emptyFlask = Instantiate(emptyFlaskPrefab, player.playerEquipmentManager.rightHandWeaponSlot.transform);
            player.playerEffectsManager.activeQuickSlotItemFX = emptyFlask;
        }

        PlayHealingFX(player);
    }

    private void PlayHealingFX(PlayerManager player)
    {
        if (isHealthFlask)
        {
            if (WorldCharacterEffectsManager.instance.healthFlaskDrinkVFX != null)
                Instantiate(WorldCharacterEffectsManager.instance.healthFlaskDrinkVFX, player.transform);
        }
        else
        {
            if (WorldCharacterEffectsManager.instance.manaFlaskDrinkVFX != null)
                Instantiate(WorldCharacterEffectsManager.instance.manaFlaskDrinkVFX, player.transform);
        }

        if (player.characterSoundFXManager != null)
            player.characterSoundFXManager.PlaySoundFX(WorldSoundFXManager.instance.flaskDrinkSFX);
    }

    public override int GetAmountOfItem(PlayerManager player)
    {
        if (isHealthFlask)
        {
            return player.playerNetworkManager.remainingHealthFlasks.Value;
        }
        else
        {
            return player.playerNetworkManager.remainingManaFlasks.Value;
        }
    }

    public override void SetAmountOfItem(PlayerManager player, int amount)
    {
        base.SetAmountOfItem(player, amount);

        if (isHealthFlask)
        {
            player.playerNetworkManager.remainingHealthFlasks.Value = amount;
        }
        else
        {
            player.playerNetworkManager.remainingManaFlasks.Value = amount;
        }
    }

}
