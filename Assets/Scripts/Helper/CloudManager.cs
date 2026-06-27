using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Manages dynamic spawning, movement, and lifecycle of background clouds in the scene.
/// </summary>
public class CloudManager : Singleton<CloudManager>
{
    [Header("Base Settings (with Sliders)")]
    [Tooltip("Base scale of the spawned clouds. A slight random variation will always be applied.")]
    [Range(1.5f, 5.0f)]
    public float baseScale = 3.00f;

    [Tooltip("Base speed of the moving clouds. A slight random variation will always be applied.")]
    [Range(0.1f, 5.0f)]
    public float baseSpeed = 1.0f;

    [Tooltip("Base grayscale tint of the clouds (0 = black, 1 = white). A slight random variation will always be applied.")]
    [Range(0.0f, 1.0f)]
    public float baseGrayscale = 0.9f;

    [Tooltip("General density/frequency of clouds in the sky. Higher density means more frequent spawns.")]
    [Range(1f, 50.0f)]
    public float cloudDensity = 10.0f;

    [Header("Rendering Settings")]
    [Tooltip("The Sorting Layer Name for the cloud sprites.")]
    public string sortingLayerName = "Background";
    [Tooltip("The Sorting Order inside the sorting layer for the clouds.")]
    public int sortingOrder = -1;

    [Header("Spawn Bounds")]
    public float spawnX = 50.0f;
    public float destroyX = -20.0f;

    private float spawnTimer = 0f;
    private const int MaxCloudSpriteIndex = 14;

    public void SetDefaultCloudSettings()
    {
        baseSpeed = 0.4f;
        baseScale = 2f;
        baseGrayscale = 0.93f;
        cloudDensity = 10f;

        ResetClouds();
    }

    /// <summary>
    /// Clears all clouds and spawns new initial ones according to the current settings.
    /// </summary>
    public void ResetClouds()
    {
        ClearAllClouds();
        PrewarmClouds();
    }

    public void ClearAllClouds()
    {
        HelperFunctions.DestroyAllChildredImmediately(gameObject);
    }

    /// <summary>
    /// Spawns initial clouds across the screen so the sky is not empty on start.
    /// </summary>
    private void PrewarmClouds()
    {
        int initialCount = Mathf.RoundToInt(5f * cloudDensity);
        for (int i = 0; i < initialCount; i++)
        {
            float randomX = Random.Range(destroyX, spawnX);
            SpawnCloud(randomX);
        }
        ResetSpawnTimer();
    }

    private void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnCloud(spawnX);
            ResetSpawnTimer();
        }
    }

    

    /// <summary>
    /// Resets the spawn timer based on density, with slight random interval variation.
    /// </summary>
    private void ResetSpawnTimer()
    {
        float baseInterval = 12.0f / Mathf.Max(cloudDensity, 0.1f);
        spawnTimer = baseInterval * Random.Range(0.8f, 1.2f);
    }

    /// <summary>
    /// Spawns a single random cloud sprite at the specified X position with variations.
    /// </summary>
    private void SpawnCloud(float startX)
    {
        int spriteIndex = Random.Range(1, MaxCloudSpriteIndex + 1);
        string resourcePath = $"Backgrounds/Clouds/Cloud_{spriteIndex}";

        Sprite cloudSprite = ResourceManager.LoadSprite(resourcePath);

        // Create cloud GameObject
        GameObject cloudObj = new GameObject($"Cloud_{spriteIndex}");
        cloudObj.transform.SetParent(transform);

        // Calculate and apply slight variations to properties

        // Y Position
        float randomY = Random.Range(4.5f, 40f);
        cloudObj.transform.position = new Vector3(startX, randomY, 0f);

        // Scale
        float randomScale = baseScale + Random.Range(-0.5f, 0.5f);
        randomScale = Mathf.Clamp(randomScale, 1.5f, 3.0f);
        cloudObj.transform.localScale = new Vector3(randomScale, randomScale, 1f);

        // Add SpriteRenderer
        SpriteRenderer sr = cloudObj.AddComponent<SpriteRenderer>();
        sr.sprite = cloudSprite;
        sr.sortingLayerName = sortingLayerName;
        sr.sortingOrder = sortingOrder;

        // Flip
        sr.flipX = Random.value < 0.5f;
        sr.flipY = Random.value < 0.5f;

        // Color: slight variation in grayscale (tinting)
        float randomGray = baseGrayscale + Random.Range(-0.04f, 0.04f);
        randomGray = Mathf.Clamp01(randomGray);
        sr.color = new Color(randomGray, randomGray, randomGray, 1f);

        // Add movement component with slight variation in speed
        float randomSpeed = baseSpeed * Random.Range(0.8f, 1.2f);
        CloudMovement movement = cloudObj.AddComponent<CloudMovement>();
        movement.speed = randomSpeed;
        movement.destroyX = destroyX;
    }
}

/// <summary>
/// Moves a cloud from right to left at a constant speed, destroying it once it goes past destroyX.
/// </summary>
public class CloudMovement : MonoBehaviour
{
    public float speed;
    public float destroyX;

    private void Update()
    {
        transform.Translate(Vector3.left * (speed * Time.deltaTime));

        if (transform.position.x <= destroyX)
        {
            Destroy(gameObject);
        }
    }
}
