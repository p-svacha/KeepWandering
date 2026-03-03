using System.Collections.Generic;
using UnityEngine;

public class Encounter_Crate : LocationEncounter
{
    private const int TAKE_ITEM_BASE_DIFFICULTY = 40;

    private static LootTable ItemTable = new LootTable
    {
        { ItemDefOf.Beans, 10 },
        { ItemDefOf.WaterBottle, 10 },
        { ItemDefOf.Bandage, 5 },
        { ItemDefOf.Antibiotics, 5 },
        { ItemDefOf.Bone, 3 },
        { ItemDefOf.Knife, 3 },
        { ItemDefOf.NutSnack, 10 },
        { ItemDefOf.MedicalKit, 1 }
    };

    // Instance
    private Item CrateItem;

    // Base
    protected override void OnEventStart()
    {
        // Sprites
        ShowEventSprite("Crate");

        // Crate item
        CrateItem = GetBiomeLootTable(ItemTable).GetItem();
        CrateItem.Renderer.SetPosition(6f, 0f);
        CrateItem.Renderer.SetRotation(-30f);
    }
    protected override EncounterStep GetInitialStep()
    {
        // Text
        string text = $"You stumble upon a crate that looks to have a {CrateItem.Label} inside.";

        // Options
        List<EncounterStepOption> options = new List<EncounterStepOption>();
        options.Add(CreateTakeItemOption()); // Take item
        options.Add(new FixedOutcomeOption($"Don't take {CrateItem.Label}.", DontTakeItem)); // Don't take item

        return new EncounterStep("You stumble upon a crate that looks to have a " + CrateItem.Label + " inside.", options);
    }

    private SkillCheckOption CreateTakeItemOption()
    {
        int difficulty = TAKE_ITEM_BASE_DIFFICULTY;
        return new SkillCheckOption($"Take {CrateItem.Label}.", difficulty, TakeItem);
    }

    protected override void OnEventEnd()
    {
        if (!CrateItem.IsPlayerOwned) Game.Instance.DestroyItem(CrateItem);
    }

    private EncounterStep TakeItem(SkillCheckOutcome outcome)
    {
        string text = "";

        if (outcome == SkillCheckOutcome.Failure)
        {
            text += $"The {CrateItem.Label} is too difficult to take out of the crate. You cut yourself on a loose nail why trying to get it out.";
            Game.AddCutWound();
        }

        else // (Partial) Success
        {
            Game.AddExistingItemToInventory(CrateItem);
            text = "You reach into the crate and take out the " + CrateItem.Label + ".";
            if (outcome == SkillCheckOutcome.PartialSuccess)
            {
                Game.AddCutWound();
                text += " Upon taking out your hand you scratch yourself on a loose nail.";
            }
        }

        return new EncounterStep(text);
    }
    private EncounterStep DontTakeItem()
    {
        return new EncounterStep("You didn't take the " + CrateItem.Label + ".");
    }
}
