using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Visual entity representing the player.
/// </summary>
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

    public GameObject PoisonOverlay1;
    public GameObject PoisonOverlay2;
    public GameObject PoisonOverlay3;

    public List<Injury> Injuries;

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

    public bool IsPoisoned; // If true, poison countdown will decrease every day
    public int PoisonCountdown; // Death upon reaching zero
    private const int POISON_COUNTDOWN_START = 20; // How many days to live when poisoning starts
    private const int REPOISON_STRENGTH = 5; // How much the poison countdown gets reduced when getting poisoned while already posioned
    private const int EXTREME_POISONING_LIMIT = 3; // At how many days left the poisoning is considered extreme
    private const int MAJOR_POISONING_LIMIT = 10; // At how many days left the poisoning is considered major

    public bool HasDog;
    public bool HasParrot;

    [Header("Status Effects")]
    public List<StatusEffect> StatusEffects = new List<StatusEffect>();

    private StatusEffect SE_Hungry;
    private StatusEffect SE_VeryHungry;
    private StatusEffect SE_Starving;

    private StatusEffect SE_Thirsty;
    private StatusEffect SE_Dehydrated;
    private StatusEffect SE_Parched;

    private StatusEffect SE_MinorFracture;
    private StatusEffect SE_MajorFracture;
    private StatusEffect SE_ExtremeFracture;

    private StatusEffect SE_MinorBloodLoss;
    private StatusEffect SE_MajorBloodLoss;
    private StatusEffect SE_ExtremeBloodLoss;

    private StatusEffect SE_MinorPoisoning;
    private StatusEffect SE_MajorPoisoning;
    private StatusEffect SE_ExtremePoisoning;

    private void Start()
    {
        // Initialize static status effects
        SE_Hungry = new StatusEffect("Hungry", "Some food would be nice", ResourceManager.Singleton.SE_Bad);
        SE_VeryHungry = new StatusEffect("Very Hungry", "I don't think I can go much longer without food.", ResourceManager.Singleton.SE_VeryBad);
        SE_Starving = new StatusEffect("Starving", "If I don't eat anything right now I'm not gonna make it another day.", ResourceManager.Singleton.SE_ExtremelyBad);

        SE_Thirsty = new StatusEffect("Thirsty", "Some water would be nice", ResourceManager.Singleton.SE_Bad, Color.clear);
        SE_Dehydrated = new StatusEffect("Dehydrated", "I don't think I can go much longer without water.", ResourceManager.Singleton.SE_VeryBad);
        SE_Parched = new StatusEffect("Parched", "If I don't drink anything right now I'm not gonna make it another day.", ResourceManager.Singleton.SE_ExtremelyBad);

        SE_MinorFracture = new StatusEffect("Minor Fracture", "Oof ouch, my bones.", ResourceManager.Singleton.SE_Bad, Color.clear);
        SE_MajorFracture = new StatusEffect("Major Fracture", "I took a major hit to my bones. I shouldn't take any big risks right now.", ResourceManager.Singleton.SE_VeryBad);
        SE_ExtremeFracture = new StatusEffect("Extreme Fracture", "My insides feel chaoticly distorted. Any more hits will certainly be my death.", ResourceManager.Singleton.SE_ExtremelyBad);

        SE_MinorBloodLoss = new StatusEffect("Minor Blood Loss", "I've been bleeding a little.", ResourceManager.Singleton.SE_Bad, Color.clear);
        SE_MajorBloodLoss = new StatusEffect("Major Blood Loss", "I lost quite a bit of blood. I shouldn't take any big risks right now.", ResourceManager.Singleton.SE_VeryBad);
        SE_ExtremeBloodLoss = new StatusEffect("Extreme Blood Loss", "I can't afford to lose one more mililiter of blood or I'll be dead.", ResourceManager.Singleton.SE_ExtremelyBad);

        SE_MinorPoisoning = new StatusEffect("Poisoning", "Something poisoned me. I need to find an antidote.", ResourceManager.Singleton.SE_Bad);
        SE_MajorPoisoning = new StatusEffect("Major Poisoning", "The poisoning is getting really bad. I need to find an antidote asap.", ResourceManager.Singleton.SE_VeryBad);
        SE_ExtremePoisoning = new StatusEffect("Extreme Poisoning", "If I don't inject an antidote right now I won't make it.", ResourceManager.Singleton.SE_ExtremelyBad);
    }

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
        bool canRegenBone = !ActiveWounds.Any(x => x.Type == InjuryId.Bruise && !x.IsTended);
        if (canRegenBone) AddBoneHealth(BaseBoneRegenPerDay);

        // Blood Amount
        float bloodChange = BaseBloodRegenPerDay;
        if(ActiveWounds.Any(x => x.Type == InjuryId.Cut && !x.IsTended))
        {
            bloodChange = 0f;
            foreach (Injury wound in ActiveWounds.Where(x => x.Type == InjuryId.Cut && !x.IsTended)) bloodChange -= CutWoundBleedPerDay; 
        }
        AddBlood(bloodChange);

        // Wounds
        foreach(Injury wound in ActiveWounds) wound.OnEndDay(game, morningReport);

        // Poison
        if (IsPoisoned) PoisonCountdown--;
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

    public Injury AddBruiseWound()
    {
        AddBoneHealth(-BruiseWoundBoneDamage);

        List<Injury> candidateWounds = Injuries.Where(x => !x.IsActive && x.Type == InjuryId.Bruise).ToList();
        if (candidateWounds.Count == 0)
        {
            Debug.LogWarning("Tried to add bruise wound but there is no more space for wounds.");
            return null;
        }
        Injury newBruiseWound = candidateWounds[Random.Range(0, candidateWounds.Count)];
        newBruiseWound.Activate(Game.Day);

        return newBruiseWound;
    }
    public Injury AddCutWound()
    {
        List<Injury> candidateWounds = Injuries.Where(x => !x.IsActive && x.Type == InjuryId.Cut).ToList();
        if (candidateWounds.Count == 0)
        {
            Debug.LogWarning("Tried to add cut wound but there is no more space for wounds.");
            return null;
        }
        Injury newCutWound = candidateWounds[Random.Range(0, candidateWounds.Count)];
        newCutWound.Activate(Game.Day);

        return newCutWound;
    }

    public void RemoveInjury(Injury injury)
    {
        injury.Heal();
    }

    public void TendWound(Injury wound)
    {
        wound.Tend(Game);
    }
    public void HealInfection(Injury wound)
    {
        wound.HealInfection(Game);
    }

    public void Poison()
    {
        if (IsPoisoned) PoisonCountdown -= REPOISON_STRENGTH;
        else
        {
            IsPoisoned = true;
            PoisonCountdown = POISON_COUNTDOWN_START;
        }
    }
    public void HealPoison()
    {
        IsPoisoned = false;
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
    /// Updates all status effects according to the player state.
    /// This function does NOT change anything about the player state.
    /// </summary>
    public void UpdateStatusEffects()
    {
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

        // Poison
        if (IsPoisoned)
        {
            StatusEffect poisonSE;
            if (PoisonCountdown <= EXTREME_POISONING_LIMIT) poisonSE = new StatusEffect(SE_ExtremePoisoning);
            else if (PoisonCountdown <= MAJOR_POISONING_LIMIT) poisonSE = new StatusEffect(SE_MajorPoisoning);
            else poisonSE = new StatusEffect(SE_MinorPoisoning);
            poisonSE.Name += " (Death in " + PoisonCountdown + " Days)";
            StatusEffects.Add(poisonSE);
        }

        foreach (Injury w in ActiveWounds)
        {
            w.UpdateStatusEffect();
            StatusEffects.Add(w.StatusEffect);
        }
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
        if (Nutrition <= 1.5f) Torso_Thin2.SetActive(true);
        else if (Nutrition <= 4f) Torso_Thin1.SetActive(true);
        else Torso_Normal.SetActive(true);

        // Hydration overlay
        if (Hydration <= 1.5f) DehydrationOverlay2.SetActive(true);
        else if (Hydration <= 3.5f) DehydrationOverlay1.SetActive(true);

        // Bone health limb sprite
        if (BoneHealth <= 0.2f) Limbs_Fractured2.SetActive(true);
        else if (BoneHealth <= 0.6f) Limbs_Fractured1.SetActive(true);
        else Limbs_Normal.SetActive(true);

        // Blood loss color
        if (BloodAmount <= 0.2f) SetCharacterColor(MajorBloodLossColor);
        else if (BloodAmount <= 0.6f) SetCharacterColor(MinorBloodLossColor);
        else SetCharacterColor(HealthyColor);

        // Poison overlay
        if (IsPoisoned)
        {
            if (PoisonCountdown <= EXTREME_POISONING_LIMIT) PoisonOverlay3.SetActive(true);
            else if (PoisonCountdown <= MAJOR_POISONING_LIMIT) PoisonOverlay2.SetActive(true);
            else PoisonOverlay1.SetActive(true);
        }

        foreach (Injury injury in Injuries) injury.SetSprites();
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

    public List<Injury> ActiveWounds { get { return Injuries.Where(x => x.IsActive).ToList(); } }
}
