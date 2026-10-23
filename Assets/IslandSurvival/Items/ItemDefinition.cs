using System;
using UnityEngine;

namespace IslandSurvival.Items
{
    public enum ItemCategory { Resource, Food, Tool }

    [Serializable]
    public sealed class ItemDefinition
    {
        public string id;
        public string displayNameKey;
        public string descriptionKey;
        public ItemCategory category;
        public Sprite icon;
        public bool stackable;
        public int maxStack = 20;
        public bool equippable;
        public float hungerRestore;
        public float thirstRestore;

        public ItemDefinition(string id, string nameKey, string descriptionKey, ItemCategory category, bool stackable = true, int maxStack = 20)
        {
            this.id = id; displayNameKey = nameKey; this.descriptionKey = descriptionKey; this.category = category;
            this.stackable = stackable; this.maxStack = stackable ? maxStack : 1;
            icon = null;
        }
    }
}
