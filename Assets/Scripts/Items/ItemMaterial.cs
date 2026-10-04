using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a physical material that an item can be made of, used purely to drive collision impact sounds.
/// Each material owns a pool of impact audio clip variants, loaded automatically from Resources/Audio/SFX/Material/{DefName}.
/// <br/>When two items with different materials collide, the material with the higher Dominance determines
/// which impact sound set is used (e.g. metal dominates over cloth, since the harder material is what you'd actually hear).
/// </summary>
public class ItemMaterialDef : Def
{
    public override string DefTypeLabel => "Item Material";

    /// <summary>
    /// When two colliding items have different materials, the one with the higher Dominance is used to
    /// select the impact sound. Rough guide: hard/resonant materials (metal, glass) should dominate soft/
    /// muffled ones (textile, meat), since that's what an impact between the two would actually sound like.
    /// </summary>
    public int Dominance { get; init; } = 1;

    private AudioClip[] ImpactClips;

    public ItemMaterialDef(string defName) : base(defName) { }

    /// <summary>
    /// Returns a random impact clip variant from this material's pool, for collision sounds that don't
    /// sound identical every time. Returns null if no clips were loaded (should not happen post-Validate).
    /// </summary>
    public AudioClip GetRandomImpactClip()
    {
        if (ImpactClips == null || ImpactClips.Length == 0) return null;
        return ImpactClips[Random.Range(0, ImpactClips.Length)];
    }

    public override bool Validate()
    {
        ImpactClips = ResourceManager.LoadAudioClipsInFolder($"Audio/SFX/Material/{DefName}");
        if (ImpactClips == null || ImpactClips.Length == 0)
        {
            ThrowValidationError($"ItemMaterialDef '{DefName}' has no impact audio clips. Make sure there is at least one audio clip in Resources/Audio/SFX/Material/{DefName}/.");
        }

        if (Dominance < 1) ThrowValidationError($"ItemMaterialDef '{DefName}' has a Dominance of {Dominance}, which must be at least 1.");

        return base.Validate();
    }
}

public static class ItemMaterialDefs
{
    public static List<ItemMaterialDef> Defs => new List<ItemMaterialDef>()
    {
        new ItemMaterialDef("MetalSolid")
        {
            Label = "solid metal",
            Dominance = 10,
        },
        new ItemMaterialDef("Glass")
        {
            Label = "glass",
            Dominance = 9,
        },
        new ItemMaterialDef("MetalThin")
        {
            Label = "thin metal",
            Dominance = 8,
        },
        new ItemMaterialDef("Wood")
        {
            Label = "wood",
            Dominance = 7,
        },
        new ItemMaterialDef("PlasticHard")
        {
            Label = "hard plastic",
            Dominance = 5,
        },
        new ItemMaterialDef("PaperHard")
        {
            Label = "hard paper",
            Dominance = 4,
        },
        new ItemMaterialDef("PlasticWrapped")
        {
            Label = "wrapped plastic",
            Dominance = 3,
        },
        new ItemMaterialDef("Textile")
        {
            Label = "textile",
            Dominance = 2,
        },
        new ItemMaterialDef("Plant")
        {
            Label = "plant",
            Dominance = 2,
        },
        new ItemMaterialDef("Meat")
        {
            Label = "meat",
            Dominance = 1,
        },
    };
}

[DefOf]
public static class ItemMaterialDefOf
{
    public static ItemMaterialDef Glass;
    public static ItemMaterialDef Meat;
    public static ItemMaterialDef MetalSolid;
    public static ItemMaterialDef MetalThin;
    public static ItemMaterialDef PaperHard;
    public static ItemMaterialDef Plant;
    public static ItemMaterialDef PlasticWrapped;
    public static ItemMaterialDef PlasticHard;
    public static ItemMaterialDef Textile;
    public static ItemMaterialDef Wood;
}