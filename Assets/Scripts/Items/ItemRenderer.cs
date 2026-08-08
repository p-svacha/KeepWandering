using UnityEngine;

public class ItemRenderer : MonoBehaviour
{
    public Item Item { get; private set; }

    public const string DEFAULT_LAYER_NAME = "Default";

    // Components
    public SpriteRenderer SpriteRenderer { get; private set; }
    private Rigidbody2D Rigidbody;
    private PolygonCollider2D Collider;
    private HingeJoint2D DragHinge;
    private Rigidbody2D DragAnchorBody;
    private GameObject DragAnchorObj;

    // State
    public bool IsFrozen => Rigidbody.bodyType == RigidbodyType2D.Static;
    public bool IsRenderingAboveUI { get; private set; }

    // Cached sorting order for drag and drop
    private int SortingOrder;

    // Collisions
    private const float COLLISION_MIN_SPEED = 0.6f;   // relative velocity below which no sound plays (settling/resting contact)
    private const float COLLISION_MAX_SPEED = 7f;      // relative velocity at/above which the sound is at full volume
    private const float COLLISION_SOUND_COOLDOWN = 0.08f; // minimum time between two collision sounds from the same item

    private float LastCollisionSoundTime = -999f;

    public void Init(Item item)
    {
        Item = item;
        SpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        SpriteRenderer.material = ResourceManager.LoadMaterial("Materials/ItemMaterial");
        SpriteRenderer.sprite = item.Sprite;
        SetSortingOrder(1000);
        Rigidbody = gameObject.AddComponent<Rigidbody2D>();
        Collider = gameObject.AddComponent<PolygonCollider2D>();
        ScaleCollider(Collider, 0.875f);
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

    public void Highlight(Color color)
    {
        SpriteRenderer.material.SetFloat("_IsGlowing", 1);
        SpriteRenderer.material.SetColor("_GlowColor", color);
    }

    public void Unhighlight()
    {
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
            SpriteRenderer.sortingLayerName = DEFAULT_LAYER_NAME;
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

    #region Collision Sound

    private void OnCollisionEnter2D(Collision2D collision)
    {
        float impactSpeed = collision.relativeVelocity.magnitude;
        if (impactSpeed < COLLISION_MIN_SPEED) return;
        if (Time.time - LastCollisionSoundTime < COLLISION_SOUND_COOLDOWN) return;

        ItemRenderer other = collision.collider.GetComponent<ItemRenderer>();

        // Both sides of an item-item collision fire this callback for the same event - only let one of
        // them actually play a sound, picked deterministically so both sides agree on which one "wins".
        if (other != null && other.GetEntityId() < GetEntityId()) return;

        LastCollisionSoundTime = Time.time;

        float t = Mathf.InverseLerp(COLLISION_MIN_SPEED, COLLISION_MAX_SPEED, impactSpeed);
        float volume = Mathf.Lerp(0.15f, 1f, t);
        float pitch = Mathf.Lerp(1.05f, 0.85f, t); // harder hits read as slightly lower-pitched/heavier

        // Reuses each item's existing unique clip. Picks whichever of the two items is heavier-sounding
        // by simply alternating/random choice between the two - either item's own clack is a fine stand-in
        // for "two things hit each other."
        Item soundSource = (other != null && Random.value < 0.5f) ? other.Item : Item;
        AudioManager.PlayItemSound(soundSource, volume, pitch, pitchVariance: 0.1f);
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
