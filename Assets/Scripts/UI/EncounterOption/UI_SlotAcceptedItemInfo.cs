using UnityEngine;
using UnityEngine.UI;

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
        ItemIcon.sprite = item.Sprite;
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
        ItemIcon.sprite = item.Sprite;
        TierContainer.SetActive(false);
        GetComponent<LayoutElement>().preferredHeight = 50;
    }
}
