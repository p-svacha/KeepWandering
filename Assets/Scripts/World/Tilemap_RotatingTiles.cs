using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class Tilemap_RotatingTiles : MonoBehaviour
{
    private Tilemap tilemap;

    public float RotationSpeed = 10f; // Degrees per second

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        tilemap.orientation = Tilemap.Orientation.Custom;
    }

    // Update is called once per frame
    void Update()
    {
        // Rotate Z of tilemap orientation matrix
        tilemap.orientationMatrix *= Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, RotationSpeed * Time.deltaTime));
    }
}
