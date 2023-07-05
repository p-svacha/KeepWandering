using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Companion : MonoBehaviour
{
    public abstract string Name { get; }
    public List<StatusEffect> StatusEffects = new List<StatusEffect>();

    public void Init()
    {
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
}
