using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("General")]
    public string Name;
    public string Description;
    public ItemType Type;

    [Header("Food")]
    public bool IsEdible;
    public float OnEatNutrition;

    [Header("Drink")]
    public bool IsDrinkable;
    public float OnDrinkHydration;

    [Header("Wounds")]
    public bool CanTendWounds;
    public bool CanHealInfections;

    [Header("Game")]
    public Game Game;
    public bool IsOwned;

    [Header("Visual")]
    public bool ForceGlow;

    public void Init(Game game)
    {
        Game = game;
    }

    public bool CanInteract()
    {
        return GetInteractionOptions().Count > 0;
    }

    public List<ItemInteractionOption> GetInteractionOptions()
    {
        List<ItemInteractionOption> allOptions = new List<ItemInteractionOption>();

        // Options by item attributes (eat, drink, etc.)
        if(IsOwned && (Game.CurrentEvent == null || Game.CurrentEvent.ItemActionsAllowed))
        {
            if (IsEdible) allOptions.Add(new ItemInteractionOption("Eat", () => Game.EatItem(this)));
            if (IsDrinkable) allOptions.Add(new ItemInteractionOption("Drink", () => Game.DrinkItem(this)));
            if (CanTendWounds)
                foreach (Wound wound in Game.Player.Wounds.Where(x => !x.IsTended))
                    allOptions.Add(new ItemInteractionOption("Tend " + HelperFunctions.GetEnumDescription(wound.Type) + " Wound", () => Game.TendWound(wound, this), onHoverStartAction: () => HighlightWound(wound), onHoverEndAction: () => UnhightlightWound(wound)));
            if (CanHealInfections)
                foreach (Wound wound in Game.Player.Wounds.Where(x => x.InfectionStage != InfectionStage.None))
                    allOptions.Add(new ItemInteractionOption("Heal " + HelperFunctions.GetEnumDescription(wound.Type) + " Wound Infection", () => Game.HealInfection(wound, this), onHoverStartAction: () => HighlightWound(wound), onHoverEndAction: () => UnhightlightWound(wound)));
        }
        

        // Options by event step
        if (Game.CurrentEventStep != null)
        {
            foreach (EventItemOption eventItemOption in Game.CurrentEventStep.EventItemOptions)
            {
                if (eventItemOption.RequiredItemType == Type)
                {
                    allOptions.Add(new ItemInteractionOption(eventItemOption.Text, () => ChoseEventItemOption(eventItemOption)));
                }
            }
        }

        return allOptions;
    }

    private void HighlightWound(Wound wound)
    {
        wound.SetHightlight(true);
        Game.Player.UpdateSpritesAndStatusEffects();
    }
    private void UnhightlightWound(Wound wound)
    {
        wound.SetHightlight(false);
        Game.Player.UpdateSpritesAndStatusEffects();
    }

    private void ChoseEventItemOption(EventItemOption option)
    {
        if (Game.State == GameState.InGame)
        {
            if (option.Action != null) option.Action(Game, this);
            if (option.NextStep != null) Game.DisplayEventStep(option.NextStep); // in certain cases the next step can be null, then the next event step gets handled by the action of the itemoption itself
        }
    }
}