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
For outlines, use black color, 12px, 100% hardness
100% opacity, and 100% flow.
Activate pen pressure for size.

FILL
same brush settings as outline with any size as needed.
Fill does not have to be perfectly inside outline, some overflow or gaps adds to the style.
Color choice is free.

## Export
When done, Image > Trim > Transparent Pixels, then export as png. place in Assets/Sprites/Items and add to ItemDef in code. Name must match the DefName in ItemDef.


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
pretty free
