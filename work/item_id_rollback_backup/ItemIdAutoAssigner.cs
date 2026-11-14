using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

[InitializeOnLoad]
internal static class ItemIdAutoAssigner
{
    internal const string LedgerPath = "ProjectSettings/ItemIdRegistry.json";
    private static bool queued;
    private static bool synchronizing;
    private static readonly string[] Categories =
    {
        "weapons", "headEquipment", "bodyEquipment", "handEquipment", "legEquipment",
        "ashesOfWar", "spells", "projectiles", "quickSlotItems"
    };

    static ItemIdAutoAssigner()
    {
        Schedule();
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                try { Synchronize(); }
                catch (Exception error)
                {
                    EditorApplication.isPlaying = false;
                    Debug.LogException(error);
                }
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
                Schedule();
        };
    }

    internal static void Schedule()
    {
        if (queued || synchronizing)
            return;
        queued = true;
        EditorApplication.delayCall += RunScheduled;
    }

    private static void RunScheduled()
    {
        queued = false;
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            Schedule();
            return;
        }
        try { Synchronize(); }
        catch (Exception error) { Debug.LogException(error); }
    }

    [MenuItem("Tools/Items/Synchronize Item IDs")]
    internal static void Synchronize()
    {
        if (synchronizing || EditorApplication.isPlaying)
            return;
        synchronizing = true;
        try
        {
            var ledger = File.Exists(LedgerPath)
                ? JsonUtility.FromJson<ItemIdLedger>(File.ReadAllText(LedgerPath))
                : new ItemIdLedger();
            string previousLedger = JsonUtility.ToJson(ledger, true);
            var priorities = GetDatabasePriorities();
            var candidates = new List<ItemIdCandidate>();
            var assets = new Dictionary<string, Item>();

            foreach (string guid in AssetDatabase.FindAssets("t:Item", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var item = AssetDatabase.LoadAssetAtPath<Item>(path);
                if (item == null)
                    continue;
                if (AssetDatabase.LoadAllAssetsAtPath(path).OfType<Item>().Count() != 1)
                    throw new InvalidOperationException("Store each item in its own asset: " + path);

                candidates.Add(new ItemIdCandidate
                {
                    guid = guid, path = path, category = GetCategory(item), previousID = item.itemID,
                    priority = priorities.TryGetValue(guid, out int priority) ? priority : int.MaxValue
                });
                assets.Add(guid, item);
            }

            ItemIdAllocation.Allocate(ledger, candidates);
            string newLedger = JsonUtility.ToJson(ledger, true);
            // Commit ownership before writing assets; an interrupted run repairs them next time.
            if (!File.Exists(LedgerPath) || previousLedger != newLedger)
            {
                string temporaryPath = LedgerPath + ".tmp";
                File.WriteAllText(temporaryPath, newLedger + Environment.NewLine);
                if (File.Exists(LedgerPath))
                    File.Replace(temporaryPath, LedgerPath, null);
                else
                    File.Move(temporaryPath, LedgerPath);
            }

            int changed = 0;
            foreach (var candidate in candidates)
            {
                Item item = assets[candidate.guid];
                if (item.itemID == candidate.assignedID)
                    continue;
                item.itemID = candidate.assignedID;
                EditorUtility.SetDirty(item);
                AssetDatabase.SaveAssetIfDirty(item);
                changed++;
                Debug.Log($"[Item IDs] {candidate.path}: {candidate.previousID} -> {candidate.assignedID}", item);
            }
            if (changed > 0)
                Debug.Log($"[Item IDs] Saved {changed} item IDs. Keep {LedgerPath} in version control.");
        }
        finally { synchronizing = false; }
    }

    private static int GetCategory(Item item)
    {
        if (item is WeaponItem) return 10000;
        if (item is HeadEquipmentItem) return 20000;
        if (item is BodyEquipmentItem) return 30000;
        if (item is HandEquipmentItem) return 40000;
        if (item is LegEquipmentItem) return 50000;
        if (item is AshOfWar) return 60000;
        if (item is SpellItem) return 70000;
        if (item is RangedProjectileItem) return 80000;
        if (item is QuickSlotItem) return 90000;
        throw new InvalidOperationException("No item ID category for " + item.GetType().Name + ": " + AssetDatabase.GetAssetPath(item));
    }

    private static Dictionary<string, int> GetDatabasePriorities()
    {
        var result = new Dictionary<string, int>();
        foreach (string path in AssetDatabase.FindAssets("World Item Database t:Prefab")
                     .Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p, StringComparer.Ordinal))
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;
            foreach (var database in prefab.GetComponentsInChildren<WorldItemDatabase>(true))
            {
                var serialized = new SerializedObject(database);
                foreach (string category in Categories)
                {
                    var list = serialized.FindProperty(category);
                    if (list == null || !list.isArray) continue;
                    for (int i = 0; i < list.arraySize; i++)
                    {
                        var item = list.GetArrayElementAtIndex(i).objectReferenceValue as Item;
                        if (item == null) continue;
                        string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(item));
                        if (!result.ContainsKey(guid)) result.Add(guid, result.Count);
                    }
                }
            }
        }
        return result;
    }
}

internal sealed class ItemIdImportProcessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        if (imported.Concat(deleted).Concat(moved).Concat(movedFrom)
            .Any(path => path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)))
            ItemIdAutoAssigner.Schedule();
    }
}

internal sealed class ItemIdBuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder => -1000;
    public void OnPreprocessBuild(BuildReport report)
    {
        try { ItemIdAutoAssigner.Synchronize(); }
        catch (Exception error) { throw new BuildFailedException(error); }
    }
}
