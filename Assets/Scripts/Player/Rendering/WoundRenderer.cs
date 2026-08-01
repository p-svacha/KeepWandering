using UnityEngine;
using UnityEngine.U2D;

public class WoundRenderer : MonoBehaviour
{
    public Wound Wound { get; private set; }

    [Header("SpriteRenderers")]
    public SpriteRenderer WoundSpriteRenderer;
    public SpriteRenderer BandageOverlaySpriteRenderer;

    public void SetWound(Wound wound)
    {
        Wound = wound;

        // Randomize rotation
        transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

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
        WoundSpriteRenderer.sprite = Wound.GetCurrentSprite();
        BandageOverlaySpriteRenderer.gameObject.SetActive(Wound.IsBandaged);
        BandageOverlaySpriteRenderer.sprite = Wound.SpriteBandaged;
    }
}
