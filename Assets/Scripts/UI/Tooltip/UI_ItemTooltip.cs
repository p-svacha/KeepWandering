using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemTooltip : UI_TooltipBase
{
    public static UI_ItemTooltip Instance;
    private Item Item;

    [Header("Elements")]
    public Image ItemImage;
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI SubtitleText;
    
    public GameObject TagDivider;
    public GameObject TagContainer;

    public GameObject ConsumptionDivider;
    public GameObject ConsumptionContainer;

    public TextMeshProUGUI ConsumptionTypeText;

    public GameObject ConsumptionNutritionRow;
    public TextMeshProUGUI ConsumptionNutritionText;
    public GameObject ConsumptionHydrationRow;
    public TextMeshProUGUI ConsumptionHydrationText;

    public GameObject ConsumptionAdditionalInfo;
    public TextMeshProUGUI ConsumptionAdditionalInfoText;

    public GameObject ConsumptionPerDayInfo;

    public GameObject PassiveEffectsDivider;
    public GameObject PassiveEffectsContainer;
    public TextMeshProUGUI PassiveEffectsText;

    public GameObject DescriptionDivider;
    public GameObject DescriptionContainer;
    public TextMeshProUGUI DescriptionText;

    [Header("Prefabs")]
    public UI_ItemTooltipTag TagPrefab;
    public UI_TooltipKeyValue KeyValuePrefab;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(Item item)
    {
        gameObject.SetActive(true);
        Item = item;

        // Header
        ItemImage.sprite = item.Sprite;
        TitleText.text = item.LabelCapWord;
        SubtitleText.text = item.GetTooltipSubtitle();

        // Tags
        bool hasTags = item.Def.Tags.Count > 0;
        TagDivider.SetActive(hasTags);
        TagContainer.SetActive(hasTags);
        if (hasTags)
        {
            HelperFunctions.DestroyAllChildredImmediately(TagContainer);
            foreach (var tag in item.Def.Tags.OrderByDescending(tag => tag.Value))
            {
                UI_ItemTooltipTag elem = Instantiate(TagPrefab, TagContainer.transform);
                elem.Init(item.Def, tag.Key);
            }
        }

        // Consumption
        bool isConsumable = item.Def.IsConsumable;
        ConsumptionDivider.SetActive(isConsumable);
        ConsumptionContainer.SetActive(isConsumable);
        if (isConsumable)
        {
            ConsumptionProperties consumptionProps = item.Def.ConsumptionProperties;
            ConsumptionTypeText.text = consumptionProps.ConsumptionType.LabelCap;

            ConsumptionNutritionRow.SetActive(consumptionProps.Nutrition > 0);
            ConsumptionNutritionText.text = $"{consumptionProps.Nutrition} {"day".Pluralize(consumptionProps.Nutrition)}*";
            ConsumptionHydrationRow.SetActive(consumptionProps.Hydration > 0);
            ConsumptionHydrationText.text = $"{consumptionProps.Hydration} {"day".Pluralize(consumptionProps.Hydration)}*";

            ConsumptionPerDayInfo.SetActive(consumptionProps.Nutrition > 0 || consumptionProps.Hydration > 0);

            string additionalEffectsText = "";

            // Stat changes
            if (consumptionProps.StatChanges.Count > 0)
            {
                foreach (var statChange in consumptionProps.StatChanges)
                {
                    additionalEffectsText += $"\n{statChange.Value.ToSignedString()} {statChange.Key.LabelCap}";
                }
            }

            // Health condition
            if (consumptionProps.AppliedHealthCondition != null)
            {
                additionalEffectsText += $"\n- Applies {consumptionProps.AppliedHealthCondition.LabelCap}";
            }

            // Severity reduction
            if (consumptionProps.SeverityReduction > 0)
            {
                additionalEffectsText += $"\n- Eases a random ailment";
            }

            

            additionalEffectsText = additionalEffectsText.Trim();
            ConsumptionAdditionalInfoText.text = additionalEffectsText;
            ConsumptionAdditionalInfo.gameObject.SetActive(additionalEffectsText != "");
        }

        // Passive Effects
        bool hasPassiveEffects = item.Def.PassiveStatChanges.Count > 0;
        PassiveEffectsDivider.SetActive(hasPassiveEffects);
        PassiveEffectsContainer.SetActive(hasPassiveEffects);
        if (hasPassiveEffects)
        {
            string text = "";
            foreach (var statChange in item.Def.PassiveStatChanges)
            {
                text += $"\n{statChange.Value.ToSignedString()} {statChange.Key.LabelCap}";
            }
            PassiveEffectsText.text = text.Trim();
        }

        // Description
        bool hasDescription = item.Def.Description != "";
        DescriptionDivider.SetActive(hasDescription);
        DescriptionContainer.SetActive(hasDescription);
        if (hasDescription)
        {
            DescriptionText.text = item.Def.Description;
        }

        // Layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());

        // Initial position
        UpdatePosition();
    }

    protected override void Update()
    {
        UpdatePosition();
    }

    public void UpdatePosition()
    {
        transform.position = Item.Renderer.transform.position + new Vector3(0.1f, -0.1f, 0f);
        ClampToScreen();
    }
}
