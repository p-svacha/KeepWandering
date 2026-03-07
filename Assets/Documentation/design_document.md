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

Everything is sprite based in an old-school flash art style, with a fixed position side-view camera and no animations. Sprites change depending on the situtation.
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

Stats are capped at -20/20.

## Health Conditions
The health system tracks the player's physical and mental condition, including injuries, illnesses, wounds etc. Health is NOT tracked as a number or health bar, but rather as a collection of conditions that the player can have. Conditions can have severity levels and each condition has their own behaviours, effects and ways of treatment.

Health conditions often affect player stats, and can also have other effects such as affecting encounter options, causing night events, etc.
Most causes of death are related to health conditions.

Conditions are split into two main technical categories: Permanent and Temporary

### Permanent Health Conditions
Permanent health conditions describe conditions that are always present on the player. They are usually tracking some sort of hidden meter that increases or decreases based on the current game state, such as hunger, thirst, blood loss etc. These conditions usually have severity stages based on the value of the hidden meter, and each stage has its own effects on the player.
Even though these conditions are technically "permanent", that does not mean they are always active. For example, if hunger is above a certain threshold, the condition is not active, therefore has no effect and is not shown to the player.

Permanent health conditions are:

#### Hunger
Increases every night.
Stages are "Hungry", "Very hungry" and "Starving". If the hunger meter reaches a limit, the player dies.
Affects morale, strength, intelligence.
Lethal

#### Thirst
Increases every night.
Stages are "Thirsty", "Very thirsty" and "Dehydrated". If the thirst meter reaches a limit, the player dies.
Affects dexterity, agility, perception.
Lethal

#### Blood loss
Increases each night for each untended cut wound.
Heals naturally over time.
Affects combat, strength, agility.
Lethal

#### Leg bones health
Stages are "Strained", "Cracked" and "Broken".
Affects agility, combat.
Heals naturally over time, but can also be healed faster with treatment.
In the "Broken" stage, the player cannot move to different tiles on the world map in the morning.

#### Arm bones health
Stages are "Strained", "Cracked" and "Broken".
Affects combat, strength, dexterity.
Heals naturally over time, but can also be healed faster with treatment.

### Temporary Health Conditions
Temporary health conditions are conditions that can be gained and lost throughout the game. They usually represent injuries, illnesses, wounds etc. that the player can get during encounters, and can be treated and healed with items or by resting.

#### Wounds
Wounds are a special subcategory of temporary health conditions. They all share some logic regarding tending and infection.
Wounds need to be tended with bandages. If left untended, they have a chance to get infected each night, increasing each day.
If tended, they have a chance to heal each night, increasing each day.
If infected, they need to be treated with antibiotics, which heals the infection but does not tend the wound. If left infected for too long, there is a chance that the infection worsens, eventually leading to death.
Wounds that have been treated with antibiotics cannot infect again.

##### Cut Wounds
Untended cut wounds increase blood loss.

##### Bruise Wounds
If a bruise wound is gained, that will always also decrease bone health by a certain amount.

#### Poisoning
The player can be poisoned, which reduces charisma and has a chance to worsen each night, eventually leading to death. Poisoning can be treaded with antidotes, which will heal the poisoning instantly.


# Inventory
The player carries a wooden cart behind them, which represents their inventory. Each item is a physics sprite in the cart, affected by gravity so it stays in the cart. Items can be dragged and dropped freely in the cart, and also into item slots for encounter options.
If an item is added to the player, it spawns above the cart and falls into it. If an item is removed from the player, it simply vanishes.
If an item falls out of the cart, it respawns back above the cart, so they cannot accidentally be lost. Hovering over an item shows a tooltip with the name and short description. Clicking on an item may give options such as "eat", "drink" if applicable.
There is a limit of how much the player can carry, but how that limit is implemented is still to be determined and needs to be experimented with.

## Gaining Items
The player starts with 4 items in the cart: 1 random food item, 1 random drink item and 1 random medical item, and 1 random miscellaneous item.
The main ways of gaining items are:

- During encounters.
- Gaining items as a reward for completing quests.
- Companions can find items during the night, which are added to the cart and shown in the morning as a night event.
- Biome encounters often have options to search the area for items, which can add items to the cart.


# Companions
The player can have companions that travel with them. Companions can affect stats or encounter options, as well as night events. Companions can be gained or lost through encounters.
Companions can have their own health conditions, which are usually simplified (i.e. dogs only need food, a plant only needs water).
When a companion dies, this usually has a big temporary negative effect on the player's morale.

# World Map
The world map is a grid of hex tiles that represent different locations in the quarantine zone. Each day, the player can move to a different adjacent tile on the world map. Each tile has a Location Encounter, that is either predetermined through a quest/landmark/rumour or determined when the player first steps on that tile. The afternoon each day is always the location encounter of the tile the player is currently on.

## Quarantine Zone Borders
A big area of the world map is enclosed by a quarantine fence, which is an impassable border that the player cannot cross. The goal of the game is to somehow reach a tile outside of the quarantine fence, which represents escaping the quarantine zone. The player starts around the center of that zone.

## Exposure
Each tile has a persistent exposure level. That level is shown to the player. The exposure level affects the likelyhood of bad night encounters happening during the night. Exposure levels are "very safe", "safe", "caution", "danger" and "extreme danger".
The calculation of the exposure level is very simple. It starts at "safe" on each tile. AFTER each night, the exposure level on the tile the player is on increases by 1 level.
This mechanic is there to encourage the player to keep moving and exploring new tiles, instead of staying on the same tile and resting all the time.

## Biomes
Each tile has a biome, which determines the types of Location Encounters that can be encountered on that tile and also the Biome Encounter, which are the options that the player can choose from in the evening. For encounters that can appear in multiple biomes, the biome can also affect the encounter itself, by adding biome specific options or changing the outcome or difficulty of certain options.
Each biome also has a specific loot table, which is often used in encounters that involve randomized items of some kind, such as searching the area for items. The biome also affects the background sprites that are shown in encounters on that tile.

The following biomes exist:

### Woods

### Swamp

### City

### Mountains

### Desert

### Farmland

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
3. The base "natural" biomes are generated (like farmland, woods, lake etc.). This is done by assigning each of these biomes a perlin noise layer and a priority. Then for each tile, the biomes are iterated through by priority. The first biome that has a perlin value > 0.65f, is assigned to that tile, with priority 1 as fallback. Priorities are Farmland > Lake > Woods.
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

## Encounter Types
There are three types of encounters:

### Location encounters
These are the main encounters that the player encounters in the afternoon and are bound to a specific tile on the world map. Location encounters are persistent and can be returned to later with the same state as they were left in.

Usually location encounters are generated when the player first steps on a tile, based on the biome of that tile and the current game state.
However, some encounters can be predetermined, meaning that they are generated before the player steps on the tile, and the player can see their encounter marker on the world map before stepping on the tile. This can be the case for either quest related encounters, landmarks that are generated with the world at the start of the game, or temporary rumours (i.e. rising smoke) that can appear during the game.
Predetermined encounters are usually a way to directly or indirectly progress the story, and are there to give the player some direction and goals to work towards.

### Biome encounters
These are the encounters that the player encounters in the evening. They are purely based on the biome of the current tile and are not persistent, meaning they do not have a specific state, so they are always encountered in their default state. Biome encounters are not meant to be narratively significant, but rather to give some control to the player as most other things in the game are very random and out of the player's control.

### Night encounters
These are special encounters that can be randomly encountered during the night. They are not tied to any specific location on the world map.

During encounters, free item use is restricted (i.e. for eating / bandaging). Items can only be used for encounter step options.
At the end of an encounter, after it ends, the player can freely use items in their cart (i.e. to eat / drink / heal injuries etc.) again. After that, the player can choose to end the current time of day and transition to the next one.

Encounters only ever take place within a single time of day.

## Design Philosophy
First design priority of all encounters is to provide the player with interesting and meaningful choices.

Encounter steps should visually give an immediate sense of what's happening. The main text on an encounter step should be short and concise, giving the player a clear idea of the situation, without much prose.
Option texts should also be short, preferably just a verb and potentially a subject (i.e. "Persuade", "Open Crate"). The option descriptions should give a sense of the possible outcomes of that option. The descriptions are only shown while hovering an option, so they can be a bit longer.

Encounters happen on a single screen. There are no camera controls in the game. The left side of the screen is always occupies by the player and their handcart (inventory). The encounter is on the right. This is important to factor in when designing encounters, as everything needs to fit that screen. The player character also visually never moves. Stop outcomes aren't animated, but rather represented by changing the sprites on the screen, sound effects, and maybe simple effects.
For example if the player selects a "Smash Crate" option, and the outcome is a success, what happens is that the crate sprite gets replaced with a broken crate sprite, a sound effect of smashing is played, and the player gains some items in their cart (meaning they spawn above the cart and fall in). If the outcome is a failure, the crate sprite stays the same, a sound effect of failure is played, and maybe the player gets injured, which is represented by a wound sprite appearing on the player character.

Even though the screen is fixed, encounters can affect the zoom level of the camera, to allow for some different scale of encounters. For example, when encountering a crate, the camera size will be quite small, since it just needs to show the player and the crate. But for example when encountering a radio tower, the camera size will be much bigger, since it needs to show the player and the whole base of the tower. This can be used to give a nice sense of scale and variety to the encounters, even though they all happen on the same screen. Orthographic size should usually be in the range of 5.4 - 12.

## Encounter Steps
Encounters are built in "steps", whereas exactly one step is active at a time. A step represents a specific point in the encounter, and the options available to the player at that point. Encounter steps and their options are often created dynamically based on the current game state, and more importantly, the current state of the encounter itself.

A step is considered the final step of an encounter if that step has no options defined. When reaching such a step, the encounter is considered to be ended, meaning the player can use items again and choose to transition to the next time of day.
 
## Options
Each step (except final steps) has a defined set of options that the player can choose from. What options are available and shown depends on the current encounter state.
If the step is a final step, the options depend on the time of day, and not on the encounter itself. Usually it's just one option to end the current time of day and transition to the next one.

### Item Slots
Each option can have any number of item slots, which are slots that the player can drag items from their cart into. 
Each slot can have the following properties:

- Allowed items: A list of specific items that are allowed in the slot. This can be a specific item, an item with a specific tag, or any combination thereof.
- Required: If the slot is required, the player has to fill that slot with a valid item in order to be able to choose that option.
- Consumption Chance: If the slot is filled with a valid item, there is a chance that the item gets consumed on use, which would remove the item from the player's cart.
- Difficulty Modifier (Skill Checks only): If the slot is filled with a valid item, the difficulty value of the option is reduced by a fixed amount. There is a default difficulty modifiers, but specific items may override that with a custom value.

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
The most common types of modifiers are (in rough order of importance):

- Morale: The morale stat is applied as a modifier to all options with a factor of 1.
- Player stats: Most options have a specific stat (or multiple) associated with them, each with a defined factor. The player's value in that stat multiplied by the factor is reduced from the difficulty value.
- Item slots: Filled item slots can reduce the difficulty value based on their difficulty modifier property.
- Companions: Some options have companion modifiers, where having a specific companion can increase or decrease the difficulty value. (usually decrease)
- Weather: Some options have weather modifiers, where certain weather conditions can increase or decrease the difficulty value.
- Biome: Some options have biome modifiers, where certain biomes can increase or decrease the difficulty value.

Difficulty is capped at 5 minimum and 200 maximum. This means that no matter how good the player is, there is always a small chance of failure, and at max difficulty the only possible outcomes are failure and critical failure.

### Option Outcomes
There are many things that can happen as a result of an option outcome. The most common are:

- Next step: The encounter progresses to a different step.
- Encouter ends: The encounter can end.
- Stat changes: The player's stats can increase or decrease.
- Item changes: The player can gain or lose items.
- Health changes: The player's health can change (gain injuries / tend injuries / heal infections / heal poisoning etc.)
- Companion changes: The player can gain or lose companions.
- Quest changes: The player can gain new quests, fulfill them, fail them, etc. With effects based on the specific quest.
- Exposure level changes: The exposure level of the current tile or other tiles can increase or decrease.


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
- Rest: Skips the afternoon and transitions the game into the evening. When resting, many injuries have a chance to heal or get better. When resting, events can also happen during the transition of morning -> evening like companions finding items.


## Afternoon
At the start of the afternoon, the player is always confronted with the daily Location Encounter. If the location encounter on that tile has already been set (either by visiting the tile before, or by a quest), the player encounters the same encounter again, in the same state as they left it.
If the location encounter on that tile has not been set yet, a new Location Encounter is generated based on the biome of that tile and the current game state, and the player encounters that.

## Evening
In the evening, the player is presented with the Biome Encounter, that is fixed depending on the biome the player is in.

## Night
Each night, it is randomly rolled if the player has any Night Encounters. These are special encounters that the player can not come back to on the world map later. Multiple night encounters may happen in a night.
The likelihood of having a bad night encounter is based on the exposure level of the tile the player is currently on, with higher exposure levels increasing the chances of having bad night encounter.
There is also the chance of neutral night encounters, that can happen randomly regardless of the exposure level.

If the player does not have a night encounter, the night is skipped and the game transitions from evening to the next morning.

During the night, separate from the night encounter, night events can happen as well. These are events that happen without any player input and without any visual representation.
They are purely based on the current game state and can have various effects on the player. For example, if the player has an untended injury, there is a chance that the injury gets infected. Or if the player has a certain companion, there is a chance that the companion finds an item during the night.
Night events are shown to the player in the morning as text.

# Ways to Win
The main goal of the game is to escape the quarantine zone.

## Cutting through the fence
At the start of the game, one random tile adjacent to the quarantine fence will have its fence encounter altered, so that the electricity on the fence doesn't work. This allows the player to cut through the fence and escape.
The location of that tile can be found at the radio tower landmark when listening to the transmission.
The fence cutter can be received by R, who will have a predetermined, initially hidden encounter in a random city. The city, where R lives, can be found in the radio tower on a note. R needs someone to escape for him to deliver a message to the outside world, so he gives the player the fence cutter if they agree to escape for him.


# Location Encounter List
Location encounters are encounters that are tied to specific tiles on the world map. They are persistent, meaning that they can be returned to later with the same state as they were left in.

## Random Encounters
These are encounters that can be generated on any tile that doesn't have a predetermined encounter. They are based on the biome of the tile and the current game state, and are generated when the player first steps on the tile or when they are revealed.

### Wooden Crate
A locked container where players can peek inside to identify hidden loot before deciding to squeeze items through a hole, pry it open with tools, or smash it at the risk of destroying the contents.

## Landmarks
Landmarks are location encounters, that are predetermined during world generation and are visible from the start.

### Radio Tower