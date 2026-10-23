using System;
using System.Collections.Generic;

namespace IslandSurvival.Items
{
    public sealed class ItemDatabase
    {
        readonly Dictionary<string, ItemDefinition> definitions = new Dictionary<string, ItemDefinition>();
        public IEnumerable<ItemDefinition> All { get { return definitions.Values; } }

        public ItemDatabase()
        {
            Add(new ItemDefinition("stick", "Stick", ItemCategory.Resource));
            Add(new ItemDefinition("stone", "Stone", ItemCategory.Resource));
            Add(new ItemDefinition("fiber", "Fiber", ItemCategory.Resource));
            Add(new ItemDefinition("wood", "Wood", ItemCategory.Resource));
            var coconut = new ItemDefinition("coconut", "Coconut", ItemCategory.Food);
            coconut.hungerRestore = 10f; coconut.thirstRestore = 15f;
            coconut.description = "A fresh coconut. Restores a little hunger and thirst.";
            Add(coconut);
            var axe = new ItemDefinition("stone_axe", "Stone Axe", ItemCategory.Tool, false);
            axe.equippable = true; axe.description = "A simple axe suitable for chopping palms.";
            Add(axe);
            var knife = new ItemDefinition("stone_knife", "Stone Knife", ItemCategory.Tool, false);
            knife.equippable = true; knife.description = "A basic cutting tool.";
            Add(knife);
        }

        void Add(ItemDefinition definition) { definitions.Add(definition.id, definition); }
        public bool TryGet(string id, out ItemDefinition item) { return definitions.TryGetValue(id, out item); }
        public ItemDefinition Get(string id)
        {
            ItemDefinition item;
            if (!definitions.TryGetValue(id, out item)) throw new ArgumentException("Unknown item: " + id);
            return item;
        }
    }
}
