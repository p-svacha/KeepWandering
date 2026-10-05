using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class UI_SlotAcceptedItemInfo : MonoBehaviour
{
    [Header("Elements")]
    public Image ItemIcon;
    public GameObject TierContainer;

    /// <summary>
    /// Shows an item and its tier level for a specific tag.
    /// </summary>
    public void Init(ItemDef item, ItemTagDef tag)
    {
        BaseInit(item);
        TierContainer.SetActive(true);
        for(int i = 0; i < ItemDef.DEFAULT_MAX_TAG_LEVEL; i++)
        {
            TierContainer.transform.GetChild(i).gameObject.SetActive(i < item.Tags[tag]);
        }
    }

    /// <summary>
    /// Shows an item without further info.
    /// </summary>
    public void Init(ItemDef item)
    {
        BaseInit(item);
        TierContainer.SetActive(false);
        GetComponent<LayoutElement>().preferredHeight = 50;
    }

    private void BaseInit(ItemDef item)
    {
        ItemIcon.sprite = item.Sprite;

        // If the player doesn't have the item, black out the icon.
        if (!Game.Instance.PlayerHasItem(item))
        {
            ItemIcon.color = Color.black;
        }
    }
}
