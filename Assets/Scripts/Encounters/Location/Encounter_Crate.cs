using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Encounter_Crate : LocationEncounter
{
    private const int MIN_INVISIBLE_CRATE_ITEMS = 0;
    private const int MAX_INVISIBLE_CRATE_ITEMS = 2;

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

    private bool IsVisibleItemGone;
    private bool IsOpened;
    private bool IsSmashed;
    private bool AllItemsGone;
    private bool AreItemsInsideKnown;

    // Base
    protected override void OnInitialize()
    {
        // Visible crate item
        VisibleCrateItem = GetBiomeAlteredLootTable(ItemTable).GetItem(hidden: true);
        VisibleCrateItem.Renderer.SetPosition(7.5f, -2.5f);
        VisibleCrateItem.Renderer.SetRandomRotation();
        VisibleCrateItem.Renderer.SetSortingOrder(0);

        // Invisible crate items
        int numInvisibleItems = Random.Range(MIN_INVISIBLE_CRATE_ITEMS, MAX_INVISIBLE_CRATE_ITEMS + 1);
        InvisibleCrateItems = new List<Item>();
        for (int i = 0; i < numInvisibleItems; i++)
        {
            Item item = GetBiomeAlteredLootTable(ItemTable).GetItem(hidden: true);
            item.Renderer.SetPosition(3f, 0f);
            item.Renderer.SetRandomRotation();
            InvisibleCrateItems.Add(item);
        }
    }

    protected override string OnStart()
    {
        // Sprites
        if (!IsVisibleItemGone) VisibleCrateItem.Show();

        string text;
        if (IsFirstVisit) text = $"You come across a locked wooden crate. You can see a {VisibleCrateItem.Label} inside through a small hole. Maybe there are more items hidden within.";
        else text = $"You are back at the {(IsSmashed || IsOpened ? "looted" : "locked wooden")} crate.";

        return text;
    }

    protected override List<EncounterOption> GetOptions()
    {
        List<EncounterOption> options = new List<EncounterOption>();
        if (!AllItemsGone)
        {
            if (!IsVisibleItemGone) options.Add(CreateTakeItemOption()); // Take item
            options.Add(CreateOpenCrateOption()); // Open crate
            options.Add(CreateSmashCrateOption()); // Smash crate
            if (!AreItemsInsideKnown) options.Add(CreatePeekOption()); // Peek inside
        }
        return options;
    }
    protected override bool IsMoveOnOptionAvailable() => true;

    protected override void RefreshSprites()
    {
        SetEncounterSpriteVisibility("Crate_Destroyed", IsSmashed);
        SetEncounterSpriteVisibility("Crate_Open", IsOpened);
        SetEncounterSpriteVisibility("Crate", !IsSmashed && !IsOpened);
    }

    protected override void OnEnd()
    {
        if (!IsVisibleItemGone) VisibleCrateItem.Hide();
    }

    private void TakeVisibleItem()
    {
        if (IsVisibleItemGone) return;

        Game.AddExistingItemToInventory(VisibleCrateItem);
        IsVisibleItemGone = true;
    }
    private void TakeAllInvisibleItems()
    {
        if (AllItemsGone) return;

        foreach (var item in InvisibleCrateItems) Game.AddExistingItemToInventory(item);
        InvisibleCrateItems.Clear();
        AreItemsInsideKnown = true;
        AllItemsGone = true;
    }
  

    private EncounterOption CreateTakeItemOption()
    {
        return new SkillCheckOption()
        {
            Text = $"Take {VisibleCrateItem.Label}",
            Description = $"Try to squeeze the {VisibleCrateItem.Label} through the hole.",
            Difficulty = 50,
            CanCriticallyFail = false,
            OncePerDay = true,
            RelevantStats = new Dictionary<StatDef, int>()
            {
                { StatDefOf.Dexterity, 2 },
            },
            Action = TakeItem,
            Sprite = VisibleCrateItem.Renderer.gameObject,
        };
    }
    private string TakeItem(OptionOutcomeDef outcome)
    {
        string text = "";

        if (outcome == OptionOutcomeDefOf.CriticalSuccess) // Take all items
        {
            text = $"You successfully maneuver the {VisibleCrateItem.Label} through the hole.";
            TakeVisibleItem();

            
            if (InvisibleCrateItems.Count > 0)
            {
                string itemText = $"{InvisibleCrateItems[0].Label}";
                foreach (var item in InvisibleCrateItems.Skip(1)) itemText += $" and {item.Label}";
                text += $" You also manage to take out the {itemText}.";
            }
            else
            {
                text += " You also manage to see that there are no more items in the crate.";
            }
            TakeAllInvisibleItems();
        }

        if (outcome == OptionOutcomeDefOf.Success)
        {
            text = $"You successfully maneuver the {VisibleCrateItem.Label} through the hole.";
            TakeVisibleItem();
        }

        if (outcome == OptionOutcomeDefOf.PartialSuccess)
        {
            text = $"You manage to get the {VisibleCrateItem.Label} through the hole, but scratch yourself in the process.";
            TakeVisibleItem();
            Game.ApplyCutWound();
        }

        if (outcome == OptionOutcomeDefOf.Failure)
        {
            text = $"You fail to get the {VisibleCrateItem.Label} through the hole and hurt yourself in the process.";
            Game.ApplyCutWound();
        }

        return text;
    }

    private EncounterOption CreateSmashCrateOption()
    {
        return new SkillCheckOption()
        {
            Text = "Smash",
            Description = "Try to destroy the crate to get its content. This might destroy some items inside.",
            Difficulty = 70,
            OncePerDay = true,
            RelevantStats = new Dictionary<StatDef, int>()
            {
                { StatDefOf.Strength, 3 }
            },
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    Tag = ItemTagDefOf.Weapon,
                }
            },
            Action = SmashCrate,
        };
    }
    private string SmashCrate(OptionOutcomeDef outcome)
    {
        string text = "";

        if (outcome.SuccessLevel >= SuccessLevel.Success)
        {
            text = "You smash the crate open, without damaging any of its content.";
            TakeVisibleItem();
            TakeAllInvisibleItems();
            IsSmashed = true;

            if (outcome == OptionOutcomeDefOf.CriticalSuccess)
            {
                text += " The act of destruction has permanently increased your strength.";
                Game.ModifyStatBaseValue(StatDefOf.Strength, 1);
            }
        }
        if (outcome == OptionOutcomeDefOf.PartialSuccess)
        {
            text = "You successfully smash the crate into pieces, unfortunately including all of its content.";
            if (!IsVisibleItemGone)
            {
                Game.DestroyItem(VisibleCrateItem);
                IsVisibleItemGone = true;
            }
            IsSmashed = true;

            foreach (var item in InvisibleCrateItems) Game.DestroyItem(item);
            InvisibleCrateItems.Clear();
            AllItemsGone = true;
            AreItemsInsideKnown = true;
        }
        if (outcome == OptionOutcomeDefOf.Failure)
        {
            text = "You fail to smash the crate open, and hurt yourself in the process.";
            Game.ApplyCutWound();
        }
        if (outcome == OptionOutcomeDefOf.CriticalFailure)
        {
            text = "You fail to smash the crate open, and cut yourself. You feel weak, your morale decreases.";
            Game.ApplyCutWound();
            Game.ModifyStatBaseValue(StatDefOf.Morale, -1);
        }

        return text;
    }


    private EncounterOption CreateOpenCrateOption()
    {
        return new SkillCheckOption()
        {
            Text = "Pry Open",
            Description = "Pry open the crate using a crowbar.",
            Difficulty = 20,
            OncePerDay = true,
            CanCriticallySucceed = false,
            CanCriticallyFail = false,
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    IsRequired = true,
                    Tag = ItemTagDefOf.PryingTool,
                }
            },
            Action = OpenCrate,
        };
    }
    private string OpenCrate(OptionOutcomeDef outcome)
    {
        string text = "";
        Item usedItem = Game.ItemUsedInSelectedOption;

        if (outcome == OptionOutcomeDefOf.Success)
        {
            text = $"You pry open the crate using the {usedItem.Label} and take everything inside.";
            TakeVisibleItem();
            TakeAllInvisibleItems();
            IsOpened = true;
        }
        if (outcome == OptionOutcomeDefOf.PartialSuccess)
        {
            text = $"You manage to pry open the crate using the {usedItem.Label}. The {usedItem.Label} breaks in the process.";
            TakeVisibleItem();
            TakeAllInvisibleItems();
            IsOpened = true;
            Game.DestroyOwnedItem(usedItem);
        }
        if (outcome == OptionOutcomeDefOf.Failure)
        {
            text = $"You fail to pry open the crate using the {usedItem.Label}. The {usedItem.Label} breaks in the process.";
            Game.DestroyOwnedItem(usedItem);
        }

        return text;
    }

    private EncounterOption CreatePeekOption()
    {
        return new SkillCheckOption()
        {
            Text = "Peek inside",
            Description = "Try to peek inside the crate to see if there are more items hidden within.",
            Difficulty = 30,
            OncePerDay = true,
            CanCriticallySucceed = false,
            CanPartiallySucceed = false,
            RelevantStats = new Dictionary<StatDef, int>()
            {
                { StatDefOf.Survival, 1 },
            },
            Action = Peek,
        };
    }
    private string Peek(OptionOutcomeDef outcome)
    {
        string text = "";

        if (outcome == OptionOutcomeDefOf.Success)
        {
            text = $"You manage to identify that there's {InvisibleCrateItems.ToNaturalLanguage()} left inside the crate.";
            AreItemsInsideKnown = true;
        }
        if (outcome == OptionOutcomeDefOf.Failure)
        {
            text = "You fail to spot anything.";
        }
        if (outcome == OptionOutcomeDefOf.CriticalFailure)
        {
            text = "While peeking through the hole, something bit you!";
            Game.ApplyCutWound();
        }

        return text;
    }

}
