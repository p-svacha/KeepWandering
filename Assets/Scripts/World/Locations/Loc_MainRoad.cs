using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Loc_MainRoad : Location
{
    public override string Name => "Main Road";
    public override LocationType Type => LocationType.MainRoad;
    public override SpriteRenderer Sprite => ResourceManager.Singleton.MainRoadBackground;

    protected override TileBase CreateBaseTextureTile()
    {
        return TileGenerator.CreateTileFromTexture(ResourceManager.Singleton.MainRoadTexture);
    }
}
