using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Container used to display sprite-bound encounter options. Container is instantiated in Canvas-space to display options bound to a specific SpriteRenderer.
/// </summary>
public class UI_SpriteEncounterOptionContainer : MonoBehaviour
{
    [Header("Prefabs")]
    public UI_EncounterStepOption OptionPrefab;
    public Button CancelLockButtonPrefab;

    // Tracked state
    public Dictionary<EncounterOption, UI_EncounterStepOption> OptionDisplays { get; private set; }

    public void Init(List<EncounterOption> options)
    {
        OptionDisplays = new Dictionary<EncounterOption, UI_EncounterStepOption>();

        HelperFunctions.DestroyAllChildredImmediately(gameObject);

        foreach(EncounterOption option in options)
        {
            UI_EncounterStepOption optionUI = Instantiate(OptionPrefab, transform);
            optionUI.Init(option);
            OptionDisplays.Add(option, optionUI);
        }

        // Add cancel button
        Button cancelButton = Instantiate(CancelLockButtonPrefab, transform);
        cancelButton.onClick.AddListener(() => SpriteOptionInteractionManager.ClearLock());

        // Rebuild while active
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

        // Hide initially
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void SetAnchoredPosition(Vector2 position)
    {
        GetComponent<RectTransform>().anchoredPosition = position;
    }
}
