using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class Location
{
    public abstract string Name { get; }
    public abstract LocationType Type { get; }
    public abstract SpriteRenderer Sprite { get; }
    public abstract bool IsPassable { get; }
    public TileBase BaseTextureTile { get; private set; }

    public Location()
    {
        BaseTextureTile = CreateBaseTextureTile();
    }

    protected abstract TileBase CreateBaseTextureTile();

    public override string ToString()
    {
        return Name;
    }
}
