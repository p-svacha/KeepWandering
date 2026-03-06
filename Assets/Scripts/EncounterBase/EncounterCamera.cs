using UnityEngine;

/// <summary>
/// The encounter camera is the main camera showing the game screen.
/// It cannot be controlled by the player, but depending on the encounter it has a different zoom level.
/// </summary>
public class EncounterCamera : MonoBehaviour
{
    public const float DEFAULT_CAMERA_SIZE = 5.4f;

    public static EncounterCamera Instance { get; private set; }
    public Camera Camera { get; private set; }

    private void Awake()
    {
        Instance = this;
        Camera = GetComponent<Camera>();
    }

    public void SetZoom(float zoomLevel)
    {
        Camera.orthographicSize = zoomLevel;

        // Bottom and left edge should always be the same, regardless of zoom
        // So with bigger camera size, the visible area should expand to the right and top, but the bottom left corner should stay fixed.

        float yPos = zoomLevel - DEFAULT_CAMERA_SIZE;
        float xPos = yPos * Camera.aspect; // Adjust x position based on aspect ratio to keep the bottom left corner fixed
        Camera.transform.position = new Vector3(xPos, yPos, Camera.transform.position.z);
    }

    public void SetBackgroundColor(Color color)
    {
        Camera.backgroundColor = color;
    }

    public void SetDefaultZoom() => SetZoom(DEFAULT_CAMERA_SIZE);
}
