using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat_Fighting : Stat
{
    public override StatId Id => StatId.Fighting;
    public override string Name => "Fighting";
    public override string Description => "How good you are at fighting.";

    public Stat_Fighting(Game game) : base(game) { }

    public override List<StatModifier> GetModifiers()
    {
        List<StatModifier> modifiers = new List<StatModifier>();

        // Get best weapon
        Item bestWeapon = null;
        foreach(Item item in Game.Inventory)
        {
            if (item.IsWeapon && (bestWeapon == null || item.WeaponStrength > bestWeapon.WeaponStrength))
                bestWeapon = item;
        }
        if (bestWeapon != null) modifiers.Add(new StatModifier(bestWeapon.Name, bestWeapon.WeaponStrength));

        return modifiers;
    }
}
