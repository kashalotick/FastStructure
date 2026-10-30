using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace FastStructure.Content;

/// <summary>
/// LMB sets P1, RMB sets P2 (tile under the cursor).
/// While held, the selection is highlighted in the world.
/// </summary>
public class StructureWand : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = 10;
        Item.useAnimation = 10;
        Item.noMelee = true;
        Item.autoReuse = false;
        Item.rare = ItemRarityID.Cyan;
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool? UseItem(Player player)
    {
        if (player.whoAmI != Main.myPlayer) return true;

        Point tile = Main.MouseWorld.ToTileCoordinates();

        if (player.altFunctionUse == 2)
        {
            StructSelection.P2 = tile;
            Main.NewText($"P2 = {tile.X}, {tile.Y}", Color.Orange);
        }
        else
        {
            StructSelection.P1 = tile;
            Main.NewText($"P1 = {tile.X}, {tile.Y}", Color.Cyan);
        }

        return true;
    }
}

public class StructureSelectionDraw : ModSystem
{
    static readonly Color P1Color = Color.Cyan;
    static readonly Color P2Color = Color.Orange;
    static readonly Color FrameColor = Color.White;
    static readonly Color OriginColor = Color.Lime;

    public static void DrawOverlay(SpriteBatch sb)
    {
        Player player = Main.LocalPlayer;
        int held = player.HeldItem.type;
        if (held != ModContent.ItemType<StructureWand>()
            && held != ModContent.ItemType<StructureMenuItem>()
            && held != ModContent.ItemType<KeepMarkerItem>()
            && !StructureMenuSystem.IsOpen)
        {
            return;
        }

        if (!StructSelection.HasP1 && !StructSelection.HasP2) return;


        // frame + origin only when both points are set
        if (StructSelection.HasP1 && StructSelection.HasP2)
        {
            Point a = StructSelection.P1, b = StructSelection.P2;
            int x0 = Math.Min(a.X, b.X), y0 = Math.Min(a.Y, b.Y);
            int w = Math.Abs(a.X - b.X) + 1, h = Math.Abs(a.Y - b.Y) + 1;

            Rectangle frame = new(
                x0 * 16 - (int)Main.screenPosition.X,
                y0 * 16 - (int)Main.screenPosition.Y,
                w * 16, h * 16);

            Fill(sb, frame, FrameColor * 0.10f);
            Outline(sb, frame, 2, FrameColor * 0.8f);

            Utils.DrawBorderString(sb, $"{w}x{h}", new Vector2(frame.X, frame.Bottom + 4), FrameColor, 0.9f);
        }

        if (StructSelection.HasP1) DrawPoint(sb, StructSelection.P1, P1Color, "P1");
        if (StructSelection.HasP2) DrawPoint(sb, StructSelection.P2, P2Color, "P2");

        // origin = top left corner, this is where Place(s, x0, y0) puts the structure
        if (StructSelection.HasP1 && StructSelection.HasP2)
        {
            Point o = new(
                Math.Min(StructSelection.P1.X, StructSelection.P2.X),
                Math.Min(StructSelection.P1.Y, StructSelection.P2.Y));

            Rectangle r = TileRect(o);
            Rectangle ring = new(r.X - 2, r.Y - 2, r.Width + 4, r.Height + 4);
            Outline(sb, ring, 2, OriginColor);
            Utils.DrawBorderString(sb, "origin", new Vector2(r.X, r.Bottom + 4), OriginColor, 0.9f);
        }
    }

    static void DrawPoint(SpriteBatch sb, Point tile, Color color, string label)
    {
        Rectangle r = TileRect(tile);
        Fill(sb, r, color * 0.5f);
        Outline(sb, r, 2, color);
        Utils.DrawBorderString(sb, label, new Vector2(r.X, r.Y - 22), color, 0.9f);
    }

    static Rectangle TileRect(Point t) => new(
        t.X * 16 - (int)Main.screenPosition.X,
        t.Y * 16 - (int)Main.screenPosition.Y,
        16, 16);

    internal static void Fill(SpriteBatch sb, Rectangle r, Color c) =>
        sb.Draw(TextureAssets.MagicPixel.Value, r, new Rectangle(0, 0, 1, 1), c);

    internal static void Outline(SpriteBatch sb, Rectangle r, int t, Color c)
    {
        Fill(sb, new Rectangle(r.X, r.Y, r.Width, t), c);
        Fill(sb, new Rectangle(r.X, r.Bottom - t, r.Width, t), c);
        Fill(sb, new Rectangle(r.X, r.Y + t, t, r.Height - 2 * t), c);
        Fill(sb, new Rectangle(r.Right - t, r.Y + t, t, r.Height - 2 * t), c);
    }
    
    internal static void DrawHover(SpriteBatch sb)
    {
        if (Main.LocalPlayer.HeldItem.type != ModContent.ItemType<StructureWand>()) return;
        if (StructureMenuSystem.MouseOverMenu || StructureMenuSystem.Pending != null) return;

        Point t = Main.MouseWorld.ToTileCoordinates();
        Rectangle r = TileRect(t);

        Fill(sb, r, Color.Yellow * 0.35f);
        Outline(sb, r, 2, Color.Yellow);

        string text;
        if (StructSelection.HasP1 && StructSelection.HasP2)
        {
            Point a = StructSelection.P1, b = StructSelection.P2;
            int ox = Math.Min(a.X, b.X), oy = Math.Min(a.Y, b.Y);
            int w = Math.Abs(a.X - b.X) + 1, h = Math.Abs(a.Y - b.Y) + 1;

            int rx = t.X - ox, ry = t.Y - oy;
            bool inside = rx >= 0 && ry >= 0 && rx < w && ry < h;

            text = $"{rx}, {ry}" + (inside ? "" : " (outside)");
        }
        else
        {
            text = $"world {t.X}, {t.Y} (no origin yet)";
        }

        Utils.DrawBorderString(sb, text, new Vector2(r.Right + 8, r.Y - 6), Color.Yellow, 0.9f);
    }
}