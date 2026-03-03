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
    public GameObject DescriptionBox;
    public TextMeshProUGUI DescriptionText;

    [Header("Prefabs")]
    public UI_StatusEffect StatusEffectPrefab;

    public void Init(PlayerCharacter player)
    {
        HelperFunctions.DestroyAllChildredImmediately(gameObject, skipElements: 2);

        Source = player;

        TitleText.text = "Health Report (You)";
        foreach (HealthCondition condition in Source.ActiveHealthConditions)
        {
            UI_StatusEffect display = Instantiate(StatusEffectPrefab, transform);
            display.Init(this, condition);
        }

        HideDescriptionBox();
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

    public void ShowDescriptionBox(HealthCondition condition)
    {
        DescriptionBox.gameObject.SetActive(true);
        DescriptionText.text = condition.GetReportDescription();
    }

    public void HideDescriptionBox()
    {
        DescriptionBox.gameObject.SetActive(false);
    }
}
