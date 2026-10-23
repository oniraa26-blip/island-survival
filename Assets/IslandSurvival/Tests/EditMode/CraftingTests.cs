using IslandSurvival.Crafting;
using IslandSurvival.Inventory;
using IslandSurvival.Items;
using NUnit.Framework;

namespace IslandSurvival.Tests
{
    public sealed class CraftingTests
    {
        [Test] public void CraftingConsumesOnlyAfterSuccess()
        {
            var inventory = new InventoryModel(new ItemDatabase(), 16); inventory.Add("stone", 2); inventory.Add("stick", 1); inventory.Add("fiber", 1);
            var system = new CraftingSystem(inventory); string message;
            Assert.IsTrue(system.TryCraft(system.Recipes.Find(r => r.id == "stone_axe"), out message));
            Assert.AreEqual(1, inventory.Count("stone_axe")); Assert.AreEqual(0, inventory.Count("stone"));
        }
        [Test] public void MissingIngredientsAreNotConsumed()
        {
            var inventory = new InventoryModel(new ItemDatabase(), 16); inventory.Add("stone", 1);
            var system = new CraftingSystem(inventory); string message;
            Assert.IsFalse(system.TryCraft(system.Recipes.Find(r => r.id == "stone_axe"), out message)); Assert.AreEqual(1, inventory.Count("stone")); StringAssert.StartsWith("Missing:", message);
        }
    }
}
