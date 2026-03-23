using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Encounter_HomeOfR : LocationEncounter
{
    private bool IsIntroduced;
    private Item FenceCutter;

    protected override void OnInitialize()
    {
        FenceCutter = GenerateEncounterItem(
            ItemDefOf.FenceCutter,
            position: new Vector2(7.41f, -0.72f),
            sortingOrder: 21
        );
    }

    protected override string OnStart()
    {
        if (IsFirstVisit) return "A person hails you through the window of a building. There's someone else in the back, lying down. They look like they might be hurt.";
        else return "R greets you.";
    }

    protected override void RefreshSprites()
    {
        SetEncounterSpriteVisibility("Building", true);
    }

    protected override List<EncounterOption> GetOptions()
    {
        List<EncounterOption> options = new List<EncounterOption>();

        if (!IsIntroduced) options.Add(GetInitialTalkOption());
        else
        {
            if (Game.IsQuestActive(QuestDefOf.FindR)) options.Add(GetAskAboutFenceOption());
            if (!Game.HasQuestStarted(QuestDefOf.DeliverMedicineToR)) options.Add(GetAskAboutSickPartnerOption());
            if (Game.IsQuestActive(QuestDefOf.DeliverMedicineToR)) options.Add(GetDeliverMedicineOption());
        }

        return options;
    }
    protected override bool IsMoveOnOptionAvailable()
    {
        return true;
    }


    // Options
    private EncounterOption GetInitialTalkOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Talk",
            Description = "Talk to the person and find out what's going on.",
            Action = InitialTalk,
        };
    }
    private string InitialTalk()
    {
        IsIntroduced = true;

        return "The person introduces themselves as R. They say that their partner is sick and getting worse.";
    }


    private EncounterOption GetAskAboutFenceOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Ask about the note",
            Description = "Ask R about the note you found at the radio tower.",
            Action = AskAboutFence,
        };
    }
    private string AskAboutFence()
    {
        Game.CompleteQuest(QuestDefOf.FindR);
        return "R tells you that the note is true. They have both information about the exact location of the weak point and a fence cutter to cut through. But they before they help you, you need to help their partner.";
    }


    private EncounterOption GetAskAboutSickPartnerOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Ask about sick partner",
            Description = "Ask R about their sick partner. Maybe you can help?",
            Action = AskAboutSickPartner,
        };
    }
    private string AskAboutSickPartner()
    {
        Game.StartQuest(QuestDefOf.DeliverMedicineToR, location: Tile);
        return "R tells you that their partner needs something to treat infections. They would reward you with a fence cutter and some information.";
    }


    private EncounterOption GetDeliverMedicineOption()
    {
        List<ItemDef> acceptedItems = DefDatabase<ItemDef>.AllDefs.Where(i => i.CanHealInfections).ToList();
        return new FixedOutcomeOption()
        {
            Text = "Deliver medicine",
            Description = "Hand over the medicine.",
            Action = DeliverMedicine,
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    IsRequired = true,
                    AllowedItems = acceptedItems,
                }
            }
        };
    }
    private string DeliverMedicine()
    {
        Game.AddExistingItemToInventory(FenceCutter);
        FenceCutter = null;
        Game.ModifyStatBaseValue(StatDefOf.Morale, 1);
        Game.StartQuest(QuestDefOf.GoToUnpoweredFence, location: StoryManager.CuttableFenceTile);

        return "R thanks you for the medicine and gives you a fence cutter. They also mark the exact location of the unpowered fence on your map.";
    }
}
