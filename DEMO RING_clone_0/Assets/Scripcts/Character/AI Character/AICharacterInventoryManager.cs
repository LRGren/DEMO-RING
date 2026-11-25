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
        if (!aiCharacterManager.IsOwner)
            return;

        if (lootItems == null || lootItems.Count == 0)
            return;

        int randomChance = Random.Range(0, 100);
        if (randomChance > dropItemChance)
            return;

        Item itemToDrop = lootItems[Random.Range(0, lootItems.Count)];
        droppedItem = Instantiate(WorldItemDatabase.instance.characterDropPickUpItemPrefab);
        droppedItem.transform.position = transform.position;

        PickUpItemInteractable pickUpItemInteractable = droppedItem.GetComponent<PickUpItemInteractable>();

        pickUpItemInteractable.itemID.Value = itemToDrop.itemID;
        pickUpItemInteractable.itemPosition.Value = transform.position;
        pickUpItemInteractable.droppingCreatureID.Value = aiCharacterManager.NetworkObjectId;
        pickUpItemInteractable.GetComponent<NetworkObject>().Spawn();
    }

    public void ClearDroppedItem()
    {
        if (droppedItem != null)
        {
            Destroy(droppedItem);
        }
    }

}
