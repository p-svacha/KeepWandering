using System.Collections.Generic;
using UnityEngine;

public static class StatDefs
{
    public static List<StatDef> Defs = new List<StatDef>()
    {
        new StatDef()
        {
            DefName = "Combat",
            Label = "combat",
            Description = "Affects combat options, such as fighting, defending, using weapons etc.",
        },

        new StatDef()
        {
            DefName = "Strength",
            Label = "strength",
            Description = "Affects physical options, such as fighting, carrying heavy items, breaking things etc.",
        },

        new StatDef()
        {
            DefName = "Agility",
            Label = "agility",
            Description = "Affects dexterity and speed, such as dodging attacks, running away from threats, picking locks etc.",
        },

        new StatDef()
        {
            DefName = "Intellect",
            Label = "intellect",
            Description = "Affects mental options, such as crafting, repairing, using complex items etc.",
        },

        new StatDef()
        {
            DefName = "Charisma",
            Label = "charisma",
            Description = "Affects social options, such as persuading NPCs, recruiting companions, intimidating threats etc.",
        },

        new StatDef()
        {
            DefName = "Dexterity",
            Label = "dexterity",
            Description = "Affects fine motor skills, such as picking locks, disarming traps, using small items etc.",
        },

        new StatDef()
        {
            DefName = "Perception",
            Label = "perception",
            Description = "Affects awareness of surroundings, such as noticing hidden threats, finding hidden items, tracking etc.",
        },

        new StatDef()
        {
            DefName = "Morale",
            Label = "morale",
            Description = "Your general mental state. Acts as a small offset to all skill checks.",
        }
    };
}
