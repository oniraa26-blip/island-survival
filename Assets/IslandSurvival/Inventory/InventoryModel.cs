using System;
using System.Collections.Generic;
using IslandSurvival.Items;

namespace IslandSurvival.Inventory
{
    [Serializable]
    public sealed class InventorySlot
    {
        public string itemId;
        public int quantity;
        public bool IsEmpty { get { return string.IsNullOrEmpty(itemId) || quantity <= 0; } }
        public InventorySlot() { itemId = string.Empty; quantity = 0; }
        public InventorySlot(string id, int count) { itemId = id; quantity = count; }
    }

    public sealed class InventoryModel
    {
        readonly ItemDatabase database;
        readonly List<InventorySlot> slots;
        public event Action Changed;
        public int Capacity { get { return slots.Count; } }
        public IReadOnlyList<InventorySlot> Slots { get { return slots; } }

        public InventoryModel(ItemDatabase database, int capacity = 16)
        {
            this.database = database;
            slots = new List<InventorySlot>(capacity);
            for (var i = 0; i < capacity; i++) slots.Add(new InventorySlot());
        }

        public int Count(string itemId)
        {
            var total = 0;
            foreach (var slot in slots) if (slot.itemId == itemId) total += slot.quantity;
            return total;
        }

        public bool CanAdd(string itemId, int amount)
        {
            if (amount <= 0) return false;
            var definition = database.Get(itemId);
            var remaining = amount;
            foreach (var slot in slots)
            {
                if (slot.IsEmpty) remaining -= definition.maxStack;
                else if (definition.stackable && slot.itemId == itemId) remaining -= definition.maxStack - slot.quantity;
                if (remaining <= 0) return true;
            }
            return false;
        }

        public bool Add(string itemId, int amount)
        {
            if (!CanAdd(itemId, amount)) return false;
            var definition = database.Get(itemId);
            var remaining = amount;
            if (definition.stackable)
            {
                foreach (var slot in slots)
                {
                    if (slot.itemId != itemId || slot.quantity >= definition.maxStack) continue;
                    var moved = Math.Min(remaining, definition.maxStack - slot.quantity);
                    slot.quantity += moved; remaining -= moved;
                    if (remaining == 0) break;
                }
            }
            foreach (var slot in slots)
            {
                if (remaining == 0) break;
                if (!slot.IsEmpty) continue;
                var moved = Math.Min(remaining, definition.maxStack);
                slot.itemId = itemId; slot.quantity = moved; remaining -= moved;
            }
            Notify(); return true;
        }

        public bool Remove(string itemId, int amount)
        {
            if (amount <= 0 || Count(itemId) < amount) return false;
            var remaining = amount;
            for (var i = slots.Count - 1; i >= 0 && remaining > 0; i--)
            {
                var slot = slots[i];
                if (slot.itemId != itemId) continue;
                var removed = Math.Min(remaining, slot.quantity);
                slot.quantity -= removed; remaining -= removed;
                if (slot.quantity == 0) { slot.itemId = string.Empty; }
            }
            Notify(); return true;
        }

        public void Replace(IEnumerable<InventorySlot> replacement)
        {
            foreach (var slot in slots) { slot.itemId = string.Empty; slot.quantity = 0; }
            var index = 0;
            foreach (var source in replacement)
            {
                if (index >= slots.Count) break;
                if (!source.IsEmpty && database.TryGet(source.itemId, out _))
                { slots[index].itemId = source.itemId; slots[index].quantity = source.quantity; }
                index++;
            }
            Notify();
        }

        void Notify() { if (Changed != null) Changed(); }
    }
}
