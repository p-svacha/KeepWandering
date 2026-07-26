# Keep Wandering

A point-and-click adventure game where the player must survive and escape a quarantined zone in a procedurally generated world by making choices in various encounters. The player manages their inventory, health, and quests to survive and eventually escape before the zone is destroyed.

This document describes the game's **systems and design**. Concrete content (specific items, health conditions, encounters, biomes, danger values, etc.) lives in the `Def` lists and is not duplicated here.

---

# Lore

The game takes place in a quarantine zone: a large area sealed off from the outside world ahead of a weapon test. The "outbreak" the public was told about is a cover. The real reason for the quarantine is that a disintegrating, spreading substance is scheduled to be deployed inside the zone. The player character is one of the survivors trapped inside, and their goal is to escape before the substance reaches them.

This gives the run a hard outer time limit: the substance is deployed on a fixed day and spreads from there, eventually consuming the entire zone (see *End-Game: The Substance*).

---

# Game Presentation

The game happens almost entirely on a single fixed screen. The player character and their handcart (inventory) occupy the left side; the current encounter occupies the right. The camera never pans and the player never moves position.

Top-of-screen UI:

- **Left:** the player's skills, morale (displayed separately), and below them the health report.
- **Centre:** the current encounter step's text and options.
- **Right:** day counter, world-map button, handbook button, settings button, and below that the "Notes" panel (active quests / learned information).

Encounter texts are strictly written in the present tense, when describing what is happening and what has happened as the outcome of the previously selected option.

## Art Direction

Strictly **2D side-view comic style with no 3D depth or perspective**. Everything is sprite-based and hand-drawn, with no frame-based animations. State changes are communicated by swapping sprites, playing a sound, and simple effects rather than motion.

Sprites may have simple shader- or transform-based animations (e.g. a flickering fire, a waving flag, a bouncing item) but these are purely cosmetic and do not affect gameplay.

- Foreground (player + encounter): black outlines, strong saturated colours.
- Background (biome + sky): no outlines, washed-out colours, minimal shading.
- Characters are stick figures with a volumized head and torso and stick-line limbs.

## Dynamic Sprites

Sprites change to reflect state, giving immediate visual feedback without text:

- The player's sprite reflects health (e.g. thinner torso when hungry, wound sprites on the head, broken-limb sprites, tint changes for other conditions).
- The background's main layer depends on the current biome; the sky depends on the time of day.
- Each encounter step has its own sprites showing the current state of that encounter.

## Game Feel

A point-and-click game with heavy emphasis on dragging and dropping items. Every item is a physics object resting in the cart and can be dragged freely; an item dragged offscreen respawns above the cart so nothing is ever lost.

Items can be dragged into encounter option **item slots**, or directly onto relevant sprites: i.e. dragging a bandage onto a wound sprite bandages it, dragging a crowbar onto a crate fills that option's slot.

Encounter options can be **bound to sprites** in the scene (see *Encounters → Sprite-Bound Options*), so the player can interact with the world directly, not only through the option list.

---

# Player

The player character has skills, morale, health conditions, and an inventory.

## Skills

Four integer skills, default value 0, that act as direct modifiers to the difficulty of encounter options of their type:

- **Strength:** Raw physical force. Forcing things open, combat, moving heavy objects, climbing.
- **Dexterity:** Bodily coordination and control. Lockpicking, sneaking, dodging, crafting, running away, bypassing obstacles.
- **Survival:** Field knowledge and awareness. Scavenging, medical actions, cooking, scouting, noticing things others would miss.
- **Social:** People skills. Persuading, trading, pleading, intimidating through personality, any interaction where how you speak matters more than what you do.

Skills can be modified **temporarily** (by currently present health conditions) or **permanently** (from specific encounter outcomes or quest completions). A permanent modifier simply changes the skill's base value, which starts at 0. Skills are clamped to **-30 / +30**.

The base value of skills generally go up during a playthrough, either by succeeding in certain skill checks, are more focussed by training a skill directly during the evening.
Negative pressure on skill values mostly comes from effects from health conditions, which are mostly temporary or healable.

## Morale

Morale is a separate integer stat, also starting at 0 and clamped to **-30 / +30**, displayed on its own in the UI to distinguish it from skills. It applies as a **flat modifier to every skill check**, making it the one number that touches everything.

Morale is intended as a **strategic trade-off lever** rather than a passive health indicator. Options that touch morale should almost always pair an immediate effect with an opposite morale shift, creating a genuine dilemma:

- **Immediate gain, morale cost:** Rob the stranger (items now, harder checks for days), take the easy selfish route, cut a corner.
- **Immediate cost, morale gain:** Help the stranger (nothing now, easier checks ahead), take the honest hard option, make a sacrifice.

This means morale options often trade present-tense outcomes against future-tense skill check odds. If a morale change doesn't create that tension, it probably shouldn't touch morale (except for clear rewards or penalties). Morale also passively aggregates the run's condition (hunger, thirst, injuries, and events all feed it) so a player who's been grinding themselves down feels it on everything.

Having low or high morale can also lead to various night events to happen.

---

## Health Conditions

Health is not a single bar. It is a **collection of conditions** the player currently has, each with its own behaviour, effects, and treatment.

**Core mechanics shared by all conditions:**

- **Severity**: A hidden meter. In general, higher severity means a stronger effect. How severity behaves is defined per condition. Severity is a value on instances of a condition, not the condition type itself. For example, a player can have two instances of the same condition type (e.g. two cut wounds) with different severities.

- **Stages**: see below.
- **Initial Severity**: The severity a condition instance starts at when first applied (can optionally be overridden when the condition is applied).
- **Max Severity**: The maximum severity a condition instance can reach.
- **Is Lethal**: Whether the condition can kill the player if severity reaches the maximum.
- **Max instances**: How many instances of a condition type can coexist. Gaining one beyond the max instead adds its severity to a random existing instance.
- **Natural healing**: Each night (and on the Rest action) severity is reduced by the condition's natural-healing value, with small random variation.
- **Natural severity change**: The absolute amount by which severity changes each night, unaffected by any modifiers or actions. Can for example be used to naturally increase severity over time (e.g. hunger, thirst) or naturally decrease it (e.g. a buff with remaining duration). This should not be used for "healing", as natural healing is already a separate mechanic.

- A condition is removed when its severity reaches 0.

### Health Condition Stages

Each condition defines a set of stages keyed to severity thresholds. The active stage is the one with the highest threshold that is less than or equal to the current severity. Each stage defines a set of effects that are applied while the condition is in that stage.

The currently active stage defines the label, description and current effects of the condition.
Possible effects are:

- **Stat modifiers**: Modifiers to one or more stats (morale or skills).
- **End of day vital change**: A change to the severity of one or more vitals at the end of the day (hunger, thirst, blood loss).
- **Chance to apply another conditions**: A chance to apply a new condition at the end of the day. Defined is the condition and the chance to apply it.
- **Chance to modify skill check rolls**: A chance to modify the result value of a skill check roll. Defined is the modified roll value and the chance to apply it. This is applied at the very end of a skill check roll during the resolution phase (animated sequence).
- **Custom effects**: Health conditions can define custom effects that are applied while the condition is in that stage. These effects can be anything, but should be used sparingly and only when necessary.

### Vitals

Vitals are a subset of health conditions, where exactly one instance of the condition is always present on the player. Unlike normal conditions, vitals are only shown in the health report once they pass a visibility threshold.

The following vitals exist:
- **Hunger:** Naturally increases each night. Reduced by consuming items that give nutrition.
- **Thirst:** Naturally increases each night. Reduced by consuming items that give hydration.
- **Blood Loss:** Naturally decreases each night. Increased by bleeding/unbandaged cut wounds.

### Wounds

A special subcategory of condition sharing common bandaging/infection logic. The severity value tracks **infection** rather than a damage amount, so wounds are always applied at the same initial severity. Each wound carries **bandaged** and **treated** flags:

- **Bandaging** (item with the appropriate medical tag) reduces ongoing harm and improves natural healing.
- **Treating** (item with an antiseptic tag) protects against infection.
- An unbandaged/untreated wound's infection severity tends to worsen each night; infection progresses through stages with escalating penalties and is lethal at the top.

Different wound types layer an additional effect on top of the shared logic (e.g. cut wounds drive ongoing blood loss while unbandaged; bruise wounds slow fracture healing). Exact wound types and values live in the Defs.

Bandaging and infection treatment are skill checks that are available in the morning or at the very end of encounters.

### Fractures

Another subcategory of condition, with its own instancing rules and healing logic. Fractures can occur on arms or legs, and always appear on a specific side (left/right). They heal naturally, but while present have a strong negative effect on strength and/or dexterity.

### Generalized Damage Helpers

Encounters apply damage through standardized helpers rather than always targeting a specific condition:

- **Apply Random Wound:** A new wound from a pool (typically cut/bruise).
- **Take Bruise Damage (severity X):** A bruise wound plus fracture damage to a random limb.
- **Take Cut Damage (severity X):** A cut wound plus an immediate blood-loss increase.
- **Take Random Damage (severity X):** Randomly one of the above.

## Natural Healing

Natural Healing is a mechanic that applies to all health conditions. Each condition defines a natural healing value, which is the amount by which the condition's severity when natural healing is applied. Natural healing always applied with a specific factor, which modifies the healing of every condition.

This system can be used for dynamic healing effects with different factors / effectiveness.

---

# Items / Inventory

The player carries a wooden handcart representing their inventory. Items are physics sprites that settle in the cart and can be dragged freely; items added spawn above the cart and fall in; removed items vanish; items knocked offscreen respawn so they cannot be lost. Hovering shows a tooltip; clicking/right-clicking offers item actions (eat, drink, apply, etc.).

This chapter covers different systems items can have (tags, consumption, passive stat modifiers). These do not exclude each other. While most items are focussed on a small number of use caese, items may have any combination of these properties.

## Item Tags

Each item can have any number of **tags**. Tags are the mechanism by which item slots decide which items they accept.

**Tag Design Philosophy:**
Tags are used in a way to describe what an item is good for, rather than what it is, since mechanically they are used to reduce the difficulty of skill checks. Tags should generally be designed in a was, so that it makes sense to describe it as "Item can be used as [tag]". Since tags are tightly coupled to the durability system, they should also be designed so it makes sense that items with a tag can be used multiple, but a limited number of times, for that purpose.

### Tag Levels

Each tag on an item has a **level from 1 to 5** expressing how good the item is at that role. (A knife might be a level-3 Weapon, level-1 Lockpick, level-4 Cutting tool.)

When an item fills a skill-check slot that accepts one of its tags, the item's level for that tag determines a **standardized difficulty reduction**:

| Level | Difficulty reduction |
|-------|----------------------|
| 1     | 20%                  |
| 2     | 40%                  |
| 3     | 60%                  |
| 4     | 80%                  |
| 5     | 100%                 |

### Durability

Every item has a **durability** value, randomized **1–5** on creation. Each time an item is used in an encounter slot, its durability drops by 1; at 0 it breaks and is removed.

Item slots in encounter options can be configured to **destroy** the item on use (e.g. giving it away) rather than ticking down durability.

Durability is intended to be used alongside with the tag system. So only items with tags that are used in skill checks have durability. Items that are only used for their consumable or medical properties do not have (visible) durability. Therefore all item slots that accept items not by tag (specific item or custom list) must destroy the item on use, as durability is not relevant for those items. In the game it is communicated as "Remaining uses" in the item tooltip.

## Consumables

Items may have consumable properties. Each item has a ConsumptionCategory which is either:
- None
- Food
- Drink
- Drug

If the category is not None, the item has a **consumption effect**. The effect can be, regardless of category, any combination of:
- Providing nutrition (reduces hunger severity)
- Providing hydration (reduces thirst severity)
- Applying a health condition (optionally with an initial severity value defined)
- Permanently modifying one or more skills

Consuming an item is a fixed outcome option available in the morning or at the very end of encounters. The item is destroyed on consumption.

## Passive Stat Modifiers

Items may have passive stat modifiers that are applied while the item is in the player's inventory. These can modify any stat by any value.

## Loot Tables

Loot tables are weighted random item selections used wherever random items are generated (general biome tables, searching, containers, rewards). A table maps items (and optionally other tables as sub-entries) to a rarity tier (from extremely rare to very common, which scale by x2 on every tier), rather than arbitrary numbers, so the relative commonness between entries stays easy to read and reason about at a glance.

When an encounter's loot table is used in a specific biome, it can be combined with that biome's loot table. When using this, items or categories present in both are averaged toward a shared rarity, while items unique to either side are carried over unchanged. This lets a biome introduce its own flavor items into an encounter's loot without needing bespoke per-biome logic, and keeps a biome's overall tendency from silently overpowering an encounter's own identity.

Loot tables should be kept small and varied between encounters and biomes so individual items retain distinct identity and use cases.

---

# World Map

A grid of pointy-top hex tiles representing the quarantine zone. Each day the player moves to an adjacent tile (the default move distance is 1). Each tile has a **biome**, a persistent **danger level**, and a **location encounter**.

## Location Encounters per Tile

At world generation, **every tile is assigned a location encounter**, but only **landmarks and special/quest encounters are visible from the start**. Generic encounters remain hidden until the player first steps on the tile. Pre-assigning everything gives full control over content distribution (e.g. spreading the generic encounters across the map by commonness and biome bias) while preserving the feel of discovery.

Location encounters are **persistent**: their state is saved, and revisiting a tile resumes the same encounter where it was left.

## Quarantine Borders

The zone is enclosed by an electrified quarantine fence — an impassable border. Outside the fence is a ring of "freedom" tiles; reaching one is a win. The player starts near the centre.

## Roads

Roads are drawn onto the map at generation and act as implicit guidance:

- They connect the cities to each other.
- They connect the starting tile to the **fence gate** (one of the escape routes), giving the player a natural breadcrumb toward a goal without an explicit quest.
- **Movement bonus:** if the player both starts and ends their daily move on a road tile, they may move **2 tiles** instead of 1.

## Danger Level

Each tile has a persistent danger level that drives the chance and intensity of night encounters. It starts at "safe" and increases by one level after each night the player spends on that tile, which pushes the player to keep moving rather than camping. This persistent value increased by nights spent on the tile, and can further be modified (in both ways) by certain encounter outcomes.

Camp setup during the evening can apply a temporary safety modifier that only affects that single night's encounter roll, without touching the tile's underlying danger level. Both the current danger level and the temporary modifier are shown to the player in the UI.

A toggleable danger overlay colour-codes every tile by (base) danger level for at-a-glance route planning.

## Biomes

A tile's biome determines which location encounters can appear there, options in the evening encounter, the biome's loot table, and the background sprites. Encounters that appear across multiple biomes may be tweaked by biome (added options, modified difficulty), but the design rule is to **avoid cases where every biome must be handled separately** — biome influence should be lightweight (a difficulty modifier, an availability chance), not bespoke per-biome content.

The following passable biomes exist:
- **Woods:** Forested areas with trees, bushes, and undergrowth. Loot is mostly natural (plants, berries, mushrooms) and scavenged (wood, sticks, branches). Encounters are often wildlife or scavenging, sometimes abandonded or hidden objects and structures. Encountering people is very rare.
- **City:** Urban areas with buildings, streets, and alleys. Loot is mostly scavenged (tools, food, medicine). Encounters are often scavenging,  people and urban structures and objects.
- **Outskirts:** Rural areas with open fields, farms, and small settlements. Loot is mostly natural (plants, crops) and scavenged (tools, food). Encounters can vary a lot from wildlife and people to rural structures and objects.

Additional impassable biomes exist to shape the map and create natural paths/borders.
- **Lake:** Simply a body of water.

## Encounter Markers

A tile's known location encounter is shown on the map as a simplified marker indicating type and state. **Generic markers are grayscale; quest markers are coloured**, so important tiles stand out. An **area** is a named collection of tiles (the zone, a city, a forest, a lake) used by quests to reference locations generally ("go to city X" rather than a specific tile).

The exact sprite that is used for a marker is determined by the encounter itself, and can vary depending on its state. This can be useful to easily identify on the map in what state an encounter was left. A simple example is if a container is opened/closed when the player left.

The same dynamic system is also in place for the encounter label that is displayed on the world map when a tile is hovered with a visible location encounter.

## Areas

An area is a named collection of connected tiles, usually a large same-biome cluster. Areas are used by quests to reference locations generally ("go to city X" rather than a specific tile). Areas are generated at world generation and are persistent.

Areas can be purely technical (like the quarantine zone) or be named and visible on the world map (like cities, forests, and lakes).

## World Generation

The map is generated per new game from the start tile at 0/0:

1. Generate the **shape** of the quarantine zone (radius expansion, random protrusions for natural edges, a final smoothing pass). This enclosed area is the zone.
2. Add an outer ring of **freedom tiles** beyond the fence.
3. Assign **natural biomes** via per-biome perlin layers and priority, with a fallback biome.
4. Generate **cities** (seed a tile, expand to a target size); all city tiles get the city biome; each city is an area.
5. Group large same-biome clusters into named **areas**.
6. Assign the **fence encounter** to every tile adjacent to the quarantine fence.
7. Generate **roads** (city-to-city, start-to-fence-gate).
8. Assign a **location encounter to every remaining tile** (hidden unless landmark/special).
9. Place **landmarks** and the predetermined (often hidden) encounters for quest chains and escape routes via the StoryManager.

---

# Quests

Quests represent some form of task (reach a tile, bring an item, meet someone, etc.) with effects on game state (unlocking encounters, changing existing ones, granting items, changing stats, can be anything). When given, a quest usually predetermines the location encounters of the affected tiles so the player can plan a route; quest markers are visible before stepping on the tile and are coloured. Quests are surfaced in the **Notes** panel.

Each `QuestDef` tracks state: Inactive, Active, Completed, or Failed. Quest state can affect encounters (e.g. an option that would grant an already-active quest is hidden).

**Repeatable quests:** a `QuestDef` can be marked repeatable, allowing multiple active instances at once; completing a repeatable quest returns it to Inactive rather than Completed/Failed. Active quests are stored as a flat list so repeatable instances can coexist.

**Auto-placement:** a `QuestDef` can specify an encounter to place and a search radius. When such a quest starts, the system finds a nearby empty tile, places the encounter there, sets the quest location, and formats the quest text with the tile coordinates. Story-driven quests with fixed or manually-set locations skip auto-placement.

## Rumours

Rumours are a system that allow the player to discover a random quest from a pool, so the same events in an encounter can lead to different quests in different playthroughs, increasing variety and reducing the potential to min-max and play optimally by adding a source of randomness.

A pool of `RumourDef`s each reference a `QuestDef` (which owns all quest behaviour — repeatability, placed encounter, radius, text) plus the rumour text shown when learned. Calling `LearnRumour()` (with no parameters — the randomization is the point) picks a random rumour, creates and starts its quest (triggering auto-placement if defined), and returns a standardized "you learned a rumour…" message. If no empty tile can be found for placement, the rumour gracefully fails to take and the encounter text adapts.

---

# Encounters

An encounter is any situation requiring player input. The player always faces a location encounter in the afternoon, an evening encounter in the evening, and possibly a night encounter. Encounters vary widely: one step or many, one option or many, linear or branching.

In each state during the encounter, the player is confronted with a set of **encounter options**. Selecting an option is how the game progresses. Options can lock out others, require prior success, be once-only, once-per-day, repeatable-until-success, and so on. The design space is intentionally open. The current text and set of available options are referred to as the **current encounter step**.

During an encounter, free item use is disabled — items can only be used through option slots. Once the encounter ends, the player can freely use items again and then choose to advance the time of day. (The morning is an exception, with free item use throughout.) An encounter always resolves within a single time of day.

## Encounter Types

### Location encounters
The main afternoon encounters, bound to a tile and **persistent** — they keep their state and can be returned to. Usually generated when the tile is first entered, based on biome and game state; some are **predetermined** (quest, landmark, or temporary rumour markers) and visible on the map beforehand, giving the player direction and goals.

### Evening encounters

The evening encounter is a single, place-agnostic encounter, not tied to any specific setting or scene, and not persistent across days. Framed purely as a generic action menu ("How would you like to spend your evening?"), with no randomized location and no implied permanence, since evenings can never be revisited the way location encounters can.

Biome still meaningfully shapes the evening through option availability, difficulty, and which biome-exclusive option (if any) is offered.

Options split into two categories:

**Camp options (non-terminal, sprite-bound)**: setting up camp, done in any combination before choosing how to spend the rest of the evening:

**Set up Shelter:** Fixed outcome, requires a Tent. Provides a temporary safety modifier for tonight only (see Danger Level).
**Set up Sleeping Spot:** Fixed outcome, requires a Bedroll. Boosts natural healing for the night.
**Set Trap:** Fixed outcome, requires a Trap. Can help reduce night attack intensity or provide resources if the trap is triggered. If triggered, trap is destroyed.
**Make Fire:** Skill check (Survival), requires a Fuel item (consumed) and accepts an optional Fire Starter item to reduce difficulty. Enables cooking and prevents animal-based night encounters.

see Camp System for more details.

Camp items leave the inventory the moment they're placed and reappear (or are reported lost/broken) in the morning report, using the same mechanism as the existing trap system generalized across all four camp components. Camp sprites remain visible into the Night encounter, and an active Shelter or Fire can affect that night's encounter difficulty or availability.

Setting up a camp grants a temporary morale-boosting health condition the following day, scaled by which components were set up:
- Bedroll: +3 morale
- Tent: +2 morale
- Fire: +1 morale

**Spend-the-evening options (terminal, dialogue list)**: exactly one is chosen, ending the encounter:

**Rest Early:** Fixed outcome, safe, available everywhere. Extra natural healing.
**Scavenge:** Skill check (Survival), available everywhere. Draws from the current biome's loot table.
**Train Skill:** Skill check, one option per biome, training a specific skill (see Skills). Difficulty self-scales against the player's current value in that skill so it tapers off as the skill grows. Each has an associated item tag for difficulty reduction. Woods trains Survival, Outskirts trains Dexterity, City trains Social.

### Night encounters
Threat encounters during the night, **not persistent**, overwhelmingly about *avoiding* bad outcomes rather than gaining good ones (though critical success can still help). Usually an attack on the camp.

**Intensity** (1–3) is rolled from the tile's danger level. Each trap set in the evening reduces intensity by 1 (reducing it below 1 nullifies the encounter). Intensity typically scales the number/strength of attackers and the severity of outcomes.

## Design Philosophy

The first priority of every encounter is **interesting, meaningful choices**. Fun gameplay outranks realism.

- **Steps read at a glance.** Step text is short and concrete; option text is a verb (+subject) like "Persuade" or "Open Crate". The longer description (shown in the details box) should state intended effects and risks plainly, **not cryptically**.
- **Everything fits one fixed screen.** No camera control; the player never moves. Outcomes are shown by swapping sprites, a sound, and simple effects — not animation. Encounters may set a **camera zoom level** (orthographic size ~6–12) for a sense of scale (a crate is tight; a radio tower is wide), or a **x offset** to set the camera position, if something behind the player or very far away from the player should be visible.
- **Lightweight biome influence only**, never per-biome bespoke handling.
- **Mini-quests and interconnection.** Lean on the persistent location-encounter system: encounters should frequently imply a simple next goal (a buried cache that needs a shovel, a flare that promises a drop in 10 days, a persistent trader to return to). These needn't be real quest-log entries, just clear, inherent reasons to route and backtrack. Landmarks visible from the start should telegraph what they offer (a pharmacy → medical, a fuel station → fuel).
- **Focussed.** Encounters should have a clear purpose and avoid unnecessary complexity.

### Step Composition (rule of thumb)

A good step often offers:

- **One FixedOutcome option, no requirements:** Safe and consistent, with neutral or mildly +/- effect.
- **One SkillCheck option with a tag item slot:** The gamble: strong on success, painful on failure, made safer by a good item. If a step has more than one skill check, their success effects must be clearly distinct.
- **One FixedOutcome option with requirements:** A safe "good" outcome gated behind an item/level/skill requirement. If more than one, requirements and effects must be clearly distinct.

This is guidance, not law. The number of options can vary a lot. Make sure to use sprite-bound options where appropriate, as those can offer more choice variety without making the option list feel cluttered. It also always possible to bind multiple options to the same sprite, so a single sprite can offer multiple distinct choices.


### Skill Diversity across Options (rule of thumb)

Within a single encounter step, the different options should draw on **different skills** where possible — a Survival skill check, a Social requirement, a FixedOutcome option shouldn't all live on the same step. This naturally creates distinct paths for different builds and makes each option feel meaningfully different in what it asks of the player. As with step composition, this is a guideline rather than a hard rule; exceptions exist, but defaulting to skill diversity keeps steps from feeling like the same option in different clothes.

Skills also function as meaningful **requirements** on FixedOutcome options, giving the player clear upgrade goals ("come back when your Survival is 5") and rewarding investment in a particular skill with a safe, reliable option that others can't take.

## Steps

Encounters are built from steps, exactly one active at a time, often generated dynamically from the encounter's current state. A step with **no options is a final step**: reaching it ends the encounter (free item use returns, and the player can advance the time of day).

Each step can determine the ambient theme that should play given the current state of the encounter (Default / Tense / Uplifting).

## Encounter Options

Each non-final step offers options determined by the current state of the encounter. Options are one of two technical types: FixedOutcome or SkillCheck.

### Option Requirements

An encounter option can have **requirements** that determine if it is selectable by the player. An option whose requirements aren't met is shown greyed-out and non-interactable, which doubles as a clear in-world goal ("come back with a shovel," "raise Dexterity to 5").

The following types of requirements exist:
- **Item slots:** Each item slot marked as "required" must be filled for the option to be selectable.
- **Skill:** The player must have a skill at or above a defined value.
- **Misc**: Options can also require completely custom requirements (i.e. the player has completed a specific quest, or has a specific health condition, or does not have a specific health condition).

These requirements can be freely combined, and multiple can exist for an option (also multiple of the same type).

Requirements are especially useful on FixedOutcome options to define a clear "best option" locked behind a condition the player can work toward.

### Item Slots

Encounter options can have any number of item slots, in which the player can drag valid items from their inventory into the slot.

Each slot accepts items in exactly one mode:

- **Specific item:** Only one explicitly defined item.
- **Custom list:** A custom set of explicitly listed items. (e.g. a list of all edible items, or all items that can treat infections, or any custom list).
- **Tag:** Any item with the given tag. In SkillCheck options, the tag's level determines the difficulty reduction (see *Tag Levels*).

Additionally, a slot can have the following properties:

- **Required:** The option can't be chosen until the slot holds a valid item.
- **Destroys Item:** For slots that fully consume the placed item (i.e. when actively giving an item away). Otherwise, using an item in a slot ticks its durability down by 1.
- **Required Tag Level:** Only applicable if the acceptance mode is a tag. The slot will only accept items with that tag at or above the given level.

Some rules apply for item slots:
- Slots that require a specific item or a custom set of items must be a requirement slot, as only tag slots can have difficulty reductions for optional slots. Therefore slots that are not a requirement slot must have a tag set, as only tag slots can have difficulty reductions for optional slots.
- Slots that require a specific item or a custom set of items must destroy the item, as the durability system is intended to work with tags. Only items with tags have their durability shown (as it wouldn't really make sense to have a "durability" on a bottle of water), and since slots that require a specific item or a custom set of items could also accept items without tags, the durability system would lead to weird behavior with those kinds of items.

### FixedOutcome options
Always call the same outcome function. That function may include custom logic and randomness, but it doesn't use the success/failure ladder. Used for simple actions (skip, ignore) and for special outcomes that don't fit a skill check.

### SkillCheck options
A standardized RPG-style check with a calculated difficulty and a rolled outcome.

**Outcomes:** always Success or Failure; optionally Partial Success, Critical Success, Critical Failure — each calling its own outcome function.

**Roll math:** roll 0–100.
- Roll ≥ difficulty → **success**; otherwise **failure**.
- In failure, roll > 50% of difficulty → **partial success** instead.
- In failure, roll < 10% of difficulty → **critical failure** instead.
- In success, roll in the top 10% of the range above difficulty → **critical success** instead.

**Roll animation:** When a skill check is chosen, a short flashy animation plays *before* the outcome resolves — a horizontal bar segmented and coloured by the possible outcomes (critical failure → failure → partial → success → critical success), with the rolled number landing on the bar. Then the outcome effect plays.

**Difficulty calculation:** Start from a base difficulty (1–100). Apply **additive** modifiers — morale (factor 1), the relevant player skill(s) times their factor (1-4), biome modifiers, and encounter-specific modifiers (e.g. prior choices in this encounter). Then apply the **percentage** reduction from any filled item slot (per tag level). Clamp the result to **[5, 200]**. The floor of 5 means success is never guaranteed by skills alone; at 200 only failure/critical failure remain.

### Option Outcomes

The outcome of a selected option, fixed or resolved from skill-check, can be pretty much anything. The design space here is very open-ended and allows for creativity.

Here's a list with some common examples of what an outcome can do, but it is not exhaustive:

- Change internal encounter state, which can affect the next step's text and available options
- Change internal state of other persistent encounters
- Change morale
- Increase skills
- Grant / remove items
- Change item durability
- Add / modify / remove health conditions
- Receive / Complete / Fail quests
- Learn rumours
- Change tile danger levels
- Reveal encounters on other tiles
- Initiate trade

Non state altering things can also happen in an outcome, like: playing a sound, changing the camera zoom or offset, or changing the background sprite.
- Playing a sound effect
- Changing the encounter visually

## Sprite-Bound Options

Because this is a point-and-click game, options can be attached to the actual scene sprites (e.g. the player's own head, an encounter prop) rather than living only in the option list:

- Every interactable sprite is traced with a **dashed outline** following the shape of its collider. There are no hidden interactions.
- The outline's colour communicates **availability**: the accent colour if at least one bound option is currently selectable, grey/dim if none are.
- Outlines sit at a **low default opacity** and brighten the closer the cursor gets to that specific sprite's shape, independently of any other sprite. Holding **Left Alt** reveals every hotspot at full opacity at once, regardless of cursor position.
- Hovering a sprite shows its option card(s)
- Clicking a sprite **locks** its card(s) in place; only one sprite can be locked at a time. Clicking a different sprite transfers the lock directly, and clicking empty space clears it. Holding a dragged item over a sprite for a short moment also locks it, so the player can drop the item without needing a separate click first.
- While a sprite is locked, hover-previews from other sprites are **suppressed** so only the locked card is shown, avoiding visual clutter.
- Locked/hovered options behave exactly like dialogue options: greyed out if requirements aren't met, accepting dragged items into their slots, then resolving on click.
- General options that can't be tied to a sprite (e.g. "Move On") stay in the list below the encounter text.

Options are the primary interaction point and are drawn large; their descriptive text lives in the details box rather than the option itself.

---

# Trading

Any encounter can start a trade session via `InitiateTrade`, temporarily replacing the normal options with trade options. The encounter defines what's buyable/sellable and whether information (rumours) can be bought.

- **Buy [item]:** Costs coins equal to its value, each coin placed in a required slot.
- **Sell [item]:** Place the item in a required slot to receive coins equal to its value.
- **Buy information** (if enabled, once per day): 3 coins to reveal a rumour.
- **Done trading:** Return to the encounter's normal options.

---

# Gameplay Loop

The player must escape the zone; death ends the run (the screen fades to black with the cause of death, then offers a new game or the main menu). The game is day-based, each day split into **Morning, Afternoon, Evening, Night**. Transitions fade through black; Night→Morning shows the new day number. During the black screen, a short handcart-and-footstep sound plays.

## Morning

On the world map at the current tile, no encounter, free item use. Any night events from the previous night are reported as bullet points. On day 1, the morning instead delivers the premise (escape ahead of the weapon test). Three actions:

- **Travel** to a highlighted adjacent tile → afternoon on that tile (2 tiles if the road bonus applies).
- **Stay** → afternoon on the same tile, resuming its location encounter in its saved state.
- **Rest** → skip the afternoon, advance to evening, and apply all conditions' natural healing.

The initial text of the morning encounter also always includes the **morning report**, basically acting as a log for night events that weren't directly visible to the player. Everything that happens during the night can append to the report. Examples are wounds infecting/healing, effect of traps, spread of the substance, and any other game-state events that happened during the night.

## Afternoon

Always the current tile's **location encounter**. Resumed in its saved state if visited before, or newly generated from biome and game state if not.

## Evening

A single generic evening encounter, place-agnostic and not persistent, where the player sets up camp (optional, non-terminal) and then chooses how to spend the rest of the evening (terminal). Biome affects option availability, difficulty, and the biome-exclusive option, but never the scene itself. See Evening encounters for the full option breakdown.

### Camp System

Camp components (Tent, Bedroll, Traps, Fire) share a common lifecycle:

The item used is removed from the inventory the moment it's placed and its sprite appears in the scene.
The camp persists visually into the Night encounter, and an active camp component can affect that night's encounter.
In the morning, each placed item either returns to the inventory (with durability reduced) or is reported lost/broken, and Fire simply extinguishes. All results appear in the morning report.

Trap specifically:

Reduces a night encounter's intensity by 1 (below 1 nullifies it).
If unused on an encounter, has a biome-based chance to catch an animal and yield an item.
If neither used nor triggered, an 80% chance to return to the cart in the morning (20% lost).

Traps (from the Trap item or evening options) protect the night. Each trap:
- Reduces a night encounter's intensity by 1 (below 1 nullifies it).
- If unused on an encounter, has a biome-based chance to catch an animal and yield an item.
- If neither used nor triggered, an 80% chance to return to the cart in the morning (20% lost).

All trap results appear in the morning report.

## Night

A chance of a **night encounter** based on the tile's effective danger level (base danger level + modifiers from camp). If none occurs, the night is skipped. Separately, invisible **night events** resolve from game state (e.g. an unbandaged wound getting infected) and are reported in the morning. Each night also advances the substance spread once it has been deployed.

---

# Ways to Win / Story Progression

The goal is to reach a freedom tile outside the fence. There are **multiple escape routes**, and they are deliberately **not linear quest lines**. The design takes from immersive sims: a key piece of information or item can be found by more than one path (a note, a rumour, or stumbling onto it). Guidance mechanisms always exist so the player can find *a* way, but they never block alternative means.

A **StoryManager** tracks progression and the state of each route, and places the predetermined (often hidden) encounters that support them.

The four escape routes for 1.0 (with exact costs, items, and locations defined in the Defs / StoryManager rather than here):

1. **Fence Gate: Bribe the guard.** The road from the start leads to a guarded gate; enough coins opens it. Coins are acquired broadly through normal encounters.
2. **Fence Gate: VIP license.** The same guard opens the gate for a license, which is locked in a town-hall safe requiring lockpicking skill/tools to crack.
3. **Cut through the fence.** One fence tile is unpowered and can be cut with a fence cutter. The fence cutter and the tile's location come from an NPC (Eli) in exchange for medicine; Eli's whereabouts can be learned at a radio tower (among other means).
4. **Helicopter.** A helipad tile holds a helicopter that needs a key and fuel. The helipad's location can surface via rumour; the key's location is revealed at the helipad; fuel comes from normal encounters or a fuel station.

Each route is modular: the player can discover its pieces through whatever combination of exploration, rumours, and scripted beats their run produces.

---

# End-Game: The Substance

The substance is the run's hard time limit and clear endpoint.

- It is **deployed on day 30** on a single tile.
- On **day 20**, that deployment tile is **marked** on the map as advance warning.
- A substance tile is **impassable**. Each night, every tile **adjacent** to a substance tile has a **50% chance** to become substance, and the tiles it will spread to next are marked.
- If the player is on (or fails to leave) a tile the substance spreads to, they **die**. As it spreads it eventually consumes the whole zone, so the pressure is ultimately unavoidable.

The substance frames the whole run: escape by the time it would reach you, or die. It also gives the otherwise open-ended map a rising tension curve toward the late game.

---

# Save / Load

- **Autosave** after each time-of-day transition.
- A saved run can be **loaded** from the menu.
- Web-build feasibility must be verified; if saving can't work reliably in a web build, the feature is cut.

---

# Handbook

A diary-style book with bookmark tabs along the top acting as a table of contents. Sections:

- **Quest Log:** An in-depth version of the HUD's Notes panel listing all learned information and quests.
- **Item Compendium:** Encountered items shown with full info; unencountered items shown as blank silhouettes. Filterable by tag and ordered by tag level within a filter.

The handbook also serves as the place to **signal acquisition routes** (how a given item can be obtained, how a given skill can be raised) so the player has a reference for working toward goals. (The HUD button for the handbook already exists.)

---

# Settings

A simple settings page with audio sliders, reachable from both the main menu and an in-game button. The in-game settings window also exposes a debug button.

---

# Tutorial

Optional, enabled when starting a game. Deliberately minimal: simple popups the first time a given thing happens. The game should otherwise be largely self-explanatory.
