using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemTooltip : MonoBehaviour
{
    public static UI_ItemTooltip Instance;

    [Header("Elements")]
    public Image ItemImage;
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI SubtitleText;
    
    public GameObject TagDivider;
    public GameObject TagContainer;

    public GameObject ConsumptionDivider;
    public GameObject ConsumptionContainer;

    public GameObject MedicalDivider;
    public GameObject MedicalContainer;
    public TextMeshProUGUI MedicalText;

    public GameObject DescriptionDivider;
    public GameObject DescriptionContainer;
    public TextMeshProUGUI DescriptionText;

    [Header("Prefabs")]
    public UI_ItemTooltipTag TagPrefab;

    private void Awake()
    {
        Instance = this;
    }
}
