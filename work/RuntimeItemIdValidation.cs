using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
internal static class RuntimeItemIdValidation
{
    [Serializable] internal class Result { public bool passed; public int itemCount; public bool uniqueIDs; public bool shieldRoundTrip; public bool repeatInitialization; public string error; }
    static RuntimeItemIdValidation() { EditorApplication.delayCall += Run; }
    static void Require(bool condition,string message) { if(!condition) throw new Exception(message); }
    static void Run()
    {
        string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../work/runtime_item_id_validation_final.json"));
        if(File.Exists(output)) return;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) { EditorApplication.delayCall+=Run; return; }
        var result=new Result();
        var previous=WorldItemDatabase.instance;
        var ids=new Dictionary<Item,int>();
        var copies=new List<WeaponItem>();
        try
        {
            var database=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Prefab/World Managers/World Item Database.prefab").GetComponent<WorldItemDatabase>();
            string[] categories={"weapons","headEquipment","bodyEquipment","handEquipment","legEquipment","ashesOfWar","spells","projectiles","quickSlotItems"};
            int[] keys={database.weaponKey,database.headKey,database.bodyKey,database.handKey,database.legKey,database.ashOfWarKey,database.spellKey,database.projectileKey,database.quickSlotItemKey};
            var all=new List<Item>();
            var expected=new Dictionary<Item,int>();
            for(int category=0;category<categories.Length;category++)
            {
                var list=(IList)typeof(WorldItemDatabase).GetField(categories[category],BindingFlags.NonPublic|BindingFlags.Instance).GetValue(database);
                foreach(Item item in list)
                {
                    Require(item!=null,"Null registered item");
                    ids.Add(item,item.itemID);
                    expected.Add(item,keys[category]+all.Count);
                    all.Add(item);
                }
            }
            var awake=typeof(WorldItemDatabase).GetMethod("Awake",BindingFlags.Instance|BindingFlags.NonPublic);
            WorldItemDatabase.instance=null; awake.Invoke(database,null);
            Require(all.All(item=>item.itemID==expected[item]),"Original index+key ID rule differs");
            result.itemCount=all.Count;
            result.uniqueIDs=all.Select(item=>item.itemID).Distinct().Count()==all.Count;
            Require(result.uniqueIDs,"Runtime IDs collide");
            foreach(string name in new[]{"Knight Shield","Mace","Rapier"})
            {
                var item=AssetDatabase.LoadAssetAtPath<WeaponItem>("Assets/Data/Items/Weapons/Melee Weapons/"+name+".asset");
                Require(database.GetWeaponByID(item.itemID)==item,"Registered weapon not found: "+name);
                var weapon=database.GetWeaponFromSerializedData(new SerializableWeapon{itemID=item.itemID,ashOfWarID=-1});
                copies.Add(weapon);
                Require(weapon!=null && weapon.weaponModel==item.weaponModel,"Saved weapon restores wrong model: "+name);
            }
            result.shieldRoundTrip=true;
            WorldItemDatabase.instance=null;
            awake.Invoke(database,null);
            var registered=(List<Item>)typeof(WorldItemDatabase).GetField("items",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(database);
            result.repeatInitialization=registered.Count==all.Count && all.All(item=>item.itemID==expected[item]);
            Require(result.repeatInitialization,"Repeated initialization duplicated items");
            result.passed=true;
        }
        catch(Exception error) { result.error=error.ToString(); Debug.LogException(error); }
        finally
        {
            foreach(var pair in ids) pair.Key.itemID=pair.Value;
            foreach(var item in copies) if(item!=null) UnityEngine.Object.DestroyImmediate(item);
            WorldItemDatabase.instance=previous;
        }
        File.WriteAllText(output,JsonUtility.ToJson(result,true));
        Debug.Log("Runtime item IDs: "+JsonUtility.ToJson(result));
    }
}
