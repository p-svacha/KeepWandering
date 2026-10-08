using UnityEngine;

/// <summary>
/// Helper script to attach to interactable spots in encounters that makes them invisible to the player.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class MakeSelfTransparent : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        MakeTransparent();
    }
    private void MakeTransparent()
    {
        Color color = spriteRenderer.color;
        color.a = 0f; // Set alpha to 0 for full transparency
        spriteRenderer.color = color;
    }
}
