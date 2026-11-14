using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class AsymmetricSlotValidation
{
    [Serializable] public class Result
    {
        public bool passed;
        public int checks;
        public string error;
    }

    static AsymmetricSlotValidation() { EditorApplication.delayCall += Run; }

    static void Run()
    {
        string output = Path.GetFullPath(Path.Combine(Application.dataPath, "../../work/asymmetric_slot_validation.json"));
        if (File.Exists(output)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.delayCall += Run; return; }

        var result = new Result();
        GameObject root = null;
        MeleeWeaponItem normal = null;
        MeleeWeaponItem asymmetric = null;
        MeleeWeaponItem shield = null;
        try
        {
            Action<bool, string> check = (valid, message) =>
            { if (!valid) throw new InvalidOperationException(message); result.checks++; };
            check((int)WeaponModelSlot.RightHandSlot == 0 && (int)WeaponModelSlot.LeftHandWeaponSlot == 1
                && (int)WeaponModelSlot.LeftHandShieldSlot == 2 && (int)WeaponModelSlot.BackSlot == 3
                && (int)WeaponModelSlot.LeftHandAsymmetricWeaponSlot == 4, "Serialized slot enum values changed.");
            root = new GameObject("Asymmetric slot validation") { hideFlags = HideFlags.HideAndDontSave };
            root.SetActive(false);
            var manager = root.AddComponent<PlayerEquipmentManager>();
            Func<string, WeaponModelSlot, WeaponModelInstantiationSlot> createSlot = (name, type) =>
            {
                var child = new GameObject(name);
                child.transform.SetParent(root.transform, false);
                var slot = child.AddComponent<WeaponModelInstantiationSlot>();
                slot.weaponSlot = type;
                return slot;
            };
            var right = createSlot("Right", WeaponModelSlot.RightHandSlot);
            var left = createSlot("Left", WeaponModelSlot.LeftHandWeaponSlot);
            var shieldSlot = createSlot("Shield", WeaponModelSlot.LeftHandShieldSlot);
            var back = createSlot("Back", WeaponModelSlot.BackSlot);
            var special = createSlot("Asymmetric", WeaponModelSlot.LeftHandAsymmetricWeaponSlot);
            var init = typeof(PlayerEquipmentManager).GetMethod("InitialiseWeaponSlots", BindingFlags.Instance | BindingFlags.NonPublic);
            var choose = typeof(PlayerEquipmentManager).GetMethod("GetLeftHandSlot", BindingFlags.Instance | BindingFlags.NonPublic);
            init.Invoke(manager, null);
            check(manager.rightHandWeaponSlot == right && manager.leftHandWeaponSlot == left
                && manager.leftHandShieldSlot == shieldSlot && manager.backSlot == back
                && manager.leftHandAsymmetricWeaponSlot == special, "Slot discovery failed or overwrote another slot.");

            normal = ScriptableObject.CreateInstance<MeleeWeaponItem>();
            asymmetric = ScriptableObject.CreateInstance<MeleeWeaponItem>();
            asymmetric.useAsymmetricLeftHandSlot = true;
            shield = ScriptableObject.CreateInstance<MeleeWeaponItem>();
            shield.weaponType = WeaponType.Shield;
            shield.useAsymmetricLeftHandSlot = true;
            Func<WeaponItem, WeaponModelInstantiationSlot> getSlot = weapon => (WeaponModelInstantiationSlot)choose.Invoke(manager, new object[] { weapon });
            check(getSlot(normal) == left, "Normal weapon did not select the ordinary left slot.");
            check(getSlot(asymmetric) == special, "Asymmetric weapon did not select the special slot.");
            check(getSlot(shield) == shieldSlot, "Shield priority was lost.");
            manager.leftHandAsymmetricWeaponSlot = null;
            check(getSlot(asymmetric) == left, "Missing special slot did not fall back to ordinary left slot.");
            manager.leftHandAsymmetricWeaponSlot = special;

            var model = new GameObject("Temporary weapon model");
            model.transform.SetParent(root.transform, false);
            special.transform.localPosition = new Vector3(0.1f, 0.2f, 0.3f);
            special.transform.localRotation = Quaternion.Euler(10, 25, 40);
            getSlot(asymmetric).PlaceWeaponIntoSlot(model);
            check(model.transform.parent == special.transform && model.transform.localPosition == Vector3.zero
                && model.transform.localRotation == Quaternion.identity && model.transform.localScale == Vector3.one,
                "Mounting did not preserve the slot-driven placement.");
            // Simulate stowing the model, then use the same selector as UnTwoHandWeapon.
            back.PlaceWeaponIntoSlot(model);
            getSlot(asymmetric).PlaceWeaponIntoSlot(model);
            check(model.transform.parent == special.transform, "Restoring the weapon selected the wrong slot.");
            getSlot(normal).PlaceWeaponIntoSlot(model);
            check(model.transform.parent == left.transform, "Ordinary left weapon mounting changed.");
            getSlot(shield).PlaceWeaponIntoSlot(model);
            check(model.transform.parent == shieldSlot.transform, "Shield mounting changed.");

            var axe = AssetDatabase.LoadAssetAtPath<MeleeWeaponItem>("Assets/Data/Items/Weapons/Melee Weapons/Battle Axe.asset");
            var sword = AssetDatabase.LoadAssetAtPath<MeleeWeaponItem>("Assets/Data/Items/Weapons/Melee Weapons/Long Sword.asset");
            check(axe != null && axe.useAsymmetricLeftHandSlot && getSlot(axe) == special, "Battle Axe serialized flag is not enabled.");
            check(sword != null && !sword.useAsymmetricLeftHandSlot && getSlot(sword) == left, "Existing weapon default changed.");
            result.passed = true;
        }
        catch (Exception ex) { result.error = ex.ToString(); }
        finally
        {
            if (root != null) UnityEngine.Object.DestroyImmediate(root);
            if (normal != null) UnityEngine.Object.DestroyImmediate(normal);
            if (asymmetric != null) UnityEngine.Object.DestroyImmediate(asymmetric);
            if (shield != null) UnityEngine.Object.DestroyImmediate(shield);
            File.WriteAllText(output, JsonUtility.ToJson(result, true));
            Debug.Log("AsymmetricSlotValidation: " + (result.passed ? "PASS" : "FAIL"));
        }
    }
}
