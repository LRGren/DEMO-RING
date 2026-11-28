using UnityEngine;

public interface IInteractable
{
    string InteractableText { get; }
    void Interact(PlayerManager player);
}
