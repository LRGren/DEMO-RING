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

    protected override void Awake()
    {
        base.Awake();

        aiCharacterManager = GetComponent<AICharacterManager>();
    }

    public void DropItem()
    {
        if (!aiCharacterManager.IsOwner)
            return;

        int randomChance = Random.Range(0, 100);
        if (randomChance > dropItemChance)
            return;

        Item itemToDrop = lootItems[Random.Range(0, lootItems.Count)];

        GameObject droppedItemPrefab = Instantiate(WorldItemDatabase.instance.characterDropPickUpItemPrefab);
        PickUpItemInteractable pickUpItemInteractable = droppedItemPrefab.GetComponent<PickUpItemInteractable>();

        pickUpItemInteractable.GetComponent<NetworkObject>().Spawn();
        pickUpItemInteractable.itemID.Value = itemToDrop.itemID;
        pickUpItemInteractable.itemPosition.Value = transform.position;
        pickUpItemInteractable.droppingCreatureID.Value = aiCharacterManager.NetworkObjectId;
    }

}
