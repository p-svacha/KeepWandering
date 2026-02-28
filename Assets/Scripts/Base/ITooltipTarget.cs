using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Everything that can spawn a tooltip when hovered needs to implement this.
/// </summary>
public interface ITooltipTarget
{
    /// <summary>
    /// The title of the tooltip that gets shown when hovering this.
    /// </summary>
    public string GetTooltipTitle();

    /// <summary>
    /// The type or category of the thing that the tooltip is for.
    /// </summary>
    public string GetTooltipType();

    /// <summary>
    /// The body text of the tooltip that gets shown when hovering this.
    /// </summary>
    public string GetTooltipBodyText();
}
