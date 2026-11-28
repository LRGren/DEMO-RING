using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PickupIdTools
{
    [MenuItem("Tools/Pick Ups/Print Pickup IDs")]
    private static void PrintPickupIds()
    {
        var currentScene = SceneManager.GetActiveScene();
        var pickups = Object.FindObjectsByType<LocalWorldPickUp>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        int enabledPickupId = 0;
        foreach (var pickup in pickups)
        {
            if (pickup.gameObject.scene != currentScene)
                continue;

            enabledPickupId = Mathf.Max(enabledPickupId, pickup.worldSpawnedLootedItemID + 1);
        }

        Debug.Log($"下一个可用的 worldSpawnedLootedItemID 是 {enabledPickupId}。");
    }
}