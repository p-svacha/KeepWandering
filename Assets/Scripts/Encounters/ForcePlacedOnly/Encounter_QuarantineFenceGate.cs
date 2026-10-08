using System.Collections.Generic;
using System.Linq;

public class Encounter_QuarantineFenceGate : LocationEncounter
{
    // Constants
    private LootTable DEMAND_TABLE = new LootTable
    {
        { ItemDefOf.Beer, Rarity.Common },
        { ItemDefOf.Chocolate, Rarity.Common },
        { ItemDefOf.Cigarettes, Rarity.Common },
        { ItemDefOf.Lighter, Rarity.Common },
        { ItemDefOf.Knife, Rarity.Common },
        { ItemDefOf.Bedroll, Rarity.Common },
        { ItemDefOf.Antibiotics, Rarity.Common },
    };
    public enum EncounterPhase
    {
        Initial,
        AskedToOpen,
        BribeDemanded,
        GateOpen,
    }

    // State
    public EncounterPhase Phase { get; private set; }
    public OptionOutcomeDef SweetTalkOutcome { get; private set; }
    public List<ItemDef> Demands { get; private set; }



    protected override void OnInitialize()
    {
        // Initial state
        Phase = EncounterPhase.Initial;
        Demands = new List<ItemDef>();
    }

    protected override string OnStart()
    {
        string baseText = "You arrive at the locked gate that leads out of the quarantine zone.";

        if (Phase == EncounterPhase.Initial) return baseText + " There is a guard looking to be responsible for opening the gate.";
        if (Phase == EncounterPhase.AskedToOpen) return baseText + " The guard seems to remember you.";
        if (Phase == EncounterPhase.BribeDemanded) return baseText + " The guard calls down: \"Got my things?\"";
        if (Phase == EncounterPhase.GateOpen) return "You arrive at the open gate that leads out of the quarantine zone.";

        throw new System.Exception("Invalid encounter phase: " + Phase);
    }

    protected override void GetOptions(List<EncounterOption> options)
    {
        if (Phase == EncounterPhase.Initial) options.Add(GetAskToLeaveOption());
        if (Phase == EncounterPhase.AskedToOpen) options.Add(GetSweekTalkOption());
        if (Phase == EncounterPhase.AskedToOpen) options.Add(GetBribeOption());
        if (Phase == EncounterPhase.BribeDemanded) options.AddRange(GetDeliverBribeOptions());
        if (Phase == EncounterPhase.BribeDemanded && Demands.Count == 0) options.Add(GetAskToOpenGateAfterBribeOption());
        if (Phase == EncounterPhase.GateOpen) options.Add(GetGoThroughGateOption());

        options.Add(GetMoveOnOption());
    }

    protected override void RefreshSprites()
    {
        SetObjectVisibility("Bucket", Phase == EncounterPhase.BribeDemanded);
        SetObjectVisibility("BucketRope", Phase == EncounterPhase.BribeDemanded);
        SetObjectVisibility("Gate", Phase != EncounterPhase.GateOpen);
        SetObjectVisibility("GateOpen", Phase == EncounterPhase.GateOpen);
    }

    /// <summary>
    /// Goes to the BribeDemanded phase and generates a list of demands for the player to meet.
    /// </summary>
    private void DemandBribe(int numDemands)
    {
        Demands = new List<ItemDef>();
        for (int i = 0; i < numDemands; i++) Demands.Add(ItemDefOf.Coin);
        for (int i = 0; i < numDemands; i++) Demands.Add(DEMAND_TABLE.Resolve());

        foreach (ItemDef item in Demands)
        {
            Game.StartQuest(QuestDefOf.DeliverGuardBribe, location: Tile, relatedItem: item);
        }

        Phase = EncounterPhase.BribeDemanded;
    }

    #region Options

    private FixedOutcomeOption GetAskToLeaveOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Ask to Leave",
            Description = "Ask the guard to open the gate.",
            Sprite = GetSprite("Guard"),
            Action = AskToLeave,
        };
    }
    private string AskToLeave()
    {
        Phase = EncounterPhase.AskedToOpen;
        return "\"Sorry, I'm not allowed to let anyone without a VIP pass out, those are the orders.\"";
    }

    private SkillCheckOption GetSweekTalkOption()
    {
        return new SkillCheckOption()
        {
            Text = "Sweet Talk",
            Description = "Try to sweet talk the guard into opening the gate.",
            Sprite = GetSprite("Guard"),
            BaseDifficulty = 60,
            RelevantStats = new Dictionary<StatDef, int>
            {
                { StatDefOf.Social, 5 },
            },
            CanPartiallySucceed = false,
            CanCriticallySucceed = false,
            CanCriticallyFail = false,
            Action = SweetTalk,
            OnceEver = true,
        };
    }
    private string SweetTalk(OptionOutcomeDef outcome)
    {
        SweetTalkOutcome = outcome;

        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            return "\"You seem like a nice person, but orders are orders, I'm sorry.\"";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            return "\"Not sure what you are trying to do here, but it's not going to work. Bugger off.\"";
        }

        throw new OutcomeNotHandledException(outcome);
    }

    private SkillCheckOption GetBribeOption()
    {
        List<DifficultyModifier> difficultyModifiers = new List<DifficultyModifier>();
        if (SweetTalkOutcome != null)
        {
            if (SweetTalkOutcome.SuccessLevel == SuccessLevel.Success)
                difficultyModifiers.Add(new DifficultyModifier("Sweet Talk", -30));
            if (SweetTalkOutcome.SuccessLevel == SuccessLevel.Failure)
                difficultyModifiers.Add(new DifficultyModifier("Failed Sweet Talk", +20));
        }


        return new SkillCheckOption()
        {
            Text = "Bribe",
            Description = "Offer a bribe to the guard to open the gate.",
            Sprite = GetSprite("Guard"),
            BaseDifficulty = 40,
            RelevantStats =
            {
                { StatDefOf.Social, 2 },
            },
            ItemSlots =
            {
                new ItemSlot()
                {
                    Item = ItemDefOf.Coin,
                    IsRequired = true,
                    IsDestroyingItem = true,
                }
            },
            FixedDifficultyModifiers = difficultyModifiers,
            CanPartiallySucceed = false,
            Action = Bribe,
            OncePerDay = true,
        };
    }
    private string Bribe(OptionOutcomeDef outcome)
    {
        if (outcome.SuccessLevel == SuccessLevel.CriticalSuccess)
        {
            DemandBribe(numDemands: 2);
            return $"\"Alright you got me, but I'm gonna need a little more. Put {Demands.ToNaturalLanguage()} in the bucket down there and I will open the gate for you.\"";
        }
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            DemandBribe(numDemands: 3);
            return $"\"Well I guess there is SOMETHING we can do, but I will need more than that. Put {Demands.ToNaturalLanguage()} in the bucket down there and I will open the gate for you.\"";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            return "\"This is not gonna work out.\"";
        }
        if (outcome.SuccessLevel == SuccessLevel.CriticalFailure)
        {
            IncreaseDangerLevelInAreaAlongFence();
            return "\"Don't even try this shit with me or I'm gonna call it in. Bugger off now!\"";
        }
        throw new OutcomeNotHandledException(outcome);
    }

    private List<FixedOutcomeOption> GetDeliverBribeOptions()
    {
        List<FixedOutcomeOption> options = new List<FixedOutcomeOption>();
        foreach(ItemDef demandedItem in Demands)
        {
            options.Add(new FixedOutcomeOption()
            {
                Text = $"Deliver {demandedItem.Label}",
                Description = $"Put the {demandedItem.Label} in the bucket for the guard.",
                Sprite = GetSprite("Bucket"),
                Action = DeliverDemandedItem,
                ItemSlots =
                {
                    new ItemSlot()
                    {
                        Item = demandedItem,
                        IsRequired = true,
                        IsDestroyingItem = true,
                    }
                }
            });
        }
        return options;
    }
    private string DeliverDemandedItem()
    {
        AudioManager.PlaySound("MetalClanks");
        Item deliveredItem = ItemUsedInOption;
        Demands.Remove(deliveredItem.Def);
        Game.CompleteQuest(Game.ActiveQuests.First(q => q.Def == QuestDefOf.DeliverGuardBribe && q.RelatedItem == deliveredItem.Def));

        return $"You put the {deliveredItem.Def.LabelSingular} in the bucket. The guard gives you a nod of approval.";
    }

    private FixedOutcomeOption GetAskToOpenGateAfterBribeOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Ask to Open Gate",
            Description = "Now that you have delivered all requested items, ask the guard to open the gate again.",
            Sprite = GetSprite("Guard"),
            Action = AskToOpenGateAfterBribe,
        };
    }
    private string AskToOpenGateAfterBribe()
    {
        AudioManager.PlaySound("DoorOpen_02");
        Phase = EncounterPhase.GateOpen;
        return "\"Alright, you have delivered everything I asked for, I will open the gate for you.\"";
    }

    private FixedOutcomeOption GetGoThroughGateOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Go Through Gate",
            Description = "Go through the gate to leave the quarantine zone.",
            Sprite = GetSprite("GateOpen"),
            Action = GoThroughGate,
        };
    }
    private string GoThroughGate()
    {
        Game.WinGame("You walk through the official gate after bribing the guard.");
        return null;
    }

    #endregion

    /// <summary>
    /// Helper function to increase the danger level of all tiles in a radius of 3 around the encounter location that are part of the quarantine fence.
    /// </summary>
    private void IncreaseDangerLevelInAreaAlongFence()
    {
        List<WorldMapTile> targetTiles = Game.CurrentPosition.GetTilesInHexRadius(3, includeSelf: true).Where(tile => tile.IsQuarantineFence).ToList();
        foreach(WorldMapTile tile in targetTiles)
        {
            Game.ModifyTileDangerLevel(tile, +1);
        }
    }
}
