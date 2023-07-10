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

    private LootTable _LootTable = new LootTable(
        new(ItemType.NutSnack, 12),
        new(ItemType.Beans, 10),
        new(ItemType.WaterBottle, 10),
        new(ItemType.Bandage, 8),
        new(ItemType.Antibiotics, 8),
        new(ItemType.MedicalKit, 5),
        new(ItemType.Coin, 5),
        new(ItemType.Knife, 6)
        );
    public override LootTable LootTable => _LootTable;

    protected override TileBase CreateBaseTextureTile()
    {
        return TileGenerator.CreateTileFromTexture(ResourceManager.Singleton.CityTexture);
    }
}
