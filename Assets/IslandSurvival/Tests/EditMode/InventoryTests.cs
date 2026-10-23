using IslandSurvival.Inventory;
using IslandSurvival.Items;
using NUnit.Framework;

namespace IslandSurvival.Tests
{
    public sealed class InventoryTests
    {
        [Test] public void CompatibleItemsStack()
        {
            var inventory = new InventoryModel(new ItemDatabase(), 16);
            Assert.IsTrue(inventory.Add("stick", 12)); Assert.IsTrue(inventory.Add("stick", 5));
            Assert.AreEqual(17, inventory.Count("stick")); Assert.AreEqual("stick", inventory.Slots[0].itemId);
        }
        [Test] public void FullInventoryRefusesWithoutLosingItems()
        {
            var inventory = new InventoryModel(new ItemDatabase(), 1);
            Assert.IsTrue(inventory.Add("stone", 20)); Assert.IsFalse(inventory.Add("stick", 1));
            Assert.AreEqual(20, inventory.Count("stone")); Assert.AreEqual(0, inventory.Count("stick"));
        }
        [Test] public void NonStackableToolsUseSeparateSlots()
        {
            var inventory = new InventoryModel(new ItemDatabase(), 2);
            Assert.IsTrue(inventory.Add("stone_axe", 2)); Assert.AreEqual(1, inventory.Slots[0].quantity); Assert.AreEqual(1, inventory.Slots[1].quantity);
        }
        [Test] public void CapacityCanExpandWithoutLosingContents()
        {
            var inventory = new InventoryModel(new ItemDatabase(), 1); inventory.Add("stone", 20);
            Assert.IsTrue(inventory.ExpandCapacity(2)); Assert.AreEqual(3, inventory.Capacity); Assert.AreEqual(20, inventory.Count("stone"));
        }
    }
}
