using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Net.Sockets;

public class UI_EncounterDisplay : MonoBehaviour
{
    public Game Game;
    public static UI_EncounterDisplay Instance;

    [Header("Elements")]
    public TextMeshProUGUI EncounterText;
    public GameObject EncounterOptionContainer;
    public TextMeshProUGUI HoveredOptionDescriptionText;

    public GameObject PreviousOutcomeContainer;
    public Image PreviousOutcomeImage;

    public GameObject OutcomeNotesContainer;
    public UI_OptionDetails OptionDetailsPanel;
    public UI_ItemSlotDetailsBox ItemSlotDetailsBox;

    [Header("Prefabs")]
    public UI_EncounterStepOption EncounterOptionPrefab;
    public UI_EncounterOutcomeNote OutcomeNotePrefab;

    public Dictionary<EncounterOption, UI_EncounterStepOption> OptionDisplays;

    private Coroutine OutcomeAnimCoroutine;
    private Material OutcomeFlashMaterial;

    private void Awake()
    {
        Instance = this;
    }

    public void Init(EncounterStep step, OptionOutcomeDef prevOutcome = null) 
    {
        Clear();

        // Previous outcome
        if (prevOutcome != null)
        {
            PreviousOutcomeContainer.SetActive(true);
            PreviousOutcomeImage.sprite = ResourceManager.LoadSprite($"UiSprites/Outcome/Outcome_{prevOutcome.DefName}");

            // Animated pop-in with random rotation
            float targetAngle = Random.Range(-25f, 25f);
            if (OutcomeAnimCoroutine != null) StopCoroutine(OutcomeAnimCoroutine);
            OutcomeAnimCoroutine = StartCoroutine(AnimatePreviousOutcome(targetAngle));
        }
        else PreviousOutcomeContainer.SetActive(false);

        // Text
        EncounterText.text = step.Text;

        // Dialogue Options
        OptionDisplays = new Dictionary<EncounterOption, UI_EncounterStepOption>();
        if (step.IsFinalStep)
        {
            FixedOutcomeOption endDayOption = new FixedOutcomeOption() { Text = "Continue journey", Description = "Continue your day.", Action = EndEncounter };
            UI_EncounterStepOption optionDisplay = Instantiate(EncounterOptionPrefab, EncounterOptionContainer.transform);
            optionDisplay.Init(this, endDayOption);
            OptionDisplays.Add(endDayOption, optionDisplay);
        }
        else
        {
            foreach (EncounterOption option in step.Options)
            {
                UI_EncounterStepOption optionDisplay = Instantiate(EncounterOptionPrefab, EncounterOptionContainer.transform);
                optionDisplay.Init(this, option);
                OptionDisplays.Add(option, optionDisplay);
            }
        }

        HideOptionDetails();
        HoveredOptionDescriptionText.gameObject.SetActive(false);
        InitEncounterStepOutcomeNotes();

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    private string EndEncounter()
    {
        if (Game.CurrentEncounter.Def.Type == EncounterType.Night)
        {
            // End night encounter
        }
        else // It is afternoon
        {
            Game.EndAfternoonEncounter();
        }
        return "";
    }

    public void OnOptionHovered(EncounterOption option)
    {
        // Show description
        ShowOptionDescription(option);

        // Skill check stuff
        if (option is SkillCheckOption skillCheckOption)
        {
            // Highlight associated stats
            foreach (var kvp in skillCheckOption.RelevantStats)
            {
                StatDef stat = kvp.Key;
                float factor = kvp.Value;

                Color highlightColor;
                if (factor <= 1f) highlightColor = ResourceManager.Color_Highlight_LowImpact;
                else if (factor <= 2) highlightColor = ResourceManager.Color_Highlight_MediumImpact;
                else if (factor <= 3) highlightColor = ResourceManager.Color_Highlight_HighImpact;
                else highlightColor = ResourceManager.Color_Highlight_UltimateImpact;

                GameUI.Instance.HightlightStat(stat, highlightColor);
            }

            // Highlight morale
            GameUI.Instance.HightlightStat(StatDefOf.Morale, ResourceManager.Color_Highlight_LowImpact); // Morale is always relevant for skill checks
            // Show option details
            ShowOptionDetails(skillCheckOption);
        }
    }

    private void ShowOptionDescription(EncounterOption option)
    {
        string descriptionText = option.Description;
        if (!option.CanSelect()) descriptionText += "\n" + ResourceManager.WarningText("Missing required item.");
        if (descriptionText != "")
        {
            HoveredOptionDescriptionText.gameObject.SetActive(true);
            HoveredOptionDescriptionText.text = descriptionText;
        }
    }

    public void OnOptionUnhovered()
    {
        // Hide description
        HoveredOptionDescriptionText.gameObject.SetActive(false);

        // Unhighlight all stats
        GameUI.Instance.UnhighlightAllStats();

        // Hide option details
        HideOptionDetails();
    }

    public void OnItemSlotHovered(ItemSlot itemSlot)
    {
        ItemSlotDetailsBox.Show(itemSlot);
        LayoutRebuilder.ForceRebuildLayoutImmediate(ItemSlotDetailsBox.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
    public void OnItemSlotUnhovered()
    {
        ItemSlotDetailsBox.Hide();
    }

    private void Clear()
    {
        if (OutcomeAnimCoroutine != null)
        {
            StopCoroutine(OutcomeAnimCoroutine);
            OutcomeAnimCoroutine = null;
            PreviousOutcomeImage.transform.localScale = Vector3.one;
            PreviousOutcomeImage.material = null;
            if (OutcomeFlashMaterial != null)
            {
                Destroy(OutcomeFlashMaterial);
                OutcomeFlashMaterial = null;
            }
        }
        HelperFunctions.DestroyAllChildredImmediately(EncounterOptionContainer);
        HelperFunctions.DestroyAllChildredImmediately(OutcomeNotesContainer);
    }

    public void RefreshOption(EncounterOption option)
    {
        OptionDisplays[option].Resfresh();

        // Option description
        ShowOptionDescription(option);

        // Option details
        if (OptionDetailsPanel.gameObject.activeSelf) OptionDetailsPanel.Refresh();

        // Item slot
        if (ItemSlotDetailsBox.gameObject.activeSelf) ItemSlotDetailsBox.Refresh();
    }

    #region Outcome Notes

    private void InitEncounterStepOutcomeNotes()
    {
        // Added items
        Dictionary<Item, int> groupedAddedItems = new Dictionary<Item, int>();
        foreach (Item item in Game.ItemsAddedSinceLastStep)
        {
            if (!groupedAddedItems.Any(x => x.Key.Def == item.Def)) groupedAddedItems.Add(item, 1);
            else groupedAddedItems[groupedAddedItems.First(x => x.Key.Def == item.Def).Key]++;
        }
        foreach (KeyValuePair<Item, int> item in groupedAddedItems)
        {
            UI_EncounterOutcomeNote outcomeNote = Instantiate(OutcomeNotePrefab, OutcomeNotesContainer.transform);
            outcomeNote.Init(item.Key.Sprite, true, item.Value);
        }

        // Removed items
        Dictionary<Item, int> groupedRemovedItems = new Dictionary<Item, int>();
        foreach (Item item in Game.ItemsRemovedSinceLastStep)
        {
            if (!groupedRemovedItems.Any(x => x.Key.Def == item.Def)) groupedRemovedItems.Add(item, 1);
            else groupedRemovedItems[groupedRemovedItems.First(x => x.Key.Def == item.Def).Key]++;
        }
        foreach (KeyValuePair<Item, int> item in groupedRemovedItems)
        {
            UI_EncounterOutcomeNote outcomeNote = Instantiate(OutcomeNotePrefab, OutcomeNotesContainer.transform);
            outcomeNote.Init(item.Key.Sprite, false, item.Value);
        }

        // Added wounds
        Dictionary<Wound, int> groupedWounds = new Dictionary<Wound, int>();
        foreach (Wound wound in Game.WoundsAddedSinceLastStep)
        {
            if (!groupedWounds.Any(x => x.Key.Def == wound.Def)) groupedWounds.Add(wound, 1);
            else groupedWounds[groupedWounds.First(x => x.Key.Def == wound.Def).Key]++;
        }
        foreach (var group in groupedWounds)
        {
            UI_EncounterOutcomeNote outcomeNote = Instantiate(OutcomeNotePrefab, OutcomeNotesContainer.transform);
            outcomeNote.Init(group.Key.SpriteBase, true, group.Value);
        }

        // Stat changes
        foreach(var statChange in Game.StatChangesSinceLastStep)
        {
            if(statChange.Value == 0) continue;
            UI_EncounterOutcomeNote outcomeNote = Instantiate(OutcomeNotePrefab, OutcomeNotesContainer.transform);
            outcomeNote.Init(statChange.Key, statChange.Value);
        }

        // Add slight rotation to all notes
        foreach(UI_EncounterOutcomeNote note in OutcomeNotesContainer.GetComponentsInChildren<UI_EncounterOutcomeNote>())
        {
            note.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-10f, 10f));
        }
    }


    #endregion

    #region Option Details

    public void ShowOptionDetails(SkillCheckOption option)
    {
        OptionDetailsPanel.gameObject.SetActive(true);
        OptionDetailsPanel.ShowDetailsFor(option);
    }

    public void HideOptionDetails()
    {
        OptionDetailsPanel.gameObject.SetActive(false);
        ItemSlotDetailsBox.Hide();
    }
    #endregion

    #region Previous Outcome Animation

    private IEnumerator AnimatePreviousOutcome(float targetAngle)
    {
        float duration = 0.5f;
        float elapsed = 0f;

        // Set up flash material
        OutcomeFlashMaterial = new Material(Shader.Find("UI/FlashSprite"));
        PreviousOutcomeImage.material = OutcomeFlashMaterial;

        // Start scaled down
        PreviousOutcomeImage.transform.localScale = Vector3.zero;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Elastic overshoot scale
            float scale = EaseOutElastic(t);
            PreviousOutcomeImage.transform.localScale = Vector3.one * scale;

            // Dampened rotation wobble
            float damping = (1f - t) * (1f - t);
            float wobble = Mathf.Sin(t * Mathf.PI * 5f) * damping * 20f;
            PreviousOutcomeImage.transform.rotation = Quaternion.Euler(0, 0, targetAngle + wobble);

            // White flash that fades quickly in the first third
            float flash = Mathf.Clamp01(1f - t * 3f);
            OutcomeFlashMaterial.SetFloat("_FlashAmount", flash * flash);

            yield return null;
        }

        // Ensure exact final state
        PreviousOutcomeImage.transform.localScale = Vector3.one;
        PreviousOutcomeImage.transform.rotation = Quaternion.Euler(0, 0, targetAngle);
        PreviousOutcomeImage.material = null;
        Destroy(OutcomeFlashMaterial);
        OutcomeFlashMaterial = null;
        OutcomeAnimCoroutine = null;
    }

    private float EaseOutElastic(float t)
    {
        if (t <= 0f) return 0f;
        if (t >= 1f) return 1f;
        float p = 0.3f;
        float s = p / 4f;
        return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t - s) * (2f * Mathf.PI) / p) + 1f;
    }

    #endregion
}
