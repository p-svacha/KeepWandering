using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCharacterRenderer : MonoBehaviour
{
    public static PlayerCharacterRenderer Instance;
    public PlayerCharacter Character => Game.Instance.Player;

    [Header("Sprites")]
    public GameObject Head;

    public GameObject Torso_Normal;
    public GameObject Torso_Thin1;
    public GameObject Torso_Thin2;

    public GameObject Limbs_Normal;
    public GameObject Limbs_Fractured1;
    public GameObject Limbs_Fractured2;

    public GameObject DehydrationOverlay1;
    public GameObject DehydrationOverlay2;

    public GameObject PoisonOverlay1;
    public GameObject PoisonOverlay2;
    public GameObject PoisonOverlay3;

    private Color HealthyColor = Color.white;
    private Color MinorBloodLossColor = new Color(1f, 0.8f, 0.8f);
    private Color MajorBloodLossColor = new Color(1f, 0.6f, 0.6f);

    [Header("Wounds")]
    public GameObject BruiseWoundsContainer;
    public GameObject CutWoundsContainer;
    private Dictionary<HealthConditionDef, List<WoundRenderer>> WoundRenderers;

    private void Awake()
    {
        Instance = this;
    }

    public void Init()
    {
        WoundRenderers = new Dictionary<HealthConditionDef, List<WoundRenderer>>();
        InitWoundRenderers(HealthConditionDefOf.Bruise, BruiseWoundsContainer);
        InitWoundRenderers(HealthConditionDefOf.Cut, CutWoundsContainer);
    }

    private void InitWoundRenderers(HealthConditionDef woundDef, GameObject container)
    {
        List<WoundRenderer> renderers = container.GetComponentsInChildren<WoundRenderer>(true).ToList();
        WoundRenderers.Add(woundDef, renderers);
    }

    /// <summary>
    /// Updates all visuals/sprites according to the player state.
    /// This function does NOT change anything about the player state.
    /// </summary>
    public void UpdateSprites()
    {
        DisableAllSprites();

        Head.SetActive(true);

        // Nutrition torso sprite
        if (Character.Hunger.Nutrition <= 1.5f) Torso_Thin2.SetActive(true);
        else if (Character.Hunger.Nutrition <= 4f) Torso_Thin1.SetActive(true);
        else Torso_Normal.SetActive(true);

        // Hydration overlay
        if (Character.Thirst.Hydration <= 1.5f) DehydrationOverlay2.SetActive(true);
        else if (Character.Thirst.Hydration <= 3.5f) DehydrationOverlay1.SetActive(true);

        // Bone health limb sprite
        if (Character.LegFracture.BoneHealth <= 0.2f) Limbs_Fractured2.SetActive(true);
        else if (Character.LegFracture.BoneHealth <= 0.6f) Limbs_Fractured1.SetActive(true);
        else Limbs_Normal.SetActive(true);

        // Blood loss color
        if (Character.BloodLoss.BloodAmount <= 0.2f) SetCharacterColor(MajorBloodLossColor);
        else if (Character.BloodLoss.BloodAmount <= 0.6f) SetCharacterColor(MinorBloodLossColor);
        else SetCharacterColor(HealthyColor);

        // Poison overlay
        if (Character.Poison.IsPoisoned)
        {
            if (Character.Poison.PoisonCountdown <= PlayerCharacter.EXTREME_POISONING_LIMIT) PoisonOverlay3.SetActive(true);
            else if (Character.Poison.PoisonCountdown <= PlayerCharacter.MAJOR_POISONING_LIMIT) PoisonOverlay2.SetActive(true);
            else PoisonOverlay1.SetActive(true);
        }

        // Wounds
        foreach (var kvp in WoundRenderers)
        {
            HealthConditionDef wound = kvp.Key;
            List<WoundRenderer> woundRenderers = kvp.Value;
            foreach(WoundRenderer woundRenderer in woundRenderers)
            {
                woundRenderer.Refresh();
            }
        }
    }

    public WoundRenderer GetUnusedWoundRenderer(HealthConditionDef woundDef)
    {
        return WoundRenderers[woundDef].Where(wr => wr.Wound == null).ToList().RandomElement();
    }

    private void DisableAllSprites()
    {
        Head.SetActive(false);
        Torso_Normal.SetActive(false);
        Torso_Thin1.SetActive(false);
        Torso_Thin2.SetActive(false);
        Limbs_Normal.SetActive(false);
        Limbs_Fractured1.SetActive(false);
        Limbs_Fractured2.SetActive(false);
        DehydrationOverlay1.SetActive(false);
        DehydrationOverlay2.SetActive(false);
        PoisonOverlay1.SetActive(false);
        PoisonOverlay2.SetActive(false);
        PoisonOverlay3.SetActive(false);
    }

    private void SetCharacterColor(Color c)
    {
        Head.GetComponent<SpriteRenderer>().color = c;
        Torso_Normal.GetComponent<SpriteRenderer>().color = c;
        Torso_Thin1.GetComponent<SpriteRenderer>().color = c;
        Torso_Thin2.GetComponent<SpriteRenderer>().color = c;
    }
}
