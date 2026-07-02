using System.Collections.Generic;
using UnityEngine;

public class Encounter_CollapsedBuilding : LocationEncounter
{
    enum WireState
    {
        Active,
        Cut,
        Absent,
    }
    enum SurvivorState
    {
        Stuck,
        Freed,
        Dead,
        Absent
    }
    enum RubbleState
    {
        Blocking,
        GapRevealed,
        Cleared,
    }


    private WireState wireState;
    private SurvivorState survivorState;
    private RubbleState rubbleState;
    private int structuralIntegrity;
    private List<Item> visibleItems = new List<Item>();
    private List<Item> invisibleItems = new List<Item>();

    private bool hasSurvivor => survivorState != SurvivorState.Absent;
    private bool isCollapsed => structuralIntegrity <= 0;
    private bool isRubbleCleared => rubbleState == RubbleState.Cleared;

    protected override void OnInitialize()
    {
        // Wires
        float wireChance = 0.7f;
        if (Biome == BiomeDefOf.City) wireChance = 0.9f;
        if (Biome == BiomeDefOf.Woods) wireChance = 0f;
        if (Random.value < wireChance) wireState = WireState.Active;
        else wireState = WireState.Absent;

        // Structural integrity
        int minStartIntegerity = 1;
        int maxStartIntegerity = 3;
        structuralIntegrity = Random.Range(minStartIntegerity, maxStartIntegerity + 1);

        // Items
        LootTable lootTable = GetBiomeAlteredLootTable(LootTables.Building);

        int numVisibleItems = Random.Range(1, 2 + 1);
        for (int i = 0; i < numVisibleItems; i++)
        {
            Vector2 position = i == 0 ? new Vector2(25.09f, 0.16f) : new Vector2(18.66f, -1.12f);
            float rotation = i == 0 ? -13f : 66f;
            visibleItems.Add(GenerateEncounterItem(lootTable.Resolve(), position, rotation, sortingOrder: 1));
        }

        int numInvisibleItems = Random.Range(1, 3 + 1);
        for (int i = 0; i < numInvisibleItems; i++) invisibleItems.Add(GenerateEncounterItem(lootTable.Resolve()));

        // Survivor
        survivorState = Random.value < 0.15f ? SurvivorState.Stuck : SurvivorState.Absent;
    }

    protected override string OnStart()
    {
        foreach (Item item in visibleItems) item.Show();

        if (isCollapsed) return "The collapsed building is nothing but a pile of rubble now.";

        string text = "A building has partially collapsed here. Rubble and broken beams are piled high.";
        if (wireState == WireState.Active) text += " Sparking wires hang from the wreckage.";
        text += " You can see some items caught in the debris.";
        if (hasSurvivor && survivorState != SurvivorState.Dead) text += " You hear a faint voice calling from somewhere inside.";
        text += structuralIntegrity switch
        {
            1 => " The structure looks very unstable.",
            2 => " The structure looks somewhat stable.",
            3 => " The structure looks fairly stable.",
            _ => throw new System.Exception("Invalid structural integrity"),
        };
        return text;
        
    }

    protected override void RefreshSprites()
    {
        SetEncounterSpriteVisibility("Building", true);
        SetEncounterSpriteVisibility("SurvivorStuck", survivorState == SurvivorState.Stuck);
        SetEncounterSpriteVisibility("SurvivorFreed", survivorState == SurvivorState.Freed);
        SetEncounterSpriteVisibility("Wires", wireState != WireState.Absent);
        SetEncounterSpriteVisibility("Rubble", rubbleState != RubbleState.Cleared);

        SetSprite("Building", isCollapsed ? "Collapsed" : "Base");
        SetSprite("Wires", wireState == WireState.Cut ? "WiresCut" : "Wires");
        SetSprite("Rubble", rubbleState == RubbleState.GapRevealed ? "RubbleGap" : "Rubble");
    }

    protected override List<EncounterOption> GetOptions()
    {
        List<EncounterOption> options = new List<EncounterOption>();

        if (isCollapsed) return options;

        if (wireState == WireState.Active) options.Add(GetCutWiresOption());
        if (visibleItems.Count > 0) options.Add(GetGrabItemsOption());
        if (!isRubbleCleared) options.Add(GetClearRubbleOption());
        if (rubbleState == RubbleState.GapRevealed && !isCollapsed) options.Add(GetCrawlInOption());
        if (survivorState == SurvivorState.Stuck) options.Add(GetCallOutOption());
        if (survivorState == SurvivorState.Freed) options.Add(GetTalkOption());

        return options;
    }
    protected override bool IsMoveOnOptionAvailable() => true;

    private void TriggerCollapse()
    {
        structuralIntegrity = 0;
        Game.ModifyMorale(-3);
        DestroyItems(visibleItems);
        DestroyItems(invisibleItems);
        Game.ApplyBruiseDamage(Random.Range(2f, 4f));

        if (survivorState == SurvivorState.Stuck) survivorState = SurvivorState.Dead;
    }

    private string FreeSurvivor()
    {
        survivorState = SurvivorState.Freed;
        Game.ModifyMorale(+2);
        return " A battered person crawls out from the debris, breathing hard. They look at you with gratitude.";
    }

    #region Options

    private EncounterOption GetCutWiresOption()
    {
        return new SkillCheckOption()
        {
            Text = "Cut wires",
            Description = "Try to safely disconnect the sparking wires.",
            Action = CutWires,
            Difficulty = 55,
            RelevantStats = new Dictionary<StatDef, float>()
            {
                { StatDefOf.Dexterity, 2 },
                { StatDefOf.Intelligence, 1 },
            },
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    Tag = ItemTagDefOf.Cutting,
                }
            },
            CanPartiallySucceed = false,
            CanCriticallySucceed = false,
            OncePerDay = true,
        };
    }
    private string CutWires(OptionOutcomeDef outcome)
    {
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            wireState = WireState.Cut;
            return "You carefully isolate and cut the wires. The sparking stops.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            // todo: electrocute (severity = 4)
            return "You grab the wrong wire.";
        }
        if (outcome.SuccessLevel == SuccessLevel.CriticalFailure)
        {
            // todo: electrocute (severity = 8)
            return "The wire whips into you with full current.";
        }
        throw new InvalidOutcomeException(outcome);
    }


    private EncounterOption GetGrabItemsOption()
    {
        Dictionary<string, int> difficultyModifiers = new Dictionary<string, int>();
        if (structuralIntegrity == 1) difficultyModifiers.Add("Very Unstable", +20);
        else if (structuralIntegrity == 2) difficultyModifiers.Add("Unstable", +10);
        if (wireState == WireState.Active) difficultyModifiers.Add("Sparking Wires", +10);

        return new SkillCheckOption()
        {
            Text = "Grab items",
            Description = "Carefully reach in and grab the visible supplies.",
            Action = GrabItems,
            Difficulty = 40,
            FixedDifficultyModifiers = difficultyModifiers,
            RelevantStats = new Dictionary<StatDef, float>()
            {
                { StatDefOf.Dexterity, 2 },
                { StatDefOf.Agility, 1 },
            },
            OncePerDay = true,
        };
    }
    private string GrabItems(OptionOutcomeDef outcome)
    {
        if (outcome == OptionOutcomeDefOf.CriticalSuccess)
        {
            TakeAllItems(visibleItems);
            TakeRandomItem(invisibleItems);

            Game.ModifyMorale(+1);

            return "You deftly pull everything free without disturbing anything. You even find more than you could initially see.";
        }
        if (outcome == OptionOutcomeDefOf.Success)
        {
            TakeAllItems(visibleItems);

            return "You grab the supplies, though some debris shifts.";
        }
        if (outcome == OptionOutcomeDefOf.PartialSuccess)
        {
            TakeRandomItem(visibleItems);
            DestroyItems(visibleItems);

            return "You manage to grab one thing before the rubble shifts and blocks the rest.";
        }
        if (outcome == OptionOutcomeDefOf.Failure)
        {
            Game.ApplyCutWound();
            return "The rubble shifts as you reach in, scraping your arm.";
        }
        if (outcome == OptionOutcomeDefOf.CriticalFailure)
        {
            structuralIntegrity--;
            if (structuralIntegrity == 0)
            {
                TriggerCollapse();
                return "A slab gives way as you reach in. The whole structure violently collapses.";
            }
            else
            {
                Game.ApplyBruiseDamage(2f);
                DestroyItems(visibleItems);
                return "A slab gives way as you reach in. The structure becomes more unstable, the items are destroyed.";
            }
        }
        throw new InvalidOutcomeException(outcome);
    }


    private EncounterOption GetClearRubbleOption()
    {
        string description = hasSurvivor
            ? "Spend time clearing the rubble. You might be able to reach whoever is calling."
            : "Spend time clearing the rubble to get deeper inside.";

        Dictionary<string, int> difficultyModifiers = new Dictionary<string, int>();
        if (structuralIntegrity == 1) difficultyModifiers.Add("Very Unstable", +20);
        else if (structuralIntegrity == 2) difficultyModifiers.Add("Unstable", +10);
        if (wireState == WireState.Active) difficultyModifiers.Add("Sparking Wires", +10);

        return new SkillCheckOption()
        {
            Text = "Clear rubble",
            Description = description,
            Action = ClearRubble,
            Difficulty = 65,
            RelevantStats = new Dictionary<StatDef, float>()
            {
                { StatDefOf.Strength, 2 },
                { StatDefOf.Intelligence, 1 },
            },
            FixedDifficultyModifiers = difficultyModifiers,
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
    private string ClearRubble(OptionOutcomeDef outcome)
    {
        if (outcome == OptionOutcomeDefOf.CriticalSuccess)
        {
            string text = "You find the load-bearing points and clear a clean path. You take everything you find in the rubble.";

            TakeAllItems(visibleItems);
            rubbleState = RubbleState.Cleared;
            TakeAllItems(invisibleItems);
            if (hasSurvivor) text += FreeSurvivor();
            Game.ModifyIntelligence(+1);

            return text;
        }
        if (outcome == OptionOutcomeDefOf.Success)
        {
            string text = "You clear enough rubble to access the interior, finding an item while clearing.";

            TakeAllItems(visibleItems);
            rubbleState = RubbleState.Cleared;
            TakeRandomItem(invisibleItems);
            if (hasSurvivor) text += FreeSurvivor();

            return text;
        }
        if (outcome == OptionOutcomeDefOf.PartialSuccess)
        {
            rubbleState = RubbleState.GapRevealed;

            return "You clear some debris and find a narrow gap, but can't open it fully.";
        }
        if (outcome == OptionOutcomeDefOf.Failure)
        {
            Game.ModifyMorale(-1);
            Game.ApplyBruiseWound();
            // todo: apply exhaustion

            return "The rubble is too heavy and unstable. You exhaust yourself.";
        }
        if (outcome == OptionOutcomeDefOf.CriticalFailure)
        {
            TriggerCollapse();

            return "A support beam snaps, causing the structure to violently collapse.";
        }
        throw new InvalidOutcomeException(outcome);
    }


    private EncounterOption GetCrawlInOption()
    {
        Dictionary<string, int> difficultyModifiers = new Dictionary<string, int>();
        if(structuralIntegrity == 1) difficultyModifiers.Add("Very Unstable", +30);
        else if(structuralIntegrity == 2) difficultyModifiers.Add("Unstable", +15);
        if(wireState == WireState.Active) difficultyModifiers.Add("Sparking Wires", +10);
        if(Game.Player.IsWellFed) difficultyModifiers.Add("Thick Body", +10);
        if(Game.Player.IsVeryHungry) difficultyModifiers.Add("Thin Body", -20);

        return new SkillCheckOption()
        {
            Text = "Crawl in",
            Description = "Squeeze through the narrow gap into the interior. Risky.",
            Action = CrawlIn,
            Difficulty = 55,
            FixedDifficultyModifiers = difficultyModifiers,
            RelevantStats = new Dictionary<StatDef, float>()
            {
                { StatDefOf.Agility, 2 },
                { StatDefOf.Perception, 1 },
            },
            CanCriticallySucceed = false,
            CanPartiallySucceed = false,
            OncePerDay = true,
        };
    }
    private string CrawlIn(OptionOutcomeDef outcome)
    {
        if (wireState == WireState.Active && Random.value < 0.5f) { } // todo: electrocute

        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            string text = "You squeeze through and find supplies in a small pocket of space.";
            TakeAllItems(invisibleItems);
            if (hasSurvivor) text += FreeSurvivor();
            return text;
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            Game.ApplyCutWound();
            return "You get stuck and have to wriggle back out, scraping yourself up.";
        }
        if (outcome.SuccessLevel == SuccessLevel.CriticalFailure)
        {
            TriggerCollapse();
            return "The gap collapses around you as you crawl in, and with it the whole structure.";
        }
        throw new InvalidOutcomeException(outcome);
    }


    private EncounterOption GetCallOutOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Call out",
            Description = "Shout to the person trapped inside.",
            Action = CallOut,
            OnceEver = true,
        };
    }
    private string CallOut()
    {
        return "A voice responds: 'I'm pinned under something. I can't move my legs. Please hurry, I don't know how long this will hold.'";
    }


    private EncounterOption GetTalkOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Talk",
            Description = "Talk to the survivor.",
            Action = Talk,
            OnceEver = true,
        };
    }
    private string Talk()
    {
        float rng = Random.value;
        if (rng < 0.4f)
        {
            string text = "They tell you something they overheard before the building came down.";
            string rumourText = Game.LearnRumour();
            if (rumourText != null) text += rumourText;
            return text;
        }
        else if(rng < 0.7f)
        {
            LootTables.Civilian.AddItemToInventory();
            return "'Take this, I don't need it anymore.'";
        }
        else 
        {
            return "'I wish I could repay you somehow. I've got nothing left.' They limp away.";
        }
    }

    #endregion
}
