using TMPro;
using UnityEngine;

public class UI_StatTooltip : UI_TooltipBase
{
    public static UI_StatTooltip Instance;
    private UI_Stat HoveredStat;

    [Header("Elements")]
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI DescriptionText;
    public GameObject StatsContainer;

    [Header("Prefabs")]
    public UI_StatModifier StatModifierPrefab;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(UI_Stat statDisplay)
    {
        gameObject.SetActive(true);
        HoveredStat = statDisplay;

        TitleText.text = statDisplay.Stat.Label;
        DescriptionText.text = statDisplay.Stat.Description;

        HelperFunctions.DestroyAllChildredImmediately(StatsContainer);
        // Base value
        UI_StatModifier baseValueRow = Instantiate(StatModifierPrefab, StatsContainer.transform);
        baseValueRow.InitBaseValue(statDisplay.Stat);

        // Modifiers
        foreach (StatModifier mod in statDisplay.Stat.GetModifiers())
        {
            UI_StatModifier modDisplay = Instantiate(StatModifierPrefab, StatsContainer.transform);
            modDisplay.Init(mod);
        }

        UpdatePositionAtUi(statDisplay.gameObject);
    }

    protected override void Update()
    {
        UpdatePositionAtUi(HoveredStat.gameObject);
    }
}
