using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Class responsible for creating Tile objects usable in Unity's tilemaps.
/// </summary>
public static class TileFactory
{
    /// <summary>
    /// Creates a simple static Tile from a single sprite.
    /// </summary>
    public static Tile CreateTileFromSprite(Sprite sprite)
    {
        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.colliderType = Tile.ColliderType.None;
        return tile;
    }

    public static Tile CreateTileFromTexture(Texture2D texture, int row, int col, int size, string name)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.name = name;
        tile.sprite = Sprite.Create(texture, new Rect(col * size, texture.height - size - (row * size), size, size), new Vector2(0.5f, 0.5f), size, 1, SpriteMeshType.Tight, Vector4.zero);
        return tile;
    }

    public static Tile CreateTileFromTexture(Texture2D texture)
    {
        Tile tile = ScriptableObject.CreateInstance<Tile>();
        tile.name = texture.name;
        tile.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.width, 1, SpriteMeshType.Tight, Vector4.zero);
        return tile;
    }
}
