using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen overlay that plays a short animated roll sequence for skill checks: a marker
/// bounces along the same horizontal outcome bar shown in option details, then settles on the
/// actual rolled value. Blocks input while active and can be skipped by click/key.
/// </summary>
public class UI_SkillCheckRollSequence : MonoBehaviour
{
    public const float FULL_SPEED_DURATION = 1f;
    public const float SETTLE_DURATION = 2.5f;
    public const float BOUNCE_SPEED = 6f; // normalized bar units per second
    public const float HOLD_AFTER_LANDING = 0.8f;
    private const float DECEL_F_PRIME_AT_ZERO = 3f; // f'(0) for f(τ)=1-(1-τ)^3

    public const float POST_JUMP_DELAY = 0.8f;
    private const float PUNCH_DURATION = 0.5f;
    private const float PUNCH_SCALE = 1.3f;

    private Coroutine PunchCoroutine;

    [Header("Elements")]
    public TextMeshProUGUI LabelText;
    public GameObject UsedItemsContainer;
    public GameObject OutcomeBarContainer;
    public RectTransform Marker;

    public GameObject RollModifierInfo;
    public Image RollModifierIcon;
    public TextMeshProUGUI RollModifierText;

    [Header("Prefabs")]
    public GameObject UsedItemPrefab;
    public Image BarSegmentPrefab;

    private Coroutine RollCoroutine;
    private Action OnComplete;
    private bool SkipRequested;
    private bool MarkerGoingRight;

    // Audio
    private const string BOUNCE_SOUND = "PutDown";

    public void Play(SkillCheckOption option, int rollValue, Action onComplete)
    {
        OnComplete = onComplete;
        SkipRequested = false;
        gameObject.SetActive(true);

        LabelText.text = option.Text;
        HelperFunctions.DestroyAllChildredImmediately(UsedItemsContainer);
        foreach (Item usedItem in Game.Instance.ItemsUsedInSelectedOption)
        {
            GameObject elem = Instantiate(UsedItemPrefab, UsedItemsContainer.transform);
            elem.transform.GetChild(0).GetComponent<Image>().sprite = usedItem.Sprite;
        }

        BuildBar(option.GetOutcomeChances());

        if (RollCoroutine != null) StopCoroutine(RollCoroutine);
        RollCoroutine = StartCoroutine(PlayRollSequence(option));
        
        RollModifierInfo.SetActive(false);
        if (PunchCoroutine != null) StopCoroutine(PunchCoroutine); PunchCoroutine = null;
    }

    private void BuildBar(List<SkillCheckOutcomeChance> outcomes)
    {
        Marker.transform.SetAsFirstSibling(); // So it doesn't get destroyed with skipElements
        HelperFunctions.DestroyAllChildredImmediately(OutcomeBarContainer, skipElements: 1);
        foreach (SkillCheckOutcomeChance outcome in outcomes)
        {
            Image barSegment = Instantiate(BarSegmentPrefab, OutcomeBarContainer.transform);
            barSegment.color = outcome.Outcome.Color;
            barSegment.GetComponent<RectTransform>().anchorMin = new Vector2((outcome.MinRoll - 1) / 100f, 0);
            barSegment.GetComponent<RectTransform>().anchorMax = new Vector2(outcome.MaxRoll / 100f, 1);
        }
        Marker.transform.SetAsLastSibling(); // So it appears on top of the bar segments
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) SkipRequested = true;
    }

    private IEnumerator PlayRollSequence(SkillCheckOption option)
    {
        AudioManager.PlaySound("SwooshQuick");

        int rollValue = option.LastRoll;
        float target = Mathf.Clamp01(rollValue / 100f);
        float decelDistance = BOUNCE_SPEED * SETTLE_DURATION / DECEL_F_PRIME_AT_ZERO; // can exceed 1 now - that's fine

        float U0 = UnityEngine.Random.Range(0.15f, 0.85f);
        float s = UnityEngine.Random.value < 0.5f ? 1f : -1f;

        // Closed-form solve: find earliest t >= FULL_SPEED_DURATION where continuing for
        // `decelDistance` more (same direction s) lands exactly on a preimage of target.
        float bestT = float.MaxValue;
        float period = 2f / BOUNCE_SPEED;
        foreach (float basePreimage in new float[] { target, 2f - target })
        {
            float tAtKZero = (basePreimage - U0 - s * decelDistance) / (s * BOUNCE_SPEED);
            float kOffset = Mathf.Ceil((FULL_SPEED_DURATION - tAtKZero) / period);
            float candidateT = tAtKZero + kOffset * period;
            if (candidateT >= FULL_SPEED_DURATION && candidateT < bestT) bestT = candidateT;
        }

        // Phase 1
        float t = 0f;
        float unfolded = U0;
        while (t < bestT && !SkipRequested)
        {
            t += Time.deltaTime;
            unfolded = U0 + s * BOUNCE_SPEED * t;

            bool prevMarkerGoingRight = MarkerGoingRight;
            MarkerGoingRight = Mathf.Abs(unfolded % 2f) <= 1f;
            if (prevMarkerGoingRight != MarkerGoingRight) AudioManager.PlaySound(BOUNCE_SOUND);

            SetMarkerPosition(Fold(unfolded));
            yield return null;
        }
        float decelStart = U0 + s * BOUNCE_SPEED * bestT; // snap exactly, no per-frame drift

        // Phase 2 - still just Fold() of a moving unfolded coordinate, so it can keep bouncing
        // off the edges during the slowdown. Lands exactly on target since decelStart was
        // chosen to be exactly decelDistance away from a true preimage.
        float elapsed = 0f;
        while (elapsed < SETTLE_DURATION && !SkipRequested)
        {
            elapsed += Time.deltaTime;
            float normalizedT = Mathf.Clamp01(elapsed / SETTLE_DURATION);
            float easedT = 1f - Mathf.Pow(1f - normalizedT, 3f);
            float unfolded2 = decelStart + s * decelDistance * easedT;

            bool prevMarkerGoingRight = MarkerGoingRight;
            MarkerGoingRight = Mathf.Abs(unfolded2 % 2f) <= 1f;
            if (prevMarkerGoingRight != MarkerGoingRight) AudioManager.PlaySound(BOUNCE_SOUND);

            SetMarkerPosition(Fold(unfolded2));
            yield return null;
        }

        SetMarkerPosition(target); // settle at the raw, unmodified roll

        // Figure out where the marker needs to end up even if the sequence gets skipped
        int finalRollValue = option.LastRoll;
        if (option.AppliedRollModifiers.Count > 0) finalRollValue = option.AppliedRollModifiers[^1].NewRollValue;

        if (!SkipRequested)
        {
            Queue<(HealthCondition Condition, int NewRollValue)> pendingModifiers = new(option.AppliedRollModifiers);

            while (!SkipRequested)
            {
                yield return new WaitForSeconds(POST_JUMP_DELAY);
                if (SkipRequested || pendingModifiers.Count == 0) break;

                var (condition, newRollValue) = pendingModifiers.Dequeue();

                // Jump straight to the modified position - no easing, this is a snap
                SetMarkerPosition(Mathf.Clamp01(newRollValue / 100f));
                AudioManager.PlaySound("Bonk");

                RollModifierInfo.SetActive(true);
                RollModifierText.text = condition.GetReportLabel();
                RollModifierText.color = condition.GetReportTextColor();
                RollModifierIcon.sprite = condition.Sprite;

                if (PunchCoroutine != null) StopCoroutine(PunchCoroutine);
                PunchCoroutine = StartCoroutine(PunchScale(RollModifierInfo.GetComponent<RectTransform>()));
            }
        }

        RollModifierInfo.SetActive(false);
        SetMarkerPosition(Mathf.Clamp01(finalRollValue / 100f)); // guaranteed-correct final position, even if skipped

        AudioManager.PlaySound($"Outcome_{option.PendingOutcome.DefName}", pitchVariance: 0.05f);
        if (!SkipRequested) yield return new WaitForSeconds(HOLD_AFTER_LANDING);

        gameObject.SetActive(false);
        RollCoroutine = null;
        Action callback = OnComplete;
        OnComplete = null;
        callback?.Invoke();
    }

    private IEnumerator PunchScale(RectTransform target)
    {
        Vector3 baseScale = Vector3.one;
        target.localScale = baseScale;

        float t = 0f;
        while (t < PUNCH_DURATION)
        {
            t += Time.deltaTime;
            float normalized = t / PUNCH_DURATION;
            float scale = 1f + (PUNCH_SCALE - 1f) * (1f - normalized) * Mathf.Sin(normalized * Mathf.PI);
            target.localScale = baseScale * scale;
            yield return null;
        }

        target.localScale = baseScale;
    }

    private void SetMarkerPosition(float normalized)
    {
        Marker.anchorMin = new Vector2(normalized - 0.005f, 0f);
        Marker.anchorMax = new Vector2(normalized - 0.005f, 1f);
    }

    /// <summary>
    /// Folds any real value into [0,1] via a period-2 triangle wave — equivalent to a particle
    /// bouncing between walls at 0 and 1 at constant speed, but expressed as simple continuous motion.
    /// </summary>
    private static float Fold(float x)
    {
        x = Mathf.Abs(x % 2f);
        return x <= 1f ? x : 2f - x;
    }
}