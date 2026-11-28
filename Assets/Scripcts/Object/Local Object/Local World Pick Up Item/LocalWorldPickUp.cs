using UnityEngine;

public class LocalWorldPickUp : MonoBehaviour, IInteractable
{
    public string InteractableText;
    string IInteractable.InteractableText => InteractableText;

    public int worldSpawnedLootedItemID;
    public Item item;
    public int itemCount = 0;
    [SerializeField] protected Collider interactableCollider;

    protected virtual void Awake()
    {
        if (interactableCollider == null)
            interactableCollider = GetComponentInChildren<Collider>();
    }

    protected virtual void Start()
    {

    }

    public virtual void Interact(PlayerManager player)
    {
        if (player.isPerformingAction)
            return;

        //Debug.Log("Interacting with " + gameObject.name);
        interactableCollider.enabled = false;
        player.playerInteractionManager.RemoveInteractable(this);
        PlayerUIManager.instance.playerUIPopUpManager.CloseAllPopUps();

        player.playerAnimatorManager.PlayerTargetActionAnimation("Pick_Up_Item_01", true);

        player.characterSoundFXManager.PlaySoundFX(player.characterSoundFXManager.pickUpItemSFX);

        player.playerInventoryManager.AddItemToInventory(item, itemCount);

        // 把物品标记为已拾取
        if (WorldSaveGameManager.instance.currentCharacterData.worldItemsLooted.ContainsKey(worldSpawnedLootedItemID))
        {
            WorldSaveGameManager.instance.currentCharacterData.worldItemsLooted.Remove(worldSpawnedLootedItemID);
        }
        WorldSaveGameManager.instance.currentCharacterData.worldItemsLooted.Add(worldSpawnedLootedItemID, true);
        RefreshFromSave(WorldSaveGameManager.instance.currentCharacterData);

        WorldSaveGameManager.instance.SaveClientInventory();

        // Display a message to the player that they have picked up the item
        PlayerUIManager.instance.playerUIPopUpManager.SendItemPopUp(item, itemCount);
    }

    public virtual void OnTriggerEnter(Collider other)
    {
        PlayerManager player = other.GetComponent<PlayerManager>();
        if (player != null)
        {
            if (!player.IsOwner)
                return;

            //Add this interactable to the player's list of nearby interactables
            player.playerInteractionManager.AddInteractable(this);
        }
    }

    public virtual void OnTriggerExit(Collider other)
    {
        PlayerManager player = other.GetComponent<PlayerManager>();
        if (player != null)
        {
            if (!player.IsOwner)
                return;

            //Remove this interactable from the player's list of nearby interactables
            player.playerInteractionManager.RemoveInteractable(this);
            PlayerUIManager.instance.playerUIPopUpManager.CloseAllPopUps();
        }
    }

    public void RefreshFromSave(CharacterSaveData currentCharacterData)
    {
        if (!currentCharacterData.worldItemsLooted.ContainsKey(worldSpawnedLootedItemID))
        {
            currentCharacterData.worldItemsLooted.Add(worldSpawnedLootedItemID, false);
        }

        bool hasBeenLooted = currentCharacterData.worldItemsLooted[worldSpawnedLootedItemID];

        gameObject.SetActive(!hasBeenLooted);
    }
}
