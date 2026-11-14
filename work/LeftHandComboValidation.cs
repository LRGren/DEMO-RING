using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class LeftHandComboValidation
{
    [Serializable] public class Result { public bool passed; public int checks; public string error; }
    static LeftHandComboValidation() { EditorApplication.delayCall += Run; }

    static void Run()
    {
        string output = Path.GetFullPath(Path.Combine(Application.dataPath, "../../work/left_hand_combo_validation.json"));
        if (File.Exists(output)) return;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)
        { EditorApplication.delayCall += Run; return; }
        var result = new Result();
        GameObject root = null;
        LightAttackWeaponItemAction action = null;
        MeleeWeaponItem weapon = null;
        try
        {
            Action<bool, string> check = (value, message) =>
            { if (!value) throw new InvalidOperationException(message); result.checks++; };
            root = new GameObject("Left-hand combo validation") { hideFlags = HideFlags.HideAndDontSave };
            root.SetActive(false);
            var player = root.AddComponent<PlayerManager>();
            var network = root.AddComponent<PlayerNetworkManager>();
            var combat = root.AddComponent<PlayerCombatManager>();
            var animator = root.AddComponent<ComboValidationAnimator>();
            animator.testPlayer = player;
            player.playerNetworkManager = network;
            player.playerCombatManager = combat;
            player.playerAnimatorManager = animator;
            typeof(PlayerCombatManager).GetField("player", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(combat, player);

            foreach (bool right in new[] { false, true })
            foreach (bool twoHanded in new[] { false, true })
            {
                network.isUsingRightHand.Value = right;
                network.isUsingLeftHand.Value = !right;
                network.isTwoHandingWeapon.Value = twoHanded;
                combat.EnableDoCombo();
                check(combat.canComboWithMainHandWeapon == (right || twoHanded), "Wrong combo window for action-hand/two-hand state.");
                combat.DisableDoCombo();
                check(!combat.canComboWithMainHandWeapon, "Combo window did not close.");
            }

            action = ScriptableObject.CreateInstance<LightAttackWeaponItemAction>();
            weapon = ScriptableObject.CreateInstance<MeleeWeaponItem>();
            var perform = typeof(LightAttackWeaponItemAction).GetMethod("PerformTwoHandLightAttack", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (bool right in new[] { true, false })
            {
                network.isUsingRightHand.Value = right;
                network.isUsingLeftHand.Value = !right;
                network.isTwoHandingWeapon.Value = true;
                network.isTwoHandingRightWeapon.Value = right;
                network.isTwoHandingLeftWeapon.Value = !right;
                combat.DisableDoCombo();
                player.isPerformingAction = false;
                perform.Invoke(action, new object[] { player, weapon });
                check(combat.lastAttackAnimation == "TH_Light_Attack_01", "First two-handed attack failed.");
                int calls = animator.attackCalls;
                perform.Invoke(action, new object[] { player, weapon });
                check(animator.attackCalls == calls, "An attack chained before the combo window opened.");
                foreach (string next in new[] { "TH_Light_Attack_02", "TH_Light_Attack_03", "TH_Light_Attack_01" })
                {
                    combat.EnableDoCombo();
                    perform.Invoke(action, new object[] { player, weapon });
                    check(combat.lastAttackAnimation == next, "Two-handed combo stopped on " + (right ? "right" : "left") + " weapon.");
                    combat.DisableDoCombo();
                }
                calls = animator.attackCalls;
                perform.Invoke(action, new object[] { player, weapon });
                check(animator.attackCalls == calls, "Attack chained after combo window closed.");
            }
            result.passed = true;
        }
        catch (Exception ex) { result.error = ex.ToString(); }
        finally
        {
            if (root != null) UnityEngine.Object.DestroyImmediate(root);
            if (action != null) UnityEngine.Object.DestroyImmediate(action);
            if (weapon != null) UnityEngine.Object.DestroyImmediate(weapon);
            File.WriteAllText(output, JsonUtility.ToJson(result, true));
            Debug.Log("LeftHandComboValidation: " + (result.passed ? "PASS" : "FAIL"));
        }
    }
}
