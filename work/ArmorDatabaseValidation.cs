using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ArmorDatabaseValidation
{
    [Serializable] public class Expected { public Entry[] existing; public Entry[] added; }
    [Serializable] public class Entry { public string path; public string guid; public string field; public int runtimeID; public int itemID; }
    [Serializable] public class Result { public bool passed; public int existingIDsPreserved; public int newItemsFound; public bool reorderedListsPassed; public bool additionalItemsPassed; public string error; }
    static ArmorDatabaseValidation() { EditorApplication.delayCall += Run; }
    static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    static void Run()
    {
        string work = Path.GetFullPath(Path.Combine(Application.dataPath, "../../work"));
        string output = Path.Combine(work, "database_unity_validation.json");
        if (File.Exists(output)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.delayCall += Run; return; }
        var result = new Result();
        var previous = WorldItemDatabase.instance;
        GameObject contents = null;
        HeadEquipmentItem extraHead = null;
        BodyEquipmentItem extraBody = null;
        try
        {
            var expected = JsonUtility.FromJson<Expected>(File.ReadAllText(Path.Combine(work, "database_registration.json")));
            contents = PrefabUtility.LoadPrefabContents("Assets/Data/Prefab/World Managers/World Item Database.prefab");
            var database = contents.GetComponent<WorldItemDatabase>();
            var awake = typeof(WorldItemDatabase).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            Action initialize = () => { WorldItemDatabase.instance = null; awake.Invoke(database, null); };
            initialize();
            var allItems = (List<Item>)typeof(WorldItemDatabase).GetField("items", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(database);
            Require(allItems.Count == 25, "Expected 17 existing and 8 new database entries.");
            Require(allItems.Select(i => i.itemID).Distinct().Count() == 25, "Duplicate item IDs.");
            foreach (var entry in expected.existing)
            {
                var item = AssetDatabase.LoadAssetAtPath<Item>(AssetDatabase.GUIDToAssetPath(entry.guid));
                Require(item != null && item.itemID == entry.runtimeID, "Existing runtime ID changed: " + entry.path);
                Require(database.GetItemByID(entry.runtimeID) == item, "Existing lookup failed: " + entry.path);
                result.existingIDsPreserved++;
            }
            foreach (var entry in expected.added)
            {
                var item = AssetDatabase.LoadAssetAtPath<Item>(AssetDatabase.GUIDToAssetPath(entry.guid));
                Require(item != null && item.itemID == entry.itemID && database.GetItemByID(entry.itemID) == item, "New item lookup failed: " + entry.path);
                Item typed = null;
                if (entry.field == "headEquipment") typed = database.GetHeadEquipmentByID(entry.itemID);
                if (entry.field == "bodyEquipment") typed = database.GetBodyEquipmentByID(entry.itemID);
                if (entry.field == "handEquipment") typed = database.GetHandEquipmentByID(entry.itemID);
                if (entry.field == "legEquipment") typed = database.GetLegEquipmentByID(entry.itemID);
                Require(typed == item, "Typed armor lookup failed: " + entry.path);
                result.newItemsFound++;
            }
            var ids = allItems.ToDictionary(i => i, i => i.itemID);
            string[] categories = { "weapons", "headEquipment", "bodyEquipment", "handEquipment", "legEquipment", "ashesOfWar", "spells", "projectiles", "quickSlotItems" };
            foreach (string category in categories)
            {
                var list = (IList)typeof(WorldItemDatabase).GetField(category, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(database);
                for (int i = 0; i < list.Count / 2; i++)
                { object temp = list[i]; list[i] = list[list.Count - 1 - i]; list[list.Count - 1 - i] = temp; }
            }
            initialize();
            Require(allItems.Count == 25 && ids.All(p => database.GetItemByID(p.Value) == p.Key && p.Key.itemID == p.Value), "IDs changed after reordering or initialization duplicated entries.");
            result.reorderedListsPassed = true;
            var heads = (List<HeadEquipmentItem>)typeof(WorldItemDatabase).GetField("headEquipment", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(database);
            var bodies = (List<BodyEquipmentItem>)typeof(WorldItemDatabase).GetField("bodyEquipment", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(database);
            int nextHead = heads.Max(i => i.itemID) + 1;
            int nextBody = bodies.Max(i => i.itemID) + 1;
            extraHead = ScriptableObject.CreateInstance<HeadEquipmentItem>();
            extraBody = ScriptableObject.CreateInstance<BodyEquipmentItem>();
            heads.Add(extraHead); bodies.Add(extraBody);
            initialize();
            Require(extraHead.itemID == nextHead && extraBody.itemID == nextBody, "New unassigned items were not allocated category IDs.");
            Require(ids.All(p => database.GetItemByID(p.Value) == p.Key && p.Key.itemID == p.Value), "Adding more equipment changed existing IDs.");
            result.additionalItemsPassed = true;
            result.passed = true;
        }
        catch (Exception ex) { result.error = ex.ToString(); }
        finally
        {
            WorldItemDatabase.instance = previous;
            if (contents != null) PrefabUtility.UnloadPrefabContents(contents);
            if (extraHead != null) UnityEngine.Object.DestroyImmediate(extraHead);
            if (extraBody != null) UnityEngine.Object.DestroyImmediate(extraBody);
            File.WriteAllText(output, JsonUtility.ToJson(result, true));
            Debug.Log("ArmorDatabaseValidation: " + (result.passed ? "PASS" : "FAIL"));
        }
    }
}
