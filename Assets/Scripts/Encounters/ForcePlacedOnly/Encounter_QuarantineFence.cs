using System.Collections.Generic;
using UnityEngine;

public class Encounter_QuarantineFence : LocationEncounter
{
    public bool IsElectrified;
    private bool HasHole;

    // Base
    protected override void OnInitialize()
    {
        IsElectrified = true;
    }

    protected override string OnStart()
    {
        string text = "";

        if (IsElectrified) text = "The quarantine fence stands before you. The loud buzzing reminds you of the many volts of electricity running through it. You should probably keep your distance.";
        else text = "The quarantine fence stands before you. It is eerily silent. There is no sound of electric buzzing in the air. This might be your chance to get through.";

        return text;
    }
    protected override void RefreshSprites()
    {
        SetObjectVisibility("BackFence", !HasHole);
        SetObjectVisibility("BackFence_Hole", HasHole);
        SetObjectVisibility("SideFence", true);
    }
    protected override List<EncounterOption> GetOptions()
    {
        List<EncounterOption> options = new List<EncounterOption>();

        if (!HasHole) options.Add(GetCutFenceOption());
        if (HasHole) options.Add(GetGoThroughHoleOption());

        return options;
    }

    protected override bool IsMoveOnOptionAvailable() => true;

    protected override void OnEnd() { }


    // Options
    private EncounterOption GetCutFenceOption()
    {
        return new SkillCheckOption()
        {
            Text = "Cut fence",
            Description = "Use fence cutters to cut through the fence and get to the other side.",
            Action = CutFence,
            Difficulty = 10,
            OncePerDay = true,
            CanCriticallyFail = false,
            CanCriticallySucceed = false,
            CanPartiallySucceed = false,
            ItemSlots = new List<ItemSlot>()
            {
                new ItemSlot()
                {
                    IsRequired = true,
                    Item = ItemDefOf.FenceCutter,
                    IsDestroyingItem = true,
                }
            },
            FixedDifficultyModifiers =
            {
                new ("Electrified", IsElectrified ? 200 : 0)
            }
        };
    }
    private string CutFence(OptionOutcomeDef outcome)
    {
        if (outcome.IsSuccess)
        {
            HasHole = true;
            return "You successfully cut through the fence, creating a hole big enough to get through.";
        }
        else
        {
            if (IsElectrified)
            {
                // todo: get electrocuted
                return "You fail to cut through the fence and get electrocuted in the process.";

            }
            else
            {
                return "You fail to cut through the fence.";
            }
        }
    }

    private EncounterOption GetGoThroughHoleOption()
    {
        return new FixedOutcomeOption()
        {
            Text = "Go through hole",
            Description = "Go through the hole in the fence to get to the other side.",
            Action = GoThroughHole,
        };
    }
    private string GoThroughHole()
    {
        Game.WinGame("You go through the hole in the fence and manage to get to the other side safely. You have successfully escaped the quarantine zone!");
        return null;
    }
}
