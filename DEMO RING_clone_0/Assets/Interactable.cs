using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string InteractableText;
    [SerializeField] protected Collider interactableCollider;
    [SerializeField] protected bool hostOnlyInteractable = false;

    protected virtual void Awake()
    {
        if (interactableCollider == null)
            interactableCollider = GetComponentInChildren<Collider>();
    }

    public virtual void Interact(PlayerManager player)
    {
        if (!player.IsOwner)
            return;

        Debug.Log("Interacting with " + gameObject.name);
        interactableCollider.enabled = false;
        player.playerInteractionManager.RemoveInteractable(this);
        PlayerUIManager.instance.playerUIPopUpManager.CloseAllPopUps();
    }

    public virtual void OnTriggerEnter(Collider other)
    {
        PlayerManager player = other.GetComponent<PlayerManager>();
        if (player != null)
        {
            if (!player.IsHost && hostOnlyInteractable)
                return;

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
            if (!player.IsHost && hostOnlyInteractable)
                return;

            if (!player.IsOwner)
                return;

            //Remove this interactable from the player's list of nearby interactables
            player.playerInteractionManager.RemoveInteractable(this);
            PlayerUIManager.instance.playerUIPopUpManager.CloseAllPopUps();
        }
    }
}
