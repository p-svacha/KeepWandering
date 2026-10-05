using System;
using UnityEditor;

public class EncounterSpriteImporter : AssetPostprocessor
{
    private const string TargetFolder = "Assets/Resources/Encounters/";

    private void OnPreprocessTexture()
    {
        // Only brand-new assets (no .meta file yet)
        if (!assetImporter.importSettingsMissing)
            return;

        if (!assetPath.StartsWith(TargetFolder, StringComparison.OrdinalIgnoreCase))
            return;

        if (!assetPath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 45f;
    }
}