using IslandSurvival.Equipment;
using IslandSurvival.Inventory;
using IslandSurvival.Items;
using NUnit.Framework;

namespace IslandSurvival.Tests
{
    public sealed class EquipmentTests
    {
        [Test] public void AxeCannotEquipUnlessOwned()
        {
            var items = new ItemDatabase(); var inventory = new InventoryModel(items); var equipment = new EquipmentModel(inventory, items);
            Assert.IsFalse(equipment.Equip("stone_axe")); Assert.AreEqual(string.Empty, equipment.MainHandItemId);
        }
        [Test] public void OwnedAxeEquipsInMainHand()
        {
            var items = new ItemDatabase(); var inventory = new InventoryModel(items); var equipment = new EquipmentModel(inventory, items);
            inventory.Add("stone_axe", 1); Assert.IsTrue(equipment.Equip("stone_axe")); Assert.AreEqual("stone_axe", equipment.MainHandItemId);
        }
    }
}
