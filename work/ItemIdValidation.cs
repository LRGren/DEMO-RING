using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
internal static class ItemIdValidation
{
    static readonly string Output = Path.GetFullPath(Path.Combine(Application.dataPath, "../../work/item_id_validation_final.json"));
    [Serializable] internal class Result { public bool passed; public int checks; public int liveItems; public string error; }
    static ItemIdValidation() { EditorApplication.delayCall += Run; }
    static void Check(bool value, Result r, string message) { if (!value) throw new Exception(message); r.checks++; }
    static ItemIdCandidate C(string guid, int id, int group = 10000, int priority = int.MaxValue) => new ItemIdCandidate { guid = guid, path = guid, previousID = id, category = group, priority = priority };
    static void Run()
    {
        if (File.Exists(Output)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) { EditorApplication.delayCall += Run; return; }
        var result = new Result();
        try
        {
            var ledger = new ItemIdLedger();
            var original = C("wooden", 10003, priority:0);
            var copy = C("knight", 10003);
            var higher = C("axe",10008);
            var fresh = C("new",0);
            var head = C("head",0,20000);
            ItemIdAllocation.Allocate(ledger,new[]{copy,head,fresh,higher,original});
            Check(original.assignedID == 10003 && higher.assignedID == 10008,result,"Legacy IDs changed");
            Check(copy.assignedID != original.assignedID && copy.assignedID > 10008,result,"Duplicate not repaired");
            Check(fresh.assignedID > 10008 && fresh.assignedID != copy.assignedID,result,"New ID collision");
            Check(head.assignedID == 20000,result,"Category boundary wrong");
            int copyID=copy.assignedID;
            var restored=JsonUtility.FromJson<ItemIdLedger>(JsonUtility.ToJson(ledger));
            var renamed=C("knight",0); renamed.path="moved/renamed";
            var spoof=C("wooden",19999);
            ItemIdAllocation.Allocate(restored,new[]{renamed,spoof});
            Check(renamed.assignedID==copyID && spoof.assignedID==10003,result,"GUID mapping not stable after restart/rename/manual edit");
            var later=C("later",copyID);
            ItemIdAllocation.Allocate(restored,new[]{later});
            Check(later.assignedID>ledger.entries.Where(e=>e.itemID<20000).Max(e=>e.itemID),result,"Deleted IDs reused");
            var returning=C("knight",0);
            ItemIdAllocation.Allocate(restored,new[]{returning,later});
            Check(returning.assignedID==copyID,result,"Restored asset lost ID");
            bool rejected=false;
            try { ItemIdAllocation.Allocate(new ItemIdLedger { entries=new List<ItemIdEntry>{new ItemIdEntry {guid="a",itemID=10001},new ItemIdEntry {guid="b",itemID=10001}}},new ItemIdCandidate[0]); } catch(InvalidOperationException) { rejected=true; }
            Check(rejected,result,"Damaged ledger accepted");
            rejected=false;
            try { ItemIdAllocation.Allocate(restored,new[]{C("knight",0,20000)}); } catch(InvalidOperationException) { rejected=true; }
            Check(rejected,result,"Changed category silently reassigned");
            rejected=false;
            try { ItemIdAllocation.Allocate(new ItemIdLedger { entries=new List<ItemIdEntry>{new ItemIdEntry {guid="last",itemID=19999}}},new[]{C("overflow",0)}); } catch(InvalidOperationException) { rejected=true; }
            Check(rejected,result,"Exhausted range overflowed");

            ItemIdAutoAssigner.Synchronize();
            string before=File.ReadAllText(ItemIdAutoAssigner.LedgerPath);
            var live=AssetDatabase.FindAssets("t:Item",new[]{"Assets"}).Select(g=>AssetDatabase.LoadAssetAtPath<Item>(AssetDatabase.GUIDToAssetPath(g))).Where(i=>i!=null).ToArray();
            result.liveItems=live.Length;
            Check(live.All(i=>i.itemID>0) && live.Select(i=>i.itemID).Distinct().Count()==live.Length,result,"Live asset IDs not unique");
            var knight=AssetDatabase.LoadAssetAtPath<WeaponItem>("Assets/Data/Items/Weapons/Melee Weapons/Knight Shield.asset");
            var wooden=AssetDatabase.LoadAssetAtPath<WeaponItem>("Assets/Data/Items/Weapons/Melee Weapons/Wooden Shield.asset");
            Check(knight.itemID!=wooden.itemID && wooden.itemID==10003,result,"Live shield IDs incorrect");
            var database=UnityEngine.Object.FindObjectOfType<WorldItemDatabase>();
            var rapier=AssetDatabase.LoadAssetAtPath<WeaponItem>("Assets/Data/Items/Weapons/Melee Weapons/Rapier.asset");
            if (database!=null && database.GetWeaponByID(rapier.itemID)==null)
            {
                var serialized=new SerializedObject(database);
                var list=serialized.FindProperty("weapons");
                int index=list.arraySize++;
                list.GetArrayElementAtIndex(index).objectReferenceValue=rapier;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(database.gameObject.scene);
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(database.gameObject.scene);
            }
            Check(database!=null && database.GetWeaponByID(rapier.itemID)==rapier,result,"Scene database missing Rapier");
            Check(database!=null && database.GetWeaponByID(knight.itemID)==knight,result,"Scene database resolves wrong shield");
            var loaded=database.GetWeaponFromSerializedData(new SerializableWeapon {itemID=knight.itemID,ashOfWarID=-1});
            Check(loaded!=null && loaded.weaponModel==knight.weaponModel,result,"Saved Knight shield restores wrong model");
            UnityEngine.Object.DestroyImmediate(loaded);
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Prefab/World Managers/World Item Database.prefab").GetComponent<WorldItemDatabase>();
            foreach(string name in new[]{"Knight Shield","Mace","Rapier"})
            {
                var item=AssetDatabase.LoadAssetAtPath<WeaponItem>("Assets/Data/Items/Weapons/Melee Weapons/"+name+".asset");
                Check(prefab.GetWeaponByID(item.itemID)==item,result,"Canonical database missing "+name);
            }
            var previous=WorldItemDatabase.instance;
            try
            {
                WorldItemDatabase.instance=null;
                typeof(WorldItemDatabase).GetMethod("Awake",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(prefab,null);
                Check(true,result,"Canonical runtime registration");
            }
            finally { WorldItemDatabase.instance=previous; }
            ItemIdAutoAssigner.Synchronize();
            Check(File.ReadAllText(ItemIdAutoAssigner.LedgerPath)==before,result,"No-op synchronization rewrote IDs");
            result.passed=true;
        }
        catch(Exception error) { result.error=error.ToString(); Debug.LogException(error); }
        File.WriteAllText(Output,JsonUtility.ToJson(result,true));
        Debug.Log("Item ID validation: "+JsonUtility.ToJson(result));
    }
}
