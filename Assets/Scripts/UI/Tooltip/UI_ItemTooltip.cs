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

    public GameObject MedicalDivider;
    public GameObject MedicalContainer;
    public TextMeshProUGUI MedicalText;

    public GameObject DescriptionDivider;
    public GameObject DescriptionContainer;
    public TextMeshProUGUI DescriptionText;

    [Header("Prefabs")]
    public UI_ItemTooltipTag TagPrefab;
    public UI_ItemTooltipKeyValue KeyValuePrefab;

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
        SubtitleText.text = $"Durability: {item.Durability}";

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
            ConsumptionTypeText.text = item.Def.ConsumptionType.LabelCap;

            ConsumptionNutritionRow.SetActive(item.Def.OnConsumptionNutrition > 0);
            ConsumptionNutritionText.text = $"{item.Def.OnConsumptionNutrition} days*";
            ConsumptionHydrationRow.SetActive(item.Def.OnConsumptionHydration > 0);
            ConsumptionHydrationText.text = $"{item.Def.OnConsumptionHydration} days*";

            ConsumptionPerDayInfo.SetActive(item.Def.OnConsumptionNutrition > 0 || item.Def.OnConsumptionHydration > 0);

            string additionalEffectsText = "";
            ConsumptionAdditionalInfoText.text = additionalEffectsText;
            ConsumptionAdditionalInfo.gameObject.SetActive(additionalEffectsText != "");
        }

        // Medical
        bool isMedical = item.Def.HasMedicalProperties;
        MedicalDivider.SetActive(isMedical);
        MedicalContainer.SetActive(isMedical);
        if (isMedical)
        {
            string medicalText = "";
            if (item.Def.CanReduceSeverity) medicalText += $"\n- Can reduce health condition severity";
            if (item.Def.CanTendWounds) medicalText += $"\n- Can tend wounds";
            if (item.Def.CanTreatInfections) medicalText += $"\n- Can treat infections";
            if (item.Def.CanHealPoisoning) medicalText += $"\n- Can heal poisoning";
            MedicalText.text = medicalText.TrimStart('\n');
        }

        // Description
        bool hasDescription = item.Def.Description != "";
        DescriptionDivider.SetActive(hasDescription);
        DescriptionContainer.SetActive(hasDescription);
        if (hasDescription)
        {
            DescriptionText.text = item.Def.Description;
        }

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
