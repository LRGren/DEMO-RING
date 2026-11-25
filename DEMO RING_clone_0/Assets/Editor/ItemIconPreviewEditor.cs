using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemIconPreview))]
internal sealed class ItemIconPreviewEditor : Editor
{
    private const string PlayerPrefabPath = "Assets/Data/Prefab/Player/Player.prefab";
    private const string UndoLabel = "Generate Item Preview";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var preview = (ItemIconPreview)target;
        var model = GetModel(preview.item);
        var hasModel = preview.item is ArmorItem || model != null;
        if (preview.item != null && !hasModel)
            EditorGUILayout.HelpBox("This item has no model or spell effect assigned.", MessageType.Info);

        using (new EditorGUI.DisabledScope(!hasModel || EditorApplication.isPlayingOrWillChangePlaymode))
        {
            if (GUILayout.Button("Generate Item Preview"))
                Generate(preview, model);
        }
    }

    private static GameObject GetModel(Item item)
    {
        switch (item)
        {
            case WeaponItem weapon:
                return weapon.weaponModel;
            case RangedProjectileItem projectile:
                return projectile.drawProjectileModel != null ? projectile.drawProjectileModel : projectile.releaseProjectileModel;
            case QuickSlotItem quickSlot:
                return quickSlot.itemModel;
            case SpellItem spell:
                if (spell.spellCastReleaseFX != null)
                    return spell.spellCastReleaseFX;
                if (spell.spellCastWarmUpFX != null)
                    return spell.spellCastWarmUpFX;
                return spell.spellCastReleaseFXFullCharge != null ? spell.spellCastReleaseFXFullCharge : spell.spellChargeFX;
            default:
                return null;
        }
    }

    private static void Generate(ItemIconPreview preview, GameObject model)
    {
        Undo.IncrementCurrentGroup();
        var undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName(UndoLabel);

        try
        {
            var instance = preview.item is ArmorItem armor
                ? GenerateArmor(armor, preview.transform)
                : InstantiateModel(model, preview.transform, preview.item.name);
            Undo.RecordObject(preview, UndoLabel);
            if (preview.generatedPreview != null && preview.generatedPreview.transform.parent == preview.transform)
                Undo.DestroyObjectImmediate(preview.generatedPreview);
            preview.generatedPreview = instance;
            PrefabUtility.RecordPrefabInstancePropertyModifications(preview);
            Undo.CollapseUndoOperations(undoGroup);
            SceneView.RepaintAll();
        }
        catch (Exception exception)
        {
            Undo.RevertAllDownToGroup(undoGroup);
            Debug.LogException(exception, preview);
            EditorUtility.DisplayDialog(UndoLabel, exception.Message, "OK");
        }
    }

    private static GameObject InstantiateModel(GameObject model, Transform parent, string itemName)
    {
        var instance = Instantiate(model, parent, false);
        Undo.RegisterCreatedObjectUndo(instance, UndoLabel);
        Undo.RegisterFullObjectHierarchyUndo(instance, UndoLabel);
        instance.name = itemName;
        instance.transform.localPosition = Vector3.zero;
        // Keep the prefab's authored rotation and scale.
        foreach (var script in instance.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (script != null)
                script.enabled = false;
        }
        instance.SetActive(true);
        return instance;
    }

    private static GameObject GenerateArmor(ArmorItem armor, Transform parent)
    {
        var player = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        if (player == null || armor.equipmentModels == null)
            throw new InvalidOperationException("The player prefab or armor model list is missing.");

        var names = armor.equipmentModels
            .Where(part => part != null && !string.IsNullOrWhiteSpace(part.equipmentModelName))
            .Select(part => part.equipmentModelName)
            .ToHashSet();
        var parts = player.GetComponentsInChildren<SkinnedMeshRenderer>(true)
            .Where(part => part.sharedMesh != null && names.Contains(part.name))
            .ToArray();
        var missing = names.Except(parts.Select(part => part.name)).ToArray();
        if (parts.Length == 0 || missing.Length != 0)
        {
            var message = missing.Length == 0 ? "No armor meshes are configured." : "Missing armor meshes: " + string.Join(", ", missing);
            throw new InvalidOperationException(message);
        }

        var instance = new GameObject(armor.name);
        Undo.RegisterCreatedObjectUndo(instance, UndoLabel);
        Undo.SetTransformParent(instance.transform, parent, UndoLabel);
        Undo.RecordObject(instance.transform, UndoLabel);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        var transforms = new Dictionary<Transform, Transform> { { player.transform, instance.transform } };
        foreach (var part in parts)
        {
            var partTransform = CopyTransform(part.transform, transforms);
            var renderer = Undo.AddComponent<SkinnedMeshRenderer>(partTransform.gameObject);
            var bones = part.bones.Select(bone => CopyTransform(bone, transforms)).ToArray();
            var rootBone = CopyTransform(part.rootBone, transforms);
            Undo.RecordObject(renderer, UndoLabel);
            EditorUtility.CopySerialized(part, renderer);
            renderer.bones = bones;
            renderer.rootBone = rootBone;
            renderer.enabled = true;
            renderer.forceRenderingOff = false;
            renderer.updateWhenOffscreen = true;
        }

        return instance;
    }

    private static Transform CopyTransform(Transform source, Dictionary<Transform, Transform> transforms)
    {
        if (source == null)
            return null;
        if (transforms.TryGetValue(source, out var existing))
            return existing;

        var parent = CopyTransform(source.parent, transforms);
        var copy = new GameObject(source.name).transform;
        Undo.RegisterCreatedObjectUndo(copy.gameObject, UndoLabel);
        Undo.SetTransformParent(copy, parent, UndoLabel);
        Undo.RecordObject(copy, UndoLabel);
        copy.localPosition = source.localPosition;
        copy.localRotation = source.localRotation;
        copy.localScale = source.localScale;
        transforms.Add(source, copy);
        return copy;
    }
}
