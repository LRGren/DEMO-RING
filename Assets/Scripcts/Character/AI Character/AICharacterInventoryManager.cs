using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AICharacterInventoryManager : CharacterInventoryManager
{
    AICharacterManager aiCharacterManager;

    [Header("Loot Chance")]
    public int dropItemChance = 50;
    [SerializeField] private List<Item> lootItems = new List<Item>();

    private GameObject droppedItem;

    protected override void Awake()
    {
        base.Awake();

        aiCharacterManager = GetComponent<AICharacterManager>();
    }

    public void DropItem()
    {
        if (!aiCharacterManager.IsServer)
            return;

        if (lootItems == null || lootItems.Count == 0)
            return;

        foreach (var player in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (player.PlayerObject == null)
                continue;

            PlayerManager client = player.PlayerObject.GetComponent<PlayerManager>();
            if (client == null)
                continue;

            int randomChance = Random.Range(0, 100);
            if (randomChance > dropItemChance)
                continue;

            if (Vector3.Distance(client.transform.position, transform.position) > 20f)
                continue;

            // If we reach here, it means there is a player within 20 units of the AI character.
            // We can proceed to drop the item.
            int droppedItemId = lootItems[Random.Range(0, lootItems.Count)].itemID;

            Vector3 spawnPosition = transform.position + new Vector3(Random.Range(-1f, 1f), 0.1f, Random.Range(-1f, 1f)); // Randomize spawn position slightly
            client.playerNetworkManager.SendPersonalDrop(droppedItemId, spawnPosition);

            /*Item itemToDrop = lootItems[Random.Range(0, lootItems.Count)];
            droppedItem = Instantiate(WorldItemDatabase.instance.characterDropPickUpItemPrefab);
            droppedItem.transform.position = transform.position;

            PickUpItemInteractable pickUpItemInteractable = droppedItem.GetComponent<PickUpItemInteractable>();

            pickUpItemInteractable.item = itemToDrop;
            // Initialize the spawn state before Netcode binds the drop's variables.
            pickUpItemInteractable.itemID.Reset(itemToDrop.itemID);
            pickUpItemInteractable.itemPosition.Reset(transform.position);
            pickUpItemInteractable.droppingCreatureID.Reset(aiCharacterManager.NetworkObjectId);
            pickUpItemInteractable.GetComponent<NetworkObject>().Spawn();*/
        }
    }

    public void ClearDroppedItem()
    {
        if (droppedItem != null)
        {
            Destroy(droppedItem);
        }
    }

}
