using System.Collections.Generic;
using IslandSurvival.Exploration;
using IslandSurvival.Save;
using IslandSurvival.World;
using NUnit.Framework;

namespace IslandSurvival.Tests
{
    public sealed class DiscoveryTests
    {
        [Test]
        public void DiscoveryIsUniqueAndRestoresPersistentState()
        {
            var discoveries = new DiscoverySystem();
            var notifications = 0;
            discoveries.Discovered += _ => notifications++;

            Assert.IsTrue(discoveries.Discover(OriginDemoZones.BlueLagoon));
            Assert.IsFalse(discoveries.Discover(OriginDemoZones.BlueLagoon));
            Assert.AreEqual(1, notifications);
            Assert.IsTrue(discoveries.Collect("sea_cave_blue_crystal"));

            var restored = new DiscoverySystem();
            restored.Restore(new List<string>(discoveries.DiscoveredIds), new List<string>(discoveries.CollectedIds));
            Assert.IsTrue(restored.IsDiscovered(OriginDemoZones.BlueLagoon));
            Assert.IsTrue(restored.IsCollected("sea_cave_blue_crystal"));
        }

        [Test]
        public void SaveMigrationAcceptsVersionOneAndTwoOnly()
        {
            Assert.IsTrue(SaveSystem.IsSupportedVersion(1));
            Assert.IsTrue(SaveSystem.IsSupportedVersion(2));
            Assert.IsFalse(SaveSystem.IsSupportedVersion(3));
        }

        [Test]
        public void RequiredZoneIdsAreUnique()
        {
            var unique = new HashSet<string>(OriginDemoZones.All);
            Assert.AreEqual(8, OriginDemoZones.All.Length);
            Assert.AreEqual(OriginDemoZones.All.Length, unique.Count);
        }
    }
}
