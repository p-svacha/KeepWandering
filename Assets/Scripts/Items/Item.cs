using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

public class Item
{
    public int Id { get; private set; }
    public Game Game { get; private set; }
    public ItemDef Def { get; private set; }
    public bool IsPlayerOwned { get; private set; }
    public bool IsDestroyed { get; private set; }
    public int Durability { get; private set; } // Remaining uses


    // Visual
    public ItemRenderer Renderer;

    public Item(Game game, int id, ItemDef def)
    {
        Game = game;
        Id = id;
        Def = def;

        // Create visual item
        GameObject visualItemObj = new GameObject(Label);
        Renderer = visualItemObj.AddComponent<ItemRenderer>();
        Renderer.Init(this);
    }

    public void SetIsPlayerOwned(bool isPlayerOwned)
    {
        IsPlayerOwned = isPlayerOwned;
    }
    public void Destroy()
    {
        IsDestroyed = true;
        GameObject.Destroy(Renderer.gameObject);
    }

    public void SetDurability(int durability)
    {
        if (durability < 0) throw new System.ArgumentException("Durability cannot be negative.");
        Durability = durability;
    }

    public void ModifyDurability(int amount)
    {
        Durability += amount;
    }

    public void Show() => Renderer.Show();
    public void Hide() => Renderer.Hide();
    public void Freeze() => Renderer.Freeze();
    public void Unfreeze() => Renderer.Unfreeze();


    #region Getters
    // General
    public override string ToString() => Label;
    public string Label => Def.Label;
    public string LabelCap => Label.CapitalizeFirst();
    public string LabelCapWord => Label.CapitalizeEachWord();
    public string Description => Def.Description;
    public Sprite Sprite => Def.Sprite;
    public bool HasTag(ItemTagDef tag) => Def.HasTag(tag);
    public bool HasAnyTag => Def.Tags.Count > 0;
    public bool IsConsumable => Def.IsConsumable;
    public bool HasMedicalProperties => Def.HasMedicalProperties;

    /// <summary>
    /// Returns the subtitle text for the tooltip of this item. Can be different things depending on the item type.
    /// </summary>
    public string GetTooltipSubtitle()
    {
        if (HasAnyTag) return $"Remaining Uses: {Durability}";
        if (IsConsumable) return "Consumable Item";
        if (HasMedicalProperties) return "Medical Item";
        return "Special Item";
    }

    #endregion
}
