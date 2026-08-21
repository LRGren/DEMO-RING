using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PickUpItemInteractable : Interactable
{
    public PickUpItemType pickUpType;

    public int itemID;
    public bool hasBeenLooted = false;

    public Item item;

    protected override void Start()
    {
        base.Start();

        if (pickUpType == PickUpItemType.WorldSpawn)
        {
            CheckIfItemWasAlreadyLooted();
        }
    }

    private void CheckIfItemWasAlreadyLooted()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            gameObject.SetActive(false);
            return;
        }

        if (!WorldSaveGameManager.instance.currentCharacterData.worldItemsLooted.ContainsKey(itemID))
        {
            WorldSaveGameManager.instance.currentCharacterData.worldItemsLooted.Add(itemID, false);
        }

        hasBeenLooted = WorldSaveGameManager.instance.currentCharacterData.worldItemsLooted[itemID];

        if (hasBeenLooted)
        {
            gameObject.SetActive(false);
        }
    }

    public override void Interact(PlayerManager player)
    {
        base.Interact(player);

        player.playerAnimatorManager.PlayerTargetActionAnimation("Pick_Up_Item_01", true);

        player.characterSoundFXManager.PlaySoundFX(player.characterSoundFXManager.pickUpItemSFX);

        player.playerInventoryManager.AddItemToInventory(item);

        // Display a message to the player that they have picked up the item
        PlayerUIManager.instance.playerUIPopUpManager.SendItemPopUp(item, 1);

        if (pickUpType == PickUpItemType.WorldSpawn)
        {
            if (WorldSaveGameManager.instance.currentCharacterData.worldItemsLooted.ContainsKey(itemID))
            {
                WorldSaveGameManager.instance.currentCharacterData.worldItemsLooted.Remove(itemID);
            }
            WorldSaveGameManager.instance.currentCharacterData.worldItemsLooted.Add(itemID, true);
        }

        Destroy(gameObject);
    }

}
