using UnityEngine;

/// <summary>
/// The encounter camera is the main camera showing the game screen.
/// It cannot be controlled by the player, but depending on the encounter it has a different zoom level.
/// </summary>
public class EncounterCamera : MonoBehaviour
{
    public const float DEFAULT_CAMERA_SIZE = 5.4f;
    public const float MAIN_MENU_CAMERA_SIZE = 8.0f;

    public static EncounterCamera Instance { get; private set; }
    public Camera Camera { get; private set; }

    // Zoom Transition
    private bool IsTransitioning;
    private float TransitionDuration;
    private float TransitionCurrentTime;
    private Vector3 TransitionStartPosition;
    private float TransitionStartZoom;
    private Vector3 TransitionTargetPosition;
    private float TransitionTargetZoom;

    private void Awake()
    {
        Instance = this;
        Camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (!IsTransitioning) return;

        TransitionCurrentTime += Time.deltaTime;
        if (TransitionCurrentTime >= TransitionDuration)
        {
            SetZoom(TransitionTargetZoom);
        }
        else
        {
            float t = TransitionCurrentTime / TransitionDuration;
            float easedT = 1f - (1f - t) * (1f - t); // Ease-out quadratic

            Camera.orthographicSize = Mathf.Lerp(TransitionStartZoom, TransitionTargetZoom, easedT);
            Camera.transform.position = Vector3.Lerp(TransitionStartPosition, TransitionTargetPosition, easedT);
        }
    }

    public void SetZoom(float zoomLevel)
    {
        IsTransitioning = false;

        Camera.orthographicSize = zoomLevel;

        // Bottom and left edge should always be the same, regardless of zoom
        // So with bigger camera size, the visible area should expand to the right and top, but the bottom left corner should stay fixed.

        float yPos = zoomLevel - DEFAULT_CAMERA_SIZE;
        float xPos = yPos * Camera.aspect; // Adjust x position based on aspect ratio to keep the bottom left corner fixed
        Camera.transform.position = new Vector3(xPos, yPos, Camera.transform.position.z);
    }

    /// <summary>
    /// Starts a smooth camera transition from a fixed start position/zoom to the given target zoom level over the specified duration.
    /// </summary>
    public void StartZoomTransition(Vector2 startPosition, float targetZoomLevel, float duration)
    {
        // Calculate target state
        float targetYPos = targetZoomLevel - DEFAULT_CAMERA_SIZE;
        float targetXPos = targetYPos * Camera.aspect;
        TransitionTargetPosition = new Vector3(targetXPos, targetYPos, Camera.transform.position.z);
        TransitionTargetZoom = targetZoomLevel;

        // Set start state
        TransitionStartPosition = new Vector3(startPosition.x, startPosition.y, Camera.transform.position.z);
        TransitionStartZoom = DEFAULT_CAMERA_SIZE;
        Camera.orthographicSize = TransitionStartZoom;
        Camera.transform.position = TransitionStartPosition;

        // Start transition
        TransitionCurrentTime = 0f;
        TransitionDuration = duration;
        IsTransitioning = true;
    }

    public void SetBackgroundColor(Color color)
    {
        Camera.backgroundColor = color;
    }

    public void SetDefaultZoom() => SetZoom(DEFAULT_CAMERA_SIZE);

    public void SetMainMenu()
    {
        SetZoom(MAIN_MENU_CAMERA_SIZE);
        transform.position = new Vector3(transform.position.x, 25f, transform.position.z);
    }
}
