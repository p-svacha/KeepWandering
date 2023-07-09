using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Loc_Lake : Location
{
    public override string Name => "Lake";
    public override LocationType Type => LocationType.Lake;
    public override SpriteRenderer Sprite => ResourceManager.Singleton.WoodsBackground;
    public override bool IsPassable => false;

    protected override TileBase CreateBaseTextureTile()
    {
        return TileGenerator.CreateTileFromTexture(ResourceManager.Singleton.LakeTexture);
    }
}
