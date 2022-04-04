using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    public Game Game;

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
    public List<Wound> Wounds;

    public Color HealthyColor;
    public Color MinorBloodLossColor;
    public Color MajorBloodLossColor;

    [Header("Constants")]
    private const float BaseNutritionDropPerDay = 1f;
    private const float BaseHydrationDropPerDay = 1f;

    private const float BaseBoneRegenPerDay = 0.1f;
    private const float BaseBloodRegenPerDay = 0.1f;

    private const float BruiseWoundBoneDamage = 0.3f;
    private const float CutWoundBleedPerDay = 0.1f;

    [Header("Values")]
    public float Nutrition;
    public float Hydration;

    public float BoneHealth; // [0-1] how fractures the bones are, 1 = healthy, 0 = dead
    public float BloodAmount; // [0-1] how much blood you have, 1 = healthy, 0 = dead

    public bool HasDog;

    [Header("Status Effects")]
    public List<StatusEffect> StatusEffects = new List<StatusEffect>();

    private StatusEffect SE_Hungry = new StatusEffect("Hungry", "Some food would be nice", new Color(0.4f, 0f, 0f), Color.clear);
    private StatusEffect SE_VeryHungry = new StatusEffect("Very Hungry", "I don't think I can go much longer without food.", new Color(0.7f, 0f, 0f), Color.clear);
    private StatusEffect SE_Starving = new StatusEffect("Starving", "If I don't eat anything right now I'm not gonna make it another day.", new Color(1f, 0f, 0f), Color.clear);

    private StatusEffect SE_Thirsty = new StatusEffect("Thirsty", "Some water would be nice", new Color(0.4f, 0f, 0f), Color.clear);
    private StatusEffect SE_Dehydrated = new StatusEffect("Dehydrated", "I don't think I can go much longer without water.", new Color(0.7f, 0f, 0f), Color.clear);
    private StatusEffect SE_Parched = new StatusEffect("Parched", "If I don't drink anything right now I'm not gonna make it another day.", new Color(1f, 0f, 0f), Color.clear);

    private StatusEffect SE_MinorFracture = new StatusEffect("Minor Fracture", "Oof ouch, my bones.", new Color(0.4f, 0f, 0f), Color.clear);
    private StatusEffect SE_MajorFracture = new StatusEffect("Major Fracture", "I took a major hit to my bones. I shouldn't take any big risks right now.", new Color(0.7f, 0f, 0f), Color.clear);
    private StatusEffect SE_ExtremeFracture = new StatusEffect("Extreme Fracture", "My insides feel chaoticly distorted. Any more hits will certainly be my death.", new Color(1f, 0f, 0f), Color.clear);

    private StatusEffect SE_MinorBloodLoss = new StatusEffect("Minor Blood Loss", "I've been bleeding a little.", new Color(0.4f, 0f, 0f), Color.clear);
    private StatusEffect SE_MajorBloodLoss = new StatusEffect("Major Blood Loss", "I lost quite a bit of blood. I shouldn't take any big risks right now.", new Color(0.7f, 0f, 0f), Color.clear);
    private StatusEffect SE_ExtremeBloodLoss = new StatusEffect("Extreme Blood Loss", "I can't afford to lose one more mililiter of blood or I'll be dead.", new Color(1f, 0f, 0f), Color.clear);

    public void Init(Game game)
    {
        Game = game;
        Nutrition = 7.5f;
        Hydration = 6.5f;
        BloodAmount = 1f;
        BoneHealth = 1f;
    }

    /// <summary>
    /// Performs all events that happen during the night and returns a list of them for the morning report.
    /// </summary>
    public List<string> OnEndDay(Game game)
    {
        List<string> nightEvents = new List<string>();

        // Nutrition
        AddNutrition(-BaseNutritionDropPerDay);

        // Hydration
        AddHydration(-BaseHydrationDropPerDay);

        // Bone Health
        bool canRegenBone = !ActiveWounds.Any(x => x.Type == WoundType.Bruise && !x.IsTended);
        if (canRegenBone) AddBoneHealth(BaseBoneRegenPerDay);

        // Blood Amount
        float bloodChange = BaseBloodRegenPerDay;
        if(ActiveWounds.Any(x => x.Type == WoundType.Cut && !x.IsTended))
        {
            bloodChange = 0f;
            foreach (Wound wound in ActiveWounds.Where(x => x.Type == WoundType.Cut && !x.IsTended)) bloodChange -= CutWoundBleedPerDay; 
        }
        AddBlood(bloodChange);

        // Wounds
        foreach(Wound wound in ActiveWounds) nightEvents.AddRange(wound.OnEndDay(game));

        UpdateSpritesAndStatusEffects();

        return nightEvents;
    }

    public void AddNutrition(float value)
    {
        Nutrition += value;
        UpdateSpritesAndStatusEffects();
    }

    public void AddHydration(float value)
    {
        Hydration += value;
        UpdateSpritesAndStatusEffects();
    }

    public void AddBoneHealth(float value)
    {
        BoneHealth += value;
        if (BoneHealth > 1f) BoneHealth = 1f;
        UpdateSpritesAndStatusEffects();
    }

    public void AddBlood(float value)
    {
        BloodAmount += value;
        if (BloodAmount > 1f) BloodAmount = 1f;
        UpdateSpritesAndStatusEffects();
    }

    public void AddBruiseWound()
    {
        List<Wound> candidateWounds = Wounds.Where(x => !x.IsActive && x.Type == WoundType.Bruise).ToList();
        if (candidateWounds.Count == 0)
        {
            Debug.LogWarning("Tried to add bruise wound but there is no more space for wounds");
            return;
        }
        Wound newBruiseWound = candidateWounds[Random.Range(0, candidateWounds.Count)];
        newBruiseWound.SetActive(Game.Day);
        AddBoneHealth(-BruiseWoundBoneDamage);
        UpdateSpritesAndStatusEffects();
    }
    public void AddCutWound()
    {
        List<Wound> candidateWounds = Wounds.Where(x => !x.IsActive && x.Type == WoundType.Cut).ToList();
        if (candidateWounds.Count == 0)
        {
            Debug.LogWarning("Tried to add cut wound but there is no more space for wounds");
            return;
        }
        Wound newCutWound = candidateWounds[Random.Range(0, candidateWounds.Count)];
        newCutWound.SetActive(Game.Day);
        UpdateSpritesAndStatusEffects();
    }

    public void RemoveWound(Wound wound)
    {
        wound.IsActive = false;
        UpdateSpritesAndStatusEffects();
    }

    public void TendWound(Wound wound)
    {
        wound.Tend(Game);
        UpdateSpritesAndStatusEffects();
    }
    public void HealInfection(Wound wound)
    {
        wound.HealInfection(Game);
        UpdateSpritesAndStatusEffects();
    }

    public void AddDog()
    {
        HasDog = true;
        UpdateSpritesAndStatusEffects();
    }


    /// <summary>
    /// Updates all status effects and and visuals (sprites on player) representing them.
    /// </summary>
    public void UpdateSpritesAndStatusEffects()
    {
        // Sprites
        DisableAllSprites();

        Head.SetActive(true);

        if (Nutrition <= 1.5f) Torso_Thin2.SetActive(true);
        else if (Nutrition <= 4f) Torso_Thin1.SetActive(true);
        else Torso_Normal.SetActive(true);

        if (Hydration <= 1.5f) DehydrationOverlay2.SetActive(true);
        else if (Hydration <= 3.5f) DehydrationOverlay1.SetActive(true);

        if (BoneHealth <= 0.2f) Limbs_Fractured2.SetActive(true);
        else if (BoneHealth <= 0.6f) Limbs_Fractured1.SetActive(true);
        else Limbs_Normal.SetActive(true);

        if (BloodAmount <= 0.2f) SetCharacterColor(MajorBloodLossColor);
        else if (BloodAmount <= 0.6f) SetCharacterColor(MinorBloodLossColor);
        else SetCharacterColor(HealthyColor);

        // Status effects
        StatusEffects.Clear();

        if (Nutrition <= 1f) StatusEffects.Add(SE_Starving);
        else if (Nutrition <= 2.5f) StatusEffects.Add(SE_VeryHungry);
        else if (Nutrition <= 5f) StatusEffects.Add(SE_Hungry);

        if (Hydration <= 1f) StatusEffects.Add(SE_Parched);
        else if (Hydration <= 2f) StatusEffects.Add(SE_Dehydrated);
        else if (Hydration <= 4f) StatusEffects.Add(SE_Thirsty);

        if (BoneHealth <= 0.2f) StatusEffects.Add(SE_ExtremeFracture);
        else if (BoneHealth <= 0.5f) StatusEffects.Add(SE_MajorFracture);
        else if (BoneHealth <= 0.9f) StatusEffects.Add(SE_MinorFracture);

        if (BloodAmount <= 0.2f) StatusEffects.Add(SE_ExtremeBloodLoss);
        else if (BloodAmount <= 0.5f) StatusEffects.Add(SE_MajorBloodLoss);
        else if (BloodAmount <= 0.9f) StatusEffects.Add(SE_MinorBloodLoss);

        foreach (Wound w in ActiveWounds) StatusEffects.Add(w.GetStatusEffect());

        Game.UpdateStatusEffects(StatusEffects);
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
    }

    private void SetCharacterColor(Color c)
    {
        Head.GetComponent<SpriteRenderer>().color = c;
        Torso_Normal.GetComponent<SpriteRenderer>().color = c;
        Torso_Thin1.GetComponent<SpriteRenderer>().color = c;
        Torso_Thin2.GetComponent<SpriteRenderer>().color = c;
    }

    public List<Wound> ActiveWounds { get { return Wounds.Where(x => x.IsActive).ToList(); } }
}
