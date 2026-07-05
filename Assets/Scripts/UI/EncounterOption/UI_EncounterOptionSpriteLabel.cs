using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple label that displays the name of a currently hovered sprite, that has encounter options bound to it.
/// </summary>
public class UI_EncounterOptionSpriteLabel : MonoBehaviour
{
    [Header("Elements")]
    public TextMeshProUGUI Label;

    public void Init(string label)
    {
        Label.text = label;

        // Rebuild while active
        Label.ForceMeshUpdate();
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);

    public void SetAnchoredPosition(Vector2 position)
    {
        GetComponent<RectTransform>().anchoredPosition = position;
    }
}
