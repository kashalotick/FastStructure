using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace FastStructure.Content;

/// <summary>
/// Use to open/close the structure menu.
/// Texture: Content/StructureMenuItem.png (path follows the namespace + class name).
/// </summary>
public class StructureMenuItem : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.autoReuse = false;
        Item.rare = ItemRarityID.Cyan;
    }

    public override bool? UseItem(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
            StructureMenuSystem.Toggle();

        return true;
    }

    // so you can get it without cheats; remove if you don't want it
    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient(ItemID.Wood)
            .Register();
    }
}