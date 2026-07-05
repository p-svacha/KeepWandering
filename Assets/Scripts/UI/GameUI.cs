using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class GameUI : Singleton<GameUI>
{
    public Game Game { get; private set; }

    [Header("Day Panel")]
    public TextMeshProUGUI DayText;
    public TextMeshProUGUI DayTimeText;
    public TextMeshProUGUI DangerLevelText;
    public UI_TooltipTarget DangerLevelTooltipTarget;

    public Button MapButton;
    public Button SettingsButton;
    public Button DiaryButton;
    public Button CraftingButton;

    [Header("Health Reports")]
    public UI_HealthReport HealthReport;

    [Header("Event Display")]
    public UI_EncounterDisplay EventStepDisplay;

    [Header("Stat Display")]
    public UI_StatPanel StatPanel;

    [Header("Mission Display")]
    public UI_Missions MissionsDisplay;

    [Header("Windows")]
    public UI_DevmodeMenu EscapeMenu;

    [Header("World Map")]
    public UI_WorldMapMenu WorldMapMenu;

    [Header("Tooltip")]
    public const float TOOLTIP_HOVER_TIME = 0.4f;

    [Header("Day Transition")]
    public Image BlackTransitionImage;
    public TextMeshProUGUI BlackTransitionText;
    public const float TRANSITION_HOLD_TIME = 3f;
    public const float TRANSITION_FADE_TIME = 1f;
    private float TransitionTargetTime;
    private float CurrentTransitionTime;
    private BlackTransitionState TransitionState;

    /// <summary>
    /// Called once when the program starts.
    /// </summary>
    public void Init(Game game)
    {
        Game = game;

        StatPanel.Init(Game);
        EscapeMenu.Init(Game);

        BlackTransitionText.text = "Day " + Game.Day;

        // Buttons
        SettingsButton.onClick.AddListener(ToggleEscapeMenu);
        MapButton.onClick.AddListener(ToggleWorldMap);
    }

    private void Update()
    {
        // Black transition
        switch(TransitionState)
        {
            case BlackTransitionState.FadeIn:
                CurrentTransitionTime += Time.deltaTime;
                if (CurrentTransitionTime >= TransitionTargetTime)
                {
                    BlackTransitionImage.color = new Color(0f, 0f, 0f, 1f);
                    BlackTransitionText.color = new Color(1f, 1f, 1f, 1f);
                    TransitionState = BlackTransitionState.Off;
                    Game.OnTransitionFadeInDone();
                }
                else
                {
                    float alpha = CurrentTransitionTime / TransitionTargetTime;
                    BlackTransitionImage.color = new Color(0f, 0f, 0f, alpha);
                    BlackTransitionText.color = new Color(1f, 1f, 1f, alpha);
                }
                break;

            case BlackTransitionState.FadeOut:
                CurrentTransitionTime += Time.deltaTime;
                if (CurrentTransitionTime >= TransitionTargetTime)
                {
                    BlackTransitionImage.color = new Color(0f, 0f, 0f, 0f);
                    BlackTransitionText.color = new Color(1f, 1f, 1f, 0f);
                    TransitionState = BlackTransitionState.Off;
                    Game.OnTransitionFadeOutDone();
                }
                else
                {
                    float alpha = 1 - (CurrentTransitionTime / TransitionTargetTime);
                    BlackTransitionImage.color = new Color(0f, 0f, 0f, alpha);
                    BlackTransitionText.color = new Color(1f, 1f, 1f, alpha);
                }
                break;

            case BlackTransitionState.Hold:
                CurrentTransitionTime += Time.deltaTime;
                if (CurrentTransitionTime >= TransitionTargetTime)
                {
                    TransitionState = BlackTransitionState.Off;
                    Game.OnTransitionHoldDone();
                }
                break;
        }
    }

    #region Stats

    public void RefreshStats()
    {
        StatPanel.Refresh();
    }

    public void HightlightStat(StatDef stat, Color color)
    {
        StatPanel.HightlightStat(stat, color);
    }
    public void UnhighlightStat(StatDef stat)
    {
        StatPanel.UnhighlightStat(stat);
    }
    public void UnhighlightAllStats() => StatPanel.UnhighlightAll();

    #endregion

    #region Refresh

    public void UpdateDayPanel()
    {
        // Danger level
        DangerLevelText.text = Game.CurrentPosition.DangerLevel.Label;
        DangerLevelText.color = Game.CurrentPosition.DangerLevel.Color;
        
        string tooltipTitle = $"Danger Level: {Game.CurrentPosition.DangerLevel.Label}";
        string tooltipDescription = Game.CurrentPosition.DangerLevel.Description;
        tooltipDescription += $"\n\nThe danger level increases after each night or when staying in the same location.";
        DangerLevelTooltipTarget.Init(tooltipTitle, tooltipDescription);
    }

    public void UpdateHealthReports()
    {
        // Display player health report
        HealthReport.Init(Game.Player);
    }

    public void UpdateQuestDisplay()
    {
        MissionsDisplay.UpdateList(Game.ActiveQuests);
    }

    #endregion

    #region Windows

    public void HideAllTooltips()
    {
        UI_SimpleTooltip.Instance.Hide();
        UI_ItemTooltip.Instance.Hide();
        UI_StatTooltip.Instance.Hide();
    }

    public void CloseAllWindows()
    {
        CloseEscapeMenu();
        CloseWorldMap();
        HideAllTooltips();
        UI_ContextMenu.Instance.Hide();
    }

    public void ToggleEscapeMenu()
    {
        if (Game.State != GameState.InGame) return;
        EscapeMenu.gameObject.SetActive(!EscapeMenu.gameObject.activeSelf);
        UI_ContextMenu.Instance.Hide();
        HideAllTooltips();
    }
    public void CloseEscapeMenu()
    {
        EscapeMenu.gameObject.SetActive(false);
        UI_ContextMenu.Instance.Hide();
        HideAllTooltips();
    }

    public void ToggleWorldMap()
    {
        if (Game.State != GameState.InGame) return;
        SetWorldMapVisible(!WorldMapMenu.gameObject.activeSelf);
    }

    public void SetWorldMapVisible(bool visible)
    {
        if (Game.State != GameState.InGame) return;

        WorldMapMenu.gameObject.SetActive(visible);
        Game.WorldMapRenderer.gameObject.SetActive(visible);

        UI_ContextMenu.Instance.Hide();
        HideAllTooltips();
    }

    public void OpenWorldMap(WorldMapTile focusTile = null, Area focusArea = null)
    {
        if (Game.State != GameState.InGame) return;

        WorldMapMenu.gameObject.SetActive(true);
        Game.WorldMapRenderer.gameObject.SetActive(true);
        if (focusTile != null) Game.WorldMapRenderer.FocusTile(focusTile);
        else if (focusArea != null) Game.WorldMapRenderer.FocusArea(focusArea);

        UI_ContextMenu.Instance.Hide();
        HideAllTooltips();
    }
    public void CloseWorldMap()
    {
        WorldMapMenu.gameObject.SetActive(false);
        Game.WorldMapRenderer.gameObject.SetActive(false);
        UI_ContextMenu.Instance.Hide();
        HideAllTooltips();
    }

    #endregion

    #region Transition

    public void FadeInBlackTransition(float timeInSeconds)
    {
        CurrentTransitionTime = 0f;
        TransitionTargetTime = timeInSeconds;
        TransitionState = BlackTransitionState.FadeIn;
    }

    public void FadeOutBlackTransition(float timeInSeconds)
    {
        CurrentTransitionTime = 0f;
        TransitionTargetTime = timeInSeconds;
        TransitionState = BlackTransitionState.FadeOut;
    }

    public void HoldBlackTransition(float timeInSeconds)
    {
        CurrentTransitionTime = 0f;
        TransitionTargetTime = timeInSeconds;
        TransitionState = BlackTransitionState.Hold;

        BlackTransitionImage.color = new Color(0f, 0f, 0f, 1f);
        BlackTransitionText.color = new Color(1f, 1f, 1f, 1f);
    }

    #endregion
}
