using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Companion : MonoBehaviour
{
    protected Game Game;
    public abstract string Name { get; }
    public List<StatusEffect> StatusEffects = new List<StatusEffect>();

    public void Init(Game game)
    {
        Game = game;
        gameObject.SetActive(true);
        OnInit();
    }
    protected abstract void OnInit();
    public abstract void OnEndDay(Game game, MorningReport morningReport);

    /// <summary>
    /// Updates all status effects and and visuals (sprites on player) representing them.
    /// This function does NOT change anything, it just sets the status effects according to the current player state.
    /// </summary>
    public abstract void UpdateStatusEffects();

    /// <summary>
    /// Returns the modified version of a loot table taking in account the current location.
    /// </summary>
    protected LootTable GetLocationLootTable(LootTable table)
    {
        return table.Union(Game.CurrentLocation.LootTable);
    }
}
