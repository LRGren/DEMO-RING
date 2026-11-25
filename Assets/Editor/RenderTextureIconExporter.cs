using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

internal static class RenderTextureIconExporter
{
    private const string MenuPath = "Assets/Export Render Texture as Icon PNG";

    [MenuItem(MenuPath, true)]
    private static bool CanExport()
    {
        return Selection.activeObject is RenderTexture source && source.dimension == TextureDimension.Tex2D;
    }

    [MenuItem(MenuPath)]
    private static void Export()
    {
        var source = (RenderTexture)Selection.activeObject;
        if (!source.IsCreated())
        {
            EditorUtility.DisplayDialog("Export Icon", "Render the camera output to this texture before exporting.", "OK");
            return;
        }

        var previousTarget = RenderTexture.active;
        var previousSrgbWrite = GL.sRGBWrite;
        var buffer = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        Texture2D pixels = null;
        byte[] png;
        try
        {
            GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear && buffer.sRGB;
            Graphics.Blit(source, buffer);
            RenderTexture.active = buffer;
            pixels = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            pixels.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);
            png = pixels.EncodeToPNG();
        }
        finally
        {
            RenderTexture.active = previousTarget;
            GL.sRGBWrite = previousSrgbWrite;
            RenderTexture.ReleaseTemporary(buffer);
            if (pixels != null)
                Object.DestroyImmediate(pixels);
        }

        var path = EditorUtility.SaveFilePanelInProject("Save Icon PNG", source.name, "png", "Choose a location for the icon.");
        if (string.IsNullOrEmpty(path))
            return;

        File.WriteAllBytes(path, png);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();

        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Sprite>(path));
    }
}
