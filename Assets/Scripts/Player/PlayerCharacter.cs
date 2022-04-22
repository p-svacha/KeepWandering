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
    public bool HasParrot;

    [Header("Status Effects")]
    public List<StatusEffect> StatusEffects = new List<StatusEffect>();
    public static string SourceName = "You";

    private StatusEffect SE_Hungry = new StatusEffect(SourceName, "Hungry", "Some food would be nice", new Color(0.4f, 0f, 0f), Color.clear);
    private StatusEffect SE_VeryHungry = new StatusEffect(SourceName, "Very Hungry", "I don't think I can go much longer without food.", new Color(0.7f, 0f, 0f), Color.clear);
    private StatusEffect SE_Starving = new StatusEffect(SourceName, "Starving", "If I don't eat anything right now I'm not gonna make it another day.", new Color(1f, 0f, 0f), Color.clear);

    private StatusEffect SE_Thirsty = new StatusEffect(SourceName, "Thirsty", "Some water would be nice", new Color(0.4f, 0f, 0f), Color.clear);
    private StatusEffect SE_Dehydrated = new StatusEffect(SourceName, "Dehydrated", "I don't think I can go much longer without water.", new Color(0.7f, 0f, 0f), Color.clear);
    private StatusEffect SE_Parched = new StatusEffect(SourceName, "Parched", "If I don't drink anything right now I'm not gonna make it another day.", new Color(1f, 0f, 0f), Color.clear);

    private StatusEffect SE_MinorFracture = new StatusEffect(SourceName, "Minor Fracture", "Oof ouch, my bones.", new Color(0.4f, 0f, 0f), Color.clear);
    private StatusEffect SE_MajorFracture = new StatusEffect(SourceName, "Major Fracture", "I took a major hit to my bones. I shouldn't take any big risks right now.", new Color(0.7f, 0f, 0f), Color.clear);
    private StatusEffect SE_ExtremeFracture = new StatusEffect(SourceName, "Extreme Fracture", "My insides feel chaoticly distorted. Any more hits will certainly be my death.", new Color(1f, 0f, 0f), Color.clear);

    private StatusEffect SE_MinorBloodLoss = new StatusEffect(SourceName, "Minor Blood Loss", "I've been bleeding a little.", new Color(0.4f, 0f, 0f), Color.clear);
    private StatusEffect SE_MajorBloodLoss = new StatusEffect(SourceName, "Major Blood Loss", "I lost quite a bit of blood. I shouldn't take any big risks right now.", new Color(0.7f, 0f, 0f), Color.clear);
    private StatusEffect SE_ExtremeBloodLoss = new StatusEffect(SourceName, "Extreme Blood Loss", "I can't afford to lose one more mililiter of blood or I'll be dead.", new Color(1f, 0f, 0f), Color.clear);

    public void Init(Game game)
    {
        Game = game;
        Nutrition = 7.5f;
        Hydration = 6.5f;
        BloodAmount = 1f;
        BoneHealth = 1f;
    }

    /// <summary>
    /// Performs all events that happen during the night and adds them to the morning report.
    /// </summary>
    public void OnEndDay(Game game, MorningReport morningReport)
    {
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
        foreach(Wound wound in ActiveWounds) wound.OnEndDay(game, morningReport);
    }

    public void AddNutrition(float value)
    {
        Nutrition += value;
    }

    public void AddHydration(float value)
    {
        Hydration += value;
    }

    public void AddBoneHealth(float value)
    {
        BoneHealth += value;
        if (BoneHealth > 1f) BoneHealth = 1f;
    }

    public void AddBlood(float value)
    {
        BloodAmount += value;
        if (BloodAmount > 1f) BloodAmount = 1f;
    }

    public void AddBruiseWound()
    {
        AddBoneHealth(-BruiseWoundBoneDamage);

        List<Wound> candidateWounds = Wounds.Where(x => !x.IsActive && x.Type == WoundType.Bruise).ToList();
        if (candidateWounds.Count == 0)
        {
            Debug.LogWarning("Tried to add bruise wound but there is no more space for wounds.");
            return;
        }
        Wound newBruiseWound = candidateWounds[Random.Range(0, candidateWounds.Count)];
        newBruiseWound.SetActive(Game.Day);
    }
    public void AddCutWound()
    {
        List<Wound> candidateWounds = Wounds.Where(x => !x.IsActive && x.Type == WoundType.Cut).ToList();
        if (candidateWounds.Count == 0)
        {
            Debug.LogWarning("Tried to add cut wound but there is no more space for wounds.");
            return;
        }
        Wound newCutWound = candidateWounds[Random.Range(0, candidateWounds.Count)];
        newCutWound.SetActive(Game.Day);
    }

    public void RemoveWound(Wound wound)
    {
        wound.IsActive = false;
    }

    public void TendWound(Wound wound)
    {
        wound.Tend(Game);
    }
    public void HealInfection(Wound wound)
    {
        wound.HealInfection(Game);
    }

    public void AddDog()
    {
        HasDog = true;
    }
    public void RemoveDog()
    {
        HasDog = false;
    }

    public void AddParrot()
    {
        HasParrot = true;
    }
    public void RemoveParrot()
    {
        HasParrot = false;
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

        foreach (Wound w in Wounds) w.SetSprites();

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

        foreach (Wound w in ActiveWounds)
        {
            w.UpdateStatusEffect();
            StatusEffects.Add(w.StatusEffect);
        }
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
