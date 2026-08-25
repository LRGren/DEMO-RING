using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PickUpItemInteractable : Interactable
{
    public PickUpItemType pickUpType;
    public Item item;

    [Header("World Spawned Item")]
    public int worldSpawnedLootedItemID;
    public bool hasBeenLooted = false;

    [Header("Character Drop Item")]
    public NetworkVariable<int> itemID = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<Vector3> itemPosition = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<ulong> droppingCreatureID = new NetworkVariable<ulong>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public bool trackDroppingCreaturePosition = true;

    protected override void Start()
    {
        base.Start();

        if (pickUpType == PickUpItemType.WorldSpawn)
        {
            CheckIfItemWasAlreadyLooted();
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        itemID.OnValueChanged += OnItemIDChanged;
        itemPosition.OnValueChanged += OnItemPositionChanged;
        droppingCreatureID.OnValueChanged += OnDroppingCreatureIDChanged;

        if (!IsOwner)
        {
            OnItemIDChanged(0, itemID.Value);
            OnItemPositionChanged(Vector3.zero, itemPosition.Value);
            OnDroppingCreatureIDChanged(0, droppingCreatureID.Value);
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        itemID.OnValueChanged -= OnItemIDChanged;
        itemPosition.OnValueChanged -= OnItemPositionChanged;
        droppingCreatureID.OnValueChanged -= OnDroppingCreatureIDChanged;
    }

    private void CheckIfItemWasAlreadyLooted()
    {
        if (!NetworkManager.Singleton.IsHost)
        {
            gameObject.SetActive(false);
            return;
        }

        if (!WorldSaveGameManager.instance.currentCharacterData.worldItemsLooted.ContainsKey(worldSpawnedLootedItemID))
        {
            WorldSaveGameManager.instance.currentCharacterData.worldItemsLooted.Add(worldSpawnedLootedItemID, false);
        }

        hasBeenLooted = WorldSaveGameManager.instance.currentCharacterData.worldItemsLooted[worldSpawnedLootedItemID];

        if (hasBeenLooted)
        {
            gameObject.SetActive(false);
        }
    }

    public override void Interact(PlayerManager player)
    {
        if (player.isPerformingAction)
            return;

        base.Interact(player);

        player.playerAnimatorManager.PlayerTargetActionAnimation("Pick_Up_Item_01", true);

        player.characterSoundFXManager.PlaySoundFX(player.characterSoundFXManager.pickUpItemSFX);

        player.playerInventoryManager.AddItemToInventory(item);

        // Display a message to the player that they have picked up the item
        PlayerUIManager.instance.playerUIPopUpManager.SendItemPopUp(item, 1);

        if (pickUpType == PickUpItemType.WorldSpawn)
        {
            if (WorldSaveGameManager.instance.currentCharacterData.worldItemsLooted.ContainsKey(worldSpawnedLootedItemID))
            {
                WorldSaveGameManager.instance.currentCharacterData.worldItemsLooted.Remove(worldSpawnedLootedItemID);
            }
            WorldSaveGameManager.instance.currentCharacterData.worldItemsLooted.Add(worldSpawnedLootedItemID, true);
        }

        DestroyItemServerRpc();
    }

    private void OnItemIDChanged(int previousValue, int newValue)
    {
        if (pickUpType != PickUpItemType.CharacterDrop)
            return;

        item = WorldItemDatabase.instance.GetItemByID(itemID.Value);
    }

    private void OnItemPositionChanged(Vector3 previousValue, Vector3 newValue)
    {
        if (pickUpType != PickUpItemType.CharacterDrop)
            return;

        transform.position = itemPosition.Value;
    }

    private void OnDroppingCreatureIDChanged(ulong previousValue, ulong newValue)
    {
        if (pickUpType != PickUpItemType.CharacterDrop)
            return;

        StartCoroutine(TrackDroppingCreaturePosition());
    }

    private IEnumerator TrackDroppingCreaturePosition()
    {
        AICharacterManager droppingCreature = NetworkManager.Singleton.SpawnManager.SpawnedObjects[droppingCreatureID.Value].GetComponent<AICharacterManager>();
        bool tracking = false;

        if (droppingCreature != null)
            tracking = true;

        if (tracking)
        {
            while (gameObject.activeSelf)
            {
                transform.position = droppingCreature.characterCombatManager.lockOnTransform.position;
                yield return null;
            }
        }

        yield return null;
    }

    [ServerRpc(RequireOwnership = false)]
    private void DestroyItemServerRpc()
    {
        if (!IsServer)
            return;

        GetComponent<NetworkObject>().Despawn();
    }

}
