using IslandSurvival.Exploration;
using IslandSurvival.UI;
using IslandSurvival.World;
using NUnit.Framework;
using UnityEngine;

namespace IslandSurvival.Tests
{
    public sealed class WorldMapTests
    {
        WorldMapDefinition definition;
        DiscoverySystem discovery;

        [SetUp] public void SetUp() { discovery = new DiscoverySystem(); definition = WorldMapDefinition.CreateOriginPlaceholder(); }
        [TearDown] public void TearDown() { Object.DestroyImmediate(definition); }

        [Test]
        public void OriginDefinitionContainsExistingEightZones()
        {
            Assert.AreEqual(8, definition.Regions.Count);
            CollectionAssert.AreEquivalent(OriginDemoZones.All, RegionIds());
        }

        [Test]
        public void WorldCoordinatesConvertToNormalizedNorthUpMap()
        {
            var center = definition.WorldToNormalized(new Vector3(0f, 0f, 150f), false);
            Assert.AreEqual(0.5f, center.x, 0.001f);
            Assert.Greater(center.y, 0.45f);
            Assert.Greater(definition.WorldToNormalized(new Vector3(0f, 0f, 300f)).y, definition.WorldToNormalized(Vector3.zero).y);
        }

        [Test]
        public void MapCoordinatesClampOutsideIslandBounds()
        {
            Assert.AreEqual(Vector2.zero, definition.WorldToNormalized(new Vector3(-999f, 0f, -999f)));
            Assert.AreEqual(Vector2.one, definition.WorldToNormalized(new Vector3(999f, 0f, 999f)));
        }

        [Test]
        public void StartingBeachVisibleAndOtherRegionRequiresDiscovery()
        {
            Assert.IsTrue(definition.IsRegionVisible(Find(OriginDemoZones.StartingBeach), discovery));
            var lagoon = Find(OriginDemoZones.BlueLagoon); Assert.IsFalse(definition.IsRegionVisible(lagoon, discovery));
            discovery.Discover(OriginDemoZones.BlueLagoon); Assert.IsTrue(definition.IsRegionVisible(lagoon, discovery));
        }

        [Test]
        public void SharedRegistryConvertsPlayerAndGatesLandmarkDiscovery()
        {
            var player = new GameObject("Player"); player.transform.position = new Vector3(13f, 0f, 11f);
            try
            {
                var registry = new MinimapMarkerRegistry(); var marker = registry.RegisterLocalPlayer(player.transform);
                var normalized = definition.WorldToNormalized(marker.Position);
                Assert.That(normalized.x, Is.InRange(0f, 1f)); Assert.That(normalized.y, Is.InRange(0f, 1f));
                registry.RegisterLandmark(OriginDemoZones.SeaCave, new Vector3(158f, 0f, 58f));
                Assert.IsFalse(discovery.IsDiscovered(registry.Markers[1].RequiredDiscoveryId));
            }
            finally { Object.DestroyImmediate(player); }
        }

        string[] RegionIds() { var ids = new string[definition.Regions.Count]; for (var i = 0; i < ids.Length; i++) ids[i] = definition.Regions[i].zoneId; return ids; }
        WorldMapRegionDefinition Find(string id) { for (var i = 0; i < definition.Regions.Count; i++) if (definition.Regions[i].zoneId == id) return definition.Regions[i]; return null; }
    }
}
