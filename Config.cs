using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace FastStructure;

public class Config: ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [DefaultValue(false)]
    public bool EnableExampleStructureGen;

}