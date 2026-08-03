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

    private Quaternion RestRotation;
    private bool RestRotationCached;

    public void Render(int stage)
    {
        if (stage == 0) Renderer.sprite = Normal;
        else if (stage == 1) Renderer.sprite = Sprained;
        else if (stage == 2) Renderer.sprite = Cracked;
        else if (stage == 3) Renderer.sprite = Broken;
        else throw new System.Exception("Stage " + stage + " not handled.");
    }

    /// <summary>
    /// Stores the limb's current local rotation as its rest pose. Must be called once before any
    /// twitch offsets are applied, so twitches always originate from and return to the correct pose.
    /// </summary>
    public void CacheRestRotation()
    {
        RestRotation = Renderer.transform.localRotation;
        RestRotationCached = true;
    }

    /// <summary>
    /// Rotates the limb away from its rest pose by the given angle (degrees, around Z).
    /// </summary>
    public void SetTwitchRotationOffset(float angleDegrees)
    {
        if (!RestRotationCached) CacheRestRotation();
        Renderer.transform.localRotation = RestRotation * Quaternion.Euler(0, 0, angleDegrees);
    }

    /// <summary>
    /// Returns the limb to its cached rest rotation.
    /// </summary>
    public void ResetRotation()
    {
        if (RestRotationCached) Renderer.transform.localRotation = RestRotation;
    }
}
