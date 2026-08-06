using UnityEngine;

/// <summary>
/// The encounter camera is the main camera showing the game screen.
/// It cannot be controlled by the player, but depending on the encounter it has a different zoom level.
/// </summary>
public class EncounterCamera : MonoBehaviourSingleton<EncounterCamera>
{
    public const float DEFAULT_CAMERA_SIZE = 6f;
    public const float DEFAULT_X_OFFSET = 0f;
    public const float MAIN_MENU_CAMERA_SIZE = 8.0f;

    public Camera Camera { get; private set; }
    public SpriteRenderer AmbienceOverlay;

    // Zoom Transition
    public bool IsTransitioning { get; private set; }
    private float TransitionDuration;
    private float TransitionCurrentTime;
    private Vector3 TransitionStartPosition;
    private float TransitionStartZoom;
    private Vector3 TransitionTargetPosition;
    private float TransitionTargetZoom;
    private float TransitionTargetXOffset;
    public event System.Action OnTransitionComplete;

    // Audio
    private const string TRANSITION_SOUND_CLIP = "WindContinuous";
    private const float TRANSITION_SOUND_FADE_IN = 1f;
    private const float TRANSITION_SOUND_END_FADE = 0.15f;
    private const float TRANSITION_SOUND_MIN_SPEED = 0.5f;
    private const float TRANSITION_SOUND_MAX_SPEED = 8f;   // speed at/above which the sound is at full intensity
    private const float TRANSITION_SOUND_SMOOTHING = 15f;  // higher = intensity reacts to speed changes faster
    private Vector3 LastTransitionPosition;
    private float LastTransitionZoom;
    private float CurrentTransitionSoundIntensity;

    private void Awake()
    {
        Camera = GetComponent<Camera>();
    }

    private void Update()
    {
        if (!IsTransitioning) return;

        TransitionCurrentTime += Time.deltaTime;
        if (TransitionCurrentTime >= TransitionDuration)
        {
            SetCameraPosition(TransitionTargetZoom, TransitionTargetXOffset);

            // Intensity should already be at/near 0 by now since camera speed approaches 0 as the ease-out
            // curve flattens near the end - this is just a safety-net fade to catch any smoothing lag, not
            // the primary mechanism for silencing the sound.
            AudioManager.SetContinuousSoundIntensity(TRANSITION_SOUND_CLIP, 0f);
            AudioManager.StopContinuousSound(TRANSITION_SOUND_CLIP, TRANSITION_SOUND_END_FADE);

            OnTransitionComplete?.Invoke();
        }
        else
        {
            float t = TransitionCurrentTime / TransitionDuration;
            float easedT = 1f - (1f - t) * (1f - t);

            float newZoom = Mathf.Lerp(TransitionStartZoom, TransitionTargetZoom, easedT);
            Vector3 newPosition = Vector3.Lerp(TransitionStartPosition, TransitionTargetPosition, easedT);

            // Derive how fast the camera is actually moving this frame (position + zoom combined into one
            // rough speed figure) and drive the wind sound's intensity from it directly, so the sound's
            // volume tracks the camera's real motion instead of a fixed, independent fade timer.
            float posDelta = (newPosition - LastTransitionPosition).magnitude;
            float zoomDelta = Mathf.Abs(newZoom - LastTransitionZoom);
            float speed = (posDelta + zoomDelta) / Mathf.Max(Time.deltaTime, 0.0001f);

            Camera.orthographicSize = newZoom;
            Camera.transform.position = newPosition;

            LastTransitionPosition = newPosition;
            LastTransitionZoom = newZoom;

            float targetIntensity = Mathf.InverseLerp(TRANSITION_SOUND_MIN_SPEED, TRANSITION_SOUND_MAX_SPEED, speed);
            CurrentTransitionSoundIntensity = Mathf.Lerp(CurrentTransitionSoundIntensity, targetIntensity, Time.deltaTime * TRANSITION_SOUND_SMOOTHING);
            AudioManager.SetContinuousSoundIntensity(TRANSITION_SOUND_CLIP, CurrentTransitionSoundIntensity);
        }
    }

    public void SetCameraPosition(float zoomLevel, float xOffset)
    {
        IsTransitioning = false;

        Camera.orthographicSize = zoomLevel;

        // Bottom and left edge should always be the same, regardless of zoom
        // So with bigger camera size, the visible area should expand to the right and top, but the bottom left corner should stay fixed.

        float yPos = zoomLevel - DEFAULT_CAMERA_SIZE;
        float xPos = (yPos * Camera.aspect) + xOffset; // Adjust x position based on aspect ratio to keep the bottom left corner fixed
        Camera.transform.position = new Vector3(xPos, yPos, Camera.transform.position.z);
    }

    /// <summary>
    /// Starts a smooth camera transition from a fixed start position/zoom to the given target zoom level over the specified duration.
    /// </summary>
    public void StartZoomTransition(Vector2 startPosition, float targetZoomLevel, float targetXOffset, float duration)
    {
        // Calculate target state
        float targetYPos = targetZoomLevel - DEFAULT_CAMERA_SIZE;
        float targetXPos = (targetYPos * Camera.aspect) + targetXOffset;
        TransitionTargetPosition = new Vector3(targetXPos, targetYPos, Camera.transform.position.z);
        TransitionTargetZoom = targetZoomLevel;
        TransitionTargetXOffset = targetXOffset;

        // Set start state
        TransitionStartPosition = new Vector3(startPosition.x, startPosition.y, Camera.transform.position.z);
        TransitionStartZoom = DEFAULT_CAMERA_SIZE;
        Camera.orthographicSize = TransitionStartZoom;
        Camera.transform.position = TransitionStartPosition;

        // Start transition
        TransitionCurrentTime = 0f;
        TransitionDuration = duration;
        IsTransitioning = true;

        LastTransitionPosition = TransitionStartPosition;
        LastTransitionZoom = TransitionStartZoom;
        CurrentTransitionSoundIntensity = 0f;
        AudioManager.StartContinuousSound(TRANSITION_SOUND_CLIP, TRANSITION_SOUND_FADE_IN);
        AudioManager.SetContinuousSoundIntensity(TRANSITION_SOUND_CLIP, 0f);
    }

    public void SetBackgroundColor(Color color)
    {
        Camera.backgroundColor = color;
    }

    public void SetAmbienceColor(Color color)
    {
        AmbienceOverlay.color = color;
    }

    public void SetDefaultZoom() => SetCameraPosition(DEFAULT_CAMERA_SIZE, DEFAULT_X_OFFSET);

    public void SetMainMenu()
    {
        SetCameraPosition(MAIN_MENU_CAMERA_SIZE, DEFAULT_X_OFFSET);
        transform.position = new Vector3(transform.position.x, 25f, transform.position.z);

        SetBackgroundColor(TimeOfDayDefOf.Morning.SkyColor);
        SetAmbienceColor(TimeOfDayDefOf.Morning.LightingAmbienceOverlayColor);
    }

    public void StartIntroCameraTransition(float duration)
    {
        if (Camera == null) Camera = GetComponent<Camera>();

        // Target state is default zoom and default position (0, 0, z)
        float targetZoomLevel = EncounterDefOf.MorningEncounter.CameraZoomLevel;
        float targetYPos = targetZoomLevel - DEFAULT_CAMERA_SIZE; // 0
        float targetXPos = (targetYPos * Camera.aspect) + EncounterDefOf.MorningEncounter.CameraXOffset; // 0
        TransitionTargetPosition = new Vector3(targetXPos, targetYPos, Camera.transform.position.z);
        TransitionTargetZoom = targetZoomLevel;
        TransitionTargetXOffset = EncounterDefOf.MorningEncounter.CameraXOffset;

        // Start state is current position and current zoom (8.0f, y = 25f)
        TransitionStartPosition = Camera.transform.position;
        TransitionStartZoom = Camera.orthographicSize;

        // Start transition
        TransitionCurrentTime = 0f;
        TransitionDuration = duration;
        IsTransitioning = true;

        LastTransitionPosition = TransitionStartPosition;
        LastTransitionZoom = TransitionStartZoom;
        CurrentTransitionSoundIntensity = 0f;
        AudioManager.StartContinuousSound(TRANSITION_SOUND_CLIP, TRANSITION_SOUND_FADE_IN);
        AudioManager.SetContinuousSoundIntensity(TRANSITION_SOUND_CLIP, 0f);
    }
}
