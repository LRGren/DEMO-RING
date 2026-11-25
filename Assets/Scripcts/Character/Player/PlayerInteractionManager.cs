using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionManager : MonoBehaviour
{
    PlayerManager player;

    [SerializeField] private List<Interactable> currentInteractableActions;

    void Awake()
    {
        player = GetComponent<PlayerManager>();
    }

    void Start()
    {
        currentInteractableActions = new List<Interactable>();
    }

    void FixedUpdate()
    {
        if (!player.IsOwner)
            return;

        if (!PlayerUIManager.instance.menuWindowIsOpen && !PlayerUIManager.instance.popUpWindowIsOpen)
            CheckForInteractable();
    }

    public void CheckForInteractable()
    {
        if (currentInteractableActions.Count == 0)
            return;

        if (currentInteractableActions[0] == null)
        {
            currentInteractableActions.RemoveAt(0);
            return;
        }

        if (currentInteractableActions[0] != null)
            PlayerUIManager.instance.playerUIPopUpManager.SendMessagePopUp(currentInteractableActions[0].InteractableText);
    }

    private void RefreshInteractableList()
    {
        for (int i = currentInteractableActions.Count - 1; i >= 0; i--)
        {
            if (currentInteractableActions[i] == null)
                currentInteractableActions.RemoveAt(i);
        }
    }

    public void Interact()
    {
        PlayerUIManager.instance.playerUIPopUpManager.CloseAllPopUps();

        if (currentInteractableActions.Count == 0)
            return;

        if (currentInteractableActions[0] != null)
            currentInteractableActions[0].Interact(player);

        RefreshInteractableList();
    }

    public void AddInteractable(Interactable interactable)
    {
        RefreshInteractableList();

        if (!currentInteractableActions.Contains(interactable))
            currentInteractableActions.Add(interactable);
    }

    public void RemoveInteractable(Interactable interactable)
    {
        if (currentInteractableActions.Contains(interactable))
            currentInteractableActions.Remove(interactable);

        RefreshInteractableList();
    }
}
