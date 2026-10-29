# FastStructure
FastStructure is a developer tool for saving parts of your world as structure files, so you can spawn them from your own mod without depending on another mod.

Build in-game, select an area, save it to a compact binary file, and load it back with a small loader you copy into your own mod's code. The finished mod has no dependency on FastStructure.

### Commands
/p1 - Point 1 read mouse position
/p2 - Point 2 read mouse position
/savestruct <name> - Save structure to file to .../MyGames/Terraria/tModLoader/SavedStructures/<name>.bin

### NOT SUPPORTED by this save format:
- Chest contents
- Sign text data
- Tile entities (mannequins, weapon racks, item frames, food platters,
  hat racks, logic sensors, mod TEs). The tiles are placed, but the entity
  and anything stored in it is not created
- Custom ITileData from mods
- Wires and actuators
- Wall paint, and the invisible/fullbright coatings on tiles and walls
- Inactive (actuated) tile state
- Mod tiles/walls whose mod is missing or renamed are skipped on load,
  leaving empty space instead of the original block.
- Structure size is limited to 32767 x 32767 tiles (stored as short).


NOTE: These dev commands are disabled by default when the mod is compiled
(they are wrapped in #if DEBUG, so a normal Release build excludes them).

To enable them:
1. Disable the mod in the in-game Mods list (so the .tmod file isn't locked).
2. In the mod's source folder (the one containing the .csproj), run: dotnet build -c Debug
3. Re-enable the mod and load the world.
