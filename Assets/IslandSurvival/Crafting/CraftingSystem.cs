using System;
using System.Collections.Generic;
using System.Text;
using IslandSurvival.Inventory;
using IslandSurvival.Items;
using IslandSurvival.Localization;

namespace IslandSurvival.Crafting
{
    public sealed class CraftingSystem
    {
        readonly InventoryModel inventory;
        readonly ItemDatabase items;
        readonly LocalizationService localization;
        public readonly List<RecipeDefinition> Recipes = new List<RecipeDefinition>();
        public CraftingSystem(InventoryModel inventory, ItemDatabase items, LocalizationService localization)
        {
            this.inventory = inventory; this.items = items; this.localization = localization;
            Recipes.Add(new RecipeDefinition("stone_knife", "item.stone_knife.name", "stone_knife", new Ingredient("stone", 1), new Ingredient("stick", 1), new Ingredient("fiber", 1)));
            Recipes.Add(new RecipeDefinition("stone_axe", "item.stone_axe.name", "stone_axe", new Ingredient("stone", 2), new Ingredient("stick", 1), new Ingredient("fiber", 1)));
        }

        public bool TryCraft(RecipeDefinition recipe, out string message)
        {
            var missing = new StringBuilder();
            foreach (var ingredient in recipe.ingredients)
                if (inventory.Count(ingredient.itemId) < ingredient.amount)
                    missing.Append(localization.Get(items.Get(ingredient.itemId).displayNameKey)).Append(" x").Append(ingredient.amount - inventory.Count(ingredient.itemId)).Append(" ");
            if (missing.Length > 0) { message = localization.Format("craft.missing", missing.ToString().TrimEnd()); return false; }
            if (!inventory.CanAdd(recipe.outputItemId, recipe.outputAmount)) { message = localization.Get("craft.inventory_full"); return false; }
            foreach (var ingredient in recipe.ingredients) inventory.Remove(ingredient.itemId, ingredient.amount);
            inventory.Add(recipe.outputItemId, recipe.outputAmount);
            message = localization.Format("craft.completed", localization.Get(recipe.displayNameKey));
            return true;
        }
    }
}
