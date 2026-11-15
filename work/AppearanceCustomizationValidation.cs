using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[InitializeOnLoad]
internal static class AppearanceCustomizationValidation
{
    [Serializable] private class Result { public bool passed; public int checks; public string error; }
    static AppearanceCustomizationValidation() { EditorApplication.delayCall += Run; }
    static void Check(bool condition, Result r, string message)
    {
        if (!condition) throw new Exception(message);
        r.checks++;
    }
    static int ActiveChildren(GameObject parent)
    {
        return parent.transform.Cast<Transform>().Count(t => t.gameObject.activeSelf);
    }
    static AxisEventData Move(MoveDirection direction)
    {
        return new AxisEventData(null) { moveDir = direction };
    }
    static void Run()
    {
        string report = Path.GetFullPath(Path.Combine(Application.dataPath, "../../work/appearance_customization_validation.json"));
        if (File.Exists(report)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorApplication.delayCall += Run;
            return;
        }
        var result = new Result();
        GameObject playerRoot = null;
        GameObject uiRoot = null;
        try
        {
            playerRoot = PrefabUtility.LoadPrefabContents("Assets/Data/Prefab/Player/Player.prefab");
            var player = playerRoot.GetComponent<PlayerManager>();
            var body = playerRoot.GetComponent<PlayerBodyManager>();
            player.playerBodyManager = body;
            body.player = player;
            Check(body.maleHead.name == "Male_Head_All_Elements" && body.femaleHead.name == "Female_Head_All_Elements", result, "Head references are not variant containers");
            Check(body.hairStyleCatalog.hairStyleOptions.Count == 39, result, "Hair catalog count");
            Check(body.headStyleCatalog.headStyleOptions.Count == 23, result, "Head catalog count");
            Check(body.maleEyebrowStyleCatalog.eyebrowStyleOptions.Count == 11 && body.femaleEyebrowStyleCatalog.eyebrowStyleOptions.Count == 8, result, "Brow catalog counts");
            Check(body.facialHairStyleCatalog.facialHairStyleOptions.Count == 19, result, "Beard catalog count");
            body.ResetAppearance();
            Check(body.CurrentHairID == "none" && body.CurrentHeadID == "head00" && body.CurrentFacialHairID == "none", result, "Default IDs");
            Check(ActiveChildren(body.maleHead) == 1 && ActiveChildren(body.femaleHead) == 1, result, "Default heads");
            foreach (var option in body.hairStyleCatalog.hairStyleOptions)
            {
                Check(body.TrySetHairStyle(option.hairID), result, "Hair model: " + option.hairID);
                Check(ActiveChildren(body.hair) == (option.hairID == "none" ? 0 : 1), result, "Hair exclusivity");
            }
            foreach (var option in body.headStyleCatalog.headStyleOptions)
            {
                Check(body.TrySetHeadStyle(option.headID), result, "Head model: " + option.headID);
                Check(body.maleHead.transform.Find(option.maleHeadModelName).gameObject.activeSelf && body.femaleHead.transform.Find(option.femaleHeadModelName).gameObject.activeSelf, result, "Head pairing");
                Check(ActiveChildren(body.maleHead) == 1 && ActiveChildren(body.femaleHead) == 1, result, "Head exclusivity");
            }
            foreach (bool male in new[] {true, false})
            {
                body.maleObject.SetActive(male); body.femaleObject.SetActive(!male);
                var parent = male ? body.maleEyebrows : body.femaleEyebrows;
                foreach (var option in body.CurrentEyebrowStyleCatalog.eyebrowStyleOptions)
                {
                    Check(body.TrySetEyebrowStyle(option.eyebrowID), result, "Eyebrow model: " + male + ":" + option.eyebrowID);
                    Check(ActiveChildren(parent) == (option.eyebrowID == "none" ? 0 : 1), result, "Eyebrow exclusivity");
                }
            }
            foreach (var option in body.facialHairStyleCatalog.facialHairStyleOptions)
            {
                Check(body.TrySetFacialHairStyle(option.facialHairID), result, "Beard model: " + option.facialHairID);
                Check(ActiveChildren(body.maleFacialHair) == (option.facialHairID == "none" ? 0 : 1), result, "Beard exclusivity");
            }

            body.maleObject.SetActive(true); body.femaleObject.SetActive(false);
            body.SetHairStyle("hair38"); body.SetHeadStyle("head22"); body.SetEyebrowStyle("eyebrow10"); body.SetFacialHairStyle("beard18");
            body.maleObject.SetActive(false); body.femaleObject.SetActive(true);
            body.SetEyebrowStyle("eyebrow07");
            var saved = body.GetAppearanceData();
            var fullSave = new CharacterSaveData { appearanceSaveData = saved };
            var restored = JsonUtility.FromJson<CharacterSaveData>(JsonUtility.ToJson(fullSave));
            body.ResetAppearance(); body.LoadAppearance(restored.appearanceSaveData);
            Check(body.CurrentHairID == "hair38" && body.CurrentHeadID == "head22" && body.CurrentFacialHairID == "beard18", result, "Appearance save round trip");
            Check(body.currentMaleEyebrowID == "eyebrow10" && body.currentFemaleEyebrowID == "eyebrow07", result, "Separate gender brow save");
            Check(!body.maleObject.activeSelf && body.CurrentEyebrowID == "eyebrow07", result, "Loading changed body type");
            Check(!body.TrySetEyebrowStyle("eyebrow10") && body.CurrentEyebrowID == "eyebrow07", result, "Female unavailable eyebrow accepted");
            Check(!ReferenceEquals(saved, body.GetAppearanceData()), result, "Save snapshot shared");
            body.DisableHead(); body.DisableHair();
            body.SetHeadStyle("head04"); body.SetHairStyle("hair05"); body.SetFacialHairStyle("beard03"); body.SetEyebrowStyle("eyebrow04");
            Check(!body.maleHead.activeSelf && !body.femaleHead.activeSelf && !body.hair.activeSelf && !body.maleFacialHair.activeSelf && !body.femaleEyebrows.activeSelf, result, "Selection broke helmet masking");
            body.EnableHead(); body.EnableHair();
            Check(body.maleHead.transform.Find("Chr_Head_Male_04").gameObject.activeSelf && !body.maleHead.transform.Find("Chr_Head_Male_00").gameObject.activeSelf, result, "Helmet removal restored default face");
            Check(ActiveChildren(body.maleHead) == 1 && ActiveChildren(body.maleFacialHair) == 1, result, "Helmet removal enabled extra variants");
            Check(!body.TrySetHeadStyle("missing") && body.CurrentHeadID == "head04", result, "Invalid face changed selection");
            Check(!body.TrySetFacialHairStyle("missing") && body.CurrentFacialHairID == "beard03", result, "Invalid beard changed selection");
            body.LoadAppearance(JsonUtility.FromJson<AppearanceSaveData>("{\"hairID\":\"hair02\"}"));
            Check(body.CurrentHairID == "hair02" && body.CurrentHeadID == "head00" && body.CurrentFacialHairID == "none" && body.CurrentEyebrowID == "eyebrow01", result, "Old hair-only save compatibility");
            body.LoadAppearance(new AppearanceSaveData { hairID = "hair03", headID = "deleted", maleEyebrowID = "deleted", femaleEyebrowID = "deleted", facialHairID = "deleted" });
            Check(body.CurrentHairID == "hair03" && body.CurrentHeadID == "head00" && body.CurrentFacialHairID == "none" && body.CurrentEyebrowID == "eyebrow01", result, "Per-part fallback overwrote valid hair");
            body.LoadAppearance(null);
            Check(body.CurrentHairID == "none" && body.CurrentHeadID == "head00" && body.CurrentFacialHairID == "none", result, "Null save default");

            uiRoot = PrefabUtility.LoadPrefabContents("Assets/Data/Prefab/UI/Title Screen Canvas.prefab");
            var menu = uiRoot.GetComponent<TitleScreenManager>();
            if (menu == null) menu = uiRoot.GetComponentInChildren<TitleScreenManager>(true);
            menu.previewPlayer = player;
            Check(menu.headStyleText != null && menu.eyebrowStyleText != null && menu.facialHairStyleText != null, result, "UI text bindings");
            var customization = menu.hairStyleText.transform.parent.parent;
            string[] names = { "Gender", "Hair", "Face", "Eyebrows", "Beard", "Return" };
            string[] methods = { "SetGender", "SelectHairStyle", "SelectHeadStyle", "SelectEyebrowStyle", "SelectFacialHairStyle" };
            for (int i = 0; i < names.Length; i++)
            {
                var row = customization.Find(names[i]);
                Check(row != null, result, "Missing row: " + names[i]);
                var button = row.GetComponent<Button>();
                Check(button.navigation.mode == Navigation.Mode.Explicit && button.navigation.selectOnLeft == null && button.navigation.selectOnRight == null, result, "Horizontal navigation escaped: " + names[i]);
                Check(button.navigation.selectOnUp == (i == 0 ? null : customization.Find(names[i-1]).GetComponent<Button>()), result, "Up link: " + names[i]);
                Check(button.navigation.selectOnDown == (i == names.Length-1 ? null : customization.Find(names[i+1]).GetComponent<Button>()), result, "Down link: " + names[i]);
                if (i < methods.Length)
                {
                    var entry = row.GetComponent<EventTrigger>().triggers.Single(t => t.eventID == EventTriggerType.Move);
                    Check(entry.callback.GetPersistentEventCount() == 1 && entry.callback.GetPersistentMethodName(0) == methods[i] && entry.callback.GetPersistentTarget(0) == menu, result, "Move callback: " + names[i]);
                }
            }
            Check(customization.GetComponentsInChildren<PlayerUISelectedOnEnable>(true).Length == 2, result, "Cloned rows stole selection on enable");
            // The two existing selectors are Gender and Hair; no cloned selector is added.
            body.maleObject.SetActive(true); body.femaleObject.SetActive(false);
            body.ResetAppearance();
            menu.SelectHeadStyle(Move(MoveDirection.Left));
            Check(body.CurrentHeadID == "head22" && menu.headStyleText.text.Contains("Face_22"), result, "Face left wrap / label");
            menu.SelectHeadStyle(Move(MoveDirection.Right));
            Check(body.CurrentHeadID == "head00", result, "Face right wrap");
            menu.SelectHairStyle(Move(MoveDirection.Left));
            Check(body.CurrentHairID == "hair38", result, "Hair left wrap");
            menu.SelectHairStyle(Move(MoveDirection.Right));
            Check(body.CurrentHairID == "none", result, "Hair right wrap");
            menu.SetEyebrowStyle("none"); menu.SelectEyebrowStyle(Move(MoveDirection.Left));
            Check(body.CurrentEyebrowID == "eyebrow10", result, "Male brow left wrap");
            menu.SelectEyebrowStyle(Move(MoveDirection.Right));
            Check(body.CurrentEyebrowID == "none", result, "Male brow right wrap");
            menu.SelectFacialHairStyle(Move(MoveDirection.Left));
            Check(body.CurrentFacialHairID == "beard18", result, "Beard left wrap");
            menu.SelectFacialHairStyle(Move(MoveDirection.Right));
            Check(body.CurrentFacialHairID == "none", result, "Beard right wrap");
            string before = JsonUtility.ToJson(body.GetAppearanceData());
            menu.SelectHairStyle(Move(MoveDirection.Up)); menu.SelectHeadStyle(Move(MoveDirection.Down));
            menu.SelectEyebrowStyle(Move(MoveDirection.Up)); menu.SelectFacialHairStyle(Move(MoveDirection.Down));
            menu.SelectHeadStyle(null);
            Check(before == JsonUtility.ToJson(body.GetAppearanceData()), result, "Up/down changed appearance");
            menu.SetEyebrowStyle("eyebrow10"); menu.SetFacialHairStyle("beard03");
            body.maleObject.SetActive(false); body.femaleObject.SetActive(true);
            menu.RefreshAppearanceTexts();
            Check(menu.genderText.text.Contains("Female") && menu.facialHairStyleText.text.Contains("Unavailable"), result, "Female labels");
            menu.SetEyebrowStyle("none"); menu.SelectEyebrowStyle(Move(MoveDirection.Left));
            Check(body.CurrentEyebrowID == "eyebrow07", result, "Female brow wrap");
            menu.SelectFacialHairStyle(Move(MoveDirection.Left));
            Check(body.CurrentFacialHairID == "beard03" && !body.maleFacialHair.activeInHierarchy, result, "Female beard changed or visible");
            body.maleObject.SetActive(true); body.femaleObject.SetActive(false);
            menu.RefreshAppearanceTexts();
            Check(body.CurrentEyebrowID == "eyebrow10" && menu.facialHairStyleText.text.Contains("Beard_03"), result, "Male appearance not preserved");
            var hairCatalog = body.hairStyleCatalog; body.hairStyleCatalog = null;
            menu.SelectHairStyle(Move(MoveDirection.Left));
            Check(body.CurrentHairID == "none" && menu.hairStyleText.text.Contains("No_Hair"), result, "Missing hair catalog fallback");
            body.hairStyleCatalog = hairCatalog;
            body.hairStyleCatalog.hairStyleOptions.Add(null);
            try { menu.SelectHairStyle(Move(MoveDirection.Left)); }
            finally { body.hairStyleCatalog.hairStyleOptions.RemoveAt(body.hairStyleCatalog.hairStyleOptions.Count - 1); }
            result.passed = true;
        }
        catch (Exception e) { result.error = e.ToString(); Debug.LogException(e); }
        finally
        {
            if (uiRoot != null) PrefabUtility.UnloadPrefabContents(uiRoot);
            if (playerRoot != null) PrefabUtility.UnloadPrefabContents(playerRoot);
            File.WriteAllText(report, JsonUtility.ToJson(result, true));
            Debug.Log("Appearance validation: " + result.passed + " (" + result.checks + " checks)");
        }
    }
}
