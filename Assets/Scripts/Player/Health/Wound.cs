using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Wound : HealthCondition
{
    public abstract Sprite SpriteBase { get; }
    public abstract Sprite SpriteInfectMinor { get; }
    public abstract Sprite SpriteInfectMajor { get; }
    public abstract Sprite SpriteTended { get; }

    [Header("Sprites")]
    public SpriteRenderer InjuryRenderer;
    public SpriteRenderer TendingRenderer;

    private int OriginDay;
    private int MinorInfectionDay;
    private int TendDay;
    
    public bool IsTended { get; private set; }
    public InfectionStage InfectionStage { get; private set; }
    public bool IsInfected => InfectionStage != InfectionStage.None;

    private const float InfectChancePerDay = 0.1f; // Every day, the chance for the to infection gets higher by this value when untended
    private const float MajorInfectChancePerDay = 0.1f; // Every day, the chance for the to infection to get worse gets higher by this value
    private const float FatalInfectChance = 0.5f; // Every day, this is the chance to die when having a major infection

    private const float HealChancePerDay = 0.25f; // Every daym the chance for a tended uninfected wound to heal gets higher by this value

    protected override void OnInit()
    {
        IsTended = false;
        InfectionStage = InfectionStage.None;
        OriginDay = Game.Instance.Day;
    }

    public override void OnUpdate() { }

    public override void OnEndDay(Game game, MorningReport morningReport)
    {
        // Chance to get minor infection
        if(!IsTended && InfectionStage == InfectionStage.None)
        {
            float infectionChance = ((game.Day) - OriginDay) * InfectChancePerDay;
            if (Random.value < infectionChance)
            {
                InfectionStage = InfectionStage.Minor;
                MinorInfectionDay = game.Day;
                morningReport.NightEvents.Add($"Your {LabelCapWord} got infected.");
            }
        }
        // Chance to get major infection
        else if(InfectionStage == InfectionStage.Minor)
        {
            float infectionChance = ((game.Day) - MinorInfectionDay) * MajorInfectChancePerDay;
            if (Random.value < infectionChance)
            {
                InfectionStage = InfectionStage.Major;
                morningReport.NightEvents.Add($"The infection of your {LabelCapWord} got worse and needs be dealt with immeadiately.");
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
                Game.Instance.RemoveInjury(this);
                morningReport.NightEvents.Add($"Your {LabelCapWord} has fully healed.");
            }
        }
    }

    public override string IsFatal()
    {
        if (InfectionStage == InfectionStage.Fatal) return "You died of an infection.";
        return "";
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

    }

    public void SetSprites()
    {
        InjuryRenderer.gameObject.SetActive(IsActive);
        InjuryRenderer.sprite = GetSprite();
        TendingRenderer.gameObject.SetActive(IsActive && IsTended);
        TendingRenderer.sprite = SpriteTended;
    }

    public Sprite GetSprite()
    {
        return InfectionStage switch
        {
            InfectionStage.None => SpriteBase,
            InfectionStage.Minor => SpriteInfectMinor,
            InfectionStage.Major => SpriteInfectMajor,
            _ => throw new System.Exception("Infection stage " + InfectionStage.ToString() + " not handled.")
        };
    }

    public void SetHightlighted(bool value)
    {
        if (value) UiDisplayElement.BackgroundImage.color = Color.red;
        else UiDisplayElement.BackgroundImage.color = Color.clear;
    }

    public override string GetReportLabel()
    {
        // Name
        string label = LabelCapWord;
        string infectionName = InfectionStage == InfectionStage.None ? "" : InfectionStage.ToString();
        string tendName = IsTended ? "Tended" : "Untended";
        return $"{infectionName} {tendName} {label}".Trim();
    }

    protected abstract string GetUntendedEffectString();
    public override string GetReportDescription()
    {
        string description = "";
        if (IsTended)
        {
            if (InfectionStage == InfectionStage.None) description = "A tended wound that will heal with time.";
            if (InfectionStage == InfectionStage.Minor) description = "A tended but infected wound. Needs antibiotics.";
            if (InfectionStage == InfectionStage.Major) description = "A tended but severely infected wound. Needs antibiotics urgently.";
        }
        else if (!IsTended)
        {
            string tendingText = IsTended ? "tended" : "untended";
            description = $"An {tendingText} {Label} wound. {GetUntendedEffectString()}";
            if (InfectionStage == InfectionStage.None) description += " Tend this wound with bandages, the wound might get infected.";
            if (InfectionStage == InfectionStage.Minor) description += " The wound is infected and needs antibiotics.";
            if (InfectionStage == InfectionStage.Major) description += " The wound is severely infected. If not tended with antibiotics immediately, it will likely be fatal.";
        }

        return description;
    }
        
}
