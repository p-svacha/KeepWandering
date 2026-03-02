using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Item
{
    public int Id { get; private set; }
    public Game Game { get; private set; }
    public ItemDef Def { get; private set; }
    public bool IsPlayerOwned { get; private set; }


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

    private void HighlightWound(Wound wound)
    {
        wound.SetHightlighted(true);
    }
    private void UnhightlightWound(Wound wound)
    {
        wound.SetHightlighted(false);
    }


    #region Getters
    public string Label => Def.Label;
    public string LabelCap => Label.CapitalizeFirst();
    public string LabelCapWord => Label.CapitalizeEachWord();
    public string Description => Def.Description;
    public Sprite Sprite => Def.Sprite;

    public bool CanInteract => GetInteractionOptions().Count > 0;
    public List<InteractionOption> GetInteractionOptions()
    {
        List<InteractionOption> options = new List<InteractionOption>();
        if (!IsPlayerOwned) return options; // todo. allow interactions of non-player items (i.e. trader)

        // Options by item attributes (eat, drink, etc.)
        if (Game.CurrentEventStep == null)
        {
            if (Def.IsEdible) options.Add(new InteractionOption("Eat", () => Game.EatItem(this)));
            if (Def.IsDrinkable) options.Add(new InteractionOption("Drink", () => Game.DrinkItem(this)));
            if (Def.CanTendWounds)
            {
                foreach (Wound wound in Game.Player.TendableWounds)
                {
                    options.Add(new InteractionOption($"Tend {wound.LabelCapWord}", () => Game.TendInjury(wound, this), onHoverStartAction: () => HighlightWound(wound), onHoverEndAction: () => UnhightlightWound(wound)));
                }
            }
            if (Def.CanHealInfections)
            {
                foreach (Wound wound in Game.Player.InfectedWounds)
                {
                    options.Add(new InteractionOption($"Heal {wound.LabelCapWord} Infection", () => Game.HealInfection(wound, this), onHoverStartAction: () => HighlightWound(wound), onHoverEndAction: () => UnhightlightWound(wound)));
                }
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
