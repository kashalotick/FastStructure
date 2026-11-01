using System;
using System.Reflection;
using Terraria.ModLoader;

namespace FastStructure.Content.Examples;

public class StructureAssets : ModSystem
{
    /// <summary>
    /// Replace the following fields with your own structure assets.
    /// </summary>
    public static readonly string Example = "Assets/example.bin";
    public static readonly string ExampleLambda = "Assets/exampleLambda.bin";
    public static readonly string ExampleV0 = "Assets/exampleV0.bin";
    public static readonly string ExampleV1 = "Assets/exampleV1.bin";
    public static readonly string ExampleV2 = "Assets/exampleV2.bin";
    public static readonly string ExampleV3 = "Assets/exampleV3.bin";


    /// <summary>
    /// Check if all required assets exist.
    /// Execute once when the mod is loaded.
    /// </summary>
    /// <exception cref="Exception"> Throws if the asset file not found</exception>
    public override void Load()
    {
        FieldInfo[] fields = typeof(StructureAssets).GetFields(BindingFlags.Public | BindingFlags.Static);

        foreach (FieldInfo field in fields)
        {
            if (field.FieldType != typeof(string)) continue;

            string path = (string)field.GetValue(null);
            if (string.IsNullOrEmpty(path)) continue;

            if (!Mod.FileExists(path))
            {
                throw new Exception(
                    $"[StructureAssets] Not found required structure asset '{path}' (field {field.Name})");
            }
        }
    } 
}