using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class ItemRenderer : MonoBehaviour
{
    public Item Item { get; private set; }

    // Components
    private SpriteRenderer SpriteRenderer;
    private Rigidbody2D Rigidbody;
    private PolygonCollider2D Collider;
    private HingeJoint2D DragHinge;
    private Rigidbody2D DragAnchorBody;
    private GameObject DragAnchorObj;

    // State
    public bool IsFrozen => Rigidbody.bodyType == RigidbodyType2D.Static;
    public bool IsRenderingAboveUI { get; private set; }

    // Glow
    public bool IsForcedGlowing { get; private set; }

    // Cached sorting order for drag and drop
    private int SortingOrder;

    public void Init(Item item)
    {
        Item = item;
        SpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        SpriteRenderer.material = ResourceManager.LoadMaterial("Materials/ItemMaterial");
        SpriteRenderer.sprite = item.Sprite;
        SetSortingOrder(1000);
        Rigidbody = gameObject.AddComponent<Rigidbody2D>();
        Collider = gameObject.AddComponent<PolygonCollider2D>();
        ScaleCollider(Collider, 0.9f);
    }

    private void ScaleCollider(PolygonCollider2D collider, float scale)
    {
        for (int i = 0; i < collider.pathCount; i++)
        {
            Vector2[] path = collider.GetPath(i);
            for (int j = 0; j < path.Length; j++)
                path[j] *= scale;
            collider.SetPath(i, path);
        }
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
    public void SetRandomRotation()
    {
        float angle = Random.Range(0f, 360f);
        SetRotation(angle);
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

    public void SetSortingOrder(int order)
    {
        SpriteRenderer.sortingOrder = order;
        SortingOrder = order;
    }

    #region Drag Rendering

    public void SetRenderAboveUI(bool above)
    {
        if (above)
        {
            SpriteRenderer.sortingLayerName = "UI";
            SpriteRenderer.sortingOrder = 1000;
            IsRenderingAboveUI = true;
        }
        else
        {
            SpriteRenderer.sortingLayerName = "Default";
            SpriteRenderer.sortingOrder = SortingOrder;
            IsRenderingAboveUI = false;
        }
    }

    public void SetColliderEnabled(bool enabled) => Collider.enabled = enabled;

    #endregion

    #region Drag Physics (Pendulum)

    private float OriginalAngularDrag;

    /// <summary>
    /// Creates a hinge joint at the local grab point so the item swings like a pendulum while dragged.
    /// </summary>
    public void StartDragPhysics(Vector2 grabWorldPos)
    {
        // Create an invisible kinematic anchor that the mouse will move
        DragAnchorObj = new GameObject("DragAnchor");
        DragAnchorObj.transform.position = grabWorldPos;
        DragAnchorBody = DragAnchorObj.AddComponent<Rigidbody2D>();
        DragAnchorBody.bodyType = RigidbodyType2D.Kinematic;

        // Make the item dynamic so physics applies
        Rigidbody.bodyType = RigidbodyType2D.Dynamic;
        Rigidbody.gravityScale = 1f;

        // Increase angular drag while dragging to prevent excessive spinning
        OriginalAngularDrag = Rigidbody.angularDamping;
        Rigidbody.angularDamping = 5f;

        // Create a hinge joint on the item, connected to the anchor
        DragHinge = gameObject.AddComponent<HingeJoint2D>();
        DragHinge.connectedBody = DragAnchorBody;
        DragHinge.autoConfigureConnectedAnchor = false;

        // Anchor on the item side = local space of the grab point
        Vector2 localGrab = transform.InverseTransformPoint(grabWorldPos);
        DragHinge.anchor = localGrab;
        DragHinge.connectedAnchor = Vector2.zero; // center of the anchor body
    }

    /// <summary>
    /// Moves the drag anchor to the given world position. The hinge joint makes the item swing naturally.
    /// </summary>
    public void UpdateDragAnchor(Vector2 worldPos)
    {
        if (DragAnchorBody != null)
            DragAnchorBody.MovePosition(worldPos);
    }

    /// <summary>
    /// Destroys the hinge joint and anchor. The item keeps its current velocity/angular velocity for inertia.
    /// </summary>
    public void StopDragPhysics()
    {
        Rigidbody.angularDamping = OriginalAngularDrag;

        if (DragHinge != null)
        {
            Object.Destroy(DragHinge);
            DragHinge = null;
        }
        if (DragAnchorObj != null)
        {
            Object.Destroy(DragAnchorObj);
            DragAnchorObj = null;
            DragAnchorBody = null;
        }
    }

    #endregion

    public void ResetVelocity()
    {
        Rigidbody.linearVelocity = Vector2.zero;
        Rigidbody.angularVelocity = 0f;
    }

    public void ClampVelocity(float maxSpeed, float maxAngularSpeed)
    {
        if (Rigidbody.linearVelocity.magnitude > maxSpeed)
            Rigidbody.linearVelocity = Rigidbody.linearVelocity.normalized * maxSpeed;
        Rigidbody.angularVelocity = Mathf.Clamp(Rigidbody.angularVelocity, -maxAngularSpeed, maxAngularSpeed);
    }

    public void Show()
    {
        SpriteRenderer.enabled = true;
        Collider.enabled = true;
    }

    public void Hide()
    {
        SpriteRenderer.enabled = false;
        Collider.enabled = false;
    }
}
