using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the players camp setup in the evening that persists into the night and gets cleaned up in the morning.
/// <br/>The camp stores a reference to the items that were used to set up the camp (tent, bedroll, traps) and whether a fire was made. In the morning, exact copies of the items with reduced durability will be added back to the player's inventory if they are still usable.
/// </summary>
public class Camp
{
    public static Camp Instance;

    public const int BEDROLL_MORALE_BONUS = +3;
    public const int TENT_MORALE_BONUS = +2;
    public const int FIRE_MORALE_BONUS = +1;

    public Game Game => Game.Instance;

    public bool HasFire { get; private set; }
    public Item Tent { get; private set; }
    public bool HasTent => Tent != null;
    public Item Bedroll { get; private set; }
    public bool HasBedroll => Bedroll != null;
    public Item Trap1 { get; private set; }
    public Item Trap2 { get; private set; }
    public Item Trap3 { get; private set; }
    public List<Item> GetTraps() => new List<Item>() { Trap1, Trap2, Trap3 }.FindAll(t => t != null);
    public int NumTrapsUsedToDefendNightAttack { get; private set; }
    public int NumTraps => GetTraps().Count;
    public bool HasTrap => NumTraps > 0;

    public Camp()
    {
        Instance = this;
    }

    public void MakeFire()
    {
        HasFire = true;
    }

    public void SetTent(Item tent)
    {
        Tent = tent;
    }

    public void SetBedroll(Item bedroll)
    {
        Bedroll = bedroll;
    }

    public void AddTrap(int slot, Item trap)
    {
        switch (slot)
        {
            case 1:
                if (Trap1 != null) throw new System.Exception("Trap slot 1 is already occupied.");
                Trap1 = trap;
                break;
            case 2:
                if (Trap2 != null) throw new System.Exception("Trap slot 2 is already occupied.");
                Trap2 = trap;
                break;
            case 3:
                if (Trap3 != null) throw new System.Exception("Trap slot 3 is already occupied.");
                Trap3 = trap;
                break;
            default:
                throw new System.ArgumentOutOfRangeException(nameof(slot), "Invalid trap slot.");
        }
    }

    public void UseTrapToDefendNightAttack()
    {
        Item triggeredTrap = GetTraps().RandomElement();
        RemoveTrap(triggeredTrap);
        NumTrapsUsedToDefendNightAttack++;
    }

    private void RemoveTrap(Item trap)
    {
        if (trap == Trap1) Trap1 = null;
        else if (trap == Trap2) Trap2 = null;
        else if (trap == Trap3) Trap3 = null;
    }


    /// <summary>
    /// Cleans up the camp. Remaining items have their durability reduced and are added back to the player's inventory if they are still usable.
    /// </summary>
    public void CleanUpCamp(MorningReport morningReport)
    {
        // Morale bonus
        if (HasTent || HasBedroll || HasFire)
        {
            List<string> sources = new List<string>();
            if (HasTent) sources.Add("a tent");
            if (HasBedroll) sources.Add("a bedroll");
            if (HasFire) sources.Add("a fire");
            string elements = sources.ToNaturalLanguage();
            string source = $"Slept in a camp with {elements}";

            HC_WellRested wellRestedCondition = (HC_WellRested)Game.ApplyHealthCondition(HealthConditionDefOf.WellRested, source);
            wellRestedCondition.Init(HasTent, HasBedroll, HasFire);
        }

        // Fire
        if (HasFire)
        {
            morningReport.AddNightEvent("The fire has burned out.");
            HasFire = false;

            // 10% chance to find charcoal
            if (Random.value < 0.1f)
            {
                Game.AddNewItemToInventory(ItemDefOf.Charcoal);
                morningReport.AddNightEvent("You found some usable charcoal in the remains of the fire.");
            }
        }

        // Tent
        if (HasTent)
        {
            Item returnedTent = Game.CopyItem(Tent);
            Game.ReduceItemDurability(returnedTent);

            if (!returnedTent.IsDestroyed)
            {
                Game.AddExistingItemToInventory(returnedTent);
                morningReport.AddNightEvent("You have packed up the tent.");
            }
            else
            {
                morningReport.AddNightEvent("The tent is no longer usable.");
            }

            Tent = null;
        }

        // Bedroll
        if (HasBedroll)
        {
            Item returnedBedroll = Game.CopyItem(Bedroll);
            Game.ReduceItemDurability(returnedBedroll);

            if (!returnedBedroll.IsDestroyed)
            {
                Game.AddExistingItemToInventory(returnedBedroll);
                morningReport.AddNightEvent("You have packed up the bedroll.");
            }
            else
            {
                morningReport.AddNightEvent("The bedroll is no longer usable.");
            }

            Bedroll = null;
        }


        // Traps
        List<Item> traps = new List<Item>(GetTraps()); // Make a copy of the list to avoid modifying it while iterating
        foreach (Item trap in traps)
        {
            // Chance for triggering on wildlife
            bool triggeredOnWildlife = Random.value < Game.CurrentPosition.Biome.TrapTriggerChance;
            if (triggeredOnWildlife)
            {
                ItemDef item = LootTables.TrapLoot.Resolve();
                Game.AddNewItemToInventory(item);
                morningReport.AddNightEvent($"A trap was triggered during the night. You found {item.Label}. The trap is now broken.");
                RemoveTrap(trap);
                continue;
            }

            // Chance for breaking
            float breakChance = 0.2f;
            if (Random.value < breakChance)
            {
                morningReport.AddNightEvent($"A trap was triggered during the night but didn't catch anything. The trap is now broken.");
                RemoveTrap(trap);
                continue;
            }
        }

        // Add remaining traps to inventory
        int numTrapsDestroyedByDurability = 0;
        int numTrapsAddedToInventory = 0;
        traps = new List<Item>(GetTraps());
        foreach (Item trap in traps)
        {
            Item returnedTrap = Game.CopyItem(trap);
            Game.ReduceItemDurability(returnedTrap);

            if (!returnedTrap.IsDestroyed)
            {
                Game.AddExistingItemToInventory(returnedTrap);
                numTrapsAddedToInventory++;
            }
            else numTrapsDestroyedByDurability++;
        }

        if (numTrapsDestroyedByDurability > 0)
        {
            string trap = "trap".Pluralize(numTrapsDestroyedByDurability);
            morningReport.AddNightEvent($"{numTrapsDestroyedByDurability} {trap} set during the evening were broken and are no longer usable.");
        }

        if (numTrapsAddedToInventory > 0)
        {
            string trap = "trap".Pluralize(numTrapsAddedToInventory);
            morningReport.AddNightEvent($"{numTrapsAddedToInventory} {trap} set during the evening were not triggered. You collect them.");
        }

        Trap1 = null;
        Trap2 = null;
        Trap3 = null;
        NumTrapsUsedToDefendNightAttack = 0;
    }
}
