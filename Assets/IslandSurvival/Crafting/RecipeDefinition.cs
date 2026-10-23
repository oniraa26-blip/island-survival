using System;
using System.Collections.Generic;

namespace IslandSurvival.Crafting
{
    [Serializable]
    public sealed class Ingredient
    {
        public string itemId;
        public int amount;
        public Ingredient(string id, int count) { itemId = id; amount = count; }
    }

    [Serializable]
    public sealed class RecipeDefinition
    {
        public string id;
        public string displayName;
        public string outputItemId;
        public int outputAmount;
        public List<Ingredient> ingredients = new List<Ingredient>();
        public RecipeDefinition(string id, string name, string output, params Ingredient[] costs)
        { this.id = id; displayName = name; outputItemId = output; outputAmount = 1; ingredients.AddRange(costs); }
    }
}
