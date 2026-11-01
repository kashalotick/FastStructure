# v0.4.1
- Add Count to StructureVariantSet
- Remove random tile number and slope for furniture

# v0.4
- Add StructureVariantSet class
- Add lambda expression support for StructureReader.Place
- Add new usage examples
- Add StructureAssets class

# v0.3
- Add relative to selection origin position under cursor for SelectionWand
- Add placement mode with keep markers in menu
- Remove saving some tiles as containers, mannequin. Full list: field `IgnoredTiles` in  [StructureSaver.cs](Content/StructureSaver.cs)

# v0.2
- Add /liststruct and /structmenu command
- Add struct
- Add selection wand, structure menu item
- Add structure menu that allows to see list of structures, save, place and other actions instead of using commands
- fix EndOfStreamException when trying to Place structure from the same stream more than once
- fix single tile variation placement

# v0.1
- Add commands
- Add example
- Add example structr spawn enable config
Support
- solid tiles
- furniture (not entity tiles and chests)
- liqueids
- walls
- not activated blocks
- keep markes to avoid replacing solid blocks with air