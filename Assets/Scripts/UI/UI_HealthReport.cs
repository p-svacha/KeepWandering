using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_HealthReport : MonoBehaviour
{
    [Header("Elements")]
    public TextMeshProUGUI TitleText;

    [Header("Prefabs")]
    public UI_StatusEffect StatusEffectPrefab;

    public void Init(PlayerCharacter player)
    {
        TitleText.text = "Health Report (You)";
        foreach (HealthCondition condition in player.HealthConditions)
        {
            UI_StatusEffect display = Instantiate(StatusEffectPrefab, transform);
            display.Init(condition);
        }
    }

    /*
    public void Init(Companion companion)
    {
        TitleText.text = "Health Report (" + companion.name + ")";
        foreach (StatusEffect statusEffect in companion.StatusEffects)
        {
            UI_StatusEffect display = Instantiate(StatusEffectPrefab, transform);
            display.Init(statusEffect);
        }
    }
    */
}
