# Keep Wandering

A point-and-click adventure game where the player must survive and escape a quarantined zone in a procedurally generated world by making choices in various encounters. The player manages their inventory, health, and quests to survive and eventually escape before the zone is destroyed.

This document describes the game's **systems and design**. Concrete content (specific items, health conditions, encounters, biomes, danger values, etc.) lives in the `Def` lists and is not duplicated here.

---

# Lore

The game takes place in a quarantine zone: a large area sealed off from the outside world ahead of a weapon test. The "outbreak" the public was told about is a cover — the real reason for the quarantine is that a disintegrating, spreading substance is scheduled to be deployed inside the zone. The player character is one of the survivors trapped inside, and their goal is to escape before the substance reaches them.

This gives the run a hard outer time limit: the substance is deployed on a fixed day and spreads from there, eventually consuming the entire zone (see *End-Game: The Substance*).

---

# Game Presentation

The game happens almost entirely on a single fixed screen. The player character and their handcart (inventory) occupy the left side; the current encounter occupies the right. The camera never pans and the player never moves position.

Top-of-screen UI:

- **Left:** day counter, world-map button, handbook button, settings button, and below them the health report.
- **Centre:** the current encounter step's text and options.
- **Right:** the player's stats and the "Notes" panel (active quests / learned information).

## Art Direction

Strictly **2D side-view comic style with no 3D depth or perspective**. Everything is sprite-based, hand-drawn, with no animation — state changes are communicated by swapping sprites, playing a sound, and simple effects rather than motion.

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

Items can be dragged into encounter option **item slots**, or directly onto relevant sprites — e.g. dragging a bandage onto a wound sprite tends it, dragging a crowbar onto a crate fills that option's slot. Item actions (eat, drink, apply) are available both via right-click context menu and via drag-and-drop onto an appropriate target.

Encounter options can be **bound to sprites** in the scene (see *Encounters → Sprite-Bound Options*), so the player can interact with the world directly, not only through the option list.

---

# Player

The player character has stats, health conditions, and an inventory.

## Stats

Eight integer stats, default value 0, that act as direct modifiers to the difficulty of encounter options of their type:

- **Combat** — fighting, defending, using weapons.
- **Strength** — physical force: fighting, carrying, breaking.
- **Dexterity** — fine control: sneaking, lockpicking, disarming.
- **Intelligence** — puzzles, crafting, finding hidden things.
- **Charisma** — persuading, intimidating, negotiating.
- **Agility** — quick reactions: dodging, running.
- **Perception** — noticing hidden enemies, items, traps.
- **Morale** — general mental state; applies to *all* options as a flat modifier. Affected by hunger, thirst, injuries, quests, and events.

Stats can be modified **temporarily** (bound to an active condition such as a health condition, biome, or time of day) or **permanently** (from specific encounter outcomes or quest completions). A permanent modifier simply changes the stat's base value, which starts at 0. Stats are clamped to **-30 / +30**.

## Health System

Health is not a single bar. It is a **collection of conditions** the player currently has, each with its own behaviour, effects, and treatment.

**Core mechanics shared by all conditions:**

- **Severity** — a hidden meter. In general, higher severity means a stronger effect. How severity behaves is defined per condition.
- **Stages** — each condition defines stages keyed to severity thresholds; the active stage determines current effects. Single-stage conditions are simply present or absent with a fixed effect.
- **Natural healing** — each night (and on the Rest action) severity is reduced by the condition's natural-healing value, with small random variation.
- **Lethal threshold** — some conditions kill the player if severity reaches their lethal value, ending the run.
- **Max instances** — how many instances of a condition type can coexist. Gaining one beyond the max instead adds its severity to a random existing instance.
- A condition is removed when its severity reaches 0.

Conditions split into two categories:

### Needs

Permanently present conditions whose severity rises over time and is reduced by consuming items (e.g. hunger reduced by nutrition, thirst by hydration). Unlike normal conditions, needs are only shown in the health report once they pass a visibility threshold.

### Conditions

Gained and lost throughout the run, always shown in the health report, applied with a defined initial severity. Includes fractures (which choose a random limb side and have their own instancing), blood loss, electrocution, and others — all defined in `HealthConditionDefs`.

### Wounds

A special subcategory of condition sharing common tending/infection logic. The severity value tracks **infection** rather than a damage amount, so wounds are always applied at the same initial severity. Each wound carries **tended** and **treated** flags:

- **Tending** (item with the appropriate medical tag) reduces ongoing harm and improves natural healing.
- **Treating** (item with an antiseptic tag) protects against infection.
- An untended/untreated wound's infection severity tends to worsen each night; infection progresses through stages with escalating penalties and is lethal at the top.

Different wound types layer an additional effect on top of the shared logic (e.g. cut wounds drive ongoing blood loss while untended; bruise wounds slow fracture healing). Exact wound types and values live in the Defs.

### Generalized Damage Helpers

Encounters apply damage through standardized helpers rather than always targeting a specific condition:

- **Apply Random Wound** — a new wound from a pool (typically cut/bruise).
- **Take Bruise Damage (severity X)** — a bruise wound plus fracture damage to a random limb.
- **Take Cut Damage (severity X)** — a cut wound plus an immediate blood-loss increase.
- **Take Random Damage (severity X)** — randomly one of the above.

---

# Inventory / Items

The player carries a wooden handcart representing their inventory. Items are physics sprites that settle in the cart and can be dragged freely; items added spawn above the cart and fall in; removed items vanish; items knocked offscreen respawn so they cannot be lost. Hovering shows a tooltip; clicking/right-clicking offers item actions (eat, drink, apply, etc.).

There is a carry limit; its exact implementation is still being tuned.

## Item Tags

Each item can have any number of **tags**. Tags are the mechanism by which item slots decide which items they accept. Tags are never shown to the player directly as raw data, but the player learns an item's tags and levels through use and the handbook.

Two conventions of tag:

- **General-purpose tags** describe what an item *is* (Food, Tool, Medical, Weapon, Trash).
- **Activity tags** describe what an item is *used for* (Combat, Scavenging, Lockpicking, Digging, Cutting…).

An item can carry tags from both. Technically there is no difference; it's a design convention. Because there are many tags, it's fine for them to be specific and overlapping.

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

This replaces the old per-slot value system: encounter options no longer specify how much each slot reduces difficulty. An option's slot only declares which tag (or specific item / custom list) it accepts; the item's level does the rest. This is easier to understand, easier to display in the UI, and far less work to author.

### Item Slot Accept Modes

Each slot accepts items in exactly one mode:

- **Specific item** — only one explicitly defined item.
- **Tag** — any item with the given tag (level drives the reduction).
- **Custom list** — a custom set of explicitly listed items.

## Requirements

Options can carry **requirements** that gate availability — an option whose requirements aren't met is shown greyed-out and non-interactable, which doubles as a clear in-world goal ("come back with a shovel," "raise Dexterity"). Requirement types:

- A **specific item** is present.
- An item with a **tag at a minimum level** (e.g. "Lvl 3 Lockpick").
- A **stat minimum** (e.g. "Dexterity 5").

Requirements are especially useful on FixedOutcome options to define a clear "best option" locked behind a condition the player can work toward.

## Durability

Every item has a **durability** value, randomized **1–4** on creation. Each time an item is used in an encounter slot, its durability drops by 1; at 0 it breaks and is removed. This replaces the old random "chance to break," which had no strategic value and only produced bad-luck frustration. Durability is predictable: the player always knows a use has a real, finite cost.

Currency and single-use items are consumed fully on use rather than ticking down durability (e.g. coins spent in trade, a fence cutter consumed on use).

## Consumables

The consumable system (eating, drinking, applying medical items) is fully separate from tag levels and durability. Consumable effects must be communicated **clearly and non-cryptically** in the item description — exact nutrition/hydration, any chance of negative effects such as food poisoning, etc. No hidden values.

Cooking is supported as a way to transform raw food (e.g. raw meat) into a safer/better form, offered as an evening action and/or item interaction.

## Loot Tables

Loot tables are weighted random item selections used wherever random items are generated (searching, containers, rewards). A table maps items — and optionally other tables as sub-entries — to weights. Each biome has its own loot table, typically referencing the general tables. Loot tables should be kept small and varied between encounters and biomes so individual items retain distinct identity and use cases.

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

Each tile has a persistent danger level, shown to the player, that drives the chance and intensity of night encounters. It **starts at "safe"** and **increases by one level after each night the player spends on that tile**, which pushes the player to keep moving rather than camping. Encounters can also modify danger levels. The five levels and their exact night-encounter probabilities are defined in `DangerLevelDefs`.

A toggleable **danger overlay** colour-codes every tile by danger level for at-a-glance route planning.

## Biomes

A tile's biome determines which location encounters can appear there, the biome encounter used in the evening, the biome's loot table, and the background sprites. Encounters that appear across multiple biomes may be tweaked by biome (added options, modified difficulty), but the design rule is to **avoid cases where every biome must be handled separately** — biome influence should be lightweight (a difficulty modifier, an availability chance), not bespoke per-biome content.

Biomes for 1.0: **Woods, City, Outskirts**, plus **Lake** (impassable; used to shape the map and create natural paths/borders). Each biome defines its most important stats and its loot table in `BiomeDefs`.

## Encounter Markers, Areas

A tile's known location encounter is shown on the map as a simplified marker indicating type and state. **Generic markers are grayscale; quest markers are coloured**, so important tiles stand out. An **area** is a named collection of tiles (the zone, a city, a forest, a lake) used by quests to reference locations generally ("go to city X" rather than a specific tile).

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

Quests are tasks given by encounters — reach a tile, bring an item, meet someone, etc. — with effects on game state (unlocking encounters, changing existing ones, granting items). When given, a quest usually predetermines the location encounters of the affected tiles so the player can plan a route; quest markers are visible before stepping on the tile and are coloured. Quests are surfaced in the **Notes** panel.

Each `QuestDef` tracks state: Inactive, Active, Completed, or Failed. Quest state can affect encounters (e.g. an option that would grant an already-active quest is hidden).

**Repeatable quests:** a `QuestDef` can be marked repeatable, allowing multiple active instances at once; completing a repeatable quest returns it to Inactive rather than Completed/Failed. Active quests are stored as a flat list so repeatable instances can coexist.

**Auto-placement:** a `QuestDef` can specify an encounter to place and a search radius. When such a quest starts, the system finds a nearby empty tile, places the encounter there, sets the quest location, and formats the quest text with the tile coordinates. Story-driven quests with fixed or manually-set locations skip auto-placement.

---

# Rumours

Rumours randomize which quests a player discovers, keeping runs varied. (Quests can also come from scripted story beats or fixed encounter outcomes.)

A pool of `RumourDef`s each reference a `QuestDef` (which owns all quest behaviour — repeatability, placed encounter, radius, text) plus the rumour text shown when learned. Calling `LearnRumour()` (with no parameters — the randomization is the point) picks a random rumour, creates and starts its quest (triggering auto-placement if defined), and returns a standardized "you learned a rumour…" message. If no empty tile can be found for placement, the rumour gracefully fails to take and the encounter text adapts.

Rumours are **always full** — the earlier partial-rumour variant (player knows location but not details) has been removed as unintuitive and hard to design around.

---

# Encounters

An encounter is any situation requiring player input. The player always faces a location encounter in the afternoon, a biome encounter in the evening, and possibly a night encounter. Encounters vary widely: one step or many, one option or many, linear or branching. Options can lock out others, require prior success, be once-only, once-per-day, repeatable-until-success, and so on. The design space is intentionally open.

During an encounter, free item use is disabled — items can only be used through option slots. Once the encounter ends, the player can freely use items again and then choose to advance the time of day. (The morning is an exception, with free item use throughout.) An encounter always resolves within a single time of day.

## Encounter Types

### Location encounters
The main afternoon encounters, bound to a tile and **persistent** — they keep their state and can be returned to. Usually generated when the tile is first entered, based on biome and game state; some are **predetermined** (quest, landmark, or temporary rumour markers) and visible on the map beforehand, giving the player direction and goals.

### Biome (evening) encounters
The evening encounter, based purely on the current biome. **Not persistent** — a fresh instance each evening — so they carry no narrative weight and exist to give the player a moment of control.

The evening is a **generic action menu** ("How would you like to spend your evening?") with **no scene-specific setting**.

Standard evening actions, with availability or chances varying by biome/tile:

- **Set Trap** *(non-terminal)* — place a trap for the night; returns to the menu so it can be combined with one terminal action.
- **Fortify** *(skill check)* — reinforce the sleeping spot to reduce the night's danger. Accepts building materials/tools to lower difficulty; a critical success grants a bonus.
- **Scavenge** *(skill check)* — search for an item from the biome loot table. Accepts scavenging items.
- **Cook** — turn raw food into a safer/better form.
- **Rest Early** *(fixed outcome)* — turn in for extra natural healing.
- **Find Trader** — seek out a trader and enter a trade session.

### Night encounters
Threat encounters during the night, **not persistent**, overwhelmingly about *avoiding* bad outcomes rather than gaining good ones (though critical success can still help). Usually an attack on the camp.

**Intensity** (1–3) is rolled from the tile's danger level. Each trap set in the evening reduces intensity by 1 (reducing it below 1 nullifies the encounter). Intensity typically scales the number/strength of attackers and the severity of outcomes.

## Design Philosophy

The first priority of every encounter is **interesting, meaningful choices**. Fun gameplay outranks realism.

- **Steps read at a glance.** Step text is short and concrete; option text is a verb (+subject) like "Persuade" or "Open Crate". The longer description (shown in the details box) should state intended effects and risks plainly — **not cryptically**.
- **Everything fits one fixed screen.** No camera control; the player never moves. Outcomes are shown by swapping sprites, a sound, and simple effects — not animation. Encounters may set a **camera zoom level** (orthographic size ~5.4–12) for a sense of scale (a crate is tight; a radio tower is wide).
- **Lightweight biome influence only**, never per-biome bespoke handling.
- **Mini-quests and interconnection.** Lean on the persistent location-encounter system: encounters should frequently imply a simple next goal (a buried cache that needs a shovel, a flare that promises a drop in 10 days, a persistent trader to return to). These needn't be real quest-log entries — just clear, inherent reasons to route and backtrack. Landmarks visible from the start should telegraph what they offer (a pharmacy → medical, a fuel station → fuel).

### Step Composition (rule of thumb)

A good step often offers:

- **One FixedOutcome option, no requirements** — safe and consistent, with neutral or mildly +/- effect.
- **One SkillCheck option with a tag item slot** — the gamble: strong on success, painful on failure, made safer by a good item. If a step has more than one skill check, their success effects must be clearly distinct.
- **One FixedOutcome option with requirements** — a safe "good" outcome gated behind an item/level/stat requirement. If more than one, requirements and effects must be clearly distinct.

This is guidance, not law. Keep the number of **unrequiremented** options to **at most 3** so steps don't overwhelm. Requirements let a step offer more total options without clutter, since locked ones read as goals rather than noise.

## Steps

Encounters are built from steps, exactly one active at a time, often generated dynamically from the encounter's current state. A step with **no options is a final step**: reaching it ends the encounter (free item use returns, and the player can advance the time of day).

## Options

Each non-final step offers options determined by current state. Options are one of two technical types.

### Item Slots

A slot accepts items by specific item, tag, or custom list (above). Slot properties:

- **Required** — the option can't be chosen until the slot holds a valid item.
- **Consumes on use** — for slots that fully consume the placed item (currency, single-use items). Otherwise, using an item in a slot ticks its durability down by 1.
- **Difficulty reduction (skill checks)** — driven entirely by the placed item's tag level (see *Tag Levels*); the option doesn't specify a per-slot amount. Final difficulty never drops below 5.

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

**Roll animation:** when a skill check is chosen, a short flashy animation plays *before* the outcome resolves — a horizontal bar segmented and coloured by the possible outcomes (critical failure → failure → partial → success → critical success), with the rolled number landing on the bar. Then the outcome effect plays.

**Difficulty calculation:** start from a base difficulty (1–100). Apply **additive** modifiers — morale (factor 1), the relevant player stat(s) times their factor, biome modifiers, and encounter-specific modifiers (e.g. prior choices in this encounter). Then apply the **percentage** reduction from any filled item slot (per tag level). Clamp the result to **[5, 200]**. The floor of 5 means success is never guaranteed by stats alone; at 200 only failure/critical failure remain. (The system is extensible to companion and weather modifiers — see *Out of Scope*.)

### Option Outcomes

Outcomes can: change stats; grant/remove items; add or modify health conditions; change quests/rumours; change tile danger levels; generate or reveal encounters on other tiles; place traps (evening); or initiate trade.

## Sprite-Bound Options

Because this is a point-and-click game, options can be attached to the actual scene sprites rather than living only in the option list:

- Interactable sprites are **highlighted with an indicator** — there are no hidden interactions.
- Hovering a highlighted sprite reveals its available options; clicking it **pins** those options on/near the sprite (only one element can be pinned at a time).
- Pinned options behave exactly like list options — greyed out if requirements aren't met, accepting dragged items into their slots, then resolving on click.
- General options that can't be tied to a sprite (e.g. "Move On") stay in the list below the encounter text.

Options are the primary interaction point and are drawn large; their descriptive text lives in the details box rather than the option itself.

---

# Trading

Any encounter can start a trade session via `InitiateTrade`, temporarily replacing the normal options with trade options. The encounter defines what's buyable/sellable and whether information (rumours) can be bought.

- **Buy [item]** — costs coins equal to its value, each coin placed in a required slot.
- **Sell [item]** — place the item in a required slot to receive coins equal to its value.
- **Buy information** (if enabled, once per day) — 3 coins to reveal a rumour.
- **Done trading** — return to the encounter's normal options.

Coins and zero-value items are excluded from buy/sell lists. Trade is used by both location encounters (e.g. a persistent trader) and biome encounters (the evening Find Trader action).

---

# Gameplay Loop

The player must escape the zone; death ends the run (the screen fades to black with the cause of death, then offers a new game or the main menu). The game is day-based, each day split into **Morning, Afternoon, Evening, Night**. Transitions fade through black; Night→Morning shows the new day number. During the black screen, a short handcart-and-footstep sound plays (see *Audio*).

## Morning

On the world map at the current tile, no encounter, free item use. Any night events from the previous night are reported as bullet points. On day 1, the morning instead delivers the premise (escape ahead of the weapon test). Three actions:

- **Travel** to a highlighted adjacent tile → afternoon on that tile (2 tiles if the road bonus applies).
- **Stay** → afternoon on the same tile, resuming its location encounter in its saved state.
- **Rest** → skip the afternoon, advance to evening, and apply all conditions' natural healing.

## Afternoon

Always the current tile's **location encounter** — resumed in its saved state if visited before, or newly generated from biome and game state if not.

## Evening

The current biome's evening encounter (the generic action menu above). A fresh instance each evening.

### Trap System

Traps (from the Trap item or evening options) protect the night. Each trap:
- Reduces a night encounter's intensity by 1 (below 1 nullifies it).
- If unused on an encounter, has a biome-based chance to catch an animal and yield an item.
- If neither used nor triggered, an 80% chance to return to the cart in the morning (20% lost).

All trap results appear in the morning report.

## Night

A chance of a **night encounter** based on the tile's danger level; if none occurs, the night is skipped. Separately, invisible **night events** resolve from game state (e.g. an untended wound getting infected) and are reported in the morning. Each night also advances the substance spread once it has been deployed (see below).

---

# Ways to Win / Story Progression

The goal is to reach a freedom tile outside the fence. There are **multiple escape routes**, and they are deliberately **not linear quest lines** — the design takes from immersive sims: a key piece of information or item can be found by more than one path (a note, a rumour, or stumbling onto it). Guidance mechanisms always exist so the player can find *a* way, but they never block alternative means.

A **StoryManager** tracks progression and the state of each route, and places the predetermined (often hidden) encounters that support them.

The four escape routes for 1.0 (with exact costs, items, and locations defined in the Defs / StoryManager rather than here):

1. **Fence Gate — bribe the guard.** The road from the start leads to a guarded gate; enough coins opens it. Coins are acquired broadly through normal encounters.
2. **Fence Gate — VIP license.** The same guard opens the gate for a license, which is locked in a town-hall safe requiring lockpicking skill/tools to crack.
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

# Audio / FX

- **SFX** for everything; the highest-priority single effect is the **skill-check resolution** sound.
- **Ambient moods:** an enum that encounters set; the AudioManager plays a different ambient track set per mood. When the mood changes, the previous set **pauses rather than stops**, so returning to it resumes mid-track instead of restarting — the player isn't endlessly hearing track intros. Moods: **Default, Tense, Hopeful/Uplifting**, with **Desolate** and **Mystery** as optional additions.
- **Transition sound:** during the black screen between times of day, hold black briefly and play a handcart-moving + footstep sound, optionally varying by biome.
- **Aesthetic direction:** sparse, handmade acoustic instrumentation (fingerpicked guitar, solo piano, cello, fiddle, harmonica) for a post-apocalyptic folk-ambient feel, matching the hand-drawn stick-figure art. Avoid synths, chiptune, beats, and cinematic orchestration.

---

# Save / Load

- **Autosave** after each time-of-day transition.
- A saved run can be **loaded** from the menu.
- Web-build feasibility must be verified; if saving can't work reliably in a web build, the feature is cut.

---

# Handbook

A diary-style book with bookmark tabs along the top acting as a table of contents. Sections:

- **Quest Log** — an in-depth version of the HUD's Notes panel listing all learned information and quests.
- **Item Compendium** — encountered items shown with full info; unencountered items shown as blank silhouettes. Filterable by tag and ordered by tag level within a filter.

The handbook also serves as the place to **signal acquisition routes** — how a given item can be obtained, how a given stat can be raised — so the player has a reference for working toward goals. (The HUD button for the handbook already exists.)

---

# Settings

A simple settings page with audio sliders, reachable from both the main menu and an in-game button. The in-game settings window also exposes a debug button.

---

# Tutorial

Optional, enabled when starting a game. Deliberately minimal: simple popups the first time a given thing happens. The game should otherwise be largely self-explanatory.

---

# Out of Scope for 1.0 (Future)

These systems are designed but deferred; none is required for the core experience and each may be added later:

- **Weather** — environmental conditions affecting sky/particles and acting as skill-check difficulty modifiers.
- **Companions** — non-inventory, non-health buff characters that modify stats/options and can trigger night events, gained and lost through encounters.
- **Additional biomes** — Mountains and Desert (each with their own important stats and loot tables).

The skill-check difficulty system is already structured to accept companion and weather modifiers when those systems land.
