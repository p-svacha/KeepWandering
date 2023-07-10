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

    private LootTable _LootTable = new LootTable(
        new(ItemType.WaterBottle, 20),
        new(ItemType.Coin, 10)
    );
    public override LootTable LootTable => _LootTable;

    protected override TileBase CreateBaseTextureTile()
    {
        return TileGenerator.CreateTileFromTexture(ResourceManager.Singleton.LakeTexture);
    }
}
