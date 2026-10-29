using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace FastStructure.Content;

public class StructSelection : ModSystem
{
    public static Point P1, P2;
}

public class KeepCommand : ModCommand
{
    public override CommandType Type => CommandType.Chat;
    public override string Command => "keep";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        caller.Player.QuickSpawnItem(caller.Player.GetSource_Misc("KeepCommand"),
            ModContent.ItemType<KeepMarkerItem>(), 9999);
        caller.Reply("Given 999 Keep Markers");
    }
}

public class P1Command : ModCommand
{
    public override CommandType Type => CommandType.Chat;
    public override string Command => "p1";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        StructSelection.P1 = Main.MouseWorld.ToTileCoordinates();
        caller.Reply($"P1 = {StructSelection.P1}");
    }
}

public class P2Command : ModCommand
{
    public override CommandType Type => CommandType.Chat;
    public override string Command => "p2";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        StructSelection.P2 = Main.MouseWorld.ToTileCoordinates();
        caller.Reply($"P2 = {StructSelection.P2}");
    }
}

public class SaveStructCommand : ModCommand
{
    public override CommandType Type => CommandType.Chat;
    public override string Command => "savestruct";
    public override string Usage => "/savestruct <name>";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        if (args.Length < 1)
        {
            caller.Reply(Usage);
            return;
        }

        int x0 = Math.Min(StructSelection.P1.X, StructSelection.P2.X);
        int y0 = Math.Min(StructSelection.P1.Y, StructSelection.P2.Y);
        int w = Math.Abs(StructSelection.P1.X - StructSelection.P2.X) + 1;
        int h = Math.Abs(StructSelection.P1.Y - StructSelection.P2.Y) + 1;

        string dir = Path.Combine(Main.SavePath, "SavedStructures");
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, args[0] + ".bin");

        StructureSaver.Save(path, x0, y0, w, h);
        caller.Reply($"Saved {w}x{h} -> {path}");
    }
}

public class PlaceStructCommand : ModCommand
{
    public override CommandType Type => CommandType.Chat;
    public override string Command => "placestruct";
    public override string Usage => "/placestruct <name>";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        if (args.Length < 1)
        {
            caller.Reply(Usage);
            return;
        }

        string path = Path.Combine(Main.SavePath, "SavedStructures", args[0] + ".bin");
        if (!File.Exists(path))
        {
            caller.Reply($"{path} not found");
            return;
        }

        Point origin = Main.MouseWorld.ToTileCoordinates();

        using var s = File.OpenRead(path);
        StructureReader.Place(s, origin.X, origin.Y);

        caller.Reply($"Placed {args[0]} at {origin}");
    }
}


public class KeepVisibleCommand : ModCommand
{
    public override CommandType Type => CommandType.Chat;
    public override string Command => "keepvis";
    public override string Usage => "/keepvis [on|off]";

    public override void Action(CommandCaller caller, string input, string[] args)
    {
        if (args.Length > 0 && args[0] == "on") KeepMarkerVisibility.Visible = true;
        else if (args.Length > 0 && args[0] == "off") KeepMarkerVisibility.Visible = false;
        else KeepMarkerVisibility.Visible = !KeepMarkerVisibility.Visible;

        caller.Reply($"Keep markers: {(KeepMarkerVisibility.Visible ? "visible" : "hidden")}");
    }
}