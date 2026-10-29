using Terraria;
using Terraria.ModLoader;

namespace FastStructure.Content;

public class KeepMarker : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        AddMapEntry(Microsoft.Xna.Framework.Color.Magenta);
        MineResist = 0.2f;

    }
}

public class KeepMarkerItem : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<KeepMarker>());
        Item.maxStack = 9999;
    }
}