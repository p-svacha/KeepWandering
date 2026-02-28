using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;


/// <summary>
/// Class used to dynamically load materials from resources on demand. All materials are cached after the first load.
/// </summary>
public static class ResourceManager_New
{
    // Global colors
    public static readonly Color Color_Text_White = new Color(1f, 1f, 1f);
    public static readonly Color Color_Text_Green = new Color(0.47f, 0.70f, 0.45f);
    public static readonly Color Color_Text_Red = new Color(0.70f, 0.47f, 0.45f);
    public static readonly Color ERROR_COLOR = new Color(1f, 0.4f, 0.7f);

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
        // cached
        if (CachedSprites.TryGetValue(resourcePath, out Sprite obj)) return obj;

        // not yet cached
        Sprite loadedSprite = Resources.Load<Sprite>(resourcePath);
        if (loadedSprite == null) throw new System.Exception($"Failed to load Sprite {resourcePath}.");
        CachedSprites.Add(resourcePath, loadedSprite);
        return loadedSprite;
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
        CachedAudioClips.Clear();
        CachedVideoClips.Clear();
        CachedTiles.Clear();
    }
}