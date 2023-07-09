using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Loc_City : Location
{
    public override string Name => "City";
    public override LocationType Type => LocationType.City;
    public override SpriteRenderer Sprite => ResourceManager.Singleton.CityBackground;
    public override bool IsPassable => true;

    protected override TileBase CreateBaseTextureTile()
    {
        return TileGenerator.CreateTileFromTexture(ResourceManager.Singleton.CityTexture);
    }
}
