using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace FastStructure.Content;

public class StructSelection : ModSystem
{
    static Point _p1, _p2;
 
    public static bool HasP1 { get; private set; }
    public static bool HasP2 { get; private set; }
 
    public static Point P1
    {
        get => _p1;
        set { _p1 = value; HasP1 = true; }
    }
 
    public static Point P2
    {
        get => _p2;
        set { _p2 = value; HasP2 = true; }
    }
 
    public override void OnWorldUnload()
    {
        HasP1 = HasP2 = false;
    }
}