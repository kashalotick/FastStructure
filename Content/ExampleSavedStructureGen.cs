using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace FastStructure.Content;

public class ExampleSavedStructureGen : ModSystem
{
    public virtual string VanillaIndexName => "Final Cleanup";

    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
    {
        if (!ModContent.GetInstance<Config>().EnableExampleStructureGen)
        {
            return;
        }
        
        
        int index = tasks.FindIndex(x => x.Name == VanillaIndexName);
        if (index != -1)
        {
            tasks.Insert(index, new PassLegacy("ExampleSavedStructureGen", (progress, config) => AddGen(progress)));
        }
    }

    void AddGen(GenerationProgress progress = null)
    {
        Console.WriteLine("ExampleSavedStructureGen");
        var x = Main.maxTilesX / 2;
        var y = Main.maxTilesY / 2;
        
        using var testStream = StructureReader.Open(Mod, "Assets/example.bin");
        var size = StructureReader.GetSize(testStream);
        
        StructureReader.Place(testStream, x - size.width / 2, y - size.height / 2);
        StructureReader.Place(testStream, x - size.width / 2, Main.UnderworldLayer - 100);

    }
}