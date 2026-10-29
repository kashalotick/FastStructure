using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace FastStructure.Content;


public class KeepMarkerVisibility : ModSystem
{
    public static bool Visible = true;
}
public class KeepMarker : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileSolid[Type] = true;
        AddMapEntry(Microsoft.Xna.Framework.Color.Magenta);
        MineResist = 0.2f;
    }
    
    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
        => KeepMarkerVisibility.Visible;

}

public class KeepMarkerItem : ModItem
{
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<KeepMarker>());
        Item.maxStack = 9999;
    }
}