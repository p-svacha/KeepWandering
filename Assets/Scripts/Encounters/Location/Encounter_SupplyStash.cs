using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputManagerEntry;

public class Encounter_SupplyStash : LocationEncounter
{
    enum ContainerType
    {
        Backpack,
        Box,
        Crate,
    }
    private Dictionary<ContainerType, float> ContainerTypeWeights = new Dictionary<ContainerType, float>()
    {
        { ContainerType.Backpack, 1 },
        { ContainerType.Box, 1 },
        { ContainerType.Crate, 1 },
    };
    private ContainerType containerType;

    enum ContainerState
    {
        Closed,
        Gone,
        Open,
    }

    private ContainerState state;

    protected override void OnInitialize()
    {
        containerType = ContainerTypeWeights.GetWeightedRandomElement();
        state = ContainerState.Closed;
    }

    private string ContainerLabel => containerType switch
    {
        ContainerType.Backpack => "backpack",
        ContainerType.Box => "locked box",
        ContainerType.Crate => "buried crate",
        _ => throw new System.Exception("Invalid container type")
    };

    protected override string OnStart()
    {
        if (IsFirstVisit)
        {
            string text = containerType switch
            {
                ContainerType.Backpack => "You find an backpack leaning behind a rock.",
                ContainerType.Box => "You find a small metal box sitting half-hidden under debris.",
                ContainerType.Crate => "You find a crate half buried in the ground.",
            };
            text += "  It looks like someone tried to hide it there.";
            return text;
        }
        else
        {
            // Chance that container is gone.
            if (state == ContainerState.Closed && Random.value < 0.35f) state = ContainerState.Gone;

            // Chance that container is looted
            if (state == ContainerState.Closed && Random.value < 0.35f) state = ContainerState.Open;

            string text = "";
            if (state == ContainerState.Gone)
            {
                text += $"The {ContainerLabel} is gone. It looks like someone else found it.";
            }
            else
            {
                text += $"The {ContainerLabel} is still here.";
                if (state == ContainerState.Open)
                {
                    text += " However, it has already been looted.";
                }
            }
            return text;
        }
    }

    protected override void RefreshSprites()
    {
        SetEncounterSpriteVisibility("Backpack", containerType == ContainerType.Backpack);
        SetEncounterSpriteVisibility("Box", containerType == ContainerType.Box);
        SetEncounterSpriteVisibility("Crate", containerType == ContainerType.Crate);
        SetSprite(containerType.ToString(), $"{containerType}_{state}");
    }

    protected override List<EncounterOption> GetOptions()
    {
        if (state == ContainerState.Gone || state == ContainerState.Open) return new List<EncounterOption>();

        // still lootable
        List<EncounterOption> options = new List<EncounterOption>();

        if (containerType == ContainerType.Backpack)
        {
            options.Add(GetOpenBackpackOption());
        }
        if (containerType == ContainerType.Box)
        {
            options.Add(GetBreakOpenBoxOption());
            options.Add(GetPickBoxLockOption());
        }
        if (containerType == ContainerType.Crate)
        {
            options.Add(GetDigUpCrateOption());
        }

        return options;
    }

    protected override bool IsMoveOnOptionAvailable() => true;

    #region Options

    private EncounterOption GetOpenBackpackOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Open backpack",
            Description = "Open the backpack and take everything inside.",
            Action = OpenBackpack,
        };
    }
    private string OpenBackpack()
    {
        BiomeLootTable.AddItemsToInventory(min: 1, max: 3);
        state = ContainerState.Open;

        return "You open the backpack and take everything that's inside.";
    }


    private EncounterOption GetBreakOpenBoxOption()
    {
        return new SkillCheckOption()
        {
            Text = "Break open",
            Description = "Try to force open the box by breaking the lock",
            Action = BreakOpenBox,
            Difficulty = 50,
            RelevantStats = new Dictionary<StatDef, int>()
            {
                { StatDefOf.Strength, 2 },
                { StatDefOf.Dexterity, 1 },
            },
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    Tag = ItemTagDefOf.PryingTool,
                },
            },
            OncePerDay = true,
        };
    }
    private string BreakOpenBox(OptionOutcomeDef outcome)
    {
        if (outcome.SuccessLevel == SuccessLevel.CriticalSuccess)
        {
            Game.ModifyDexterity(+1);
            BiomeLootTable.AddItemsToInventory(min: 1, max: 3);
            state = ContainerState.Open;
            return "The lock pops right off.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            BiomeLootTable.AddItemsToInventory(min: 1, max: 3);
            state = ContainerState.Open;

            return "You get it open.";
        }
        if (outcome.SuccessLevel == SuccessLevel.PartialSuccess)
        {
            BiomeLootTable.AddItemsToInventory(1);
            state = ContainerState.Open;
            return "You break it open successfully, but everything except one iteam breaks in the process.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            Game.ModifyMorale(-1);
            return "The lock holds. You can't get it open.";
        }
        if (outcome.SuccessLevel == SuccessLevel.CriticalFailure)
        {
            Game.ModifyMorale(-1);
            Game.ApplyRandomWound(source: "Breaking box lock");
            return "You slip and hurt yourself while trying to break it. The lock holds.";
        }
        throw new InvalidOutcomeException();
    }

    private EncounterOption GetPickBoxLockOption()
    {
        return new SkillCheckOption()
        {
            Text = "Pick lock",
            Description = "Try to pick the lock on the box",
            Action = PickBoxLock,
            Difficulty = 60,
            RelevantStats = new Dictionary<StatDef, int>()
            {
                { StatDefOf.Dexterity, 3 },
            },
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    Tag = ItemTagDefOf.Lockpicking,
                },
            },
            OncePerDay = true,
        };
    }
    private string PickBoxLock(OptionOutcomeDef outcome)
    {
        if (outcome.SuccessLevel == SuccessLevel.CriticalSuccess)
        {
            Game.ModifyDexterity(+1);
            BiomeLootTable.AddItemsToInventory(min: 1, max: 3);
            state = ContainerState.Open;
            return "You pick the lock with ease.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            BiomeLootTable.AddItemsToInventory(min: 1, max: 3);
            state = ContainerState.Open;
            return "You successfully pick the lock.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            return "You fail to pick the lock.";
        }
        if (outcome.SuccessLevel == SuccessLevel.CriticalFailure)
        {
            Game.ModifyMorale(-2);
            return "You fail at picking the lock and get very frustrated.";
        }
        throw new InvalidOutcomeException();
    }


    private EncounterOption GetDigUpCrateOption()
    {
        return new SkillCheckOption()
        {
            Text = "Dig up",
            Description = "Dig out the buried cache. It'll take some effort.",
            Action = DigUpCrate,
            Difficulty = 45,
            RelevantStats = new Dictionary<StatDef, int>()
            {
                { StatDefOf.Strength, 2 },
            },
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    Tag = ItemTagDefOf.Digging,
                },
            },
            OncePerDay = true,
        };
    }
    private string DigUpCrate(OptionOutcomeDef outcome)
    {
        if (outcome.SuccessLevel == SuccessLevel.CriticalSuccess)
        {
            Game.ModifyStrength(+1);
            BiomeLootTable.AddItemsToInventory(min: 2, max: 4);
            state = ContainerState.Open;
            return "You unearth the whole cache quickly. There's more here than you expected.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            BiomeLootTable.AddItemsToInventory(min: 1, max: 3);
            state = ContainerState.Open;
            return "You successfully dig up the crate and take its contents.";
        }
        if (outcome.SuccessLevel == SuccessLevel.PartialSuccess)
        {
            BiomeLootTable.AddItemsToInventory(1);
            state = ContainerState.Open;
            return "You dig up the crate, but it takes a lot of effort and you only manage to salvage one item.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            Game.ModifyMorale(-1);
            return "You fail to dig up the crate.";
        }
        if (outcome.SuccessLevel == SuccessLevel.CriticalFailure)
        {
            Game.ModifyMorale(-1);
            Game.ApplyRandomWound(source: "Digging up crate");
            return "You hurt yourself while trying to dig it out. You fail to find anything.";
        }
        throw new InvalidOutcomeException();
    }

    #endregion
}
