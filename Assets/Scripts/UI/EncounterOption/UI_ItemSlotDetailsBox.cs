using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ItemSlotDetailsBox : MonoBehaviour
{
    public ItemSlot Slot { get; private set; }

    [Header("Elements")]
    public TextMeshProUGUI TitleText;
    public Image OptionalityFrame;
    public TextMeshProUGUI OptionalityText;
    public GameObject AcceptedItemsContainer;
    public TextMeshProUGUI AcceptedItemsText;
    public GameObject ItemWillBeDestroyedInfo;
    public GameObject ItemDurabilityLossInfo;

    [Header("Prefabs")]
    public UI_SlotAcceptedItemInfo AcceptedItemPrefab;

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
        TitleText.text = $"{Slot.Label()} Slot";
        TitleText.text = $"{Slot.Label()} Slot";

        OptionalityFrame.color = Slot.IsRequired ? new Color(0.37f, 0.98f, 1f) : new Color(1f, 0.98f, 0.5f);
        OptionalityText.text = Slot.IsRequired ? "REQUIRED" : "OPTIONAL";

        // Accepted items
        AcceptedItemsText.text = "Accepted Items";
        if (Slot.Option is SkillCheckOption) AcceptedItemsText.text += "\n<color=#666666>Higher tiers reduce difficulty more</color>";

        HelperFunctions.DestroyAllChildredImmediately(AcceptedItemsContainer);
        foreach (ItemDef itemDef in Slot.GetSlottableItemDefs())
        {
            UI_SlotAcceptedItemInfo itemInfo = Instantiate(AcceptedItemPrefab, AcceptedItemsContainer.transform);
            if(Slot.Tag != null) itemInfo.Init(itemDef, Slot.Tag);
            else itemInfo.Init(itemDef);
        }

        // Destruction / Durability loss
        ItemWillBeDestroyedInfo.SetActive(Slot.IsDestroyingItem);
        ItemDurabilityLossInfo.SetActive(!Slot.IsDestroyingItem);
    }
}
