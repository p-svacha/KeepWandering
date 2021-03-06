using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Companion : MonoBehaviour
{
    public List<StatusEffect> StatusEffects = new List<StatusEffect>();

    public void Init()
    {
        gameObject.SetActive(true);
        OnInit();
    }
    protected abstract void OnInit();
    public abstract void OnEndDay(Game game, MorningReport morningReport);
    public abstract void UpdateStatusEffects();
}
