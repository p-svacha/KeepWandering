using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Wound : HealthCondition
{
    public override Sprite Sprite => GetCurrentSprite();
    public Sprite SpriteBase => ResourceManager.LoadSpriteFromSheet("HealthConditions", $"{Def.DefName}");
    public Sprite SpriteInfectMinor => ResourceManager.LoadSpriteFromSheet("HealthConditions", $"{Def.DefName}_InfectedMinor");
    public Sprite SpriteInfectMajor => ResourceManager.LoadSpriteFromSheet("HealthConditions", $"{Def.DefName}_InfectedMajor");
    public Sprite SpriteBandaged => ResourceManager.LoadSpriteFromSheet("HealthConditions", $"{Def.DefName}_Bandaged");

    public const float NATURAL_HEALING_UNBANDAGED = 0.2f;
    public const float NATURAL_HEALING_BANDAGED = 1f;

    private const float MIN_NATURAL_SEVERITY_CHANGE = 0.7f;
    private const float MAX_NATURAL_SEVERITY_CHANGE = 1.3f;
    private float naturalSeverityChange; // Randomized per wound, applied if the wound is unbandaged or infected and untreated

    public bool IsBandaged { get; private set; }
    public bool IsTreated { get; private set; }

    public InfectionStage InfectionStage => (InfectionStage)ActiveStageIndex;
    public bool IsInfected => InfectionStage != InfectionStage.None;
    private InfectionStage preChangeInfectionStage; // Temp value used to track is infection stage changes at the end of the day, used to determine if a message should be displayed to the player

    public WoundRenderer Renderer { get; private set; }

    // Def override
    public override float InitialSeverity => 1.5f;
    public override float MaxSeverity => 13;
    private List<HealthConditionStage> stages;
    public override List<HealthConditionStage> Stages => stages;

    // Base effects
    private Dictionary<StatDef, int> BaseUnbandagedStatModifiers => new Dictionary<StatDef, int>()
    {
        { StatDefOf.Strength, -2 },
        { StatDefOf.Social, -2 },
    };
    private Dictionary<StatDef, int> BaseBandagedStatModifiers => new Dictionary<StatDef, int>()
    {
        { StatDefOf.Strength, -1 },
        { StatDefOf.Social, -1 },
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
                { StatDefOf.Strength, -2 },
                { StatDefOf.Social, -2 },
                { StatDefOf.Dexterity, -2 },
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
                { StatDefOf.Strength, -3 },
                { StatDefOf.Social, -3 },
                { StatDefOf.Dexterity, -3 },
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
                { StatDefOf.Strength, -5 },
                { StatDefOf.Social, -5 },
                { StatDefOf.Dexterity, -5 },
            },
            Color = ResourceManager.Color_Text_ExtremelyNegative
        },
    };

    protected override void OnInit()
    {
        // Init stages (usually done in Def, but we need to do it here because we are using a base class for all wounds)
        stages = WoundStages;
        foreach (var stage in stages) stage.ResolveReferences(Def);

        // Natural severity change
        naturalSeverityChange = Random.Range(MIN_NATURAL_SEVERITY_CHANGE, MAX_NATURAL_SEVERITY_CHANGE);
    }

    public override float GetNaturalHealing()
    {
        if (IsBandaged) return NATURAL_HEALING_BANDAGED;
        else return NATURAL_HEALING_UNBANDAGED;
    }

    protected override void OnActiveStageChanged()
    {
        if (Renderer != null) Renderer.Refresh();
    }

    public override float GetNaturalSeverityChange()
    {
        if (!IsBandaged || (IsInfected && !IsTreated)) return naturalSeverityChange;
        else return 0;
    }

    protected override void OnEndDay_PreSeverityChange()
    {
        preChangeInfectionStage = InfectionStage;
    }

    protected override void OnEndDay_PostSeverityChange(MorningReport morningReport)
    {
        InfectionStage postChangeInfectionStage = (InfectionStage)ActiveStageIndex;

        if (preChangeInfectionStage == InfectionStage.None && postChangeInfectionStage != InfectionStage.None)
        {
            morningReport.NightEvents.Add($"Your {Def.Label} got infected.");
        }
        else if (preChangeInfectionStage >= InfectionStage.Minor && postChangeInfectionStage > preChangeInfectionStage)
        {
            morningReport.NightEvents.Add($"The infection of your {Def.Label} got worse and needs be dealt with immediately.");
        }
        else if (preChangeInfectionStage >= InfectionStage.Minor && postChangeInfectionStage == InfectionStage.None && SeverityValue > 0)
        {
            morningReport.NightEvents.Add($"Your {Def.Label} has healed from the infection.");
        }
        else if (preChangeInfectionStage >= InfectionStage.Minor && postChangeInfectionStage < preChangeInfectionStage)
        {
            morningReport.NightEvents.Add($"The infection of your {Def.Label} has improved.");
        }
    }

    public override Dictionary<StatDef, int> GetStatCurrentModifiers()
    {
        Dictionary<StatDef, int> modifiers = new(ActiveStage.StatModifiers); // Copy to avoid modifying the original
        modifiers.IncrementMultiple(IsBandaged ? BaseBandagedStatModifiers : BaseUnbandagedStatModifiers);
        return modifiers;
    }

    public void SetRenderer(WoundRenderer renderer)
    {
        Renderer = renderer;
    }

    public void Bandage()
    {
        IsBandaged = true;
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
        string bandageName = IsBandaged ? "Bandaged" : "Unbandaged";
        string label = $"{bandageName} {Def.LabelCap}";
        if (IsInfected)
        {
            label += $" ({ActiveStage.Label}";
            if (IsTreated) label += ", treated";
            label += ")";
        }
        return label;
    }

    public override string GetInterActionsString()
    {
        if (IsBandaged && (IsTreated || !IsInfected)) return $"{HealthConditionDef.HEALS_NATURALLY}";

        string s = $"";
        if (!IsBandaged) s += $"\nNeeds to be bandaged to heal.\n{GetUnbandagedEffectString()}";
        if (IsInfected) s += $"\n{ActiveStage.Description}";
        return s.Trim();
    }

    protected abstract string GetUnbandagedEffectString();

    public Sprite GetCurrentSprite()
    {
        return InfectionStage switch
        {
            InfectionStage.None => SpriteBase,
            InfectionStage.Minor => SpriteInfectMinor,
            InfectionStage.Major => SpriteInfectMajor,
            _ => throw new System.Exception("Infection stage " + InfectionStage.ToString() + " not handled.")
        };
    }

    public override string Label => GetReportLabel();
    public override string Description => ActiveStageIndex == 0 ? $"A {Def.Label} wound." : base.Description;
}

public enum InfectionStage
{
    None = 0,
    Minor = 1,
    Major = 2,
    Critical = 3,
}
