using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace FastStructure.Content.Examples;

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
        PlaceSingleStructure();
        PlaceStructureVariants();
        PlaceLambdaExample();
    }

    void PlaceSingleStructure()
    {
        var x = Main.maxTilesX / 2;
        var y = Main.maxTilesY / 2;

        using var exampleStream = StructureReader.Open(Mod, StructureAssets.Example);
        var size = StructureReader.GetSize(exampleStream);

        StructureReader.Place(exampleStream, x - size.width / 2, y - size.height / 2);
        StructureReader.Place(exampleStream, x - size.width / 2, Main.UnderworldLayer - 100);
    }

    void PlaceLambdaExample()
    {
        var x = Main.maxTilesX / 2;
        var minY = (int)Main.rockLayer;
        var maxY = (int)Main.UnderworldLayer;

        const int step = 100;

        using var lambdaStream = StructureReader.Open(Mod, StructureAssets.ExampleLambda);


        // place diamond if dont has tile
        // random chance to place emerald gemspark
        StructureReader.CellFilter filter = (x, y, existing, newType) =>
        {
            Console.WriteLine(newType);
            if (newType == TileID.ExposedGems)
            {
                return !existing.HasTile;
            }

            if (newType == TileID.EmeraldGemspark)
            {
                if (WorldGen.genRand.Next(2) == 0)
                {
                    return true;
                }
                existing.HasTile = false;
                return false;
            }

            return true;
        };
        for (int y = minY; y < maxY; y += step)
        {
            StructureReader.Place(lambdaStream, x, y, filter: filter);
        }
    }


    void PlaceStructureVariants()
    {
        var y = (int)Main.worldSurface / 2;

        var variants = StructureVariantSet.Open(Mod, [
            StructureAssets.ExampleV0,
            StructureAssets.ExampleV1,
            StructureAssets.ExampleV2,
            StructureAssets.ExampleV3,
        ]);

        const int step = 100;
        var variant = 0;
        for (int x = 0; x < Main.maxTilesX; x += step)
        {
            variants.Place(variant, x, y - variants.GetSize(variant).height / 2);
            variant = (variant + 1) % 4;
        }
    }
}