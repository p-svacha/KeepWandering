using System.Collections.Generic;
using UnityEngine;

public class AreaTypeDef : Def
{
    public override string DefTypeLabel => "Area Type";
    public AreaTypeDef(string defName) : base(defName) { }

    /// <summary>
    /// If the label of the area should be shown on the world map.
    /// </summary>
    public bool ShowLabel { get; init; }

    /// <summary>
    /// The font size of the area label on the world map.
    /// </summary>
    public float LabelFontSize { get; init; } = 2f;

    /// <summary>
    /// The color of the area label on the world map.
    /// </summary>
    public Color LabelColor { get; init; }

    /// <summary>
    /// The label is only shown when the camera's orthographic size is at least this value, i.e. when zoomed
    /// out far enough. A lower value keeps the label visible while zooming further in.
    /// </summary>
    public float LabelMinCameraSize { get; init; } = 3.5f;
}

public static class AreaTypeDefs
{
    public static List<AreaTypeDef> Defs => new List<AreaTypeDef>()
    {
        new AreaTypeDef("QuarantineZone")
        {
            Label = "Quarantine Zone",
            ShowLabel = false,
        },
        new AreaTypeDef("City")
        {
            Label = "City",
            ShowLabel = true,
            LabelFontSize = 4f,
            LabelColor = new Color(0.77f, 0.67f, 0.57f),
            LabelMinCameraSize = 2f,
        },
        new AreaTypeDef("Forest")
        {
            Label = "Forest",
            ShowLabel = true,
            LabelFontSize = 3f,
            LabelColor = new Color(0.65f, 0.94f, 0.65f),
            LabelMinCameraSize = 3.5f,
        },
        new AreaTypeDef("Lake")
        {
            Label = "Lake",
            ShowLabel = true,
            LabelFontSize = 3f,
            LabelColor = new Color(0.65f, 0.65f, 0.94f),
            LabelMinCameraSize = 3.5f,
        }
    };
}

[DefOf]
public static class AreaTypeDefOf
{
    public static AreaTypeDef QuarantineZone;
    public static AreaTypeDef City;
    public static AreaTypeDef Forest;
    public static AreaTypeDef Lake;
}
