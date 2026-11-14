using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class ArmorSetValidation
{
    [Serializable] public class Manifest { public Entry[] items; }
    [Serializable] public class Entry
    {
        public string set;
        public string slot;
        public string path;
        public Model[] models;
    }
    [Serializable] public class Model
    {
        public string path;
        public int type;
        public string male;
        public string female;
    }
    [Serializable] public class Result
    {
        public bool passed;
        public int items;
        public int genderModelChecks;
        public int attachmentCleanupChecks;
        public string error;
        public string[] messages;
    }

    static ArmorSetValidation()
    {
        EditorApplication.delayCall += Run;
    }

    static void Require(bool valid, string message)
    {
        if (!valid) throw new InvalidOperationException(message);
    }

    static void Run()
    {
        string work = Path.GetFullPath(Path.Combine(Application.dataPath, "../../work"));
        string report = Path.Combine(work, "armor_unity_validation.json");
        if (File.Exists(report)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += Run;
            return;
        }
        var result = new Result();
        var messages = new List<string>();
        GameObject contents = null;
        try
        {
            var manifest = JsonUtility.FromJson<Manifest>("{\"items\":" + File.ReadAllText(Path.Combine(work, "armor_sets_manifest.json")) + "}");
            Require(manifest.items.Length == 8, "Expected eight equipment items.");
            contents = PrefabUtility.LoadPrefabContents("Assets/Data/Prefab/Player/Player.prefab");
            var player = contents.GetComponent<PlayerManager>();
            var equipment = contents.GetComponent<PlayerEquipmentManager>();
            var body = contents.GetComponent<PlayerBodyManager>();
            Require(player != null && equipment != null && body != null, "Player components missing.");
            player.playerEquipmentManager = equipment;
            player.playerBodyManager = body;
            typeof(PlayerEquipmentManager).GetField("player", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(equipment, player);

            var initialize = typeof(PlayerEquipmentManager).GetMethod("InitializeEquipmentModel", BindingFlags.Instance | BindingFlags.NonPublic);
            string source = File.ReadAllText(Path.Combine(Application.dataPath, "Scripcts/Character/Player/PlayerEquipmentManager.cs"));
            var pairs = System.Text.RegularExpressions.Regex.Matches(source, @"InitializeEquipmentModel\(ref (\w+), ref (\w+)\)");
            foreach (System.Text.RegularExpressions.Match pair in pairs)
            {
                var parent = typeof(PlayerEquipmentManager).GetField(pair.Groups[1].Value);
                var models = typeof(PlayerEquipmentManager).GetField(pair.Groups[2].Value);
                object[] args = { parent.GetValue(equipment), null };
                initialize.Invoke(equipment, args);
                models.SetValue(equipment, args[1]);
            }
            var transforms = contents.GetComponentsInChildren<Transform>(true).ToDictionary(t => t.name, t => t.gameObject);
            foreach (var entry in manifest.items)
            {
                var item = AssetDatabase.LoadAssetAtPath<ArmorItem>(entry.path);
                Require(item != null, "Cannot deserialize item: " + entry.path);
                Require(item.equipmentModels.Length == entry.models.Length, "Model count mismatch: " + entry.path);
                Require(item.itemIcon != null, "Missing item icon: " + entry.path);
                Require(item.itemWeight > 0 && item.physicalDamageAbsorption > 0 && item.poise > 0, "Missing stats: " + entry.path);
                if (entry.slot == "Head")
                {
                    var expected = entry.set == "Archer" ? HeadEquipmentType.Hat : HeadEquipmentType.Hood;
                    Require(item is HeadEquipmentItem && ((HeadEquipmentItem)item).headEquipmentType == expected, "Wrong head type.");
                }
                else if (entry.slot == "Body") Require(item is BodyEquipmentItem, "Wrong body item type.");
                else if (entry.slot == "Hand") Require(item is HandEquipmentItem, "Wrong hand item type.");
                else if (entry.slot == "Leg") Require(item is LegEquipmentItem, "Wrong leg item type.");
                for (int i = 0; i < entry.models.Length; i++)
                {
                    var model = item.equipmentModels[i];
                    var expected = entry.models[i];
                    Require(model != null && (int)model.equipmentModelType == expected.type, "Missing or wrong EquipmentModel.");
                    Require(model.equipmentModelName == expected.male && model.femaleEquipmentModelName == expected.female, "Gender mapping mismatch.");
                    foreach (bool male in new[] { true, false })
                    {
                        string name = male ? expected.male : expected.female;
                        Require(transforms.ContainsKey(name), "Model missing from Player: " + name);
                        GameObject target = transforms[name];
                        Require(target.GetComponent<SkinnedMeshRenderer>()?.sharedMesh != null, "Model has no mesh: " + name);
                        target.SetActive(false);
                        model.LoadEquipmentModel(player, male);
                        Require(target.activeSelf, "Runtime model loader failed: " + name);
                        target.SetActive(false);
                        result.genderModelChecks++;
                    }
                }
                result.items++;
                messages.Add("Loaded " + entry.path + "; both gender mappings passed.");
            }

            // Exercise the actual unload methods on isolated prefab contents.
            // No prefab or scene is saved by this validation.
            foreach (bool male in new[] { true, false })
            {
                foreach (string set in new[] { "Archer", "Berserker", "Archer" })
                {
                    equipment.UnloadBodyEquipment();
                    equipment.UnloadLegEquipment();
                    foreach (var entry in manifest.items.Where(e => e.set == set && (e.slot == "Body" || e.slot == "Leg")))
                        foreach (var model in AssetDatabase.LoadAssetAtPath<ArmorItem>(entry.path).equipmentModels)
                            model.LoadEquipmentModel(player, male);
                    string backName = set == "Archer" ? "Chr_BackAttachment_11" : "Chr_BackAttachment_14";
                    Require(equipment.backAccessories.Count(g => g.activeSelf) == 1 && transforms[backName].activeSelf, "Back attachments overlap during set switch.");
                    Require(equipment.hipAccessories.Count(g => g.activeSelf) == (set == "Berserker" ? 1 : 0), "Hip attachment remains after set switch.");
                    equipment.UnloadBodyEquipment();
                    equipment.UnloadLegEquipment();
                    Require(equipment.backAccessories.All(g => !g.activeSelf), "Back attachment did not unload.");
                    Require(equipment.hipAccessories.All(g => !g.activeSelf), "Hip attachment did not unload.");
                    result.attachmentCleanupChecks++;
                }
            }
            result.passed = true;
            messages.Add("Archer -> Berserker -> Archer switch and attachment cleanup passed for both genders.");
        }
        catch (Exception ex)
        {
            result.error = ex.ToString();
        }
        finally
        {
            if (contents != null) PrefabUtility.UnloadPrefabContents(contents);
            result.messages = messages.ToArray();
            File.WriteAllText(report, JsonUtility.ToJson(result, true));
            Debug.Log("ArmorSetValidation: " + (result.passed ? "PASS" : "FAIL") + "; " + report);
        }
    }
}
