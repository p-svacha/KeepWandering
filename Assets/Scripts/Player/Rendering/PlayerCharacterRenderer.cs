using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Renderer responsible for the visual representation of the player character, such as body parts, health conditions, wounds and more.
/// </summary>
public class PlayerCharacterRenderer : MonoBehaviour
{
    public static PlayerCharacterRenderer Instance;
    public PlayerCharacter Character => Game.Instance.Player;

    [Header("Sprites")]
    public SpriteRenderer Head;

    public GameObject Torso;
    public GameObject DehydrationOverlay;
    public LimbRenderer LegFront;
    public LimbRenderer LegBack;
    public LimbRenderer RightArm;
    public LimbRenderer LeftArm;
    public GameObject PoisonOverlay;

    [Header("Wounds")]
    public GameObject WoundsContainer;
    private List<WoundRenderer> WoundRenderers;

    // Condition Effects
    public const float ElectrocutionTwitchAngle = 8f;      // degrees, multiplied by (stage + 1)
    public const float ElectrocutionJerkDuration = 0.04f;   // duration of each individual jerk
    public const float ElectrocutionJerkIntervalMin = 1f;   // minimum time in seconds between jerks
    public const float ElectrocutionJerkIntervalMax = 10f;   // maximum time in seconds between jerks

    public const float IntoxicationBobAngle = 6f;           // degrees, multiplied by (stage + 1)
    public const float IntoxicationBobSpeed = 0.4f;         // base wander speed, multiplied by (stage + 1)

    private int CurrentElectrocutionStage = -1;
    private List<Coroutine> ElectrocutionCoroutines = new List<Coroutine>();

    private int CurrentIntoxicationStage = -1;
    private Quaternion HeadRestRotation;
    private float IntoxicationNoiseSeed;

    private List<Color> BloodLossColors = new List<Color>()
    {
        Color.white,
        new Color(1f, 0.9f, 0.9f),
        new Color(1f, 0.75f, 0.75f),
        new Color(1f, 0.6f, 0.6f),
    };

    private void Awake()
    {
        Instance = this;
    }

    public void Init()
    {
        WoundRenderers = new List<WoundRenderer>();
        List<WoundRenderer> renderers = WoundsContainer.GetComponentsInChildren<WoundRenderer>(true).ToList();
        WoundRenderers.AddRange(renderers);

        // Condition effects
        RightArm.CacheRestRotation();
        LeftArm.CacheRestRotation();
        LegFront.CacheRestRotation();
        LegBack.CacheRestRotation();
        HeadRestRotation = Head.transform.localRotation;
        IntoxicationNoiseSeed = Random.Range(0f, 1000f);
    }

    private void Update()
    {
        UpdateIntoxicationBob();
    }

    /// <summary>
    /// Refreshes the visual representation of the player character based on their current health conditions, wounds, and other relevant factors.
    /// </summary>
    public void Refresh()
    {
        if (Game.Instance.State == GameState.Initializing) return;
        Debug.Log("Refreshing player character renderer.");

        // Thirst overlay
        SetActiveSprite(DehydrationOverlay, Character.Thirst.ActiveStageIndex - 2);

        // Hunger overlay
        SetActiveSprite(Torso, Character.Hunger.ActiveStageIndex);

        // Poison overlay
        HealthCondition poison = Character.HealthConditions.FirstOrDefault(hc => hc.Def == HealthConditionDefOf.Poisoning);
        if (poison == null) SetActiveSprite(PoisonOverlay, -1);
        else SetActiveSprite(PoisonOverlay, poison.ActiveStageIndex);

        // Electrocution twitching
        HealthCondition electrocution = Character.HealthConditions.FirstOrDefault(hc => hc.Def == HealthConditionDefOf.Electrocution);
        SetElectrocutionStage(electrocution?.ActiveStageIndex ?? -1);

        // Intoxication head bob
        HealthCondition intoxication = Character.HealthConditions.FirstOrDefault(hc => hc.Def == HealthConditionDefOf.Intoxication);
        CurrentIntoxicationStage = intoxication?.ActiveStageIndex ?? -1;

        // Arms
        RightArm.Render(Character.RightArmFracture?.ActiveStageIndex + 1 ?? 0);
        LeftArm.Render(Character.LeftArmFracture?.ActiveStageIndex + 1 ?? 0);

        // Legs
        LegFront.Render(Character.RightLegFracture?.ActiveStageIndex + 1 ?? 0);
        LegBack.Render(Character.LeftLegFracture?.ActiveStageIndex + 1 ?? 0);

        // Blood loss
        SetCharacterColor(BloodLossColors[Character.Bloodloss.ActiveStageIndex]);
    }

    /// <summary>
    /// Sets all children of the given object to inactive except for the child with the given index, which is set to active.
    /// If index is negative, all children are set to inactive.
    /// </summary>
    public void SetActiveSprite(GameObject obj, int index)
    {
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            obj.transform.GetChild(i).gameObject.SetActive(i == index);
        }
    }

    public WoundRenderer GetUnusedWoundRenderer()
    {
        List<WoundRenderer> unusedWoundRenderers = WoundRenderers.Where(wr => wr.Wound == null).ToList();

        if (unusedWoundRenderers.Count == 0)
        {
            Debug.LogWarning("No unused wound renderers available.");
            return null;
        }

        return unusedWoundRenderers.RandomElement();
    }


    public void SetCharacterColor(Color c)
    {
        for (int i = 0; i < Torso.transform.childCount; i++)
        {
            Torso.transform.GetChild(i).GetComponent<SpriteRenderer>().color = c;
        }
        Head.GetComponent<SpriteRenderer>().color = c;
    }

    #region Electrocution

    private void SetElectrocutionStage(int stage)
    {
        if (stage == CurrentElectrocutionStage) return;
        CurrentElectrocutionStage = stage;

        // Stop any running per-limb loops and snap everything back to rest
        foreach (Coroutine c in ElectrocutionCoroutines)
        {
            if (c != null) StopCoroutine(c);
        }
        ElectrocutionCoroutines.Clear();
        RightArm.ResetRotation();
        LeftArm.ResetRotation();
        LegFront.ResetRotation();
        LegBack.ResetRotation();

        if (stage < 0) return;

        // Start one independent loop per limb, each rolling its own random interval
        LimbRenderer[] limbs = { RightArm, LeftArm, LegFront, LegBack };
        foreach (LimbRenderer limb in limbs)
        {
            ElectrocutionCoroutines.Add(StartCoroutine(ElectrocutionTwitchLoop(limb, stage)));
        }
    }

    private IEnumerator ElectrocutionTwitchLoop(LimbRenderer limb, int stage)
    {
        while (true)
        {
            float wait = Random.Range(ElectrocutionJerkIntervalMin, ElectrocutionJerkIntervalMax);
            yield return new WaitForSeconds(wait);
            yield return TwitchLimbOnce(limb, stage);
        }
    }

    /// <summary>
    /// Snaps a single limb to a few random rotations in quick succession, then returns it to rest.
    /// Jerk count and angle both scale with stage, so higher-severity electrocution reads as more violent.
    /// </summary>
    private IEnumerator TwitchLimbOnce(LimbRenderer limb, int stage)
    {
        float maxAngle = ElectrocutionTwitchAngle * (stage + 1);
        int jerkCount = 2 + stage;

        for (int j = 0; j < jerkCount; j++)
        {
            limb.SetTwitchRotationOffset(Random.Range(-maxAngle, maxAngle));
            yield return new WaitForSeconds(ElectrocutionJerkDuration);
        }

        limb.ResetRotation();
    }

    #endregion

    #region Intoxication

    /// <summary>
    /// Continuously wanders the head's rotation using layered Perlin noise. Amplitude and speed scale
    /// with stage; a second, faster noise octave is blended in at higher stages for a more erratic bob.
    /// </summary>
    private void UpdateIntoxicationBob()
    {
        if (CurrentIntoxicationStage < 0)
        {
            Head.transform.localRotation = Quaternion.Slerp(Head.transform.localRotation, HeadRestRotation, Time.deltaTime * 5f);
            return;
        }

        float stageFactor = CurrentIntoxicationStage + 1;
        float speed = IntoxicationBobSpeed * stageFactor;
        float amplitude = IntoxicationBobAngle * stageFactor;

        float baseNoise = Mathf.PerlinNoise(Time.time * speed, IntoxicationNoiseSeed);
        float sharpNoise = Mathf.PerlinNoise(Time.time * speed * 2.3f, IntoxicationNoiseSeed + 100f);

        // Blend in the higher-frequency octave more as severity increases, for added irregularity.
        float irregularity = Mathf.InverseLerp(1f, 2f, stageFactor);
        float combined = Mathf.Lerp(baseNoise, (baseNoise + sharpNoise) * 0.5f, irregularity);

        float angle = (combined * 2f - 1f) * amplitude;
        Head.transform.localRotation = HeadRestRotation * Quaternion.Euler(0, 0, angle);
    }

    #endregion
}
