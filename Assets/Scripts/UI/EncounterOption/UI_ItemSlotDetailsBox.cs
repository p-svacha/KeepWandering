using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemSlotDetailsBox : MonoBehaviour
{
    public ItemSlot Slot { get; private set; }

    [Header("Elements")]
    public TextMeshProUGUI TitleText;
    public GameObject AcceptedItemsContainer;
    public TextMeshProUGUI AcceptedItemsText;
    public TextMeshProUGUI DifficultyModifierText;
    public TextMeshProUGUI DestructionChanceText;

    [Header("Prefabs")]
    public GameObject AcceptedItemColumnPrefab;

    public void Show(ItemSlot itemSlot)
    {
        Slot = itemSlot;
        gameObject.SetActive(true);

        Refresh();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Refresh()
    {
        TitleText.text = $"Item Slot ({(Slot.IsRequired ? "REQUIRED" : "OPTIONAL")})";

        // Accepted items
        if (Slot.DifficultyReduction == 0 || !Slot.HasMultipleDifficultyReductions()) AcceptedItemsText.text = "Accepted Items";
        else AcceptedItemsText.text = "Accepted Items\n<color=#666666>Difficulty Reduction</color>";

        HelperFunctions.DestroyAllChildredImmediately(AcceptedItemsContainer);
        foreach (ItemDef itemDef in Slot.GetSlottableItemDefs())
        {
            GameObject column = Instantiate(AcceptedItemColumnPrefab, AcceptedItemsContainer.transform);
            column.GetComponentInChildren<Image>().sprite = itemDef.Sprite;
            TextMeshProUGUI difficultyReductionText = column.GetComponentInChildren<TextMeshProUGUI>();
            if (Slot.HasMultipleDifficultyReductions())
            {
                difficultyReductionText.text = Slot.GetDifficultyReduction(itemDef).ToString();
            }
            else
            {
                // Just show the item sprite without a difficulty reduction value if there are no custom reductions, since the difficulty modifier below will apply to all accepted items equally
                difficultyReductionText.gameObject.SetActive(false);
                column.GetComponent<LayoutElement>().preferredHeight = 50;
            }
        }
        // Difficulty modifier
        if (Slot.DifficultyReduction != 0 && !Slot.HasMultipleDifficultyReductions())
        {
            DifficultyModifierText.text = $"Difficulty Reduction: -{Slot.DifficultyReduction}";
            DifficultyModifierText.gameObject.SetActive(true);
        }
        else DifficultyModifierText.gameObject.SetActive(false);

        // Destruction chance
        if (Slot.DestructionChance > 0f)
        {
            DestructionChanceText.text = $"Chance to Break: {Slot.DestructionChance * 100f:0}%";
            DestructionChanceText.gameObject.SetActive(true);
        }
        else DestructionChanceText.gameObject.SetActive(false);
    }
}
