using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class ItemRenderer : MonoBehaviour
{
    public Item Item { get; private set; }

    // Components
    private SpriteRenderer SpriteRenderer;
    private Rigidbody2D Rigidbody;
    private PolygonCollider2D Collider;

    // Glow
    public bool IsForcedGlowing { get; private set; }

    public void Init(Item item)
    {
        Item = item;
        SpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        SpriteRenderer.material = ResourceManager.LoadMaterial("Materials/ItemMaterial");
        SpriteRenderer.sprite = item.Sprite;
        Rigidbody = gameObject.AddComponent<Rigidbody2D>();
        Collider = gameObject.AddComponent<PolygonCollider2D>();
    }

    public void Freeze() => Rigidbody.bodyType = RigidbodyType2D.Static;
    public void Unfreeze() => Rigidbody.bodyType = RigidbodyType2D.Dynamic;

    public void SetPosition(float x, float y)
    {
        transform.position = new Vector3(x, y, 0);
    }
    public void SetRotation(float angle)
    {
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>
    /// Makes the item glow with the specified color. If forced, the unhighlight must removeForce to remove the glow.
    /// </summary>
    public void Highlight(Color color, bool forced = false)
    {
        if (IsForcedGlowing && !forced) return; // Don't override a forced glow with a non-forced one

        IsForcedGlowing = forced;
        SpriteRenderer.material.SetFloat("_IsGlowing", 1);
        SpriteRenderer.material.SetColor("_GlowColor", color);
    }

    public void Unhighlight(bool removeForced = false)
    {
        if (IsForcedGlowing && !removeForced) return; // Don't unhighlight a forced glow with a non-forced unhighlight

        IsForcedGlowing = false;
        SpriteRenderer.material.SetFloat("_IsGlowing", 0);
    }

    public void Show() => SpriteRenderer.enabled = true;
    public void Hide() => SpriteRenderer.enabled = false;
}
