# UI
use photoshop brush type: KYLE Ultimate Charcoal Pencil 25px Med2 
with very slight color dynamics (6% brightness jitter, 3% hue jitter) and a low flow (0%)


# Item Sprites

## Setup
Make 256x256 canvas in Photoshop. draw into it according to item size, small items like coin are 40x40, bigger ones like medkit 150x170. does not have to be square.

## Drawing
Item sprites are generally diagonally angled from bottom left to top right.

Make 2 layers: Outlines in front and Fill in back.

OUTLINE
For outlines, use black color, 8px, 100% hardness
100% opacity, and 100% flow.
Activate pen pressure for size.

FILL
same brush settings as outline with any size as needed.
Fill does not have to be perfectly inside outline, some overflow or gaps adds to the style.
Color choice is free.

## Export
When done, Image > Trim > Transparent Pixels, then export as png. place in Assets/Sprites/Items and add to ItemDef in code. Name must match the DefName in ItemDef.


# Health Condition Sprites

6px black outline, 100% hardness, opacity, flow, pressure for size. Fill with any color, but keep it simple and readable. No gradients or shading.
Basically default


# Characters
Outline full black, 10px without pressure (or 14px with pressure), 100% hardness,opacity,flow



# Encounter Markers
64x64 px transparent Canvas
Use only grayscale colors, except for quest markers, 6px brush, 100% hardness, 100% opacity, and 100% flow. Pen pressure for size.



# Biome Backgrounds
Side-view biome backgrounds

## Setup
2600x[height] px canvas
needs to be seamless on x axis, so it can repeat nicely when camera zooms out
height is free
sky needs to be transparent. sky sprite is set according to time of day / weather and independent from biome.
~lower 400px need to represent the path the player is on.

## Drawing
no black outlines, use more desaturated colors. flowy brush strokes, make use of opacity, color and shape dynamics for organic looks.



# Encounter Sprites

Outline full black, 8-16px (usually 12px), 100% hardness,opacity,flow, pressure for size

Keep it strictly 2D, no shading or gradients. A "dirty brush" with slightly randomized color, shape, scatter can be used for details like moss/dirt.
Else just use flat color with slightly different color than base for details. Avoid gradients, shading, or lighting effects. Keep it simple and readable.

## Encounter Sprite Art Guide
 
This guide describes the visual style used for **encounter sprites**: the objects, structures, and props the player interacts with during encounters (radio towers, sheds, crates, collapsed buildings, walls, fallen trees, stashes, doors, rubble, NPCs etc). This is useful for visual consistency.
 
### Overall Feel
 
Hand-drawn diary/journal illustrations — like something sketched with a marker and lightly colored in with a soft digital brush. Charming, a little rough, worn and lived-in. Not cartoonish-cute, not painterly-realistic. Think: illustrated field notes from someone surviving a quarantine zone, not a polished game asset.
 
Every encounter sprite should look like it belongs in the same sketchbook, regardless of subject.
 
### Line Work
 
- **Thick, black, hand-drawn outlines** on every silhouette edge. The line has visible pressure variation (thicker in places, thinner in others) as if drawn with a marker or brush pen — never a uniform vector stroke.
- Outlines are **slightly imperfect and wobbly** — not geometrically straight, even on man-made objects like walls or towers. Corners are rounded off rather than sharp.
- **Interior detail lines are thinner** than the outer silhouette outline, but still hand-drawn and slightly uneven (wood grain, brick lines, cracks, chain-link mesh, individual planks).
- Line color is always near-black/dark brown-black, never pure vector black, never colored outlines.
- No outlines are used for soft shading blobs (see below) — only for hard object edges and structural detail lines.
 
### Shading & Fill
 
This is **not** flat single-tone fill, and **not** cel-shaded/gradient rendering. It's a middle ground:
 
- Each shape gets a **base flat color fill**.
- On top of that, **1–2 soft, loosely-painted shadow/tone patches** are added in a darker or lighter shade of the same hue — irregular cloud-like blobs, not hard-edged geometric shading. They suggest volume and grime rather than accurate lighting.
- These tone patches often follow the object's dirt, wear, or shadowed recesses — e.g. the underside of a rock, the shaded half of a roof, the interior of a broken window.
- No gradients, no specular highlights, no rim lighting. Everything reads as **2–3 flat tones per shape**, blended only by the soft edge of the painted blob, not by digital gradient tools.
 
### Color Palette
 
- **Desaturated, muted, and earthy.** Browns, grays, mossy greens, faded rust-oranges, dull stone tones. Nothing neon or highly saturated — this is a decaying, overgrown, abandoned world.
- Man-made structures (sheds, towers, buildings, fences) lean gray/brown/weathered-wood.
- Natural terrain features (rocks, fallen trees, rubble) lean brown/gray/green with moss accents.
- Small accent colors (a rusty stain, a hint of teal broken glass, a dab of moss) are allowed but should stay desaturated and used sparingly — they exist to sell wear and storytelling detail, not to add vibrancy.
- Avoid pure white or pure black fills; even "white" or "black" elements should read as warm/cool off-tones consistent with the marker-and-wash palette.
 
### Texture & Detail Language
 
Detail is communicated through a **small vocabulary of hand-drawn marks**, reused consistently across sprites:
 
- **Wood:** parallel grain lines following the plank's length, occasional knots (small ovals), rough-cut end grain shown as concentric or radial lines.
- **Stone/brick/concrete:** blocky irregular shapes separated by thin crack-like dividing lines; occasional larger structural cracks.
- **Metal (fences, wires, tower struts):** cross-hatched or diagonal-weave line patterns for mesh; simple riveted joints shown as small dots/ovals at connection points.
- **Moss/overgrowth:** small irregular dark-green dabs or patches, clustered in corners, seams, and shaded recesses — never covering a whole surface evenly.
- **Damage/wear:** jagged torn holes (fences, boarded windows), splintered board edges, cracked glass shown as small jagged white/teal shards, scorch or stain patches as soft dark smudges.
- **Sparking/electrical hazards:** simple zig-zag lines in a bright accent color (yellow/orange) as the one deliberately saturated exception, used only for danger callouts.
Every object should tell a small story of decay or history through these marks — a locked crate looks forced-at before, a shed roof has caved in, a wall has lost some bricks — without needing extra props or text.
 
### Composition & Framing
 
- Each encounter sprite is a **single isolated object or object cluster**, cropped tightly around its silhouette, on a transparent/blank background — no ground plane, no cast shadow puddle, no scene context baked in (that's handled separately by encounter background art).
- **Strictly 2D, front-facing or shallow 3/4 "clip-art" perspective.** No true 3D depth, no vanishing-point perspective, no foreshortening. Objects are drawn the way you'd draw an icon of them, flattened.
- Compositions can be **dynamic and irregular** rather than centered/symmetrical — a fallen tree crosses the frame diagonally, a rubble pile is asymmetric, a wall has broken/missing sections rather than being a uniform rectangle. This avoids a "game asset" stiffness and keeps things feeling sketched from life.
- Objects are drawn at a **consistent implied scale/weight** to their real-world size relative to other props (a crate is small and boxy; a radio tower is tall and looms), since the encounter camera zoom is the only thing communicating scale to the player.
- Multi-part objects (e.g. a supply stash with a lid, a door hanging off a frame) show their **current state** clearly — open/closed, broken/intact — since sprites are swapped rather than animated.
 
### What to Avoid

- No smooth vector shapes or perfectly straight/parallel lines.
- No gradients or soft airbrushed lighting.
- No high-saturation or neon colors outside the electrical-hazard exception.
- No painterly realism, no photo-textures.
- No drop shadows, ground contact shadows, or background elements baked into the sprite.
- No outlines around the internal soft-shading blobs — only around hard structural edges/details.
- No cutesy/rounded "mobile game" style — the wobble and roughness should read as hand-sketched, not stylized-cute.
 
### Prompt Template
 
When generating a new encounter sprite, describe:
 
1. **The object** and its current state (intact / damaged / open / closed / etc.)
2. **Category cues** — is it man-made (wood/metal/stone marks) or natural (moss/rock/wood-grain marks)?
3. **The style block below**, appended to every prompt:
> Hand-drawn illustration in a rough marker-and-wash style: thick uneven black outlines with pressure variation, flat muted/desaturated color fill with 1–2 soft irregular painted shadow blobs for volume (no gradients, no cel-shading), hand-drawn interior detail lines (wood grain, brick lines, cross-hatched mesh) thinner than the outer outline, small moss/crack/wear details for a weathered look, strictly flat 2D front-facing clip-art perspective with no 3D depth or cast ground shadow, isolated single object tightly cropped on a transparent/white background.
 
### Reference Notes by Category
 
- **Structures (sheds, towers, buildings, walls):** weathered wood or stone/brick textures, visible construction seams (planks, blocks), damage as missing/broken sections rather than uniform grime.
- **Containers (crates, stashes, boxes):** simple boxy silhouettes, latch/lid details, dirt mound or displaced ground fragment at the base to imply burial/hiding without a full ground plane.
- **Natural terrain props (fallen trees, rock piles, rubble):** organic, irregular silhouettes, moss dabs, exposed inner wood/stone color where broken.
- **Fences/barriers:** thin cross-hatched mesh line pattern within a simple outlined boundary, damage shown as a jagged torn gap.
- **NPCs/figures glimpsed in encounters (e.g. through a window):** kept in the same stick-figure/chunky-head language as the player, but rendered with the encounter sprite's flatter, more muted palette rather than the player's saturated foreground colors, since they sit "in" the scene rather than in the foreground layer.
