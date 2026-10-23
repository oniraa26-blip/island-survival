using System;
using UnityEngine;

namespace IslandSurvival.Items
{
    public enum ItemCategory { Resource, Food, Tool }

    [Serializable]
    public sealed class ItemDefinition
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public ItemCategory category;
        public bool stackable;
        public int maxStack = 20;
        public bool equippable;
        public float hungerRestore;
        public float thirstRestore;

        public ItemDefinition(string id, string name, ItemCategory category, bool stackable = true, int maxStack = 20)
        {
            this.id = id; displayName = name; this.category = category;
            this.stackable = stackable; this.maxStack = stackable ? maxStack : 1;
            description = string.Empty;
        }
    }
}
