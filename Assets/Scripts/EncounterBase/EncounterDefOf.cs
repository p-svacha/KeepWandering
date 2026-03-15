using UnityEngine;

[DefOf]
public class EncounterDefOf : MonoBehaviour
{
    // Time of Day
    public static EncounterDef MorningEncounter;
    public static EncounterDef EveningFallback;

    // Landmarks
    public static EncounterDef RadioTower;

    // Location
    public static EncounterDef SupplyStash;

    // Special
    public static EncounterDef QuarantineFence;
    public static EncounterDef HomeOfR;
}
