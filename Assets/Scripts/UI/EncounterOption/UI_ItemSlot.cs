using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public ItemSlot ItemSlot { get; private set; }
    public UI_EncounterDisplay EncounterDisplay => UI_EncounterDisplay.Instance;

    [Header("Elements")]
    public Image Background;
    public Image ItemIcon;
    public TextMeshProUGUI ItemLabelText;

    public Image IsRequiredIndicator;
    public Image DestroyedIndicator;

    private bool ShowRequiredIndicator => ItemSlot.IsRequired;
    private bool ShowDestroyedIndicator => ItemSlot.IsDestroyingItem;

    // Preview cycling
    private const float CYCLE_INTERVAL = 1f;
    private float CycleTimer;
    private int PreviewIndex;
    private List<ItemDef> SlottableItemDefs;

    public void Init(ItemSlot itemSlot)
    {
        ItemSlot = itemSlot;

        // Show/Hide elements that never change
        IsRequiredIndicator.gameObject.SetActive(ShowRequiredIndicator);
        DestroyedIndicator.gameObject.SetActive(ShowDestroyedIndicator);

        Refresh();

        // Init cycling state for unfilled preview
        SlottableItemDefs = ItemSlot.GetSlottableItemDefs();
        PreviewIndex = 0;
        CycleTimer = 0f;
        if (!ItemSlot.IsFilled && SlottableItemDefs.Count > 0)
        {
            UpdatePreviewDisplay(SlottableItemDefs[0]);
        }
    }

    public void Refresh()
    {
        // Filled
        if (ItemSlot.IsFilled) SetFilledDisplay();

        // Unfilled
        else SetUnfilledDisplay();

        // Item preview handled in update (changing item icon, modifier text, destruction chance text)
    }

    private void SetFilledDisplay()
    {
        Background.color = ResourceManager.Color_Option_Slot_Filled;
        ItemIcon.sprite = ItemSlot.FilledItem.Def.Sprite;
        ItemIcon.material = ResourceManager.LoadMaterial("Materials/UI/ItemSlotIconMaterial_Filled");
        ItemIcon.color = Color.white;
        DestroyedIndicator.color = new Color(0.78f, 0.37f, 0.32f);
        UpdatePreviewDisplay(ItemSlot.FilledItem.Def);
    }

    private void SetUnfilledDisplay()
    {
        // Background is red if the player has no items that can be slotted here (only if required), otherwise white
        bool playerHasSlottableItems = ItemSlot.PlayerHasSlottableItem();
        Background.color = (ItemSlot.IsRequired && !playerHasSlottableItems) ? ResourceManager.Color_Option_Slot_Unmet : Color.white;

        // Sprite handled in Update (cycling through possible items that can be dragged into this slot)
        ItemIcon.material = ResourceManager.LoadMaterial("Materials/UI/ItemSlotIconMaterial_Unfilled");
        ItemIcon.color = new Color(1f, 1f, 1f, 0.3f);
        Color greyedOutIndicatorColor = new Color(0.5f, 0.5f, 0.5f);
        DestroyedIndicator.color = greyedOutIndicatorColor;
    }

    private void Update()
    {
        if (ItemSlot.IsFilled) return;
        if (SlottableItemDefs.Count == 0) return;

        CycleTimer += Time.deltaTime;
        if (CycleTimer >= CYCLE_INTERVAL)
        {
            CycleTimer -= CYCLE_INTERVAL;

            PreviewIndex = (PreviewIndex + 1) % SlottableItemDefs.Count;
            UpdatePreviewDisplay(SlottableItemDefs[PreviewIndex]);
        }
    }

    private void UpdatePreviewDisplay(ItemDef itemDef)
    {
        ItemIcon.sprite = itemDef.Sprite;
        ItemLabelText.text = itemDef.LabelCap;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right && ItemSlot.IsFilled)
        {
            ItemSlot.Empty();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ItemDragDropManager.HoveredItemSlot = this;
        EncounterDisplay.OnItemSlotHovered(ItemSlot);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemDragDropManager.HoveredItemSlot == this)
            ItemDragDropManager.HoveredItemSlot = null;
        EncounterDisplay.OnItemSlotUnhovered();
    }

    public void SetDragGreyedOut(bool greyedOut)
    {
        if (greyedOut)
            Background.color = ResourceManager.Color_Button_Disabled;
        else
            Refresh();
    }

    public void SetDragHighlighted(bool highlighted)
    {
        if (highlighted)
            Background.color = ResourceManager.Color_Button_Default;
        else
            Refresh();
    }
}
