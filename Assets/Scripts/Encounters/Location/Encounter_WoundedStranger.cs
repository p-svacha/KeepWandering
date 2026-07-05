using System.Collections.Generic;
using UnityEngine;

public class Encounter_WoundedStranger : LocationEncounter
{
    private const string GRATITUDE_TEXT = "They look at you with gratitude. 'I owe you. What can I do?'";
    enum StrangerState
    {
        Weary,
        Grateful,
        Robbed,
        Gone,
    }
    private StrangerState state = StrangerState.Weary;

    private List<ItemDef> items = new List<ItemDef>();

    private bool isKnowledgeExtracted;
    

    protected override void OnInitialize()
    {
        state = StrangerState.Weary;
        
        int numItems = Random.Range(1, 2 + 1);
        for (int i = 0; i < numItems; i++) items.Add(GetBiomeAlteredLootTable(LootTables.Civilian).Resolve());
    }

    protected override string OnStart()
    {
        if (IsFirstVisit)
        {
            return "You find someone slumped on the ground, breathing heavily. They look up at you with wary eyes.";
        }
        else
        {
            // Chance for stranger to be gone
            if (state != StrangerState.Gone)
            {
                float goneChance = DaysSinceLastVisit * 0.1f;
                if (Random.value < goneChance) state = StrangerState.Gone;
            }
            if (state == StrangerState.Robbed) state = StrangerState.Gone;

            if (state == StrangerState.Grateful) return "The stranger you helped is still here. They look a little better.";
            else if (state == StrangerState.Gone) return "The person you encountered earlier is no longer here.";
            else if (state == StrangerState.Weary) return "The person you encountered earlier is still here, looking weary.";
            else throw new System.Exception("Invalid state");
        }
    }

    protected override void RefreshSprites()
    {
        SetEncounterSpriteVisibility("Stranger", state != StrangerState.Gone);
        if (state != StrangerState.Gone) SetSprite("Stranger", $"{state}");
    }

    protected override List<EncounterOption> GetOptions()
    {
        if (state == StrangerState.Weary)
        {
            return new List<EncounterOption>()
            {
                GetHelpOption(),
                GetTalkOption(),
                GetRobOption()
            };
        }
        else if (state == StrangerState.Grateful)
        {
            var options = new List<EncounterOption>();
            if (!isKnowledgeExtracted) options.Add(GetAskForInformationOption());
            options.Add(GetAskForItemsOption());
            if (items.Count > 0) options.Add(GetTradeOption());
            return options;
        }
        else
        {
            return new List<EncounterOption>();
        }
    }
    protected override bool IsMoveOnOptionAvailable() => true;


    private string GainStrangerKnowledge()
    {
        isKnowledgeExtracted = true;

        float rng = Random.value;
        if (rng < 0.5f) // 50%: partial rumour
        {
            string rumourText = Game.LearnPartialRumour();
            if (rumourText != null)
                return "They share something they heard." + rumourText;

            return "They don't seem to know anything useful.";
        }
        else if (rng < 0.8f) // 30%: full rumour
        {
            string rumourText = Game.LearnRumour();
            if (rumourText != null)
                return "They tell you about something interesting nearby." + rumourText;

            return "They don't seem to know anything useful.";
        }
        else // 20%: nothing
        {
            return "They don't seem to know anything useful.";
        }
    }

    #region Options

    private EncounterOption GetHelpOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Help",
            Description = "Tend to their wounds. They might be grateful.",
            Action = Help,
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    CustomItemSet = ItemSet.MedicalItems,
                    CustomItemSetName = "Medical",
                    IsRequired = true,
                    IsDestroyingItem = true,
                }
            }
        };
    }
    private string Help()
    {
        state = StrangerState.Grateful;
        Game.ModifyMorale(+2);
        return $"You tend to their wounds as best you can. {GRATITUDE_TEXT}";
    }

    private EncounterOption GetTalkOption()
    {
        return new SkillCheckOption()
        {
            Text = "Talk",
            Description = "Try to get them talking. See what they know.",
            Action = Talk,
            Difficulty = 45,
            RelevantStats = new Dictionary<StatDef, int>()
            {
                { StatDefOf.Social, 3 },
            },
            CanCriticallyFail = false,
            OnceEver = true,
        };
    }
    private string Talk(OptionOutcomeDef outcome)
    {
        if (outcome.SuccessLevel == SuccessLevel.CriticalSuccess)
        {
            state = StrangerState.Grateful;
            Game.ModifyMorale(+1);
            return $"They open up quickly. They seem relieved to have someone to talk to. {GRATITUDE_TEXT}";
        }
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            string knowledgeText = GainStrangerKnowledge();
            return $"They talk, but cautiously. {knowledgeText}";
        }
        if (outcome.SuccessLevel == SuccessLevel.PartialSuccess)
        {
            Game.RevealRandomNearbyLocationEncounter();
            isKnowledgeExtracted = true;
            return "They talk a little, but seem hesitant to share much.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            return "They turn away. They don't want to talk.";
        }
        throw new InvalidOutcomeException();
    }

    private EncounterOption GetRobOption()
    {
        return new SkillCheckOption()
        {
            Text = "Rob",
            Description = "They're in no position to stop you. Take what they have.",
            Action = Rob,
            Difficulty = 25,
            RelevantStats = new Dictionary<StatDef, int>()
            {
                { StatDefOf.Strength, 2 },
                { StatDefOf.Social, 1 }
            },
            CanCriticallySucceed = false,
            CanPartiallySucceed = false,
        };
    }
    private string Rob(OptionOutcomeDef outcome)
    {
        state = StrangerState.Robbed;

        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            Game.ModifyMorale(-3);
            Game.AddNewItemsToInventory(items);
            items.Clear();
            return "You take their things. They don't resist.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            Game.ModifyMorale(-3);
            Game.ApplyRandomDamage(2f);
            return "They fight back harder than expected.";
        }
        if (outcome.SuccessLevel == SuccessLevel.CriticalFailure)
        {
            Game.ModifyMorale(-4);
            Game.ApplyCutDamage(3f);
            Game.RemoveRandomItemFromInventory();
            state = StrangerState.Gone;

            return "They pull a hidden knife. You weren't expecting that. They take something and crawl away.";
        }
        throw new InvalidOutcomeException();
    }


    private EncounterOption GetAskForInformationOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Ask what they know",
            Description = "Maybe they've heard or seen something useful.",
            Action = AskForInformation,
        };
    }
    private string AskForInformation()
    {
        string knowledgeText = GainStrangerKnowledge();
        return $"{knowledgeText}";
    }

    private EncounterOption GetAskForItemsOption()
    {
        return new SkillCheckOption()
        {
            Text = "Ask for supplies",
            Description = "See if they can spare anything.",
            Action = AskForItems,
            Difficulty = 35,
            RelevantStats = new Dictionary<StatDef, int>()
            {
                { StatDefOf.Social, 3 }
            },
            CanCriticallySucceed = false,
            CanCriticallyFail = false,
            OnceEver = true,
        };
    }
    private string AskForItems(OptionOutcomeDef outcome)
    {
        if (outcome.SuccessLevel == SuccessLevel.Success)
        {
            Game.AddNewItemToInventory(items[0]);
            items.RemoveAt(0);
            return "'It's not much, but take it.' They hand you something.";
        }
        if (outcome.SuccessLevel == SuccessLevel.PartialSuccess)
        {
            LootTables.Trash.AddItemToInventory();
            return "'I really can't spare much...' They hesitantly offer something small.";
        }
        if (outcome.SuccessLevel == SuccessLevel.Failure)
        {
            return "'I'm sorry, I need everything I have just to survive.' They look away.";
        }
        throw new InvalidOutcomeException();
    }

    private EncounterOption GetTradeOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Trade",
            Description = "Offer to exchange items.",
            Action = Trade,
        };
    }
    private string Trade()
    {
        return InitiateTrade("They seem interested in trading.", items);
    }

    #endregion
}
