using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_RequirementsRow : MonoBehaviour
{
    [Header("Elements")]
    public Image MetIcon;
    public TextMeshProUGUI Label;

    public void Init(string label, bool isMet)
    {
        Label.text = label;
        MetIcon.sprite = isMet ? ResourceManager.LoadSprite("UiSprites/Checkmark") : ResourceManager.LoadSprite("UiSprites/Cross");
    }
}
