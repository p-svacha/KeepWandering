using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Loc_Farmland : Location
{
    public override string Name => "Farmland";
    public override LocationType Type => LocationType.Farmland;
    public override SpriteRenderer Sprite => ResourceManager.Singleton.FarmlandBackground;
    public override bool IsPassable => true;


    protected override TileBase CreateBaseTextureTile()
    {
        return TileGenerator.CreateTileFromTexture(ResourceManager.Singleton.FarmlandTexture);
    }
}
