using UnityEngine;

public enum EncounterOptionType
{
    /// <summary>
    /// FixedOutcome options have a fixed outcome, meaning on a technical level, that choosing the options always calls the same function. That function can still have custom logic and random elements, but the outcome is always determined by that function. They are usually used for very simple options like ignoring/skipping something, or for options with special outcomes that don't fit into the classic success/partial success/failure outcome structure of a skill check.
    /// </summary>
    FixedOutcome,

    /// <summary>
    /// Skillcheck options follow a classic, standardized RPG style skillcheck structure, where the option has a calculated difficulty value and a rolled outcome that is determined by the difficulty value and a random roll, where the difficulty can be affected by a variety of modifiers. They have different possible success levels (usually "success", "partial success", "failure"), with each of these calling a different function that determines the outcome of that option.
    /// </summary>
    SkillCheck,
}
