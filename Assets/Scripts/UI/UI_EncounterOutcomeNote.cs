using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_EncounterOutcomeNote : MonoBehaviour
{
    public enum ValueChangeType { Added, Removed, Increased, Decreased }
    public UI_TooltipTarget TooltipTarget;

    public GameObject ItemContainer;
    public Image ItemIcon;
    public TextMeshProUGUI PlusText;
    public TextMeshProUGUI MinusText;
    public TextMeshProUGUI AmountText;
    public GameObject IncreaseIndicator;
    public GameObject DecreaseIndicator;

    public GameObject StatChangeContainer;
    public TextMeshProUGUI StatChangeValueText;
    public TextMeshProUGUI StatChangeLabelText;

    public GameObject TransformContainer;
    public Image TransformationOldItemIcon;
    public Image TransformationNewItemIcon;
    public Image TransformationMethodIcon;
    public Image TransformationArrowIcon;

    public void Init(Sprite sprite, ValueChangeType valueChangeType, int amount = 1, string tooltipTitle = "", string tooltipText = "")
    {
        ItemContainer.SetActive(true);
        StatChangeContainer.SetActive(false);
        TransformContainer.SetActive(false);

        ItemIcon.sprite = sprite;

        PlusText.gameObject.SetActive(valueChangeType == ValueChangeType.Added);
        MinusText.gameObject.SetActive(valueChangeType == ValueChangeType.Removed);
        IncreaseIndicator.SetActive(valueChangeType == ValueChangeType.Increased);
        DecreaseIndicator.gameObject.SetActive(valueChangeType == ValueChangeType.Decreased);

        AmountText.text = "x" + amount.ToString();
        AmountText.gameObject.SetActive(amount > 1);
        AmountText.color = valueChangeType == ValueChangeType.Added ? PlusText.color : MinusText.color;

        TooltipTarget.Init(tooltipTitle, tooltipText);
    }

    public void Init(StatDef stat, int value)
    {
        ItemContainer.SetActive(false);
        StatChangeContainer.SetActive(true);
        TransformContainer.SetActive(false);

        StatChangeValueText.text = value.ToString("+#;-#;0");
        StatChangeLabelText.text = stat.Abbreviation;

        TooltipTarget.Init(text: $"Base value of {stat.LabelCapWord} has {(value > 0 ? "increased" : "decreased")} by {Mathf.Abs(value)}.");
    }

    public void Init(Item oldItem, Item newItem, ItemTransformationMethodDef transformationMethod)
    {
        ItemContainer.SetActive(false);
        StatChangeContainer.SetActive(false);
        TransformContainer.SetActive(true);

        TransformationOldItemIcon.sprite = oldItem.Sprite;
        TransformationNewItemIcon.sprite = newItem.Sprite;
        TransformationMethodIcon.sprite = transformationMethod.Sprite;

        string tooltipText = $"Transformed {oldItem.Def.LabelCapWord} into {newItem.Def.LabelCapWord} via {transformationMethod.Label}.";
        TooltipTarget.Init("", tooltipText);
    }
}
