using UnityEngine;

public class LocalCharacterDropItem : MonoBehaviour, IInteractable
{
    public string InteractableText => "Press Y to Pick Up Item";

    public Item item;
    public int itemCount = 0;
    [SerializeField] private Collider interactableCollider;

    private void Awake()
    {
        if (interactableCollider == null)
            interactableCollider = GetComponentInChildren<Collider>();
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

        WorldSaveGameManager.instance.SaveClientInventory();

        // Display a message to the player that they have picked up the item
        PlayerUIManager.instance.playerUIPopUpManager.SendItemPopUp(item, itemCount);

        Destroy(gameObject);
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

}
