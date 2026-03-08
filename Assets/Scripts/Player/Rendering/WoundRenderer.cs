using UnityEngine;
using UnityEngine.U2D;

public class WoundRenderer : MonoBehaviour
{
    public Wound Wound { get; private set; }

    [Header("SpriteRenderers")]
    public SpriteRenderer WoundSpriteRenderer;
    public SpriteRenderer TendOverlaySpriteRenderer;

    public void SetWound(Wound wound)
    {
        Wound = wound;
        Refresh();
    }

    public void Refresh()
    {
        if (Wound == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // Wound is present
        gameObject.SetActive(true);
        WoundSpriteRenderer.gameObject.SetActive(true);
        WoundSpriteRenderer.sprite = GetWoundSprite();
        TendOverlaySpriteRenderer.gameObject.SetActive(Wound.IsTended);
        TendOverlaySpriteRenderer.sprite = Wound.SpriteTended;
    }

    private Sprite GetWoundSprite()
    {
        return Wound.InfectionStage switch
        {
            InfectionStage.None => Wound.SpriteBase,
            InfectionStage.Minor => Wound.SpriteInfectMinor,
            InfectionStage.Major => Wound.SpriteInfectMajor,
            _ => throw new System.Exception("Infection stage " + Wound.InfectionStage.ToString() + " not handled.")
        };
    }
}
