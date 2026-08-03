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
    private const float WEIGHT_EASE_SPEED = 3f; // how fast condition effects fade in/out on toggle

    private const float ELECTROCUTION_TWITCH_ANGLE = 8f;      // degrees, multiplied by (stage + 1)
    private const float ELECTROCUTION_JERK_DURATION = 0.04f;   // duration of each individual jerk
    private const float ELECTROCUTION_JERK_INTERVAL_MIN = 1f;   // minimum time in seconds between jerks
    private const float ELECTROCUTION_JERK_INTERVAL_MAX = 10f;   // maximum time in seconds between jerks

    private const float INTOXICATION_BOB_ANGLE = 6f;           // degrees, multiplied by (stage + 1)
    private const float INTOXICATION_BOB_SPEED = 0.4f;         // base wander speed, multiplied by (stage + 1)

    private const float EXHAUSTION_SLUMP_ANGLE = -12f;      // head tilt, degrees
    private const float EXHAUSTION_SLUMP_DROP = 0.05f;     // torso sink, world units
    private const float EXHAUSTION_TORSO_ANGLE = -4f;      // torso tilt, degrees
    private Vector2 EXHAUSTION_HEAD_OFFSET = new Vector2(0.3f, -0.1f); // head drop, world units

    private int CurrentElectrocutionStage = -1;
    private List<Coroutine> ElectrocutionCoroutines = new List<Coroutine>();

    private int CurrentIntoxicationStage = -1;
    private float IntoxicationWeight;        // eases 0->1 while active, 1->0 while inactive
    private float IntoxicationNoiseSeed;

    private bool CurrentlyExhausted;
    private float ExhaustionWeight;          // eases 0->1 while active, 1->0 while inactive

    // Cached transform data
    private Vector3 DefaultHeadPosition;
    private Quaternion DefaultHeadRotation;

    private Vector3 DefaultTorsoPosition;
    private Quaternion DefaultTorsoRotation;

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
        // Cache default positions and rotations
        DefaultHeadPosition = Head.transform.localPosition;
        DefaultHeadRotation = Head.transform.localRotation;
        DefaultTorsoPosition = Torso.transform.localPosition;
        DefaultTorsoRotation = Torso.transform.localRotation;

        // Wound renderers
        WoundRenderers = new List<WoundRenderer>();
        List<WoundRenderer> renderers = WoundsContainer.GetComponentsInChildren<WoundRenderer>(true).ToList();
        WoundRenderers.AddRange(renderers);

        // Condition effects
        RightArm.CacheRestRotation();
        LeftArm.CacheRestRotation();
        LegFront.CacheRestRotation();
        LegBack.CacheRestRotation();
        IntoxicationNoiseSeed = Random.Range(0f, 1000f);
    }

    private void Update()
    {
        UpdateHeadAndTorsoEffects();
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

        // Exhaustion slump + breathing
        HealthCondition exhaustion = Character.HealthConditions.FirstOrDefault(hc => hc.Def == HealthConditionDefOf.Exhaustion);
        CurrentlyExhausted = exhaustion != null;

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
            float wait = Random.Range(ELECTROCUTION_JERK_INTERVAL_MIN, ELECTROCUTION_JERK_INTERVAL_MAX);
            yield return new WaitForSeconds(wait);
            yield return TwitchLimbOnce(limb, stage);
        }
    }

    /// <summary>
    /// Snaps a single limb to a few random rotations in quick succession, then returns it to rest.
    /// Jerk count and angle both scale with stage, so higher-severity electrocution reads as more violent.
    /// <br/>Note: electrocution is currently the only effect driving limb rotation. If another condition
    /// ever needs to animate limbs too, this should move to the same additive-contribution pattern used
    /// below for the head and torso, rather than setting rotation directly.
    /// </summary>
    private IEnumerator TwitchLimbOnce(LimbRenderer limb, int stage)
    {
        float maxAngle = ELECTROCUTION_TWITCH_ANGLE * (stage + 1);
        int jerkCount = 2 + stage;

        for (int j = 0; j < jerkCount; j++)
        {
            limb.SetTwitchRotationOffset(Random.Range(-maxAngle, maxAngle));
            yield return new WaitForSeconds(ELECTROCUTION_JERK_DURATION);
        }

        limb.ResetRotation();
    }

    #endregion

    #region Head & Torso Effect Compositing

    /// <summary>
    /// Every condition that affects the head or torso contributes an additive offset (angle, position,
    /// or scale delta) and an independent weight that eases in/out on activation, rather than writing
    /// directly to the transform. This is what lets any number of these effects stack freely and fade
    /// in/out independently, instead of one effect fighting another for control of the same transform.
    /// </summary>
    private void UpdateHeadAndTorsoEffects()
    {
        // Ease each condition's weight toward active (1) or inactive (0)
        IntoxicationWeight = Mathf.MoveTowards(IntoxicationWeight, CurrentIntoxicationStage >= 0 ? 1f : 0f, Time.deltaTime * WEIGHT_EASE_SPEED);
        ExhaustionWeight = Mathf.MoveTowards(ExhaustionWeight, CurrentlyExhausted ? 1f : 0f, Time.deltaTime * WEIGHT_EASE_SPEED);

        // --- Head position: sum of every condition's offset contribution ---
        Vector3 headOffset = Vector3.zero;
        headOffset += new Vector3(EXHAUSTION_HEAD_OFFSET.x, EXHAUSTION_HEAD_OFFSET.y, 0) * ExhaustionWeight;

        Head.transform.localPosition = DefaultHeadPosition + headOffset;

        // --- Head rotation: sum of every condition's angle contribution ---
        float headAngle = 0f;
        headAngle += GetIntoxicationHeadAngleContribution();
        headAngle += EXHAUSTION_SLUMP_ANGLE * ExhaustionWeight;

        Head.transform.localRotation = DefaultHeadRotation * Quaternion.Euler(0, 0, headAngle);

        // --- Torso position: sum of every condition's offset contribution ---
        Vector3 torsoOffset = Vector3.zero;
        torsoOffset += new Vector3(0, -EXHAUSTION_SLUMP_DROP, 0) * ExhaustionWeight;

        Torso.transform.localPosition = DefaultTorsoPosition + torsoOffset;

        // --- Torso rotation: sum of every condition's angle contribution ---
        float torsoAngle = 0f;
        torsoAngle += EXHAUSTION_TORSO_ANGLE * ExhaustionWeight;

        Torso.transform.localRotation = DefaultTorsoRotation * Quaternion.Euler(0, 0, torsoAngle);
    }

    #endregion

    #region Intoxication

    /// <summary>
    /// Returns intoxication's current contribution to head rotation: layered Perlin noise wandering
    /// around 0, weighted so it fades in/out smoothly when the condition is applied or removed. A second,
    /// faster noise octave blends in at higher stages for a more erratic bob.
    /// </summary>
    private float GetIntoxicationHeadAngleContribution()
    {
        if (IntoxicationWeight <= 0f) return 0f;

        // Keep using the last active stage's amplitude/speed while easing out, so the motion doesn't
        // instantly flatten to stage-0 values the moment the condition is removed.
        float stageFactor = Mathf.Max(CurrentIntoxicationStage, 0) + 1;
        float speed = INTOXICATION_BOB_SPEED * stageFactor;
        float amplitude = INTOXICATION_BOB_ANGLE * stageFactor;

        float baseNoise = Mathf.PerlinNoise(Time.time * speed, IntoxicationNoiseSeed);
        float sharpNoise = Mathf.PerlinNoise(Time.time * speed * 2.3f, IntoxicationNoiseSeed + 100f);

        // Blend in the higher-frequency octave more as severity increases, for added irregularity.
        float irregularity = Mathf.InverseLerp(1f, 2f, stageFactor);
        float combined = Mathf.Lerp(baseNoise, (baseNoise + sharpNoise) * 0.5f, irregularity);

        float angle = (combined * 2f - 1f) * amplitude;
        return angle * IntoxicationWeight;
    }

    #endregion
}