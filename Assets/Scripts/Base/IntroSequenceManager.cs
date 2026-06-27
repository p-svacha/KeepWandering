using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IntroSequenceManager : Singleton<IntroSequenceManager>
{
    public const float INITIAL_PAUSE = 2.0f;
    public const float FADE_IN_DURATION = 1.0f;
    public const float FADE_OUT_DURATION = 1.0f;
    public const float LINE_DELAY = 3f;
    public const float AFTER_REVEAL_PAUSE = 1.5f;
    public const float AFTER_SECTION_PAUSE = 1.0f;
    public const float CAMERA_MOVE_DURATION = 4.0f;
    public const KeyCode SKIP_KEY = KeyCode.Space;

    public bool IsIntroRunning { get; private set; }
    private bool _skipRequested;

    private List<TextMeshProUGUI> sectionALines = new List<TextMeshProUGUI>();
    private List<TextMeshProUGUI> sectionBLines = new List<TextMeshProUGUI>();
    private List<TextMeshProUGUI> sectionCLines = new List<TextMeshProUGUI>();
    private List<GameObject> allTextBoxChildren = new List<GameObject>();

    public TextMeshProUGUI SkipText;

    private void Update()
    {
        if (IsIntroRunning && !_skipRequested && Input.GetKeyDown(SKIP_KEY))
            _skipRequested = true;
    }

    private void Start()
    {
        EnsureCached();
        gameObject.SetActive(false);
    }

    private void EnsureCached()
    {
        if (allTextBoxChildren.Count > 0) return;

        Transform textBox = transform.Find("TextBox");
        if (textBox == null) return;

        allTextBoxChildren.Clear();
        sectionALines.Clear();
        sectionBLines.Clear();
        sectionCLines.Clear();

        for (int i = 0; i < textBox.childCount; i++)
        {
            Transform child = textBox.GetChild(i);
            allTextBoxChildren.Add(child.gameObject);

            var textComp = child.GetComponent<TextMeshProUGUI>();
            if (textComp != null)
            {
                string childName = child.name;
                if (childName.Contains("LineA")) sectionALines.Add(textComp);
                else if (childName.Contains("LineB")) sectionBLines.Add(textComp);
                else if (childName.Contains("LineC")) sectionCLines.Add(textComp);
            }
            child.gameObject.SetActive(false);
        }
    }

    public void StartIntroSequence()
    {
        IsIntroRunning = true;
        _skipRequested = false;
        gameObject.SetActive(true);
        SkipText.gameObject.SetActive(true);

        EnsureCached();

        Game.Instance.StartNewGame();
        EncounterCamera.Instance.SetMainMenu();
        GameUI.Instance.gameObject.SetActive(false);

        StartCoroutine(RunIntroSequenceCoroutine());
    }

    private System.Collections.IEnumerator RunIntroSequenceCoroutine()
    {
        foreach (GameObject child in allTextBoxChildren) child.SetActive(false);

        yield return StartCoroutine(SkippableWait(INITIAL_PAUSE));

        SkipText.gameObject.SetActive(false);

        yield return StartCoroutine(PlaySection(sectionALines));
        yield return StartCoroutine(SkippableWait(AFTER_SECTION_PAUSE));

        yield return StartCoroutine(PlaySection(sectionBLines));
        yield return StartCoroutine(SkippableWait(AFTER_SECTION_PAUSE));

        yield return StartCoroutine(PlaySection(sectionCLines));
        yield return StartCoroutine(SkippableWait(AFTER_SECTION_PAUSE));

        foreach (GameObject child in allTextBoxChildren) child.SetActive(false);

        EncounterCamera.Instance.StartIntroCameraTransition(CAMERA_MOVE_DURATION);
        while (EncounterCamera.Instance.IsTransitioning) yield return null;

        gameObject.SetActive(false);
        GameUI.Instance.gameObject.SetActive(true);
        IsIntroRunning = false;
    }

    private System.Collections.IEnumerator PlaySection(List<TextMeshProUGUI> lines)
    {
        if (lines.Count == 0) yield break;

        foreach (GameObject child in allTextBoxChildren) child.SetActive(false);

        foreach (var line in lines)
        {
            line.gameObject.SetActive(true);
            line.color = new Color(line.color.r, line.color.g, line.color.b, 0f);
        }

        for (int i = 0; i < lines.Count; i++)
        {
            if (_skipRequested) yield break;

            var line = lines[i];
            float elapsed = 0f;
            while (elapsed < FADE_IN_DURATION)
            {
                if (_skipRequested) yield break;
                elapsed += Time.deltaTime;
                line.color = new Color(line.color.r, line.color.g, line.color.b, Mathf.Clamp01(elapsed / FADE_IN_DURATION));
                yield return null;
            }
            line.color = new Color(line.color.r, line.color.g, line.color.b, 1f);

            if (i < lines.Count - 1)
                yield return StartCoroutine(SkippableWait(LINE_DELAY));
        }

        yield return StartCoroutine(SkippableWait(AFTER_REVEAL_PAUSE));

        float fadeOutElapsed = 0f;
        while (fadeOutElapsed < FADE_OUT_DURATION)
        {
            if (_skipRequested) yield break;
            fadeOutElapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (fadeOutElapsed / FADE_OUT_DURATION));
            foreach (var line in lines)
                line.color = new Color(line.color.r, line.color.g, line.color.b, alpha);
            yield return null;
        }

        foreach (var line in lines)
        {
            line.color = new Color(line.color.r, line.color.g, line.color.b, 0f);
            line.gameObject.SetActive(false);
        }
    }

    private System.Collections.IEnumerator SkippableWait(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (_skipRequested) yield break;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}