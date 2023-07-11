using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Item : MonoBehaviour
{
    private Game Game;

    [Header("General")]
    public string Name;
    public string Description;
    public ItemType Type;
    public bool IsPlayerOwned;

    [Header("Food")]
    public bool IsEdible;
    public float OnEatNutrition;
    public float OnEatHydration;

    [Header("Drink")]
    public bool IsDrinkable;
    public float OnDrinkHydration;

    [Header("Medical")]
    public bool CanTendWounds;
    public bool CanHealInfections;

    [Header("Misc")]
    public int WeaponStrength;

    [Header("Visual")]
    public bool ForceGlow;

    public Sprite Sprite { get; private set; }

    public void Init(Game game)
    {
        Game = game;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        Sprite = GetComponent<SpriteRenderer>().sprite;
    }

    private void HighlightWound(Wound wound)
    {
        wound.SetHightlight(true);
    }
    private void UnhightlightWound(Wound wound)
    {
        wound.SetHightlight(false);
    }

    private void ChoseEventItemOption(EventItemOption option)
    {
        if (Game.State == GameState.InGame)
        {
            if (option.Action != null)
            {
                EventStep nextEventStep = option.Action(this);
                Game.DisplayEventStep(nextEventStep);
            }
        }
    }

    public void Show()
    {
        GetComponent<SpriteRenderer>().enabled = true;  
    }
    public void Hide()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }

    #region Getters

    public bool CanInteract => GetInteractionOptions().Count > 0;
    public List<InteractionOption> GetInteractionOptions()
    {
        List<InteractionOption> allOptions = new List<InteractionOption>();
        if (!IsPlayerOwned) return allOptions; // todo. allow interactions of non-player items (i.e. trader)

        // Options by item attributes (eat, drink, etc.)
        if (Game.CurrentEventStep == null || Game.CurrentEventStep.ItemsAllowed)
        {
            if (IsEdible) allOptions.Add(new InteractionOption("Eat", () => Game.EatItem(this)));
            if (IsDrinkable) allOptions.Add(new InteractionOption("Drink", () => Game.DrinkItem(this)));
            if (CanTendWounds)
                foreach (Wound wound in Game.Player.ActiveWounds.Where(x => !x.IsTended))
                    allOptions.Add(new InteractionOption("Tend " + HelperFunctions.GetEnumDescription(wound.Type) + " Wound", () => Game.TendWound(wound, this), onHoverStartAction: () => HighlightWound(wound), onHoverEndAction: () => UnhightlightWound(wound)));
            if (CanHealInfections)
                foreach (Wound wound in Game.Player.ActiveWounds.Where(x => x.InfectionStage != InfectionStage.None))
                    allOptions.Add(new InteractionOption("Heal " + HelperFunctions.GetEnumDescription(wound.Type) + " Wound Infection", () => Game.HealInfection(wound, this), onHoverStartAction: () => HighlightWound(wound), onHoverEndAction: () => UnhightlightWound(wound)));
        }

        // Item-specific options
        if (Type == ItemType.NutSnack && Game.Player.HasParrot) allOptions.Add(new InteractionOption("Feed to Parrot", () => Game.FeedParrot(this, OnEatNutrition)));

        // Options by event step
        if (Game.CurrentEventStep != null)
        {
            foreach (EventItemOption eventItemOption in Game.CurrentEventStep.EventItemOptions)
            {
                if (eventItemOption.RequiredItemType == Type)
                {
                    allOptions.Add(new InteractionOption(eventItemOption.Text, () => ChoseEventItemOption(eventItemOption)));
                }
            }
        }

        return allOptions;
    }

    public bool IsWeapon => WeaponStrength > 0;

    #endregion
}
