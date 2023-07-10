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

    private LootTable _LootTable = new LootTable(
        new(ItemType.NutSnack, 5),
        new(ItemType.Beans, 12),
        new(ItemType.WaterBottle, 8),
        new(ItemType.Bandage, 2),
        new(ItemType.Antibiotics, 2),
        new(ItemType.MedicalKit, 2),
        new(ItemType.Coin, 2),
        new(ItemType.Knife, 8),
        new(ItemType.Bone, 5)
    );
    public override LootTable LootTable => _LootTable;


    protected override TileBase CreateBaseTextureTile()
    {
        return TileGenerator.CreateTileFromTexture(ResourceManager.Singleton.FarmlandTexture);
    }
}
