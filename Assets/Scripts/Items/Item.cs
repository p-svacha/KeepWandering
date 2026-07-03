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

    private void HighlightWound(Wound wound)
    {
        wound.SetHightlighted(true);
    }
    private void UnhightlightWound(Wound wound)
    {
        wound.SetHightlighted(false);
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

    // Interactions
    public bool CanInteract => GetInteractionOptions().Count > 0;
    public List<InteractionOption> GetInteractionOptions()
    {
        List<InteractionOption> options = new List<InteractionOption>();
        if (!IsPlayerOwned) return options; // todo. allow interactions of non-player items (i.e. trader)

        // Options by item attributes (eat, drink, etc.)
        if (Def.IsConsumable) options.Add(new InteractionOption(Def.ConsumptionType.ConsumptionVerb.CapitalizeFirst(), () => Game.ConsumeItem(this)));
        if (Def.CanTendWounds)
        {
            foreach (Wound wound in Game.Player.TendableWounds)
            {
                options.Add(new InteractionOption($"Tend {wound.Def.Label}", () => Game.TendWound(wound, this), onHoverStartAction: () => HighlightWound(wound), onHoverEndAction: () => UnhightlightWound(wound)));
            }
        }
        if (Def.CanTreatInfections)
        {
            foreach (Wound wound in Game.Player.TreatableWounds)
            {
                options.Add(new InteractionOption($"Treat {wound.Def.Label}", () => Game.TreatWound(wound, this), onHoverStartAction: () => HighlightWound(wound), onHoverEndAction: () => UnhightlightWound(wound)));
            }
        }
        

        /*
        // Item-specific options
        if (Def == ItemDefOf.NutSnack && Game.Player.HasParrot) allOptions.Add(new InteractionOption("Feed to Parrot", () => Game.FeedParrot(this, Def.OnEatNutrition)));
        */

        return options;
    }

    #endregion
}
