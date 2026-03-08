using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Wound : HealthCondition
{
    public Sprite SpriteBase => ResourceManager.LoadSprite($"Character/Wounds/{Def.DefName}/{Def.DefName}_Base");
    public Sprite SpriteInfectMinor => ResourceManager.LoadSprite($"Character/Wounds/{Def.DefName}/{Def.DefName}_InfectedMinor");
    public Sprite SpriteInfectMajor => ResourceManager.LoadSprite($"Character/Wounds/{Def.DefName}/{Def.DefName}_InfectedMajor");
    public Sprite SpriteTended => ResourceManager.LoadSprite($"Character/Wounds/{Def.DefName}/{Def.DefName}_Tended");

    public const float NATURAL_HEALING_UNTENDED = 0.2f;
    public const float NATURAL_HEALING_TENDED = 1f;


    public bool IsTended { get; private set; }
    public bool IsTreated { get; private set; }

    public InfectionStage InfectionStage => (InfectionStage)ActiveStageIndex;
    public bool IsInfected => InfectionStage != InfectionStage.None;

    public WoundRenderer Renderer { get; private set; }

    // Def override
    public override float InitialSeverity => 1.5f;
    public override float MaxSeverity => 13;
    public override List<HealthConditionStage> Stages => WoundStages;

    // Base effects
    private Dictionary<StatDef, int> BaseUntendedStatModifiers => new Dictionary<StatDef, int>()
    {
        { StatDefOf.Combat, -2 },
        { StatDefOf.Charisma, -2 },
    };
    private Dictionary<StatDef, int> BaseTendedStatModifiers => new Dictionary<StatDef, int>()
    {
        { StatDefOf.Combat, -1 },
        { StatDefOf.Charisma, -1 },
    };

    // Infection stages
    private List<HealthConditionStage> WoundStages = new List<HealthConditionStage>()
    {
        new HealthConditionStage()
        {
            Label = "not infected",
            SeverityThreshold = 0,
            Color = ResourceManager.Color_Text_Negative
        },
        new HealthConditionStage()
        {
            Label = "infected",
            Description = "The wound is infected and needs to be treated with antibiotics to heal.",
            SeverityThreshold = 4f,
            StatModifiers = new Dictionary<StatDef, int>()
            {
                { StatDefOf.Combat, -2 },
                { StatDefOf.Strength, -2 },
                { StatDefOf.Agility, -2 },
            },
            Color = ResourceManager.Color_Text_Negative
        },
        new HealthConditionStage()
        {
            Label = "majorly infected",
            Description = "The wound is infected and needs to be treated with antibiotics to heal.",
            SeverityThreshold = 7f,
            StatModifiers = new Dictionary<StatDef, int>()
            {
                { StatDefOf.Combat, -3 },
                { StatDefOf.Strength, -3 },
                { StatDefOf.Agility, -3 },
            },
            Color = ResourceManager.Color_Text_VeryNegative
        },
        new HealthConditionStage()
        {
            Label = "critically infected",
            Description = "The wound is infected and needs to be treated with antibiotics to heal.",
            SeverityThreshold = 10f,
            StatModifiers = new Dictionary<StatDef, int>()
            {
                { StatDefOf.Combat, -5 },
                { StatDefOf.Strength, -5 },
                { StatDefOf.Agility, -5 },
            },
            Color = ResourceManager.Color_Text_ExtremelyNegative
        },
    };

    public override float GetNaturalHealing()
    {
        if (IsTended) return NATURAL_HEALING_TENDED;
        else return NATURAL_HEALING_UNTENDED;
    }

    protected override void OnActiveStageChanged()
    {
        if (Renderer != null) Renderer.Refresh();
    }

    protected override void OnEndDay(MorningReport morningReport)
    {
        InfectionStage beforeStage = (InfectionStage)ActiveStageIndex;

        // If the wound is untended or infected and untreated, increase severity random amount between 0.5 and 1.5.
        if (!IsTended || (IsInfected && !IsTreated))
        {
            float severityIncrease = Random.Range(0.5f, 1.5f);
            ModifySeverity(severityIncrease);
        }

        InfectionStage afterStage = (InfectionStage)ActiveStageIndex;

        if (beforeStage == InfectionStage.None && afterStage != InfectionStage.None)
        {
            morningReport.NightEvents.Add($"Your {Def.Label} got infected.");
        }
        else if (beforeStage >= InfectionStage.Minor && afterStage > beforeStage)
        {
            morningReport.NightEvents.Add($"The infection of your {Def.Label} got worse and needs be dealt with immeadiately.");
        }
        else if (beforeStage >= InfectionStage.Minor && afterStage == InfectionStage.None && SeverityValue > 0)
        {
            morningReport.NightEvents.Add($"Your {Def.Label} has healed from the infection.");
        }
        else if (beforeStage >= InfectionStage.Minor && afterStage < beforeStage)
        {
            morningReport.NightEvents.Add($"The infection of your {Def.Label} has improved.");
        }
    }

    public override Dictionary<StatDef, int> GetCurrentModifiers()
    {
        Dictionary<StatDef, int> modifiers = new(ActiveStage.StatModifiers); // Copy to avoid modifying the original
        modifiers.IncrementMultiple(IsTended ? BaseTendedStatModifiers : BaseUntendedStatModifiers);
        return modifiers;
    }

    public void SetRenderer(WoundRenderer renderer)
    {
        Renderer = renderer;
    }

    public void Tend()
    {
        IsTended = true;
    }
    public void Treat()
    {
        IsTreated = true;
    }

    public override void OnRemoved()
    {
        Renderer.SetWound(null);
    }

    public void SetHightlighted(bool value)
    {
        if (value) UiDisplayElement.BackgroundImage.color = Color.red;
        else UiDisplayElement.BackgroundImage.color = Color.clear;
    }

    public override string GetReportLabel()
    {
        // Name
        string tendName = IsTended ? "Tended" : "Untended";
        string label = $"{tendName} {Def.LabelCap}";
        if (IsInfected)
        {
            label += $" ({ActiveStage.Label}";
            if (IsTreated) label += ", treated";
            label += ")";
        }
        return label;
    }

    protected abstract string GetUntendedEffectString();
    public override string GetReportDescription()
    {
        string description = Def.Description;
        if (!IsTended) description += $"\nNeeds to be tended to heal. {GetUntendedEffectString()}";
        if (IsInfected) description += $"\n{ActiveStage.Description}";
        return description;
    }
        
}

public enum InfectionStage
{
    None = 0,
    Minor = 1,
    Major = 2,
    Critical = 3,
}
