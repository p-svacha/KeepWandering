using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;


/// <summary>
/// Class used to dynamically load materials from resources on demand. All materials are cached after the first load.
/// </summary>
public static class ResourceManager
{
    // Global colors
    public static readonly Color Color_Text_Default = Color.black;
    public static readonly Color Color_Text_Positive = new Color(0.18f, 0.60f, 0.18f);
    public static readonly Color Color_Text_VeryPositive = new Color(0.10f, 0.50f, 0.10f);
    public static readonly Color Color_Text_ExtremelyPositive = new Color(0.00f, 0.40f, 0.00f);
    public static readonly Color Color_Text_Negative = new Color(0.80f, 0.20f, 0.20f);
    public static readonly Color Color_Text_VeryNegative = new Color(0.70f, 0.10f, 0.10f);
    public static readonly Color Color_Text_ExtremelyNegative = new Color(0.55f, 0.00f, 0.00f);

    public static readonly Color Color_Button_Default = new Color(1f, 0.81f, 0f);
    public static readonly Color Color_Button_Disabled = new Color(0.5f, 0.5f, 0.5f);
    public static readonly Color Color_Option_Slot = new Color(1f, 0.98f, 0.78f);
    public static readonly Color Color_Option_Slot_Req_Met = new Color(0.59f, 1f, 0.47f);
    public static readonly Color Color_Option_Slot_Req_Unmet = new Color(0.96f, 0.58f, 0.54f);

    public static readonly Color Color_Panel_Highlighted = new Color(1f, 0.91f, 0.53f);

    public static readonly Color Color_Highlight_LowImpact = new Color(1f, 1f, 0f);
    public static readonly Color Color_Highlight_MediumImpact = new Color(1f, 0.7f, 0f);
    public static readonly Color Color_Highlight_HighImpact = new Color(1f, 0.45f, 0f);
    public static readonly Color Color_Highlight_UltimateImpact = new Color(1f, 0f, 0f);

    public static readonly Color Color_Text_Warning = new Color(0.8f, 0.2f, 0.2f);

    public static readonly Color ItemHighlight_UsableColor = new Color(0.90f, 0.61f, 0f);
    public static string WarningText(string text) => $"<color=#{ColorUtility.ToHtmlStringRGB(Color_Text_Warning)}>{text}</color>";

    private static Dictionary<string, Material> CachedMaterials = new Dictionary<string, Material>();
    public static Material LoadMaterial(string resourcePath)
    {
        // cached
        if (CachedMaterials.TryGetValue(resourcePath, out Material mat)) return mat;

        // not yet cached
        Material newMat = Resources.Load<Material>(resourcePath);
        if (newMat == null) throw new System.Exception($"Failed to load material {resourcePath}.");
        CachedMaterials.Add(resourcePath, newMat);
        return newMat;
    }

    private static Dictionary<string, Texture2D> CachedTextures = new Dictionary<string, Texture2D>();
    public static Texture2D LoadTexture(string resourcePath)
    {
        // cached
        if (CachedTextures.TryGetValue(resourcePath, out Texture2D tex)) return tex;

        // not yet cached
        Texture2D newTex = Resources.Load<Texture2D>(resourcePath);
        if (newTex == null) throw new System.Exception($"Failed to load texture {resourcePath}.");
        CachedTextures.Add(resourcePath, newTex);
        return newTex;
    }

    private static Dictionary<string, GameObject> CachedPrefabs = new Dictionary<string, GameObject>();
    public static GameObject LoadPrefab(string resourcePath)
    {
        // cached
        if (CachedPrefabs.TryGetValue(resourcePath, out GameObject obj)) return obj;

        // not yet cached
        GameObject loadedPrefab = Resources.Load<GameObject>(resourcePath);
        if (loadedPrefab == null) throw new System.Exception($"Failed to load GameObject {resourcePath}.");
        CachedPrefabs.Add(resourcePath, loadedPrefab);
        return loadedPrefab;
    }

    private static Dictionary<string, Sprite> CachedSprites = new Dictionary<string, Sprite>();
    public static Sprite LoadSprite(string resourcePath)
    {
        if (CachedSprites.TryGetValue(resourcePath, out Sprite obj)) return obj;

        Sprite loadedSprite = Resources.Load<Sprite>(resourcePath);
        if (loadedSprite == null) throw new System.Exception($"Failed to load Sprite {resourcePath}.");
        CachedSprites.Add(resourcePath, loadedSprite);
        return loadedSprite;
    }

    public static Sprite LoadSpriteFromSheet(string sheetPath, string spriteName)
    {
        string cacheKey = $"{sheetPath}::{spriteName}";
        if (CachedSprites.TryGetValue(cacheKey, out Sprite obj)) return obj;

        Sprite[] allSprites = Resources.LoadAll<Sprite>(sheetPath);
        Sprite found = allSprites.FirstOrDefault(s => s.name == spriteName);
        if (found == null) throw new System.Exception($"Failed to load Sprite '{spriteName}' from sheet {sheetPath}.");

        CachedSprites.Add(cacheKey, found);
        return found;
    }

    private static Dictionary<string, Sprite[]> CachedSpriteSheets = new Dictionary<string, Sprite[]>();
    public static Sprite LoadRandomSprite(string resourcePath)
    {
        if (!CachedSpriteSheets.TryGetValue(resourcePath, out Sprite[] sprites))
        {
            sprites = Resources.LoadAll<Sprite>(resourcePath);
            if (sprites.Length == 0) throw new System.Exception($"No sprites found at {resourcePath}.");
            CachedSpriteSheets.Add(resourcePath, sprites);
        }

        return sprites[Random.Range(0, sprites.Length)];
    }

    private static Dictionary<string, AudioClip> CachedAudioClips = new Dictionary<string, AudioClip>();
    public static AudioClip LoadAudioClip(string resourcePath)
    {
        // cached
        if (CachedAudioClips.TryGetValue(resourcePath, out AudioClip obj)) return obj;

        // not yet cached
        AudioClip loadedAudioClip = Resources.Load<AudioClip>(resourcePath);
        if (loadedAudioClip == null) throw new System.Exception($"Failed to load AudioClip {resourcePath}.");
        CachedAudioClips.Add(resourcePath, loadedAudioClip);
        return loadedAudioClip;
    }

    private static Dictionary<string, UnityEngine.Video.VideoClip> CachedVideoClips = new Dictionary<string, UnityEngine.Video.VideoClip>();
    public static UnityEngine.Video.VideoClip LoadVideoClip(string resourcePath)
    {
        // cached
        if (CachedVideoClips.TryGetValue(resourcePath, out UnityEngine.Video.VideoClip obj)) return obj;

        // not yet cached
        UnityEngine.Video.VideoClip loadedVideoClip = Resources.Load<UnityEngine.Video.VideoClip>(resourcePath);
        if (loadedVideoClip == null) throw new System.Exception($"Failed to load VideoClip {resourcePath}.");
        CachedVideoClips.Add(resourcePath, loadedVideoClip);
        return loadedVideoClip;
    }

    private static Dictionary<string, Tile> CachedTiles = new Dictionary<string, Tile>();
    public static Tile LoadTile(string resourcePath)
    {
        // cached
        if (CachedTiles.TryGetValue(resourcePath, out Tile obj)) return obj;

        // not yet cached
        Tile loadedTile = Resources.Load<Tile>(resourcePath);
        if (loadedTile == null) throw new System.Exception($"Failed to load TileBase {resourcePath}.");
        CachedTiles.Add(resourcePath, loadedTile);
        return loadedTile;
    }

    public static void ClearCache()
    {
        CachedMaterials.Clear();
        CachedTextures.Clear();
        CachedPrefabs.Clear();
        CachedSprites.Clear();
        CachedSpriteSheets.Clear();
        CachedAudioClips.Clear();
        CachedVideoClips.Clear();
        CachedTiles.Clear();
    }
}