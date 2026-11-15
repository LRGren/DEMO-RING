using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[InitializeOnLoad]
internal static class HairNavigationValidation
{
    [Serializable] private class Result { public bool passed; public int checks; public string error; }
    static HairNavigationValidation() { EditorApplication.delayCall+=Run; }
    static void Check(bool value,Result r,string message) { if(!value) throw new Exception(message); r.checks++; }
    static void Move(GameObject target,EventSystem system,MoveDirection direction)
    {
        ExecuteEvents.Execute(target,new AxisEventData(system){moveDir=direction},ExecuteEvents.moveHandler);
    }
    static void Run()
    {
        string output=Path.GetFullPath(Path.Combine(Application.dataPath,"../../work/hair_navigation_validation_verified.json"));
        if(File.Exists(output)) return;
        if(EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) { EditorApplication.delayCall+=Run; return; }
        var r=new Result(); GameObject fixture=null,player=null,canvas=null,eventRoot=null; HairStyleCatalog catalog=null;
        var oldEvent=EventSystem.current;
        try
        {
            fixture=new GameObject("Hair cycle validation"){hideFlags=HideFlags.HideAndDontSave};
            var body=fixture.AddComponent<PlayerBodyManager>();
            body.hair=new GameObject("Hair"); body.hair.transform.SetParent(fixture.transform);
            foreach(string name in new[]{"First","Second"}) new GameObject(name).transform.SetParent(body.hair.transform);
            catalog=ScriptableObject.CreateInstance<HairStyleCatalog>();
            catalog.hairStyleOptions=new List<HairStyleOption>{new HairStyleOption{hairID="none"},new HairStyleOption{hairID="one",hairModelName="First"},new HairStyleOption{hairID="two",hairModelName="Second"}};
            body.hairStyleCatalog=catalog;
            body.ResetAppearance();
            Check(body.TryCycleHairStyle(1) && body.CurrentHairID=="one",r,"Right first");
            Check(body.TryCycleHairStyle(1) && body.CurrentHairID=="two",r,"Right second");
            Check(body.TryCycleHairStyle(1) && body.CurrentHairID=="none",r,"Right wrap");
            Check(body.TryCycleHairStyle(-1) && body.CurrentHairID=="two",r,"Left wrap");
            Check(body.TryCycleHairStyle(-1) && body.CurrentHairID=="one",r,"Left previous");
            Check(!body.TryCycleHairStyle(0) && body.CurrentHairID=="one",r,"Zero direction changed appearance");
            catalog.hairStyleOptions.Insert(2,null);
            catalog.hairStyleOptions.Insert(3,new HairStyleOption());
            Check(body.TryCycleHairStyle(1) && body.CurrentHairID=="two",r,"Null/empty option not skipped");
            body.DisableHair(); body.TryCycleHairStyle(1);
            Check(!body.hair.activeSelf && body.CurrentHairID=="none",r,"Cycling overrides helmet hiding");
            var options=catalog.hairStyleOptions;
            catalog.hairStyleOptions=new List<HairStyleOption>{options[0]};
            Check(!body.TryCycleHairStyle(1) && body.CurrentHairID=="none",r,"Single current option");
            catalog.hairStyleOptions.Clear(); Check(!body.TryCycleHairStyle(-1),r,"Empty catalog");
            catalog.hairStyleOptions=null; Check(!body.TryCycleHairStyle(1),r,"Null list");
            body.hairStyleCatalog=null; Check(!body.TryCycleHairStyle(1),r,"Missing catalog");
            body.hairStyleCatalog=catalog; catalog.hairStyleOptions=options;
            typeof(PlayerBodyManager).GetField("currentHairID",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(body,"removed");
            Check(body.TryCycleHairStyle(1) && body.CurrentHairID=="none",r,"Unknown current right");
            typeof(PlayerBodyManager).GetField("currentHairID",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(body,"removed");
            Check(body.TryCycleHairStyle(-1) && body.CurrentHairID=="two",r,"Unknown current left");

            player=PrefabUtility.LoadPrefabContents("Assets/Data/Prefab/Player/Player.prefab");
            var actualPlayer=player.GetComponent<PlayerManager>();
            actualPlayer.playerBodyManager=player.GetComponent<PlayerBodyManager>();
            var actualBody=actualPlayer.playerBodyManager;
            actualBody.ResetAppearance();
            canvas=PrefabUtility.LoadPrefabContents("Assets/Data/Prefab/UI/Title Screen Canvas.prefab");
            var title=canvas.GetComponent<TitleScreenManager>(); title.previewPlayer=actualPlayer;
            var row=canvas.GetComponentInChildren<UI_HairStyleSelector>(true);
            Check(row!=null,r,"Missing prefab row component");
            for(var current=row.transform;current!=null;current=current.parent) current.gameObject.SetActive(true);
            typeof(UI_HairStyleSelector).GetMethod("OnEnable",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(row,null);
            var button=row.GetComponent<Button>();
            eventRoot=new GameObject("UI navigation validation"){hideFlags=HideFlags.HideAndDontSave};
            var system=eventRoot.AddComponent<EventSystem>();
            var systems=(List<EventSystem>)typeof(EventSystem).GetField("m_EventSystems",BindingFlags.Static|BindingFlags.NonPublic).GetValue(null);
            if(!systems.Contains(system)) typeof(EventSystem).GetMethod("OnEnable",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(system,null);
            EventSystem.current=system;
            system.SetSelectedGameObject(row.gameObject);
            Check(system.currentSelectedGameObject==row.gameObject,r,"Row not selected");
            var right=new AxisEventData(system){moveDir=MoveDirection.Right};
            ExecuteEvents.Execute(row.gameObject,right,ExecuteEvents.moveHandler);
            Check(actualBody.CurrentHairID=="hair01" && right.used,r,"Right event did not cycle: id="+actualBody.CurrentHairID+", used="+right.used+", current="+(EventSystem.current==system)+", rowActive="+row.isActiveAndEnabled+", interactable="+button.IsInteractable());
            Check(system.currentSelectedGameObject==row.gameObject,r,"Right event moved focus");
            var label=row.GetComponentInChildren<TMP_Text>();
            Check(label.text.Contains("Hair_01"),r,"Value text did not update");
            Move(row.gameObject,system,MoveDirection.Left);
            Check(actualBody.CurrentHairID=="none" && system.currentSelectedGameObject==row.gameObject,r,"Left event did not cycle");
            Move(row.gameObject,system,MoveDirection.Left);
            Check(actualBody.CurrentHairID=="hair02",r,"UI left wrap");
            var returnButton=button.navigation.selectOnDown;
            Check(returnButton!=null && returnButton.gameObject.name=="Return",r,"Missing return navigation");
            Move(row.gameObject,system,MoveDirection.Down);
            Check(system.currentSelectedGameObject==returnButton.gameObject && actualBody.CurrentHairID=="hair02",r,"Vertical move changed hair or lost focus");
            Move(row.gameObject,system,MoveDirection.Right);
            Check(actualBody.CurrentHairID=="hair02",r,"Unselected row handled horizontal input");
            Move(returnButton.gameObject,system,MoveDirection.Up);
            Check(system.currentSelectedGameObject==row.gameObject,r,"Return to hair row failed");
            button.interactable=false;
            Move(row.gameObject,system,MoveDirection.Right);
            Check(actualBody.CurrentHairID=="hair02",r,"Disabled row changed hairstyle");
            button.interactable=true;
            ExecuteEvents.Execute(row.gameObject,new BaseEventData(system),ExecuteEvents.submitHandler);
            Check(actualBody.CurrentHairID=="hair02",r,"Submit still invokes fixed hair choice");
            var saved=actualBody.GetAppearanceData(); actualBody.ResetAppearance(); actualBody.LoadAppearance(saved);
            Check(actualBody.CurrentHairID=="hair02",r,"Cycled hairstyle not persisted");
            row.transform.parent.gameObject.SetActive(false);
            Move(row.gameObject,system,MoveDirection.Right);
            Check(actualBody.CurrentHairID=="hair02",r,"Closed menu changed hair");
            Check(!row.transform.parent.Find("Hair02").gameObject.activeSelf,r,"Legacy choice is still visible");
            Check(title.FirstCUSTOMIZATIONButton==button,r,"Initial focus points to wrong row");
            var eventPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/Prefab/UI/EventSystem.prefab");
            var module=eventPrefab.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            Check(module!=null && module.move!=null && module.move.action!=null,r,"Missing UI input action");
            var paths=module.move.action.bindings.Select(binding=>binding.path).ToArray();
            Check(paths.Contains("<Gamepad>/dpad") && paths.Contains("<Gamepad>/leftStick"),r,"Missing gamepad navigation binding");
            Check(!paths.Any(path=>path.Contains("rightStick")),r,"Preview rotation also navigates UI");
            r.passed=true;
        }
        catch(Exception error) { r.error=error.ToString(); Debug.LogException(error); }
        finally
        {
            if(eventRoot!=null)
            {
                var system=eventRoot.GetComponent<EventSystem>();
                typeof(EventSystem).GetMethod("OnDisable",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(system,null);
                UnityEngine.Object.DestroyImmediate(eventRoot);
            }
            if(oldEvent!=null) EventSystem.current=oldEvent;
            if(canvas!=null) PrefabUtility.UnloadPrefabContents(canvas);
            if(player!=null) PrefabUtility.UnloadPrefabContents(player);
            if(fixture!=null) UnityEngine.Object.DestroyImmediate(fixture);
            if(catalog!=null) UnityEngine.Object.DestroyImmediate(catalog);
        }
        File.WriteAllText(output,JsonUtility.ToJson(r,true));
        Debug.Log("Hair navigation validation: "+JsonUtility.ToJson(r));
    }
}
