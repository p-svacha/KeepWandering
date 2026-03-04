using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemSlotDetailsBox : MonoBehaviour
{
    [Header("Elements")]
    public TextMeshProUGUI TitleText;
    public GameObject AcceptedItemsContainer;
    public TextMeshProUGUI DifficultyModifierText;
    public TextMeshProUGUI DestructionChanceText;

    [Header("Prefabs")]
    public GameObject AcceptedItemColumnPrefab;

    public void Show(ItemSlot itemSlot)
    {
        gameObject.SetActive(true);

        TitleText.text = $"Item Slot ({(itemSlot.IsRequired ? "REQUIRED" : "OPTIONAL")})";

        // Accepted items
        HelperFunctions.DestroyAllChildredImmediately(AcceptedItemsContainer);
        foreach (ItemDef itemDef in itemSlot.GetSlottableItemDefs())
        {
            GameObject column = Instantiate(AcceptedItemColumnPrefab, AcceptedItemsContainer.transform);
            column.GetComponentInChildren<Image>().sprite = itemDef.Sprite;
            TextMeshProUGUI difficultyReductionText = column.GetComponentInChildren<TextMeshProUGUI>();
            if (itemSlot.HasCustomDifficultyReductions)
            {
                difficultyReductionText.text = itemSlot.GetDifficultyReduction(itemDef).ToString();
            }
            else
            {
                // Just show the item sprite without a difficulty reduction value if there are no custom reductions, since the difficulty modifier below will apply to all accepted items equally
                difficultyReductionText.gameObject.SetActive(false);
                column.GetComponent<LayoutElement>().preferredHeight = 50;
            }
        }
        // Difficulty modifier
        if (itemSlot.DefaultDifficultyReduction != 0 && !itemSlot.HasCustomDifficultyReductions)
        {
            DifficultyModifierText.text = $"Difficulty Modifier: {itemSlot.DefaultDifficultyReduction}";
            DifficultyModifierText.gameObject.SetActive(true);
        }
        else DifficultyModifierText.gameObject.SetActive(false);

        // Destruction chance
        if (itemSlot.DestructionChance > 0f)
        {
            DestructionChanceText.text = $"Destruction Chance: {itemSlot.DestructionChance * 100f:0}%";
            DestructionChanceText.gameObject.SetActive(true);
        }
        else DestructionChanceText.gameObject.SetActive(false);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
