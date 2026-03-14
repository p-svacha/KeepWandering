# Keep Wandering
A point and click adventure game where the player has to survive and escape a quarantined zone in a procedurally generated world by making choices in various encounters. The player has to manage their inventory, health, companions and quests to survive as long as possible and eventually escape the quarantine zone.

# Lore
The games takes place in a quarantined zone, which is a large area that has been sealed off from the outside world due to a mysterious outbreak of 'something'. The player character is one of the survivors trapped inside the quarantine zone, and their goal is to survive and eventually find a way to escape.


# Game Presentation
The game mainly happens on the same screen, which shows the player character and their cart with all their items on the left side, and the current encounter step on the right side.
The top side of the screen shows the all UI information:

- On the left the day counter, button to show the world map and below the health report of the player (and companions).
- In the center the current encounter step's text and options.
- On the right the player's stats and current active quests.

Everything is sprite based in an old-school flash art style, with a fixed position fully side-view camera and no animations. Black outlines and strong colors in foreground (player + encounter), no outlines and more washed colors in background. Characters are represented by stick figures, with head and torso being volumized and extremities just being stick lines. Persepective and depth is kept to a minimum, strict 2D side view. Some shading can be used but very sparingly. Colors used are quite saturated. The player character and their cart with all their items on the left side, and the current encounter on the right side.
Sprites change depending on the situation.
For example, the player should be rendered differently based on their health:

- If the player is hungry, the torso sprite is thinner.
- Wounds show as sprites on the big head, with overlays for tending and infection.
- Thirst shows as sweat drops on the head sprite.
- Poisoning shows as greenish tint on the head sprite.
- Blood loss shows as discoloring of torso sprite.
- Broken legs show as a broken leg sprite instead of the normal legs sprite.
- Broken arms show as a broken arm sprite instead of the normal arms sprite.
- etc.

The background is also a collection of sprites that change based on the current encounter step:

- The main background depends on the current biome.
- Sky sprites depend on the current weather and time of day.
- Some particle sprites can be added based on the current weather. For example, if it's raining, rain particle sprites can be added to the background.
- Additional background sprites can be added based on the current encounter step. For example, if the player is in a woods
biome and the encounter step is "hiding in bushes", additional bush sprites can be added to the background to make it look like the player is hiding in bushes.

And of course each encounter step has its own unique sprites showing the current state of the encounter.

Game feel wise a it's a simple point and click game, with additional focus on dragging and dropping items. All items are physical objects in the players handcart, that can be dragged and dropped around freely. When an item falls offscreen, it simple spawns back above the cart again, so nothing gets lost.
Items can also be dragged into encounter option item slots. Encounter options may also be connected to a sprite on screen, which will then have the same click/hover/item drag controls as the option itself. For example, if there is an option to "open crate", the crate sprite can be clicked/hovered/dragged into just like the option itself, and it will have the same effects.
Item interactions (like "eat beans", "apply bandage") etc. can either be done through a context menu when right clicking the item, or by dragging the item onto an appropriate sprite on the screen. For example, if the player has a bandage item, they can either right click the bandage and select "apply to wound", or drag the item on the wound sprite on the player character to apply it.


# Player
The player character is what the player controls and represents them in the game world. The player has an inventory, stats, health and can have companions.

## Stats
The player has a fixed set of stats that have an integer value. The default value is 0. The stats act as a direct modifier to the difficulty of encounter options of that type. The stats are:

- Combat: Affects combat options, such as fighting, defending, using weapons etc.
- Strength: Affects physical options, such as fighting, carrying heavy items, breaking things etc.
- Dexterity: Affects dexterous options, such as sneaking, picking locks, disarming traps etc.
- Intelligence: Affects intellectual options, such as solving puzzles, crafting items, finding hidden things etc.
- Charisma: Affects social options, such as persuading, intimidating, negotiating etc.
- Agility: Affects options that require quick reactions, such as dodging, running away, etc.
- Perception: Affects options that require noticing things, such as spotting hidden enemies, finding hidden items, noticing traps etc.
- Morale: Affects all options, as a general representation of the player's mental state. Morale can be affected by various things, such as hunger, thirst, injuries, companions, quests etc. High morale can give a bonus to all options, while low morale can give a penalty to all options.

Stat Values can both be temporarily and permanently affected.

Temporary modifiers are bound to a condition, and are active, such as long as that condition is present (i.e. health conditions, companions, weather, biome, time of day).

Permanent modifiers are usually the result of specific encounter option outcomes, such as "gain 1 strength permanently". These modifiers are active for the rest of the game and can not be removed or expire. Permanent modifiers can also be the result of fulfilling quests, such as "fulfill quest X to gain 2 charisma permanently". On a technical level, and also communicated in-game, these permanent modifiers simply change the base value of the stat, which is 0 at the start of the game.

Stats are capped at -30/30.

## Health Conditions
The health system tracks the player's physical and mental condition, including injuries, illnesses, wounds etc. Health is NOT tracked as a global number or health bar, but rather as a collection of conditions that the player can have. Conditions can have severity levels and each condition has their own behaviours, effects and ways of treatment.

All health conditions have a severity value, which is a meter that is hidden to the player. How that value behaves differs for each condition, but the general rule is that the higher the severity value, the bigger the effect of the condition.

Health conditions have a "Natural Healing" value. Each night, the severity value of each condition is automatically reduced by that natural healing value (with a small random variation). This healing is also applied when the player chooses the "Rest" action in the morning.

All health conditions also have defined set of stages. The active stage depends on the current severity value.
There's also single-stage conditions, which are either present or not. They have a fixed effect that does not depend on severity, and they usually have a specific way of treatment that instantly cures the condition, instead of reducing severity gradually.

Health conditions can have a Lethal Threshold. If the severity value reaches that threshold, the player dies and the game is over.

Health conditions often affect player stats, and can also have other effects, such as causing night events. The exact behaviours often depends on the current stage.
Most causes of death are related to health conditions. But there are also positive health conditions.

In the stages descriptions, the first value is always inlusive, and the second value up to the next integer value. So for example "Hungry: 8-10" means that the player is in the "Hungry" stage if the severity value of the Hunger condition is equal or greater than 8, and less than 11.

Conditions are split into two categories: Needs and Conditions

### Needs
Needs are health conditions that are permanently present on the player. A special property of needs is that, unlike Conditions, they are not necessarily shown on the health report, only if reaching a certain severity threshold.

The following needs exist:

#### Hunger
Severity increases by 1 every night. Starts at 5.

Stages:
- Well Fed: 0-2, +5 morale
- Nothing: 3-7, no effect, hidden in health report
- Hungry: 8-10, -2 morale
- Very Hungry: 11-13, -5 morale, -2 strength, -1 intelligence
- Starving: 14-16, -10 morale, -5 strength, -3 intelligence
- Lethal at 17

Severity can be reduced by consuming items that give nutrition.

#### Thirst
Severity increases by 1 every night. Starts at 5.

Stages:
- Hydrated: 0-1, +3 morale
- Nothing: 2-6, no effect, hidden in health report
- Thirsty: 7-8, -2 dexterity
- Very Thirsty: 9-10, -5 dexterity, -2 agility, -1 perception
- Dehydrated: 11-12, -10 dexterity, -5 agility, -3 perception
- Lethal at 13

Severity can be reduced by consuming items that give hydration.

### Conditions
Conditions are health conditions that can be gained and lost throughout the game. They are always shown in the health report, and therefore should have some effect at all stages. When a condition is applied, it is always applied with a specific initial severity value.

Conditions have a maximum instance amount. This amount defines how many instances of the condition type the player can have at the same time. If the player gains a condition that would put them over the maximum amount, this severity is instead added to a random existing instance of that condition.

If the severity of a condition reaches 0, it is considered healed and removed from the player.

#### Blood Loss
Max instances: 1
Natural Healing: 0.5

Stages:
- Stable: 0-1, no effect, invisible in health report
- Light Blood Loss: 2-4, -2 combat, -2 strength, -2 agility
- Heavy Blood Loss: 5-7, -5 combat, -5 strength, -5 agility
- Critical Blood Loss: 8-9, -8 combat, -8 strength, -8 agility
- Lethal at 10

Treatment: none - only heals naturally.

#### Leg Fracture
Max instances: 2
Natural Healing: 0.5

Stages:
- Sprained Leg: 0-3, -3 agility, -1 combat
- Cracked Leg: 4-7, -5 agility, -3 combat
- Broken Leg: 8+, -10 agility, -5 combat. Cannot move to different tiles on the world map in the morning.
- Capped at 10.

Treatment: Natural Healing can be increased by applying an item with the splint flag. Consumes the item.

Fractures have their own instancing logic, where each time a fracture is applied, a random side (left or right) is chosen. If a fracture already exists, the severity is added. Else a new one is created.

#### Arm Fracture
Max instances: 2
Natural Healing: 0.5

Stages:
- Sprained Arm: 0-3 -2 combat, -2 strength, -2 dexterity
- Cracked Arm: 4-7, -4 combat, -4 strength, -4 dexterity
- Broken Arm: 8+, -6 combat, -6 strength, -6 dexterity
- Capped at 10.

Treatment: Natural Healing can be increased by applying an item with the splint flag. Consumes the item.

Fractures have their own instancing logic, where each time a fracture is applied, a random side (left or right) is chosen. If a fracture already exists, the severity is added. Else a new one is created.

#### Electrocution
Max instances: 1
Natural Healing: 1

Stages:
- Stunned: 0-3, -3 dexterity, -3 agility, -1 combat, -1 perception
- Shocked: 4-7, -5 dexterity, -5 agility, -3 combat, -3 perception. 20% chance to gain Heart Arrhythmia condition during the night
- Severly Shocked: 8-9, -5 dexterity, -5 agility, -5 combat, -5 perception, -5 strength, -5 morale. 40% chance to gain Heart Arrhythmia condition during the night
- Lethal at 10

Treatment: none - only heals naturally.

#### Heart Arrhythmia
Max instances: 1
Natural Healing: none

Single Stage Condition: -3 agility, -3 morale

Treatment: Instantly cured with a defibrillator or heart medication, which consumes the item.

#### Wounds
Max instances: 5 (per wound type)
Natural Healing: 0.2 if untended, 1 if tended

Wounds are a special subcategory of conditions. They all share some logic regarding tending and infection. The severity value is used for the infection state. Additionally, wounds have a "tended" and "treated" flag. Opposed to most other conditions, wounds are always applied with the same initial severity (1.5), as the severity is used to track the infection state.

Wound can be tended by using items with the bandage flag, which consumes the item and sets the "tended" flag to true.

Wounds can be treated by using items with the antiseptic flag, which consumes the item and sets the "treated" flag to true.

Additionally to natural healing, the severity changes each night according to these rules (multiple can apply):
- If the wound is untended or infected and untreated, it will increase by a random amount between 0.5 and 1.5.

The effect of a wound is the combination of the base effects, infection effects and wound-specific effects. Infection (severity) works the same for all wounds.

Base effects:
Untended: -2 combat, -2 charisma
Tended: -1 combat, -1 charisma

Stages:
- Not Infected: 0-3, no effect
- Minor Infection: 4-6, -1 combat, -1 strength, -1 dexterity
- Major Infection: 7-9, -3 combat, -3 strength, -3 dexterity
- Critical Infection: 10-12, -5 combat, -5 strength, -5 dexterity
- Lethal at 13

##### Cut Wounds
Additional effect while untended: +0.5 blood loss each night.
Additional effect while tended: none

##### Bruise Wounds
Additional effect while untended: Slows healing of all fractures by 0.2.
Additional effect while tended: none

## Taking Damage
There's a few generalized ways of taking damage that are used in various encounters. These are additional to just gaining a specific condition (which is of course also possible).

**Applying Random Wound**: This just applies a new wound out of a pool of possible wounds (usually bruise/cut).

**Taking Bruise Damage**: The player can take bruise damage of a specific severity. This applies a new bruise wound and bone damage (fracture) to a random arm or leg with the given severity.

**Taking Cut Damage**: The player can take cut damage of a specific severity. This applies a new cut wound and immediately increases blood loss by the given severity.

**Taking Random Damage**: The player can take random damage of a specific severity. This just randomly chooses one of the above damage types and applies it with the given severity.


# Inventory / Items
The player carries a wooden cart behind them, which represents their inventory. Each item is a physics sprite in the cart, affected by gravity so it stays in the cart. Items can be dragged and dropped freely in the cart, and also into item slots for encounter options.
If an item is added to the player, it spawns above the cart and falls into it. If an item is removed from the player, it simply vanishes.
If an item falls out of the cart, it respawns back above the cart, so they cannot accidentally be lost. Hovering over an item shows a tooltip with the name and short description. Clicking on an item may give options such as "eat", "drink" if applicable.
There is a limit of how much the player can carry, but how that limit is implemented is still to be determined and needs to be experimented with.

## Item Tags
Each item can have any number of tags. Tags are the primary mechanism for defining which items are accepted by which item slots in encounter options. They are never communicated to the player directly.

There are two broad categories of tags:

- **General-purpose tags** describe what an item *is* (e.g. "Food", "Tool", "Medical", "Weapon", "Trash"). These are used for straightforward slot requirements such as "accepts any food item".
- **Activity tags** describe what an item can be *used for* (e.g. "Combat", "Scavenging", "Fortifying", "Lockpicking", "Digging"). These allow slots to accept any item that is useful for a given activity, even if those items are otherwise very different from each other.

An item can (and often should) have tags from both categories. Because tags are invisible to the player, it is fine to have many, specific, overlapping, or technical tags, as long as they make sense from a design perspective. From a technical perspective there is no difference between these two categories of tags, they are just a convention for how to use them in design.

### Tag Value Modifiers
Items can optionally define a **tag value modifier** for any of their tags. This is a signed integer that expresses how particularly good or bad the item is at that tag's activity.

When an item with a tag value modifier is placed into a slot that accepts that tag, the slot's default difficulty reduction is adjusted by the modifier value. For example:

- A **bone** has the *Combat* tag with a tag value modifier of **-5**. If placed in a combat slot whose default difficulty reduction is 20, the effective reduction becomes 15.
- A **flashlight** has the *Scavenging* tag with a tag value modifier of **+5**. If placed in a scavenging slot whose default difficulty reduction is 10, the effective reduction becomes 15.

This system makes it easy to add variety: a single tag can encompass many items of varying quality, and the modifier captures how well each item fits the role. It also creates strategic trade-offs for the player — using an item with a negative modifier in one slot frees up a better-suited item for another.

#### Difficulty Reduction Priority
When an item is placed in a slot, the effective difficulty reduction is determined by the following priority:

1. **Item-specific override** — A slot can define a custom difficulty reduction for a specific item. If present, this value is used as-is (no further modifiers apply).
2. **Tag value modifier** — If no item-specific override exists and the item has a tag value modifier for the slot's accepted tag, the slot's default difficulty reduction is adjusted by that modifier.
3. **Default** — If neither of the above apply, the slot's default difficulty reduction is used unchanged.


## Loot Tables
Loot tables define weighted random item selections. They are used throughout the game whenever random items need to be generated, such as searching areas, opening containers, or receiving rewards.

A loot table maps items (and optionally other loot tables as sub-entries) to weight values. When resolved, an item is randomly selected based on the relative weights. Sub-tables allow an entire category of items to compete as a single weighted entry.

There are general-purpose loot tables (Food, Drinks, Medical, Tools, Weapons, Trash) that are used across the game. Each biome also has its own loot table that is used in biome-specific encounters like scavenging. Biome loot tables typically reference the general tables as sub-entries. When an encounter uses items from a biome loot table, it can combine the biome table with an encounter-specific table to create a union of both.

## Gaining Items
The player starts with 4 items in the cart: 1 random food item, 1 random drink item and 1 random medical item, and 1 random miscellaneous item.
The main ways of gaining items are:

- During encounters.
- Companions can find items during the night, which are added to the cart and shown in the morning as a night event.
- Biome encounters often have options to search the area for items (e.g. Scavenge).


# Companions
The player can have companions that travel with them. Companions can affect stats or encounter options, as well as cause night events. Companions can be gained or lost through encounters.
Companions do not have their own health or inventory. Mechanically they act purely as buffs.
When a companion dies, this usually has a big temporary negative effect on the player's morale.


# World Map
The world map is a grid of hex tiles that represent different locations in the quarantine zone. Each day, the player can move to a different adjacent tile on the world map. Each tile has a Location Encounter, that is either predetermined through a quest/landmark/rumour or determined when the player first steps on that tile. The afternoon each day is always the location encounter of the tile the player is currently on.

## Quarantine Zone Borders
A big area of the world map is enclosed by an electrified quarantine fence, which is an impassable border that the player cannot cross. The goal of the game is to somehow reach a tile outside of the quarantine fence, which represents escaping the quarantine zone. The player starts around the center of that zone.

## Danger Level
Each tile has a persistent danger level. That level is shown to the player. The danger level affects the likelihood and intensity of night encounters happening during the night. Danger levels are "very safe", "safe", "precarious", "dangerous" and "very dangerous".
The calculation of the danger level is very simple. It starts at "safe" on each tile. AFTER each night, the danger level on the tile the player is on increases by 1 level.
This mechanic is there to encourage the player to keep moving and exploring new tiles, instead of staying on the same tile and resting all the time.
The danger level of tiles can also be affected by encounters.

The chances of a night encounter occuring are (broken down by intensity):

| Danger Level   | No Encounter | Intensity 1 | Intensity 2 | Intensity 3 |
|----------------|--------------|-------------|-------------|-------------|
| Very Safe      | 100%         | 0%          | 0%          | 0%          |
| Safe           | 95%          | 5%          | 0%          | 0%          |
| Precarious     | 75%          | 20%         | 5%          | 0%          |
| Dangerous      | 50%          | 20%         | 20%         | 10%         |
| Very Dangerous | 20%          | 10%         | 35%         | 35%         |

## Biomes
Each tile has a biome, which determines the types of Location Encounters that can be encountered on that tile and also the Biome Encounter, which are the options that the player can choose from in the evening. For encounters that can appear in multiple biomes, the biome can also affect the encounter itself, by adding biome specific options or changing the outcome or difficulty of certain options.
Each biome also has a specific loot table, which is often used in encounters that involve randomized items of some kind, such as searching the area for items. The biome also affects the background sprites that are shown in encounters on that tile.

The following biomes exist:

### Woods
Most important stats: Intelligence, Perception, Dexterity

### City
Most important stats: Combat, Charisma, Perception

### Mountains
Most important stats: Agility, Strength, Intelligence

### Desert
Most important stats: Agility, Perception, Combat

### Outskirts
Most important stats: Charisma, Strength, Dexterity

**Evening encounter**: In addition to standard options, the player may **Flag down passerby** to trade items or information for coins (always available at roadside ditch, 50% elsewhere).

### Lake
Impassable biome that cannot be entered. Mostly acts as a way to make the world map more interesting and to create natural borders and paths for the player to follow.


## Encounter Markers
The Location Encounter on a tile is represented by a small sprite on the world map, that shows what type of encounter is on that tile and in what state it is, in a very simplified way. Obviously this only applies to to tiles that have a Location Encounter determined already (so either the player has stepped on the tile before, or a quest has predetermined the encounter).
Location Encounter markers are always grayscale, while quest markers are colored. This way the player can easily distinguish the "important" tiles.

## Area
An area is simply a collection of hex tiles with a name. Examples of areas or the quarantine zone, cities, woods, lakes etc. Areas can be used in quests to specify locations in a more general way, such as "go to city XY" instead of "go to tile 3/4".

## World Generation
The world is is a pointy top hex tile map that is procedurally generated at the start of each new game. Each day, the player (at default) moves 1 tile. The player starts at coordinates 0/0. The world generation follow these steps:

1. The shape of the quarantine zone is generated. This is done by first expanding 12 tiles in a radius around the starting tile, and then generating some random protrusions (200 tiles) look more natural. After the protrusions, the shape is once again expanded by 1 tiles in a radius to smoothen the shape a bit. This generated area is the quarantine zone, enclosed by a fence.
2. An additional ring of tiles outside the fence is added that represent freedom. Reaching one of these tiles is a way to win the game.
3. The base "natural" biomes are generated (like outskirts, woods, lake etc.). This is done by assigning each of these biomes a perlin noise layer and a priority. Then for each tile, the biomes are iterated through by priority. The first biome that has a perlin value > 0.65f, is assigned to that tile, with priority 1 as fallback. Priorities are Outskirts > Lake > Woods.
4. 5 Cities are generated. Cities start by picking a tile and then expanding randomly around that tile until a desired size is reached (3-10). All tiles in a city get assigned the city biome. Cities are areas.
5. Now that all biomes are set, clusters of adjacent tiles sharing the same biome above a certain size are grouped together into areas. This creates named areas like forests and lakes that can be used in quests.
6. Each tile adjacent to the quarantine fence gets assigned the "fence" encounter. This is simply an encounter where the player faces the electric quarantine fence. 
7. Landmarks are placed depending on their definitions (where and how often they can appear). Landmarks are predetermined encounters visible from the start. During landmark generation, more tiles may get predetermined encounters for quest chains, some of which may be hidden.

# Quests
Quests are special tasks that the player can receive from certain encounters. They usually have a specific goal that the player has to achieve, such as reaching a specific location on the world map, bringing a specific item, meeting someone, etc. Quests can have various effects on the game state, such as unlocking new encounters, changing the state of existing encounters, giving the player new items or companions, etc.
Quests usually require the player to go to a specific tile on the world map. When a quest is given, some location encounters of affected tiles are predetermined, so the player knows what they will encounter there. Functionally quest markers work as any other encounter marker on the world map, with the only difference that they are visible before stepping on the tile, so the player can plan their route accordingly.
In the game quests are communicated in panel titled "Notes".

On a technical level, quests are defined via QuestDefs. For each QuestDef, the state of that quest is tracked and is either "Inactive", "Active", "Completed" or "Failed". The state of quests can affect encounters. For example, an encounter that would usually give a specific quest, but that quest is already active/done, then the option that would result in giving that quest is not shown.


# Encounters
An encounter is a situation that requires player input. Each day, the player will encounter a semi-random encounter during the afternoon. Depending on the current game state, additional encounters may be encountered during the night.
Different encounters can work in very different ways, with different numbers of steps, different options, and different outcomes. For example, one encounter can be a simple one-step encounter with only one option, while another encounter can be a complex multi-step encounter with many options at each step and various branching paths.
Also options can work in many different ways, with some options locking out others, some requiring others to succeed first, some options may only be available once, once per day, repeatable until success, or any other behaviour.
There's really not a lot of restrictions in the design space of encounters.

During encounters, free item use is disabled (i.e. for eating / bandaging). Items can only be used for encounter step options.
At the end of an encounter, after it ends, the player can freely use items in their cart (i.e. to eat / drink / heal injuries etc.) again. After that, the player can choose to end the current time of day and transition to the next one.
An exception to this is the morning encounter. During the morning, items can be used freely.

Encounters only ever take place within a single time of day.

## Encounter Types
There are three types of encounters:

### Location encounters
These are the main encounters that the player encounters in the afternoon and are bound to a specific tile on the world map. Location encounters are persistent and can be returned to later with the same state as they were left in.

Usually location encounters are generated when the player first steps on a tile, based on the biome of that tile and the current game state.
However, some encounters can be predetermined, meaning that they are generated before the player steps on the tile, and the player can see their encounter marker on the world map before stepping on the tile. This can be the case for either quest related encounters, landmarks that are generated with the world at the start of the game, or temporary rumours (i.e. rising smoke) that can appear during the game.
Predetermined encounters are usually a way to directly or indirectly progress the story, and are there to give the player some direction and goals to work towards.

### Biome encounters
These are the encounters that the player encounters in the evening. They are purely based on the biome of the current tile and are not persistent, meaning that players can not come back to a specific biome encounter and a new instance is created every evening. Biome encounters are not meant to be narratively significant, but rather to give the player some control, as most other things in the game are very random and out of the player's control.

Biome encounters work on an "evening action" system: the player is presented with a set of options for how to spend the evening, but only one can be chosen. Once chosen, that action may either end the encounter immediately or lead to follow-up options depending on the action.

Some evening actions are standardized and available across multiple biomes (controlled by the biome subclass):

- **Rest early** (FixedOutcome): Turn in early for some extra natural healing (0.5x). Always available.
- **Fortify** (SkillCheck): Reinforce the sleeping spot to reduce the night's danger level. Uses Strength, Dexterity, and Intelligence. Difficulty varies by biome. Accepts building materials and tools in item slots to reduce difficulty. On critical success, grants a random stat improvement and morale boost. Available when the biome defines a fortify difficulty.
- **Scavenge** (SkillCheck): Search the area for items from the biome's loot table. Uses Dexterity and Perception. Accepts scavenging items in an item slot. On success, adds an item from the biome loot table to the inventory. Available when the biome supports scavenging.

Each biome encounter subclass can also define additional biome-specific options (e.g. flagging down a passerby in the outskirts). Some of these biome-specific options can lead to follow-up steps with further choices (e.g. trading with a passerby after successfully flagging them down).

Biome encounters usually have a setting. The setting is a randomly rolled place where the player is setting up camp for the night. The setting can affect the base difficulty of the evening actions, or can have other biome-specific effects.

### Night encounters
These are special encounters that happen during the night. They are not persistent and cannot be returned to after an encounter.

Night encounter are designed as a threat to the player, as a consequence of being in high danger level areas, and are therefore overwhelmingly negative in their outcomes. Different than other encounters where good outcomes lead to positive effects, night encounters are more about avoiding negative effects (although (critical) success can still provide some benefits).
Night encounters are usually some form of attack on your sleeping spot/camp.

#### Intensity
Night encounters have a special additional property called "intensity". The intensity is determined when the encounter is initialized and is calculated based on the danger level of the current tile and some variation.

If the player has placed traps during the evening, each trap reduces the intensity of the encounter by 1.

Intensity is a value from 1 to 3. It usually shows itself in the amount or size/strength of the attackers but how exactly it affects the encounter is based on the individual night encounter.


## Design Philosophy
First design priority of all encounters is to provide the player with interesting and meaningful choices.

Encounter steps should visually give an immediate sense of what's happening. The main text on an encounter step should be short and concise, giving the player a clear idea of the situation, without much prose.
Option texts should also be short, preferably just a verb and potentially a subject (i.e. "Persuade", "Open Crate"). The option descriptions should give a sense of the possible outcomes of that option. The descriptions are only shown while hovering an option, so they can be a bit longer.

Encounters happen on a single screen. There are no camera controls in the game. The left side of the screen is always occupies by the player and their handcart (inventory). The encounter is on the right. This is important to factor in when designing encounters, as everything needs to fit that screen. The player character also visually never moves. Stop outcomes aren't animated, but rather represented by changing the sprites on the screen, sound effects, and maybe simple effects.
For example if the player selects a "Smash Crate" option, and the outcome is a success, what happens is that the crate sprite gets replaced with a broken crate sprite, a sound effect of smashing is played, and the player gains some items in their cart (meaning they spawn above the cart and fall in). If the outcome is a failure, the crate sprite stays the same, a sound effect of failure is played, and maybe the player gets injured, which is represented by a wound sprite appearing on the player character.

Even though the screen is fixed, encounters can affect the zoom level of the camera, to allow for some different scale of encounters. For example, when encountering a crate, the camera size will be quite small, since it just needs to show the player and the crate. But for example when encountering a radio tower, the camera size will be much bigger, since it needs to show the player and the whole base of the tower. This can be used to give a nice sense of scale and variety to the encounters, even though they all happen on the same screen. Orthographic size should usually be in the range of 5.4 - 12.

Encounters can be affected by specific biomes in specific ways, for when the encounter happens in that biome. This can be something completely individual to the encounter, or by using biome difficulty modifiers in skill check options. What should be avoided are cases where each existing biome has to be factored in separately and handled manually for something, as that would create a lot of extra work and complexity.

## Encounter Steps
Encounters are built in "steps", whereas exactly one step is active at a time. A step represents a specific point in the encounter, and the options available to the player at that point. Encounter steps and their options are often created dynamically based on the current game state, and more importantly, the current state of the encounter itself.

A step is considered the final step of an encounter if that step has no options defined. When reaching such a step, the encounter is considered to be ended, meaning the player can use items again and choose to transition to the next time of day.
 
## Options
Each step (except final steps) has a defined set of options that the player can choose from. What options are available and shown depends on the current encounter state.
If the step is a final step, the options depend on the time of day, and not on the encounter itself. Usually it's just one option to end the current time of day and transition to the next one.

### Item Slots
Each option can have any number of item slots, which are slots that the player can drag items from their cart into. 
Each slot accepts items in exactly one of three modes (mutually exclusive):

- **Specific item** — Only a single, explicitly defined item is accepted.
- **Tag** — Any item that has the specified tag is accepted.
- **Custom list** — A custom set of explicitly listed items is accepted.

Each slot can additionally have the following properties:

- Required: If the slot is required, the player has to fill that slot with a valid item in order to be able to choose that option.
- Consumption Chance: If the slot is filled with a valid item, there is a chance that the item gets consumed on use, which would remove the item from the player's cart.
- Difficulty Modifier (Skill Checks only): If the slot is filled with a valid item, the difficulty value of the option is reduced. The effective reduction follows a priority: (1) an item-specific override defined on the slot, (2) the slot's default reduction adjusted by the item's tag value modifier for the matching tag (tag mode only), or (3) the slot's default reduction unchanged. See *Item Tags > Difficulty Reduction Priority* for details. The difficulty reduction will never go below 5.

### Option Types
On a technical level, options fall into one of two categories: "Skillchecks" or "FixedOutcome"

#### FixedOutcome options
FixedOutcome options have a fixed outcome, meaning on a technical level, that choosing the options always calls the same function. That function can still have custom logic and random elements, but the outcome is always determined by that function. They are usually used for very simple options like ignoring/skipping something, or for options with special outcomes that don't fit into the classic success/partial success/failure outcome structure of a skill check.

#### Skillcheck options
Skillcheck options follow a classic, standardized RPG style skillcheck structure, where the option has a calculated difficulty value and a rolled outcome that is determined by the difficulty value and a random roll, where the difficulty can be affected by a variety of modifiers. They have different possible success levels, with each of these calling a different function that determines the outcome of that option.

##### Skill Check Outcomes
Each skill check option has the possible outcomes:

- Success: The player fully succeeds in the action they are trying to do.
- Failure: The player fails in the action they are trying to do.

Depending on the option, there may also be additional outcomes:

- Partial success: The player partially succeeds in the action they are trying to do. This is usually a middle ground between success and failure, with an outcome that is better than failure but worse than success.
- Critical success: The player critically succeeds in the action they are trying to do. This is usually a better version of success, with an outcome that is even better than success.
- Critical failure: The player critically fails in the action they are trying to do. This is usually a worse version of failure, with an outcome that is even worse than failure.

##### Skill Check Outcome Calculation
Skillcheck options have a rolled outcome, where the player rolls a random number from 0 to 100. The outcome depends on the rolled number and the calculated difficulty value of the option:

- If the rolled number is equal or greater than the difficulty value, the player succeeds.
- If the rolled number is less than the difficulty value, the player fails.

Additionally, if there are additional outcomes, they are calculated as follows:

- In failure, if the rolled number greater than 50% of the difficulty value, the player partially succeeds instead of fails.
- In failure, if the rolled number is less than 10% of the difficulty value, the player critically fails instead of fails.
- In success, if the rolled number is in the top 10% of the range above the difficulty value, the player critically succeeds instead of succeeds. (for example, if the difficulty value is 70, and the rolled number is greater than 97, the player critically succeeds instead of just succeeds)


##### Skill Check Difficulty Calculation
Skillcheck options have a fixed base difficulty value (1-100). On top of that, various modifiers can be added to the difficulty value based on the current game state. All modifiers are additive/subtractive (no multiplicative modifiers).
The most common types of modifiers are:

- Morale: The morale stat is applied as a modifier to all options with a factor of 1.
- Player stats: Most options have a specific stat (or multiple) associated with them, each with a defined factor. The player's value in that stat multiplied by the factor is reduced from the difficulty value.
- Item slots: Filled item slots can reduce the difficulty value based on their difficulty modifier property.
- Companions: Some options have companion modifiers, where having a specific companion can increase or decrease the difficulty value. (usually decrease)
- Weather: Some options have weather modifiers, where certain weather conditions can increase or decrease the difficulty value.
- Biome: Some options have biome modifiers, where certain biomes can increase or decrease the difficulty value.
- Other encounter-specific modifiers: Some options can have specific modifiers based on the current state of the encounter itself, such as previous choices made in the encounter, or specific things that happened during the encounter.

Difficulty is capped at 5 minimum and 200 maximum. This means that no matter how good the player is, there is always a small chance of failure, and at max difficulty the only possible outcomes are failure and critical failure.

### Option Outcomes
There are many things that can happen as a result of an option outcome. The most common are:

- Stat changes: The player's stats can increase or decrease.
- Item changes: The player can gain or lose items.
- Health changes: The player can gain new health conditions or change existing health conditions in all kind of ways.
- Companion changes: The player can gain or lose companions.
- Quest changes: The player can gain new quests, fulfill them, fail them, etc. With effects based on the specific quest. This includes rumours.
- Danger level changes: The danger level of the current tile or other tiles can increase or decrease.
- World location encouters: Encounters on other tiles on the world map can be generated and/or revealed.
- Placing traps (evening only): In the evening traps can be placed to reduce night encounter intensity.
- Initiating trade: An option outcome can initiate a trading session (see Trading below).

## Trading
Any encounter can initiate a trading session as part of an option outcome by calling `InitiateTrade`. When trading is initiated, the encounter enters a special trading mode that temporarily replaces the normal encounter options with a set of trading options. The encounter defines which items are available for buying and selling, and whether buying information (rumours) is available.

While in trading mode, the following options are presented to the player:

- **Buy [item]** (one per buyable item): Costs coins equal to the item's value. Each coin must be placed in a required item slot. Purchasing adds the item to the player's inventory.
- **Sell [item]** (one per sellable item): The item to sell must be placed in a required item slot. Selling adds coins equal to the item's value to the player's inventory.
- **Buy information** (if enabled, once per day): Costs 3 coins placed in required item slots. Reveals a rumour.
- **Done trading**: Exits trading mode and returns to the encounter's normal options.

Items with a value of 0 or less, and coins themselves, are excluded from buy/sell lists.

Trading can be initiated from both location encounters and biome encounters. For example, the Outskirts biome encounter uses it when the player successfully flags down a passerby, and the Wounded Stranger location encounter uses it when a grateful stranger offers to trade.

# Gameplay Loop
The player must somehow escape the quarantine zone. If the player character dies, the game is over. When the player dies, the screen fades to black showing the reason of death. The player can then choose to start a new game, which generates a new world and resets all progress, or quit to the main menu.

There are many ways to leave the quarantine zone, and also many ways to die.

The game is day based. Each day is split into 4 times of day: Morning, Afternoon, Evening and Night.

All time of day transitions are presented as a fade to black, and then fade back in. From Night to Morning, the game also shows a big text with "Day 4" switching to "Day 5" for example.

## Morning
The day starts in the morning on the world map tile of the current location. There is no encounter in the morning, the player can freely use items in their cart (i.e. to eat / drink). If any night events happened during the night, they are shown to the player as text at the start of the morning.
In the morning, the player is presented with 3 options:

- Travel to an adjacent world tile: This shows the world map with the adjacent world map tiles that the player can move to being highlighted. Choosing one of the tiles will end the morning and transition to the afternoon on the selected tile.
- Stay on the current tile: This simply ends the morning and transitions to the afternoon on the same tile, presenting the player with the same Location Encounter as the day before, in the same state as they left it.
- Rest: Skips the afternoon and transitions the game into the evening. When resting, all health conditions apply their natural healing (same effect as during the night).

## Afternoon
At the start of the afternoon, the player is always confronted with the daily Location Encounter. If the location encounter on that tile has already been set (either by visiting the tile before, or by a quest), the player encounters the same encounter again, in the same state as they left it.
If the location encounter on that tile has not been set yet, a new Location Encounter is generated based on the biome of that tile and the current game state, and the player encounters that.

## Evening
In the evening, the player is presented with the Biome Encounter for the biome of the current tile. Each biome has a defined evening encounter type. If no specific encounter is defined for a biome, a fallback encounter is used.

A new biome encounter instance is created each evening — they are not persistent across days. The encounter presents the player with a set of options for how to spend the evening (see Biome encounters above). Only one evening action can be chosen, after which the encounter either ends immediately or continues with follow-up options specific to that action.

### Trap System
The evening has a trap system. Either through the Trap item or through encounter options, the player can set traps to protect themselves during the night. They see how ma ny traps are set in the UI. Each trap has the following effect:

- If there is a night encounter, each trap will reduce the intensity of the encounter by 1. If this reduces the intensity below 1, it nullifies the encounter entirely.
- If a trap wasn't used for an encounter, it has a X% chance trigger on an animal, giving the player an item. X is based on the biome.
- If a trap was neither used for an encounter nor triggered on an animal, it has a 80% chance to be returned to the player's cart in the morning, and a 20% chance to be lost.

These effects are communicated as night events in the morning report.


## Night
Each night, there is a chance for a night encounter to happen, based on the danger level of the tile the player is currently on. (see Night Encounters chapter for more info).

If the player does not have a night encounter, the night is skipped and the game transitions from evening to the next morning.

During the night, separate from the night encounter, night events can happen as well. These are events that happen without any player input and without any visual representation.
They are purely based on the current game state and can have various effects on the player. For example, if the player has an untended injury, there is a chance that the injury gets infected. Or if the player has a certain companion, there is a chance that the companion finds an item during the night.
Night events are shown to the player in the morning as bullet points in the morning report (First encounter step in the morning when choosing action for the day).


# Ways to Win / Story Progression
The main goal of the game is to escape the quarantine zone.

The design philosophy here is to have multiple ways to win the game. And these ways are not clear linear quest lines. The setup should be modular, so multiple paths can lead to the same information/quests/ways to win. For example a location of something important could be revealed through finding a note somewhere, hearing it through a rumour, or just stumbling upon it while exploring.
As an example, to win by cutting through the fence, the player needs to be on the correct tile with a fence cutter. How the player knows of that tile or how they got the fence cutter is not 100% scripted.
Of course there should always be predetermined encounters / story beats / guidance mechanicms in place to make sure the player can find their way to the different ways to win, but these "guidance ways" don't prevent the player of achieving the same goal through other means.
In that sense the game takes some inspitation from Immersive Sim games, where the world is designed in a way that allows the player to find their own way through the story, without being forced down a specific path.

On a technical level, a StoryManager tracks the story progression and the different ways to win. It also tracks the state of important quests and story beats, and can trigger certain encounters or events based on that state. It also places predetermined, often hidden encounters on the world map for important story beats, quests, and ways to win, so the player can find them and progress the story.

## Cutting through the fence
At the start of the game, one random tile adjacent to the quarantine fence will have its fence encounter altered, so that the electricity on the fence doesn't work. This allows the player to cut through the fence and escape.
The location of that tile can be found at one randomly determined radio tower landmark when listening to the transmission.
The fence cutter can be received by R, who will have a predetermined, initially hidden encounter in a random city. The city, where R lives, can be found at the same radio tower on a note. R has a sick partner. In exchange for medicine, they give the player the fence cutter and exact location of the unpowered fence.