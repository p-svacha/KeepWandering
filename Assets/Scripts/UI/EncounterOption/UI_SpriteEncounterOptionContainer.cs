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
    private Button CancelButton;
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
        CancelButton = Instantiate(CancelLockButtonPrefab, transform);
        CancelButton.onClick.AddListener(() => SpriteOptionInteractionManager.ClearLock());

        // Rebuild while active
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

        // Hide initially
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows the container. The cancel-lock button is only meaningful while the sprite is actually locked,
    /// so callers say whether it should be visible (it's hidden when the options are only revealed via Alt).
    /// </summary>
    public void Show(bool showCancelButton)
    {
        gameObject.SetActive(true);
        SetCancelButtonVisible(showCancelButton);
    }

    private void SetCancelButtonVisible(bool visible)
    {
        if (CancelButton == null || CancelButton.gameObject.activeSelf == visible) return;

        CancelButton.gameObject.SetActive(visible);
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>()); // card size changes with the button
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
