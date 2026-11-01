using System;
using System.IO;
using Microsoft.Build.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace FastStructure.Content;

public class StructureReader
{
    const byte FormatVersion = 1;

    // Per-cell marker byte
    const byte CellKeep = 0; // leave the world as it is
    const byte CellNormal = 1; // cell data follows

    /// <summary>
    /// Open a structure file from the mod as a seekable stream
    /// </summary>
    /// <param name="mod">Mod that contains the file</param>
    /// <param name="path">Path to the file inside the mod, without the mod's folder</param>
    /// <returns>
    /// A <see cref="MemoryStream"/> positioned at the start of the file.
    /// Unlike the stream from <see cref="Mod.GetFileStream(string)"/>, it supports <see cref="Stream.Position"/>,
    /// which <see cref="GetSize"/> relies on.
    /// </returns>
    /// <remarks>The caller owns the returned stream and should dispose it.</remarks>
    public static MemoryStream Open(Mod mod, string path)
    {
        using var s = mod.GetFileStream(path);
        var ms = new MemoryStream();
        s.CopyTo(ms);
        ms.Position = 0;
        return ms;
    }

    /// <summary>
    /// Get the size of the structure from a stream
    /// </summary>
    /// <param name="s">stream from <see cref="Open"/></param>
    /// <returns>width and height of the structure</returns>
    /// <exception cref="InvalidDataException">The file has an unknown format version.</exception>
    /// <exception cref="ArgumentException">The stream is not seekable.</exception>
    /// <remarks>The stream position is restored after reading.</remarks>
    public static (int width, int height) GetSize(Stream s)
    {
        if (!s.CanSeek)
            throw new ArgumentException("Stream must be seekable. Use StructureLoader.Open().", nameof(s));

        long start = s.Position;
        try
        {
            using var br = new BinaryReader(s, System.Text.Encoding.UTF8, leaveOpen: true);

            byte ver = br.ReadByte();
            if (ver != FormatVersion) throw new InvalidDataException($"Unknown structure format v{ver}");

            return new(br.ReadInt16(), br.ReadInt16());
        }
        finally
        {
            s.Position = start;
        }
    }

    
    /// <summary>
    /// Decides whether a cell of the structure is applied to the world.
    /// </summary>
    /// <param name="x">World X of the cell</param>
    /// <param name="y">World Y of the cell</param>
    /// <param name="existing">Tile currently in the world at this position</param>
    /// <param name="newType">
    /// Tile type the structure wants to place here, or -1 if the cell has no tile
    /// (or its tile is from a mod that is not loaded)
    /// </param>
    /// <returns>true to apply the cell (tile, wall and liquid), false to leave the world untouched there</returns>
    public delegate bool CellFilter(int x, int y, Tile existing, int newType);


    /// <summary>
    /// Place structure in the world from stream
    /// </summary>
    /// <param name="s">stream from <see cref="Open"/>, positioned at the start of the file</param>
    /// <param name="x0">X of the top left corner</param>
    /// <param name="y0">Y of the top left corner</param>
    /// <param name="keepMarkerType">
    /// Tile type to put on cells saved as Keep Marker. -1 (default) leaves those cells untouched.
    /// </param>
    /// <param name="filter">
    /// Optional. Called for every normal cell before it is applied, see <see cref="CellFilter"/>.
    /// Not called for Keep Marker cells.
    /// </param>
    /// <remarks>
    /// Tiles and walls from mods that are not loaded are skipped.
    /// Cells saved as Keep Marker are not touched.
    /// If the stream is seekable, its position is restored after placing,
    /// so the same stream can be passed to <see cref="Place"/> several times.
    /// The filter runs while the structure is still being placed, so neighbouring cells
    /// may not be placed yet. Collect positions in it and act after Place returns.
    /// </remarks>
    /// <exception cref="InvalidDataException">The file has an unknown format version.</exception>
    public static void Place(Stream s, int x0, int y0, int keepMarkerType = -1, CellFilter filter = null)
    {
        long start = s.CanSeek ? s.Position : 0;
        try
        {
            using var br = new BinaryReader(s, System.Text.Encoding.UTF8, leaveOpen: true);

            byte ver = br.ReadByte();
            if (ver != FormatVersion) throw new InvalidDataException($"Unknown structure format v{ver}");

            int w = br.ReadInt16(), h = br.ReadInt16();

            for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
            {
                if (br.ReadByte() == CellKeep)
                {
                    if (keepMarkerType >= 0)
                    {
                        Tile k = Main.tile[x0 + x, y0 + y];
                        k.HasTile = true;
                        k.TileType = (ushort)keepMarkerType;
                        k.TileFrameX = 0;
                        k.TileFrameY = 0;
                        k.TileColor = 0;
                        k.BlockType = BlockType.Solid;
                        k.IsActuated = false;
                    }

                    continue;
                }

                // read the whole cell first, so the stream stays in sync even if the cell is skipped
                bool hasTile = br.ReadBoolean();
                int type = -1;
                short fx = 0, fy = 0;
                byte color = 0, block = 0, flags = 0;

                if (hasTile)
                {
                    type = ReadType(br, false);
                    fx = br.ReadInt16();
                    fy = br.ReadInt16();
                    color = br.ReadByte();
                    block = br.ReadByte();
                    flags = br.ReadByte();
                }

                int wall = ReadType(br, true);
                byte liquidAmount = br.ReadByte();
                byte liquidType = br.ReadByte();

                Tile t = Main.tile[x0 + x, y0 + y];

                if (filter != null && !filter(x0 + x, y0 + y, t, type)) continue;

                if (type >= 0)
                {
                    t.HasTile = true;
                    t.TileType = (ushort)type;
                    t.TileFrameX = fx;
                    t.TileFrameY = fy;
                    t.TileFrameNumber = TileObjectData.GetTileData(t) == null
                        ? (byte)WorldGen.genRand.Next(3)
                        : (byte)0;                    
                    t.TileColor = color;
                    
                    bool furniture = TileObjectData.GetTileData(t) != null && !TileID.Sets.Platforms[type];
                    t.BlockType = furniture ? BlockType.Solid : (BlockType)block;
                    
                    t.HasActuator = (flags & 1) != 0;
                    t.IsActuated = (flags & 2) != 0;
                }
                else t.HasTile = false;

                if (wall >= 0)
                {
                    t.WallType = (ushort)wall;
                    t.WallFrameNumber = (byte)WorldGen.genRand.Next(3);
                }
                else t.WallType = 0;

                t.LiquidAmount = liquidAmount;
                t.LiquidType = liquidType;
            }

            WorldGen.RangeFrame(x0, y0, x0 + w, y0 + h);
            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendTileSquare(-1, x0, y0, w, h);
        }
        finally
        {
            if (s.CanSeek) s.Position = start;
        }
    } // returns -1 if the mod type can't be found

    static int ReadType(BinaryReader br, bool isWall)
    {
        if (br.ReadBoolean()) return br.ReadUInt16();

        string name = br.ReadString();
        if (isWall) return ModContent.TryFind(name, out ModWall mw) ? mw.Type : -1;
        return ModContent.TryFind(name, out ModTile mt) ? mt.Type : -1;
    }
}

public class StructureVariantSet
{
    readonly byte[][] _data;
    readonly (int width, int height)[] _sizes;
    
    public int Count => _data.Length;

    public StructureVariantSet(byte[][] data, (int width, int height)[] sizes)
    {
        _data = data;
        _sizes = sizes;
    }

    public static StructureVariantSet Open(Mod mod, string[] paths)
    {
        var data = new byte[paths.Length][];
        var sizes = new (int, int)[paths.Length];
        for (int i = 0; i < paths.Length; i++)
        {
            using var ms = StructureReader.Open(mod, paths[i]);
            sizes[i] = StructureReader.GetSize(ms);
            data[i] = ms.ToArray();
        }


        return new StructureVariantSet(data, sizes);
    }

    public (int width, int height) GetSize(int variant) => _sizes[variant];


    public void Place(int variant, int x0, int y0, int keepMarkerType = -1, StructureReader.CellFilter filter = null)
    {
        using var ms = new MemoryStream(_data[variant], writable: false);
        StructureReader.Place(ms, x0, y0, keepMarkerType, filter);
    }
}