using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace FastStructure.Content;

public static class StructureSaver
{
    const byte FormatVersion = 1;

    // Per-cell marker byte
    const byte CellKeep = 0; // leave the world as it is
    const byte CellNormal = 1; // cell data follows


    /// <summary>
    /// Save structure to file
    /// </summary>
    /// <param name="path">Full path of the output file, including its name. An existing file is overwritten.</param>
    /// <param name="x0">X of the top left corner</param>
    /// <param name="y0">Y of the top left corner</param>
    /// <param name="w">Width</param>
    /// <param name="h">Height</param>
    /// <remarks>
    /// Chest contents, sign text, tile entities and wires are not saved. See the format notes.
    /// Keep Marker blocks are saved as "keep what is in the world" cells.
    /// </remarks>
    public static void Save(string path, int x0, int y0, int w, int h)
    {
        using var bw = new BinaryWriter(File.Create(path));
        bw.Write(FormatVersion);
        bw.Write((short)w);
        bw.Write((short)h);

        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
        {
            Tile t = Main.tile[x0 + x, y0 + y];

            bool keep = t.HasTile && t.TileType == ModContent.TileType<KeepMarker>();
            if (keep)
            {
                bw.Write(CellKeep);
                continue;
            }

            bw.Write(CellNormal);
            bw.Write(t.HasTile);
            if (t.HasTile)
            {
                WriteType(bw, t.TileType, isWall: false);
                bw.Write(t.TileFrameX);
                bw.Write(t.TileFrameY);
                bw.Write((byte)t.TileColor);
                bw.Write((byte)t.BlockType); // slope/half-brick
                bw.Write((byte)((t.HasActuator ? 1 : 0) | (t.IsActuated ? 2 : 0)));
            }

            WriteType(bw, t.WallType, isWall: true);
            bw.Write(t.LiquidAmount);
            bw.Write((byte)t.LiquidType);
        }
    }

    static void WriteType(BinaryWriter bw, int type, bool isWall)
    {
        ModBlockType mod = isWall ? ModContent.GetModWall(type) : ModContent.GetModTile(type);
        if (mod == null)
        {
            bw.Write(true);
            bw.Write((ushort)type);
        }
        else
        {
            bw.Write(false);
            bw.Write(mod.FullName);
        }
    }
}