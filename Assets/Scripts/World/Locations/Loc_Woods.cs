using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Loc_Woods : Location
{
    public override string Name => "Woods";
    public override LocationType Type => LocationType.Woods;
    public override SpriteRenderer Sprite => ResourceManager.Singleton.WoodsBackground;
    public override bool IsPassable => true;

    protected override TileBase CreateBaseTextureTile()
    {
        return TileGenerator.CreateTileFromTexture(ResourceManager.Singleton.WoodsTexture);
    }
}
