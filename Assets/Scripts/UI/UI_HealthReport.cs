using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_HealthReport : MonoBehaviour
{
    public PlayerCharacter Source { get; private set; }

    [Header("Elements")]
    public TextMeshProUGUI TitleText;

    [Header("Prefabs")]
    public UI_StatusEffect StatusEffectPrefab;

    public void Init(PlayerCharacter player)
    {
        HelperFunctions.DestroyAllChildredImmediately(gameObject, skipElements: 3);

        TitleText.text = "Status Effects";

        foreach (HealthCondition condition in player.HealthConditions)
        {
            if(!condition.ActiveStage.IsVisible) continue; // Don't display invisible conditions
            UI_StatusEffect display = Instantiate(StatusEffectPrefab, transform);
            display.Init(this, condition);
        }
    }
}
