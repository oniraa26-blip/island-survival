using IslandSurvival.Crafting;
using IslandSurvival.Inventory;
using IslandSurvival.Items;
using IslandSurvival.Localization;
using NUnit.Framework;

namespace IslandSurvival.Tests
{
    public sealed class CraftingTests
    {
        static LocalizationService English()
        {
            var localization = LocalizationService.LoadDefault();
            localization.SetLocale(LocalizationService.EnglishLocale, false);
            return localization;
        }

        [Test] public void CraftingConsumesOnlyAfterSuccess()
        {
            var items = new ItemDatabase(); var inventory = new InventoryModel(items, 16); inventory.Add("stone", 2); inventory.Add("stick", 1); inventory.Add("fiber", 1);
            var system = new CraftingSystem(inventory, items, English()); string message;
            Assert.IsTrue(system.TryCraft(system.Recipes.Find(r => r.id == "stone_axe"), out message));
            Assert.AreEqual(1, inventory.Count("stone_axe")); Assert.AreEqual(0, inventory.Count("stone"));
        }
        [Test] public void MissingIngredientsAreNotConsumed()
        {
            var items = new ItemDatabase(); var inventory = new InventoryModel(items, 16); inventory.Add("stone", 1);
            var system = new CraftingSystem(inventory, items, English()); string message;
            Assert.IsFalse(system.TryCraft(system.Recipes.Find(r => r.id == "stone_axe"), out message)); Assert.AreEqual(1, inventory.Count("stone")); StringAssert.StartsWith("Missing:", message);
        }
    }
}
