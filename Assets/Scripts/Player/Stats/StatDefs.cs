using System.Collections.Generic;
using UnityEngine;

public static class StatDefs
{
    public static List<StatDef> Defs = new List<StatDef>()
    {
        new StatDef("Combat")
        {
            Label = "combat",
            Description = "Affects combat options, such as fighting, defending, using weapons etc.",
        },

        new StatDef("Strength")
        {
            Label = "strength",
            Description = "Affects physical options, such as fighting, carrying heavy items, breaking things etc.",
        },

        new StatDef("Agility")
        {
            Label = "agility",
            Description = "Affects dexterity and speed, such as dodging attacks, running away from threats, picking locks etc.",
        },

        new StatDef("Intelligence")
        {
            Label = "intellect",
            Description = "Affects mental options, such as crafting, repairing, using complex items etc.",
        },

        new StatDef("Charisma")
        {
            Label = "charisma",
            Description = "Affects social options, such as persuading NPCs, recruiting companions, intimidating threats etc.",
        },

        new StatDef("Dexterity")
        {
            Label = "dexterity",
            Description = "Affects fine motor skills, such as picking locks, disarming traps, using small items etc.",
        },

        new StatDef("Perception")
        {
            Label = "perception",
            Description = "Affects awareness of surroundings, such as noticing hidden threats, finding hidden items, tracking etc.",
        },

        new StatDef("Morale")
        {
            Label = "morale",
            Description = "Your general mental state. Acts as a small offset to all skill checks.",
        }
    };
}
