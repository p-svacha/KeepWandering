using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class GameUI : MonoBehaviour
{
    public Game Game { get; private set; }

    [Header("Day Panel")]
    public TextMeshProUGUI DayText;
    public TextMeshProUGUI DayTimeText;
    public Button MapButton;
    public Button SettingsButton;
    public Button DiaryButton;
    public Button CraftingButton;

    [Header("Health Reports")]
    public GameObject HealthReportContainer;
    public UI_HealthReport HealthReportPrefab;

    [Header("Event Display")]
    public UI_EventDisplay EventStepDisplay;

    [Header("Mission Display")]
    public UI_Missions MissionsDisplay;

    [Header("Windows")]
    public UI_EscapeMenu EscapeMenu;

    [Header("World Map")]
    public UI_WorldMapMenu WorldMapMenu;

    [Header("Description Box")]
    public UI_DescriptionBox DescriptionBox;
    public const float TOOLTIP_HOVER_TIME = 1f;

    [Header("Interaction Box")]
    public UI_InteractionBox InteractionBox;

    [Header("Day Transition")]
    public Image BlackTransitionImage;
    public TextMeshProUGUI BlackTransitionText;
    public const float TRANSITION_HOLD_TIME = 3f;
    public const float TRANSITION_FADE_TIME = 1f;
    private float TransitionTargetTime;
    private float CurrentTransitionTime;
    private BlackTransitionState TransitionState;

    public void Init(Game game)
    {
        Game = game;

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

    #region UI Elements

    public void ShowDescriptionBox(Item item)
    {
        DescriptionBox.gameObject.SetActive(true);
        DescriptionBox.Init(item);
    }
    public void HideDescriptionBox()
    {
        DescriptionBox.gameObject.SetActive(false);
    }

    public void ShowInteractionBox(Item item)
    {
        InteractionBox.gameObject.SetActive(true);
        InteractionBox.Init(item);
    }
    public void HideInteractionBox()
    {
        InteractionBox.gameObject.SetActive(false);
        InteractionBox.Clear();
    }

    public void UpdateHealthReports()
    {
        // Reset
        HelperFunctions.DestroyAllChildredImmediately(HealthReportContainer);

        // Display player health report
        UI_HealthReport playerHealthReport = Instantiate(HealthReportPrefab, HealthReportContainer.transform);
        playerHealthReport.Init(Game.Player);

        // Display companion health reports
        foreach(Companion companion in Game.Companions)
        {
            UI_HealthReport companionHealthReport = Instantiate(HealthReportPrefab, HealthReportContainer.transform);
            companionHealthReport.Init(companion);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(HealthReportContainer.GetComponent<RectTransform>());
    }

    public void UpdateMissionDisplay()
    {
        MissionsDisplay.UpdateList(Game.Missions.Values.ToList());
    }

    #endregion

    #region Windows

    public void CloseAllWindows()
    {
        CloseEscapeMenu();
        CloseWorldMap();
    }

    public void ToggleEscapeMenu()
    {
        if (Game.State != GameState.InGame) return;
        EscapeMenu.gameObject.SetActive(!EscapeMenu.gameObject.activeSelf);
    }
    public void CloseEscapeMenu()
    {
        EscapeMenu.gameObject.SetActive(false);
    }

    public void ToggleWorldMap()
    {
        if (Game.State != GameState.InGame) return;
        WorldMapMenu.gameObject.SetActive(!WorldMapMenu.gameObject.activeSelf);
        Game.WorldMap.gameObject.SetActive(!Game.WorldMap.gameObject.activeSelf);
    }
    public void OpenWorldMap()
    {
        if (Game.State != GameState.InGame) return;
        WorldMapMenu.gameObject.SetActive(true);
        Game.WorldMap.gameObject.SetActive(true);
    }
    public void CloseWorldMap()
    {
        WorldMapMenu.gameObject.SetActive(false);
        Game.WorldMap.gameObject.SetActive(false);
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
