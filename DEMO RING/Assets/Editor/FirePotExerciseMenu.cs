using Unity.Netcode;
using UnityEditor;
using UnityEngine;

internal static class FirePotExerciseMenu
{
    private const string MenuPath = "Tools/DEMO RING/Fire Pot Exercise/Add 3 Fire Pots";
    private const string ItemPath = "Assets/Data/Items/QuickSlotItem/Fire Pot.asset";

    [MenuItem(MenuPath, true)]
    private static bool CanGrantFirePots()
    {
        return EditorApplication.isPlaying;
    }

    [MenuItem(MenuPath)]
    private static void GrantFirePots()
    {
        var network = NetworkManager.Singleton;
        var playerObject = network != null && network.IsClient ? network.LocalClient?.PlayerObject : null;
        if (playerObject == null || !playerObject.TryGetComponent(out PlayerManager player))
        {
            Debug.LogWarning("Enter a playable scene with a local player before adding practice items.");
            return;
        }

        var definition = AssetDatabase.LoadAssetAtPath<FirePot>(ItemPath);
        if (definition == null)
        {
            Debug.LogError("Cannot find the Fire Pot item asset at " + ItemPath);
            return;
        }

        var stack = Object.Instantiate(definition);
        stack.itemCount = 3;
        player.playerInventoryManager.AddItemToInventory(stack);
        Object.Destroy(stack);
        Debug.Log("Added 3 Fire Pots to the local player's inventory. Equip them in a quick slot to test.");
    }
}
