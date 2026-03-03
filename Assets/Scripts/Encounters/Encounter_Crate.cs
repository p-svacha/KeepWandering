using System.Collections.Generic;
using UnityEngine;

public class Encounter_Crate : LocationEncounter
{
    private const int MIN_INVISIBLE_CRATE_ITEMS = 0;
    private const int MAX_INVISIBLE_CRATE_ITEMS = 2;

    private const int TAKE_ITEM_BASE_DIFFICULTY = 30;
    private const int SMASH_CRATE_BASE_DIFFICULTY = 70;

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

    // State
    private Item VisibleCrateItem;
    private List<Item> InvisibleCrateItems;
    private bool IsVisibleItemTaken;
    private bool IsSmashed;

    // Base
    protected override void OnInitialize()
    {
        // Visible crate item
        VisibleCrateItem = GetBiomeLootTable(ItemTable).GetItem();
        VisibleCrateItem.Renderer.SetPosition(6f, 0f);
        VisibleCrateItem.Renderer.SetRotation(-30f);

        // Invisible crate items
        int numInvisibleItems = Random.Range(MIN_INVISIBLE_CRATE_ITEMS, MAX_INVISIBLE_CRATE_ITEMS + 1);
        InvisibleCrateItems = new List<Item>();
        for (int i = 0; i < numInvisibleItems; i++)
        {
            Item item = GetBiomeLootTable(ItemTable).GetItem(hidden: true);
            item.Renderer.SetPosition(3f, 0f);
            item.Renderer.SetRandomRotation();
            InvisibleCrateItems.Add(item);
        }

        // Initial state
        IsSmashed = false;
    }

    protected override EncounterStep OnStart()
    {
        // Sprites
        if (!IsVisibleItemTaken) VisibleCrateItem.Show();
        ShowEventSprite("Crate");

        string text;
        if (IsFirstVisit) text = $"You stumble upon a crate with a {VisibleCrateItem.Label} stuck in it. It looks like there could be more stuff inside.";
        else text = $"You are back at the crate.";

        return new EncounterStep(text, GetOptions());
    }

    protected override void OnEnd()
    {
        if (!IsVisibleItemTaken) VisibleCrateItem.Hide();
    }

    private List<EncounterStepOption> GetOptions()
    {
        List<EncounterStepOption> options = new List<EncounterStepOption>();
        if (!IsVisibleItemTaken) options.Add(CreateTakeItemOption()); // Take item
        if (!IsSmashed) options.Add(CreateSmashCrateOption()); // Smash crate
        options.Add(new FixedOutcomeOption($"Ignore", Ignore)); // Ignore
        return options;
    }

    private SkillCheckOption CreateTakeItemOption()
    {
        string text = $"Take {VisibleCrateItem.Label}";
        int difficulty = TAKE_ITEM_BASE_DIFFICULTY;
        Dictionary<StatDef, float> relevantStats = new Dictionary<StatDef, float>()
        {
            { StatDefOf.Dexterity, 1f },
            { StatDefOf.Strength, 1f }
        };
        return new SkillCheckOption(text, difficulty, TakeItem, relevantStats, canPartiallySucceed: true);
    }

    private SkillCheckOption CreateSmashCrateOption()
    {
        string text = "Smash";
        int difficulty = SMASH_CRATE_BASE_DIFFICULTY;
        return new SkillCheckOption(text, difficulty, SmashCrate);
    }


    private EncounterStep TakeItem(OptionOutcomeDef outcome)
    {
        string text = "";

        if (outcome == OptionOutcomeDefOf.Failure)
        {
            text += $"The {VisibleCrateItem.Label} is too difficult to take out of the crate. You cut yourself on a loose nail why trying to get it out.";
            Game.AddCutWound();
        }

        else // (Partial) Success
        {
            Game.AddExistingItemToInventory(VisibleCrateItem);
            IsVisibleItemTaken = true;

            text = "You reach into the crate and take out the " + VisibleCrateItem.Label + ".";
            if (outcome == OptionOutcomeDefOf.PartialSuccess)
            {
                Game.AddCutWound();
                text += " Upon taking out your hand you scratch yourself on a loose nail.";
            }
        }

        return new EncounterStep(text);
    }

    private EncounterStep SmashCrate(OptionOutcomeDef outcome)
    {
        return null;
    }

    private EncounterStep Ignore()
    {
        return new EncounterStep("You didn't take the " + VisibleCrateItem.Label + ".");
    }
}
