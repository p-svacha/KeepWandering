using System.Collections.Generic;
using UnityEngine;

public static class EncounterDefs
{
    // DEFAULT_CAMERA_SIZE = 5.4

    public static List<EncounterDef> Defs => new List<EncounterDef>()
    {
        #region Misc Encounters

        new EncounterDef("MorningEncounter")
        {
            EncounterClass = typeof(MorningEncounter),
            Type = EncounterType.Morning,
            DevNotes = "This is the daily wake-up encounter that plays at the start of each day. On day one it delivers the game's premise (escaping a quarantine zone) and offers a single option to open the map. On subsequent days it reports any night events and gives the player three choices: move to a new location, stay to continue yesterday's encounters (increasing exposure), or rest to recover energy and heal (skipping the afternoon encounter). Both staying and resting come with a warning about increased exposure leading to nighttime attacks. It's a simple routing encounter with no skill checks — just narrative framing and a daily strategic decision.",
            CameraXOffset = -3.5f,
        },

        new EncounterDef("EveningFallback")
        {
            EncounterClass = typeof(BiomeEncounter_Fallback),
            Type = EncounterType.Biome,
        },

        #endregion

        #region Biome Encounters

        new EncounterDef("BiomeEncounter_Outskirts")
        {
            EncounterClass = typeof(BiomeEncounter_Outskirts),
            Type = EncounterType.Biome,
            CameraZoomLevel = 8f,
            DevNotes = "Evening encounter for the outskirts biome. Randomly selects one of four settings (abandoned farmstead, roadside ditch, crumbling wall, old shed) which affects the step text and fortify difficulty. In addition to the standard options, offers a 'Flag down passerby' charisma check (always available at roadside, 50% elsewhere) that on success opens a trading step where the player can buy an item or information for coins.",
        },

        new EncounterDef("BiomeEncounter_Woods")
        {
            EncounterClass = typeof(BiomeEncounter_Woods),
            Type = EncounterType.Biome,
            CameraZoomLevel = 8f,
            DevNotes = "Evening encounter for the woods biome. Randomly selects one of four settings (dense thicket, forest clearing, fallen tree, stream bank) which affects step text and difficulties. Stream bank grants a small morale bonus on arrival. In addition to standard options, offers 'Set a trap' (Intelligence/Dexterity check, crafts a trap for the night) and 'Forage' (Perception/Intelligence check, yields food or medicinal plants, difficulty varies by setting).",
        },

        new EncounterDef("BiomeEncounter_City")
        {
            EncounterClass = typeof(BiomeEncounter_City),
            Type = EncounterType.Biome,
            CameraZoomLevel = 8f,
            DevNotes = "Evening encounter for the city biome. Randomly selects one of four settings (abandoned apartment, parking garage, boarded-up shop, alleyway) which affects step text and difficulties. Rest early is not available in the city. In addition to standard options, offers 'Keep watch' (Perception/Combat check, reduces danger level on success, increases it on critical failure, difficulty varies by setting) and 'Eavesdrop' (Perception/Charisma check, reveals nearby encounters or creates a supply stash, always available at apartment and parking garage, 50% at shop, never in alleyway).",
        },

        #endregion

        #region Night Encounters

        new EncounterDef("Bandits")
        {
            EncounterClass = typeof(NightEncounter_Bandits),
            Type = EncounterType.Night,
            BaseProbability = 5,
            BiomeProbabilityOverrides = new Dictionary<BiomeDef, float>()
            {
                { BiomeDefOf.City, 10 }
            },
            CameraZoomLevel = 7f,
            DevNotes = "Bandits raid the player's camp during the night. Intensity determines the number of bandits (1 = lone bandit, 2 = pair, 3 = small group), which scales all option difficulties and the severity of negative outcomes. The player can fight (Combat/Strength, hardest at high intensity but keeps all items on success), sneak away (Dexterity/Agility, easier in woods, harder in outskirts, always loses some items), intimidate (Charisma/Combat, weapon slot is very effective, no critical failure), or hide (Perception/Dexterity, only available at intensity 1-2, failure leads to a second step where the player is caught and must fight at +15 difficulty or beg). Beg is a desperation option with three coin slots for scaling bribes. All outcomes scale item loss and injury severity with intensity.",
        },

        #endregion

        #region Location Encounters

        new EncounterDef("Crate")
        {
            Label = "Crate",
            EncounterClass = typeof(Encounter_Crate),
            Type = EncounterType.Location,
            BaseProbability = 6,
            CameraZoomLevel = EncounterCamera.DEFAULT_CAMERA_SIZE,
            DevNotes = "A locked wooden crate with one visible item and potentially hidden items inside. The player can try to squeeze the visible item through a hole (dexterity check), smash the crate (strength check, may destroy contents), pry it open with a crowbar, or peek inside to learn what's hidden. Outcomes range from getting everything intact to destroying all items or injuring yourself. The encounter tracks whether the crate has been opened, smashed, or looted and adjusts options accordingly on revisits.",
        },

        new EncounterDef("SupplyStash")
        {
            Label = "Supply Stash",
            EncounterClass = typeof(Encounter_SupplyStash),
            Type = EncounterType.Location,
            BaseProbability = 0.2f,
            CameraZoomLevel = EncounterCamera.DEFAULT_CAMERA_SIZE,
            DevNotes = "A simple positive location encounter where the player finds a hidden stash of supplies. Randomly selects a container type (backpack, locked box, buried crate) which determines the opening method. Backpacks open freely with no check. Locked boxes can be forced open (Strength/Dexterity) or lockpicked (Dexterity, very hard without a lockpicking tool). Buried crates require digging (Strength, helped by digging tools). Loot is drawn from the biome loot table. On revisit, there is a chance the stash has been taken or looted by someone else, creating urgency to open it on first visit. Can spawn naturally (rare) or be generated by encounters like city eavesdrop.",
        },

        new EncounterDef("WoundedStranger")
        {
            Label = "Wounded Stranger",
            EncounterClass = typeof(Encounter_WoundedStranger),
            Type = EncounterType.Location,
            BaseProbability = 4f,
            BiomeProbabilityOverrides = new Dictionary<BiomeDef, float>()
            {
                { BiomeDefOf.Woods, 1f },
            },
            CameraZoomLevel = EncounterCamera.DEFAULT_CAMERA_SIZE,
            DevNotes = "A social location encounter where the player finds an injured stranger. The player can help (requires a medical item, sets the stranger to grateful, +2 morale), talk (Charisma/Perception check, critical success also makes them grateful, success extracts their knowledge directly), rob (low difficulty but -3 morale on success, critical failure results in the stranger wounding you and stealing an item), or leave. A grateful stranger offers follow-up options: ask what they know (rumor, supply stash location, or nothing — 50/30/20 chance), ask for supplies (Charisma check), or trade using the generalized trading system. The encounter is persistent — on revisit the stranger may be gone based on days elapsed. Can appear in all biomes.",
        },

        new EncounterDef("CollapsedBuilding")
        {
            Label = "Collapsed Building",
            EncounterClass = typeof(Encounter_CollapsedBuilding),
            Type = EncounterType.Location,
            BaseProbability = 4f,
            BiomeProbabilityOverrides = new Dictionary<BiomeDef, float>()
            {
                { BiomeDefOf.City, 8f },
                { BiomeDefOf.Woods, 1f },
            },
            CameraZoomLevel = 12f,
            DevNotes = "A partially collapsed building with visible and hidden loot, optional live wires, and a chance of a trapped survivor. Players can grab visible items (Dexterity/Agility), cut sparking wires to avoid electrocution (Dexterity/Intelligence), clear rubble (Strength/Intelligence) or crawl through a gap (Agility/Perception) to reach deeper supplies. Structural integrity degrades on bad rolls and can trigger a full collapse that destroys remaining loot and kills any trapped survivor; freeing the survivor rewards a rumour, item, or morale boost.",
        },

        #endregion

        #region Landmarks

        new EncounterDef("RadioTower")
        {
            Label = "Radio Tower",
            EncounterClass = typeof(Encounter_RadioTower),
            Type = EncounterType.Landmark,
            CameraZoomLevel = 12f,
            MinOccurences = 2,
            MaxOccurences = 2,
            MinDistanceFromStart = 7,
            MinDistanceBetween = 12,
            DevNotes = " A radio tower location that serves as the quest origin for finding a weak point in a perimeter fence. A note on the door directs the player to find R in a nearby city, and a faint radio transmission can optionally be decoded (high perception check) to learn the fence coordinates directly. The player can also force the locked door open for supplies and climb the tower for stat boosts and map reveals. The encounter tracks the player's position (outside, inside, on top) and gates options based on progress and location.",
        },

        #endregion

        #region Special (force-placed) Encounters

        new EncounterDef("QuarantineFence")
        {
            Label = "Quarantine Fence",
            EncounterClass = typeof(Encounter_QuarantineFence),
            Type = EncounterType.ForcePlacedOnly,
            CameraZoomLevel = 8.5f,
            DevNotes = "The quarantine fence is the game's final obstacle and win condition. It starts electrified with a massive difficulty spike, but becomes easy to cut if the player finds the unpowered segment through the quest chain. Cutting requires a fence cutter (always consumed on use), and failure on an electrified fence results in electrocution. Once a hole is cut, walking through ends the game in victory.",
        },

        new EncounterDef("HomeOfR")
        {
            Label = "Home of R",
            EncounterClass = typeof(Encounter_HomeOfR),
            Type = EncounterType.ForcePlacedOnly,
            CameraZoomLevel = 6f,
            DevNotes = "The player meets an NPC named R whose partner is sick. After an initial conversation, the player can ask about a note found at the radio tower (completing a \"Find R\" quest) and learn about R's partner needing infection medicine. Delivering the right medicine rewards the player with a fence cutter and map coordinates for an unpowered fence segment, advancing the main storyline. It's essentially a quest hub that ties together the radio tower and fence-cutting objectives.",
        }

        #endregion
    };
}
