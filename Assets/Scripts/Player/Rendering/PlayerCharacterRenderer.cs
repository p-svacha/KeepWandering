using System;
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
    }

    /// <summary>
    /// Gets called whenever a health condition is added, removed, or has its active stage changed.
    /// </summary>
    public void OnHealthConditionChanged()
    {
        // Thirst overlay
        SetActiveSprite(DehydrationOverlay, Character.Thirst.ActiveStageIndex - 2);

        // Hunger overlay
        SetActiveSprite(Torso, Character.Hunger.ActiveStageIndex);

        // Poison overlay
        HealthCondition poison = Character.HealthConditions.FirstOrDefault(hc => hc.Def == HealthConditionDefOf.Poisoning);
        if (poison == null) SetActiveSprite(PoisonOverlay, -1);
        else SetActiveSprite(PoisonOverlay, poison.ActiveStageIndex);

        // Arms
        RightArm.Render(Character.RightArmFracture?.ActiveStageIndex ?? 0);
        LeftArm.Render(Character.LeftArmFracture?.ActiveStageIndex ?? 0);

        // Legs
        LegFront.Render(Character.RightLegFracture?.ActiveStageIndex ?? 0);
        LegBack.Render(Character.LeftLegFracture?.ActiveStageIndex ?? 0);

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
}
