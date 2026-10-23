using System;
using System.Collections.Generic;
using System.Text;
using IslandSurvival.Inventory;

namespace IslandSurvival.Crafting
{
    public sealed class CraftingSystem
    {
        readonly InventoryModel inventory;
        public readonly List<RecipeDefinition> Recipes = new List<RecipeDefinition>();
        public CraftingSystem(InventoryModel inventory)
        {
            this.inventory = inventory;
            Recipes.Add(new RecipeDefinition("stone_knife", "Stone Knife", "stone_knife", new Ingredient("stone", 1), new Ingredient("stick", 1), new Ingredient("fiber", 1)));
            Recipes.Add(new RecipeDefinition("stone_axe", "Stone Axe", "stone_axe", new Ingredient("stone", 2), new Ingredient("stick", 1), new Ingredient("fiber", 1)));
        }

        public bool TryCraft(RecipeDefinition recipe, out string message)
        {
            var missing = new StringBuilder();
            foreach (var ingredient in recipe.ingredients)
                if (inventory.Count(ingredient.itemId) < ingredient.amount)
                    missing.Append(ingredient.itemId).Append(" x").Append(ingredient.amount - inventory.Count(ingredient.itemId)).Append(" ");
            if (missing.Length > 0) { message = "Missing: " + missing; return false; }
            if (!inventory.CanAdd(recipe.outputItemId, recipe.outputAmount)) { message = "Inventory full"; return false; }
            foreach (var ingredient in recipe.ingredients) inventory.Remove(ingredient.itemId, ingredient.amount);
            inventory.Add(recipe.outputItemId, recipe.outputAmount);
            message = "Crafted " + recipe.displayName;
            return true;
        }
    }
}
