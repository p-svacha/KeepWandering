using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wound : MonoBehaviour
{
    [Header("Sprites")]
    public GameObject Sprite_Base;
    public GameObject Sprite_Infect1;
    public GameObject Sprite_Infect2;
    public GameObject Sprite_Tend;

    [Header("Constant Attributes")]
    public PlayerCharacter Player;
    public WoundType Type;

    [Header("Dynamic Attributes")]
    public bool IsActive;

    private int OriginDay;
    private int MinorInfectionDay;
    private int TendDay;
    
    public bool IsTended;
    public InfectionStage InfectionStage;

    private const float InfectChancePerDay = 0.1f; // Every day, the chance for the to infection gets higher by this value when untended
    private const float MajorInfectChancePerDay = 0.1f; // Every day, the chance for the to infection to get worse gets higher by this value
    private const float FatalInfectChance = 0.5f; // Every day, this is the chance to die when having a major infection

    private const float HealChancePerDay = 0.25f; // Every daym the chance for a tended uninfected wound to heal gets higher by this value

    // Visual
    public StatusEffect StatusEffect;

    public void SetActive(int originDay)
    {
        IsActive = true;
        IsTended = false;
        InfectionStage = InfectionStage.None;
        OriginDay = originDay;
    }

    /// <summary>
    /// Performs all events that happen during the night and returns a list of them for the morning report.
    /// </summary>
    public void OnEndDay(Game game, MorningReport morningReport)
    {
        // Chance to get minor infection
        if(!IsTended && InfectionStage == InfectionStage.None)
        {
            float infectionChance = ((game.Day) - OriginDay) * InfectChancePerDay;
            if (Random.value < infectionChance)
            {
                InfectionStage = InfectionStage.Minor;
                MinorInfectionDay = game.Day;
                morningReport.NightEvents.Add("Your " + HelperFunctions.GetEnumDescription(Type) + " got infected.");
            }
        }
        // Chance to get major infection
        else if(InfectionStage == InfectionStage.Minor)
        {
            float infectionChance = ((game.Day) - MinorInfectionDay) * MajorInfectChancePerDay;
            if (Random.value < infectionChance)
            {
                InfectionStage = InfectionStage.Major;
                morningReport.NightEvents.Add("The infection of your " + HelperFunctions.GetEnumDescription(Type) + " got worse and needs be dealt with immeadiately.");
            }
        }
        // Chance to get fatal infection
        else if(InfectionStage == InfectionStage.Major)
        {
            if (Random.value < FatalInfectChance) InfectionStage = InfectionStage.Fatal;
        }

        // Chance to go away when tended
        if(IsTended && InfectionStage == InfectionStage.None)
        {
            float infectionChance = ((game.Day) - TendDay) * HealChancePerDay;
            if (Random.value < infectionChance)
            {
                Player.RemoveWound(this);
                morningReport.NightEvents.Add("Your " + HelperFunctions.GetEnumDescription(Type) + " has fully healed.");
            }
        }
    }

    public void Tend(Game game)
    {
        IsTended = true;
        TendDay = game.Day;
    }

    public void HealInfection(Game game)
    {
        InfectionStage = InfectionStage.None;
        OriginDay = game.Day;
    }

    public void UpdateStatusEffect()
    {
        // Name
        string baseName = HelperFunctions.GetEnumDescription(Type);
        string infectionName = InfectionStage == InfectionStage.None ? "" : (HelperFunctions.GetEnumDescription(InfectionStage) + " ");
        string tendName = IsTended ? "Tended " : "Untended ";
        string name = infectionName + tendName + baseName;

        // Description
        Dictionary<WoundType, string> woundTypeText = new Dictionary<WoundType, string>()
        {
            { WoundType.Bruise, "An untended bruise wound. While untended, this wound prevents bone fractures from healing." },
            { WoundType.Cut, "An untended cut wound. While untended, this wound causes bleeding." },
        };
        string untendedText = " Tend this wound with bandages, the wound might get infected.";
        string infectedText = " The wound is infected and needs antibiotics.";
        string severlyInfectedText = " The wound is severely infected. If not tended with antibiotics immeadiately, it will likely be fatal";

        string description = "";
        if (IsTended)
        {
            if (InfectionStage == InfectionStage.None) description = "A tended wound that will heal with time.";
            if (InfectionStage == InfectionStage.Minor) description = "A tended but infected wound. Needs antibiotics.";
            if (InfectionStage == InfectionStage.Major) description = "A tended but severely infected wound. Needs antibiotics urgently.";
        }
        else if (!IsTended)
        {
            description = woundTypeText[Type] + untendedText;
            if (InfectionStage == InfectionStage.Minor) description += infectedText;
            if (InfectionStage == InfectionStage.Major) description += severlyInfectedText;
        }

        // Color
        Color color;
        if (InfectionStage == InfectionStage.Major) color = Color.red;
        if (InfectionStage == InfectionStage.Minor || !IsTended) color = new Color(0.7f, 0f, 0f);
        else color = new Color(0.4f, 0f, 0f);

        // Background Color
        Color backgroundColor = Color.clear;

        StatusEffect = new StatusEffect(PlayerCharacter.SourceName, name, description, color, backgroundColor);
    }

    public void SetSprites()
    {
        Sprite_Base.SetActive(IsActive && InfectionStage == InfectionStage.None);
        Sprite_Infect1.SetActive(IsActive && InfectionStage == InfectionStage.Minor);
        Sprite_Infect2.SetActive(IsActive && InfectionStage == InfectionStage.Major);
        Sprite_Tend.SetActive(IsActive && IsTended);
    }

    public void SetHightlight(bool value)
    {
        if (value) StatusEffect.UI.BackgroundImage.color = Color.red;
        else StatusEffect.UI.BackgroundImage.color = Color.clear;
    }
}
