using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
internal static class HairAppearanceValidation
{
    [Serializable] private class Result { public bool passed; public int checks; public string error; }
    static HairAppearanceValidation() { EditorApplication.delayCall += Run; }
    static void Check(bool condition, Result result, string message) { if(!condition) throw new Exception(message); result.checks++; }
    static void Run()
    {
        string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../work/hair_appearance_validation.json"));
        if(File.Exists(output)) return;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) { EditorApplication.delayCall += Run; return; }
        var result=new Result();
        GameObject root=null;
        GameObject prefab=null;
        HairStyleCatalog catalog=null;
        try
        {
            root=new GameObject("Hair appearance validation") {hideFlags=HideFlags.HideAndDontSave};
            var body=root.AddComponent<PlayerBodyManager>();
            body.hair=new GameObject("All_01_Hair"); body.hair.transform.SetParent(root.transform);
            var first=new GameObject("Chr_Hair_01"); first.transform.SetParent(body.hair.transform);
            var second=new GameObject("Chr_Hair_02"); second.transform.SetParent(body.hair.transform);
            catalog=ScriptableObject.CreateInstance<HairStyleCatalog>();
            catalog.hairStyleOptions=new List<HairStyleOption>
            {
                new HairStyleOption{hairID="none",hairName="No hair"},
                new HairStyleOption{hairID="hair01",hairModelName="Chr_Hair_01"},
                new HairStyleOption{hairID="hair02",hairModelName="Chr_Hair_02"},
                new HairStyleOption{hairID="missing",hairModelName="MissingModel"},
                new HairStyleOption{hairID="blank",hairModelName=" "}
            };
            body.hairStyleCatalog=catalog;
            Check(body.TrySetHairStyle("hair01") && first.activeSelf && !second.activeSelf && body.CurrentHairID=="hair01",result,"First hairstyle");
            Check(body.TrySetHairStyle("hair02") && !first.activeSelf && second.activeSelf && body.CurrentHairID=="hair02",result,"Second hairstyle");
            var saved=body.GetAppearanceData();
            var savedAgain=body.GetAppearanceData();
            Check(!ReferenceEquals(saved,savedAgain),result,"Save snapshot shared");
            body.ResetAppearance();
            Check(body.CurrentHairID=="none" && !first.activeSelf && !second.activeSelf,result,"Bald selection");
            body.LoadAppearance(JsonUtility.FromJson<AppearanceSaveData>(JsonUtility.ToJson(saved)));
            Check(body.CurrentHairID=="hair02" && second.activeSelf && !first.activeSelf,result,"JSON round trip");
            body.LoadAppearance(null);
            Check(body.CurrentHairID=="none" && !first.activeSelf && !second.activeSelf,result,"Null save fallback");
            foreach(string missing in new[]{null,"","  "})
            {
                body.SetHairStyle("hair01");
                body.LoadAppearance(new AppearanceSaveData{hairID=missing});
                Check(body.CurrentHairID=="none" && !first.activeSelf && !second.activeSelf,result,"Empty saved ID fallback");
            }
            body.SetHairStyle("hair01");
            body.DisableHair();
            body.SetHairStyle("hair02");
            Check(!body.hair.activeSelf && second.activeSelf && body.CurrentHairID=="hair02",result,"Helmet hiding was overridden");
            body.EnableHair();
            Check(second.activeInHierarchy && !first.activeSelf,result,"Helmet removal lost selected hairstyle");
            foreach(string invalid in new[]{null,""," ","unknown","missing","blank"})
            {
                Check(!body.TrySetHairStyle(invalid) && body.CurrentHairID=="hair02" && second.activeSelf && !first.activeSelf,result,"Invalid selection changed current appearance: "+invalid);
            }
            catalog.hairStyleOptions.Add(null);
            Check(body.TrySetHairStyle("hair01"),result,"Null catalog entry");
            catalog.hairStyleOptions.Add(new HairStyleOption{hairID="hair02",hairModelName="Chr_Hair_02"});
            Check(!body.TrySetHairStyle("hair02") && body.CurrentHairID=="hair01" && first.activeSelf,result,"Duplicate ID accepted");
            catalog.hairStyleOptions=null;
            Check(!body.TrySetHairStyle("hair02") && body.CurrentHairID=="hair01" && first.activeSelf,result,"Null catalog list");
            body.LoadAppearance(new AppearanceSaveData{hairID="unknown"});
            Check(body.CurrentHairID=="none" && !first.activeSelf && !second.activeSelf,result,"Removed saved option fallback");
            body.hairStyleCatalog=null;
            Check(!body.TrySetHairStyle("hair01") && body.CurrentHairID=="none",result,"Missing catalog");
            Check(body.TrySetHairStyle("none"),result,"Default bald requires catalog");
            var parent=body.hair;
            body.hair=null;
            Check(!body.TrySetHairStyle("hair01") && body.CurrentHairID=="none",result,"Missing hair parent");
            body.EnableHair(); body.DisableHair();
            body.hair=parent;

            prefab=PrefabUtility.LoadPrefabContents("Assets/Data/Prefab/Player/Player.prefab");
            var actual=prefab.GetComponent<PlayerBodyManager>();
            Check(actual!=null && actual.hairStyleCatalog!=null && actual.hair!=null,result,"Real prefab binding");
            Check(actual.TrySetHairStyle("hair02") && actual.CurrentHairID=="hair02",result,"Real prefab hairstyle");
            Check(actual.hair.transform.Cast<Transform>().Count(child=>child.gameObject.activeSelf)==1,result,"Real prefab has multiple hairs");
            var snapshot=actual.GetAppearanceData();
            actual.ResetAppearance(); actual.LoadAppearance(snapshot);
            Check(actual.CurrentHairID=="hair02" && actual.hair.transform.Find("Chr_Hair_02").gameObject.activeSelf,result,"Real prefab save restore");
            actual.ResetAppearance();
            Check(actual.hair.transform.Cast<Transform>().All(child=>!child.gameObject.activeSelf),result,"Real prefab bald");
            result.passed=true;
        }
        catch(Exception error) { result.error=error.ToString(); Debug.LogException(error); }
        finally
        {
            if(prefab!=null) PrefabUtility.UnloadPrefabContents(prefab);
            if(root!=null) UnityEngine.Object.DestroyImmediate(root);
            if(catalog!=null) UnityEngine.Object.DestroyImmediate(catalog);
        }
        File.WriteAllText(output,JsonUtility.ToJson(result,true));
        Debug.Log("Hair appearance validation: "+JsonUtility.ToJson(result));
    }
}
