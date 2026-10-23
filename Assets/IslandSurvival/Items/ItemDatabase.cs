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
            Add(new ItemDefinition("stick", "item.stick.name", "item.stick.description", ItemCategory.Resource));
            Add(new ItemDefinition("stone", "item.stone.name", "item.stone.description", ItemCategory.Resource));
            Add(new ItemDefinition("fiber", "item.fiber.name", "item.fiber.description", ItemCategory.Resource));
            Add(new ItemDefinition("wood", "item.wood.name", "item.wood.description", ItemCategory.Resource));
            var coconut = new ItemDefinition("coconut", "item.coconut.name", "item.coconut.description", ItemCategory.Food);
            coconut.hungerRestore = 10f; coconut.thirstRestore = 15f;
            Add(coconut);
            var axe = new ItemDefinition("stone_axe", "item.stone_axe.name", "item.stone_axe.description", ItemCategory.Tool, false);
            axe.equippable = true;
            Add(axe);
            var knife = new ItemDefinition("stone_knife", "item.stone_knife.name", "item.stone_knife.description", ItemCategory.Tool, false);
            knife.equippable = true;
            Add(knife);
            Add(new ItemDefinition("blue_crystal_fragment", "item.blue_crystal_fragment.name", "item.blue_crystal_fragment.description", ItemCategory.Resource, false));
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
