using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the players camp setup in the evening that persists into the night and gets cleaned up in the morning.
/// </summary>
public class Camp : Singleton<Camp>
{
    public Game Game => Game.Instance;

    public bool HasFire { get; private set; }
    public Item Tent { get; private set; }
    public Item Bedroll { get; private set; }
    public List<Item> Traps { get; private set; } = new List<Item>();
    public int NumTrapsUsedToDefendNightAttack { get; private set; }
    public int NumTraps => Traps.Count;

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

    public void AddTrap(Item trap)
    {
        Traps.Add(trap);
    }

    public void UseTrapToDefendNightAttack()
    {
        Item triggeredTrap = Traps[Random.Range(0, Traps.Count)];
        Game.DestroyItem(triggeredTrap);
        Traps.Remove(triggeredTrap);

        NumTrapsUsedToDefendNightAttack++;
    }


    /// <summary>
    /// Cleans up the camp. Remaining items have their durability reduced and are added back to the player's inventory if they are still usable.
    /// </summary>
    public void CleanUpCamp(MorningReport morningReport)
    {
        // Fire
        if (HasFire)
        {
            morningReport.AddNightEvent("The fire has burned out.");
            HasFire = false;
        }

        // Tent
        if (Tent != null)
        {
            Game.ReduceItemDurability(Tent);
            if (!Tent.IsDestroyed)
            {
                Game.AddExistingItemToInventory(Tent);
                morningReport.AddNightEvent("You have packed up the tent.");
            }
            else
            {
                morningReport.AddNightEvent("The tent is no longer usable.");
            }

            Tent = null;
        }

        // Bedroll
        if (Bedroll != null)
        {
            Game.ReduceItemDurability(Bedroll);
            if (!Bedroll.IsDestroyed)
            {
                Game.AddExistingItemToInventory(Bedroll);
                morningReport.AddNightEvent("You have packed up the bedroll.");
            }
            else
            {
                morningReport.AddNightEvent("The bedroll is no longer usable.");
            }

            Bedroll = null;
        }


        // Traps
        List<Item> traps = new List<Item>(Traps); // Make a copy of the list to avoid modifying it while iterating
        foreach (Item trap in traps)
        {
            if (trap.IsDestroyed) throw new System.Exception("Trap is destroyed but still in camp. This should not happen.");

            // Chance for triggering on wildlife
            bool triggeredOnWildlife = Random.value < Game.CurrentPosition.Biome.TrapTriggerChance;
            if (triggeredOnWildlife)
            {
                ItemDef item = LootTables.TrapLoot.Resolve();
                morningReport.AddNightEvent($"A trap was triggered during the night. You found {item.Label}. The trap is now broken.");
                Game.DestroyItem(trap);
                Traps.Remove(trap);
                continue;
            }

            // Chance for breaking
            float breakChance = 0.2f;
            if (Random.value < breakChance)
            {
                morningReport.AddNightEvent($"A trap was triggered during the night but didn't catch anything. The trap is now broken.");
                Game.DestroyItem(trap);
                Traps.Remove(trap);
                continue;
            }
        }

        // Add remaining traps to inventory
        int numTrapsDestroyedByDurability = 0;
        int numTrapsAddedToInventory = 0;
        foreach (Item trap in Traps)
        {
            Game.ReduceItemDurability(trap);
            if (!trap.IsDestroyed)
            {
                Game.AddExistingItemToInventory(trap);
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

        Traps.Clear();
        NumTrapsUsedToDefendNightAttack = 0;
    }
}
