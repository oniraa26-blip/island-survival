using IslandSurvival.UI;
using NUnit.Framework;
using UnityEngine;

namespace IslandSurvival.Tests
{
    public sealed class MinimapTests
    {
        [TestCase(0f, 0f)]
        [TestCase(90f, -90f)]
        [TestCase(180f, -180f)]
        [TestCase(270f, 90f)]
        public void PlayerArrowUsesNorthUpWorldFacing(float worldYaw, float expectedRotation)
        {
            var forward = Quaternion.Euler(0f, worldYaw, 0f) * Vector3.forward;
            Assert.AreEqual(expectedRotation, MinimapProjection.PlayerArrowRotation(forward), 0.001f);
        }

        [Test]
        public void RegistryAllowsOneLocalAndThreeTeammates()
        {
            var registry = new MinimapMarkerRegistry(); var player = new GameObject("Player");
            try
            {
                Assert.IsNotNull(registry.RegisterLocalPlayer(player.transform));
                Assert.IsNull(registry.RegisterLocalPlayer(player.transform));
                Assert.IsNotNull(registry.RegisterTeammate("1", "One", Color.cyan, null));
                Assert.IsNotNull(registry.RegisterTeammate("2", "Two", Color.green, null));
                Assert.IsNotNull(registry.RegisterTeammate("3", "Three", Color.magenta, null));
                Assert.IsNull(registry.RegisterTeammate("4", "Four", Color.white, null));
            }
            finally { Object.DestroyImmediate(player); }
        }

        [Test]
        public void ProjectionIsNorthUpAndReportsOutsideRange()
        {
            bool outside;
            var north = MinimapProjection.Project(new Vector3(0f, 0f, 10f), Vector3.zero, 20f, out outside);
            Assert.AreEqual(new Vector2(0f, 0.5f), north); Assert.IsFalse(outside);
            var eastFar = MinimapProjection.Project(new Vector3(30f, 0f, 0f), Vector3.zero, 20f, out outside);
            Assert.AreEqual(new Vector2(1.5f, 0f), eastFar); Assert.IsTrue(outside);
        }

        [Test]
        public void LandmarkMarkerRequiresItsDiscoveryId()
        {
            var registry = new MinimapMarkerRegistry();
            Assert.IsTrue(registry.RegisterLandmark("starting_beach", new Vector3(13f, 0f, 11f)));
            Assert.AreEqual("starting_beach", registry.Markers[0].RequiredDiscoveryId);
            Assert.AreEqual(MinimapMarkerCategory.Landmark, registry.Markers[0].Category);
        }

        [Test]
        public void EveryMarkerCategoryHasAnIconDefinition()
        {
            foreach (MinimapMarkerCategory category in System.Enum.GetValues(typeof(MinimapMarkerCategory)))
                Assert.AreEqual(category, MinimapIconLibrary.Get(category).category);
        }
    }
}
