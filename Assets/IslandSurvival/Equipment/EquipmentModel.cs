using System;
using IslandSurvival.Inventory;
using IslandSurvival.Items;

namespace IslandSurvival.Equipment
{
    public sealed class EquipmentModel
    {
        readonly InventoryModel inventory;
        readonly ItemDatabase items;
        public string MainHandItemId { get; private set; }
        public event Action Changed;
        public EquipmentModel(InventoryModel inventory, ItemDatabase items)
        { this.inventory = inventory; this.items = items; MainHandItemId = string.Empty; }
        public bool Equip(string itemId)
        {
            ItemDefinition item;
            if (inventory.Count(itemId) <= 0 || !items.TryGet(itemId, out item) || !item.equippable) return false;
            MainHandItemId = itemId; if (Changed != null) Changed(); return true;
        }
        public void Unequip() { MainHandItemId = string.Empty; if (Changed != null) Changed(); }
        public void Restore(string itemId)
        { if (string.IsNullOrEmpty(itemId)) Unequip(); else Equip(itemId); }
    }
}
