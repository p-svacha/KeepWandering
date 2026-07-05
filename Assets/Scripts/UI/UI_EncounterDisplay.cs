using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_EncounterDisplay : Singleton<UI_EncounterDisplay>
{
    public Game Game;
    public static string SPRITE_ENCOUNTER_OPTION_LAYER = "EncounterOptionSprite";

    [Header("Elements")]
    public TextMeshProUGUI EncounterText;
    public GameObject EncounterOptionContainer;
    public GameObject FloatingOptionsContainer;

    public GameObject PreviousOutcomeContainer;
    public Image PreviousOutcomeImage;

    public GameObject OutcomeNotesContainer;
    public UI_OptionDetails OptionDetailsPanel;
    public UI_ItemSlotDetailsBox ItemSlotDetailsBox;

    [Header("Trap Display")]
    public GameObject TrapDisplay;
    public TextMeshProUGUI TrapNumText;
    public UI_TooltipTarget TrapImageTooltipTarget;
    public UI_TooltipTarget TrapTextTooltipTarget;

    [Header("Prefabs")]
    public UI_EncounterStepOption EncounterOptionPrefab;
    public UI_EncounterOutcomeNote OutcomeNotePrefab;
    public UI_SpriteEncounterOptionContainer SpriteEncounterOptionContainer;
    public UI_EncounterOptionSpriteLabel EncounterOptionSpriteLabelPrefab;

    public Dictionary<EncounterOption, UI_EncounterStepOption> OptionDisplays;

    private Coroutine OutcomeAnimCoroutine;
    private Material OutcomeFlashMaterial;

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

        // Options
        OptionDisplays = new Dictionary<EncounterOption, UI_EncounterStepOption>();
        Debug.Log($"Available options are: {string.Join(", ", step.Options.Select(o => o.Text))}");

        // Group options by sprite (sprite-bound vs non-sprite-bound)
        var spriteGroups = step.Options
            .Where(o => o.Sprite != null)
            .GroupBy(o => o.Sprite);

        var nonSpriteOptions = step.Options.Where(o => o.Sprite == null).ToList();

        // Handle sprite-bound options
        foreach (var spriteGroup in spriteGroups)
        {
            GameObject spriteGameObject = spriteGroup.Key;
            List<EncounterOption> optionsForSprite = spriteGroup.ToList();

            // Configure the sprite GameObject
            if (spriteGameObject.GetComponent<PolygonCollider2D>() == null)
            {
                PolygonCollider2D collider = spriteGameObject.AddComponent<PolygonCollider2D>();
            }
            spriteGameObject.GetComponent<PolygonCollider2D>().isTrigger = true;
            spriteGameObject.layer = LayerMask.NameToLayer(SPRITE_ENCOUNTER_OPTION_LAYER);

            // Instantiate container for this sprite's options
            UI_SpriteEncounterOptionContainer container = Instantiate(SpriteEncounterOptionContainer, FloatingOptionsContainer.transform);
            container.Init(optionsForSprite);

            // Merge container's option displays into this class's OptionDisplays dictionary
            foreach (var kvp in container.OptionDisplays)
            {
                OptionDisplays.Add(kvp.Key, kvp.Value);
            }

            // Instantiate and initialize the sprite label
            UI_EncounterOptionSpriteLabel spriteLabel = Instantiate(EncounterOptionSpriteLabelPrefab, FloatingOptionsContainer.transform);
            spriteLabel.Init(spriteGameObject.name);

            // Register with the interaction manager
            SpriteOptionInteractionManager.RegisterSprite(spriteGameObject, container, spriteLabel, optionsForSprite);
        }

        // Handle non-sprite-bound options
        foreach (EncounterOption option in nonSpriteOptions)
        {
            UI_EncounterStepOption optionDisplay = Instantiate(EncounterOptionPrefab, EncounterOptionContainer.transform);
            optionDisplay.Init(option);
            OptionDisplays.Add(option, optionDisplay);
        }
        

        // Option details
        HideOptionDetails();

        // Outcome notes
        InitEncounterStepOutcomeNotes();

        // Trap display
        TrapDisplay.SetActive(Game.TimeOfDay == TimeOfDayDefOf.Evening && Game.NumEveningTraps > 0);
        if(Game.TimeOfDay == TimeOfDayDefOf.Evening)
        {
            TrapNumText.text = Game.NumEveningTraps.ToString();
            string tooltipTitle = "Traps";
            string tooltipText = "Traps help defending against attacks in the night, or may catch wildlife, providing resources.";
            TrapImageTooltipTarget.Init(tooltipTitle, tooltipText);
            TrapTextTooltipTarget.Init(tooltipTitle, tooltipText);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    public void OnOptionHovered(EncounterOption option)
    {
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
        }

        // Show option details
        ShowOptionDetails(option);
    }

    public void OnOptionUnhovered()
    {
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
        HelperFunctions.DestroyAllChildredImmediately(FloatingOptionsContainer);
        SpriteOptionInteractionManager.ClearAll();
    }

    public void RefreshOption(EncounterOption option)
    {
        OptionDisplays[option].Resfresh();

        // Option details
        if (OptionDetailsPanel.gameObject.activeSelf) OptionDetailsPanel.Refresh();

        // Item slot
        if (ItemSlotDetailsBox.gameObject.activeSelf) ItemSlotDetailsBox.Refresh();

        // Sprite-bound option availability color
        if (option.Sprite != null) SpriteOptionInteractionManager.RefreshAvailability(option.Sprite);
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
            outcomeNote.Init(item.Key.Sprite, true, item.Value, item.Key.Def.LabelCapWord, item.Key.Description);
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
            outcomeNote.Init(item.Key.Sprite, false, item.Value, item.Key.Def.LabelCapWord, item.Key.Description);
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
            outcomeNote.Init(group.Key.SpriteBase, true, group.Value, group.Key.Def.LabelCapWord, group.Key.Description);
        }

        // Stat changes
        foreach(var statChange in Game.StatChangesSinceLastStep)
        {
            if(statChange.Value == 0) continue;
            UI_EncounterOutcomeNote outcomeNote = Instantiate(OutcomeNotePrefab, OutcomeNotesContainer.transform);
            outcomeNote.Init(statChange.Key, statChange.Value);
        }

        // Revealed tiles
        if (Game.NumRevealedLocationEncountersSinceLastStep > 0)
        {
            UI_EncounterOutcomeNote outcomeNote = Instantiate(OutcomeNotePrefab, OutcomeNotesContainer.transform);
            Sprite sprite = ResourceManager.LoadSprite("UiSprites/RevealEye");
            outcomeNote.Init(sprite, isAdded: true, Game.NumRevealedLocationEncountersSinceLastStep, tooltipText: $"Revealed {Game.NumRevealedLocationEncountersSinceLastStep} location{(Game.NumRevealedLocationEncountersSinceLastStep > 1 ? "s" : "")} on the world map.");
        }

        // New quests
        if (Game.NumAddedQuestsSinceLastStep > 0)
        {
            UI_EncounterOutcomeNote outcomeNote = Instantiate(OutcomeNotePrefab, OutcomeNotesContainer.transform);
            Sprite sprite = ResourceManager.LoadSprite("UiSprites/NewNote");
            outcomeNote.Init(sprite, isAdded: true, Game.NumAddedQuestsSinceLastStep, tooltipText: $"Gained {Game.NumAddedQuestsSinceLastStep} new quest{(Game.NumAddedQuestsSinceLastStep > 1 ? "s" : "")}.");
        }

        // Completed quests
        if (Game.NumCompletedQuestsSinceLastStep > 0)
        {
            UI_EncounterOutcomeNote outcomeNote = Instantiate(OutcomeNotePrefab, OutcomeNotesContainer.transform);
            Sprite sprite = ResourceManager.LoadSprite("UiSprites/QuestCompleted");
            outcomeNote.Init(sprite, isAdded: true, Game.NumCompletedQuestsSinceLastStep, tooltipText: $"Completed {Game.NumCompletedQuestsSinceLastStep} quest{(Game.NumCompletedQuestsSinceLastStep > 1 ? "s" : "")}.");
        }

        // Failed quests
        if (Game.NumFailedQuestsSinceLastStep > 0)
        {
            UI_EncounterOutcomeNote outcomeNote = Instantiate(OutcomeNotePrefab, OutcomeNotesContainer.transform);
            Sprite sprite = ResourceManager.LoadSprite("UiSprites/QuestFailed");
            outcomeNote.Init(sprite, isAdded: false, Game.NumFailedQuestsSinceLastStep, tooltipText: $"Failed {Game.NumFailedQuestsSinceLastStep} quest{(Game.NumFailedQuestsSinceLastStep > 1 ? "s" : "")}.");
        }

        // Add slight rotation to all notes
        foreach (UI_EncounterOutcomeNote note in OutcomeNotesContainer.GetComponentsInChildren<UI_EncounterOutcomeNote>())
        {
            note.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-10f, 10f));
        }
    }


    #endregion

    #region Option Details

    public void ShowOptionDetails(EncounterOption option)
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
