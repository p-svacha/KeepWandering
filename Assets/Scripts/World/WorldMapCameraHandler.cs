using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This is the default controls for handling camera movement on the world map.
/// Attach this script to the main camera.
/// </summary>
public class WorldMapCameraHandler : Singleton<WorldMapCameraHandler>
{
    private Game Game;
    public Camera Camera { get; private set; }

    private const float ZOOM_SPEED = 0.45f;
    private const float PAN_SPEED = 20f; // WASD Speed
    private const float MIN_CAMERA_SIZE = 1f;
    private const float MAX_CAMERA_SIZE = 6f;
    public const float DEFAULT_CAMERA_SIZE = 2f;
    private const float EDGE_PADDING = 10f; // Padding from the edge of the map when zooming/panning
    private bool IsLeftMouseDown;
    private bool IsRightMouseDown;
    private bool IsMouseWheelDown;
    private Vector3 DragAnchorWorldPos; // world position under the cursor at drag start, kept pinned to the cursor for the whole drag

    // Size
    private float CameraHeightWorld => Camera.orthographicSize;
    private float CameraWidthWorld => Camera.orthographicSize * Camera.aspect;

    // Bounds
    protected float MinX, MinY, MaxX, MaxY;

    public void SetPosition(Vector2 pos)
    {
        transform.position = new Vector3(pos.x, pos.y, transform.position.z);
    }

    public void SetZoom(float zoom)
    {
        Camera.orthographicSize = zoom;
    }

    public void Init(Game game)
    {
        Game = game;
        Camera = GetComponent<Camera>();
    }


    private void Update()
    {
        if (Program.Instance.State != ProgramState.Game) return;
        if (!Game.WorldMapRenderer.gameObject.activeSelf) return;


        // Scroll - zoom toward cursor position
        if (Input.mouseScrollDelta.y != 0)
        {
            Vector3 worldPosBeforeZoom = Game.WorldMapRenderer.GetCursorWorldPosition();

            Camera.orthographicSize += -Input.mouseScrollDelta.y * ZOOM_SPEED;
            Camera.orthographicSize = Mathf.Clamp(Camera.orthographicSize, MIN_CAMERA_SIZE, MAX_CAMERA_SIZE);

            Vector3 worldPosAfterZoom = Game.WorldMapRenderer.GetCursorWorldPosition();

            // Shift the camera by however much the same screen point's world position moved due to the zoom,
            // so the point under the cursor stays fixed rather than the view zooming toward its center.
            Vector3 worldPosDelta = worldPosBeforeZoom - worldPosAfterZoom;
            transform.position += new Vector3(worldPosDelta.x, worldPosDelta.y, 0f);
        }


        // Dragging with right/middle mouse button - the world point under the cursor at drag-start stays
        // pinned exactly under the cursor for the duration of the drag (re-solved every frame, not speed-based).
        if (Input.GetKeyDown(KeyCode.Mouse2)) { IsMouseWheelDown = true; DragAnchorWorldPos = Game.WorldMapRenderer.GetCursorWorldPosition(); }
        if (Input.GetKeyUp(KeyCode.Mouse2)) IsMouseWheelDown = false;
        if (Input.GetKeyDown(KeyCode.Mouse1)) { IsRightMouseDown = true; DragAnchorWorldPos = Game.WorldMapRenderer.GetCursorWorldPosition(); }
        if (Input.GetKeyUp(KeyCode.Mouse1)) IsRightMouseDown = false;

        if (IsMouseWheelDown || IsRightMouseDown)
        {
            Vector3 cursorWorldNow = Game.WorldMapRenderer.GetCursorWorldPosition();
            Vector3 delta = DragAnchorWorldPos - cursorWorldNow;
            transform.position += delta;
        }

        // Panning with WASD
        if (Input.GetKey(KeyCode.W)) transform.position += new Vector3(0f, PAN_SPEED * Time.deltaTime, 0f);
        if(Input.GetKey(KeyCode.A)) transform.position += new Vector3(-PAN_SPEED * Time.deltaTime, 0f, 0f);
        if(Input.GetKey(KeyCode.S)) transform.position += new Vector3(0f, -PAN_SPEED * Time.deltaTime, 0f);
        if(Input.GetKey(KeyCode.D)) transform.position += new Vector3(PAN_SPEED * Time.deltaTime, 0f, 0f);

        // Drag triggers
        if (Input.GetKeyDown(KeyCode.Mouse0) && !IsLeftMouseDown)
        {
            IsLeftMouseDown = true;
            OnLeftMouseDragStart();
        }
        if (Input.GetKeyUp(KeyCode.Mouse0) && IsLeftMouseDown)
        {
            IsLeftMouseDown = false;
            OnLeftMouseDragEnd();
        }

        // Bounds
        float realMinX = MinX + CameraWidthWorld - EDGE_PADDING;
        float realMaxX = MaxX - CameraWidthWorld + EDGE_PADDING;
        float realMinY = MinY + CameraHeightWorld - EDGE_PADDING;
        float realMaxY = MaxY - CameraHeightWorld + EDGE_PADDING;
        if (transform.position.x < realMinX) transform.position = new Vector3(realMinX, transform.position.y, transform.position.z);
        if (transform.position.x > realMaxX) transform.position = new Vector3(realMaxX, transform.position.y, transform.position.z);
        if (transform.position.y < realMinY) transform.position = new Vector3(transform.position.x, realMinY, transform.position.z);
        if (transform.position.y > realMaxY) transform.position = new Vector3(transform.position.x, realMaxY, transform.position.z);
    }

    public void SetBounds(float minX, float minY, float maxX, float maxY)
    {
        MinX = minX;
        MinY = minY;
        MaxX = maxX;
        MaxY = maxY;
    }

    #region Triggers

    protected virtual void OnLeftMouseDragStart() { }

    protected virtual void OnLeftMouseDragEnd() { }

    #endregion
}
