# FastStructure
FastStructure is a developer tool for saving parts of your world as structure files, so you can spawn them from your own mod without depending on another mod.

Build in-game, select an area, save it to a compact binary file, and load it back with a small loader you copy into your own mod's code. The finished mod has no dependency on FastStructure.

**This is a developer tool, not a gameplay mod. The commands exist only in Debug builds.**

### Config
- `EnableExampleStructureGen` -  enable or disable the example structure world gen (default: false)

### Commands
- /p1 - Point 1 read mouse position
- /p2 - Point 2 read mouse position
- /savestruct <name> - Save structure to file to .../MyGames/Terraria/tModLoader/SavedStructures/<name>.bin
- /placestruct <name> - Place structure from file .../MyGames/Terraria/tModLoader/SavedStructures/<name>.bin
- /keepvis off - Turn off visibility of keep markers (it still solid)
- /keepvis on - Turn on visibility of keep markers
- /keep - gives the keep markers

### NOT SUPPORTED by this save format:
- Chest contents
- Sign text data
- Tile entities (mannequins, weapon racks, item frames, food platters,
  hat racks, logic sensors, mod TEs). The tiles are placed, but the entity
  and anything stored in it is not created
- Custom ITileData from mods
- Wires
- Wall paint, and the invisible/fullbright coatings on tiles and walls
- Mod tiles/walls whose mod is missing or renamed are skipped on load,
  leaving empty space instead of the original block.
- Structure size is limited to 32767 x 32767 tiles (stored as short).

## How to use
1. Build a structure
2. (Optional) Use Keep Markers where the structure must not change the world. When the structure is placed, those cells keep whatever is already in the world (terrain, walls, liquids) instead of being replaced with air.
3. Place the cursor on the first point and write `/p1`
4. Place the cursor on the second point and write `/p2`
5. Write `/savestruct <name>`
6. Copy the binary file .../MyGames/Terraria/tModLoader/SavedStructures/<name>.bin to your mod source
7. Copy the `Reader.cs` file
8. See the [UsageExample.cs](Content/UsageExample.cs) for how to use it