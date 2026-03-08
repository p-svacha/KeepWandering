using UnityEngine;

public class LimbRenderer : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite Normal;
    public Sprite Sprained;
    public Sprite Cracked;
    public Sprite Broken;

    [Header("Renderer")]
    public SpriteRenderer Renderer;

    public void Render(int stage)
    {
        if (stage == 0) Renderer.sprite = Normal;
        else if (stage == 1) Renderer.sprite = Sprained;
        else if (stage == 2) Renderer.sprite = Cracked;
        else if (stage == 3) Renderer.sprite = Broken;
        else throw new System.Exception("Stage " + stage + " not handled.");
    }
}
