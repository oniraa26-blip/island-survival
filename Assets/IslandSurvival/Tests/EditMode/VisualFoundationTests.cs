using IslandSurvival.Platform;
using IslandSurvival.Presentation;
using IslandSurvival.Visual;
using IslandSurvival.World;
using NUnit.Framework;
using UnityEngine;

namespace IslandSurvival.Tests
{
    public sealed class VisualFoundationTests
    {
        [Test]
        public void ResolverUsesMarkedProceduralFallbackWhenCatalogIsMissing()
        {
            var parent = new GameObject("GameplayRoot");
            try
            {
                var resolver = new VisualAssetResolver(null, PlatformProfile.WebMobile());
                var visual = resolver.CreatePresentation(VisualAssetKey.PlayerCharacter, parent.transform, "player", transform =>
                {
                    var fallback = new GameObject("FallbackVisual"); fallback.transform.SetParent(transform, false); return fallback;
                });
                Assert.IsNotNull(visual);
                Assert.AreEqual(parent.transform, visual.transform.parent);
                var marker = visual.GetComponent<VisualPrefabInstance>();
                Assert.IsNotNull(marker);
                Assert.IsTrue(marker.IsProceduralFallback);
                Assert.AreEqual("player", marker.GameplayIdentity);
                Assert.AreEqual(VisualAssetKey.PlayerCharacter, marker.AssetKey);
            }
            finally { Object.DestroyImmediate(parent); }
        }

        [Test]
        public void TerrainCarriesNormalizedFourWayBlendMasks()
        {
            var terrain = StylizedGeometry.Terrain();
            Assert.AreEqual(terrain.vertexCount, terrain.colors.Length);
            foreach (var color in terrain.colors)
                Assert.AreEqual(1f, color.r + color.g + color.b + color.a, 0.002f);
        }

        [Test]
        public void VisualFoundationShadersAreResourceAddressable()
        {
            Assert.IsNotNull(Resources.Load<Shader>("Visual/Shaders/StylizedSurface"));
            Assert.IsNotNull(Resources.Load<Shader>("Visual/Shaders/StylizedTerrain"));
            Assert.IsNotNull(Resources.Load<Shader>("Visual/Shaders/TropicalWater"));
        }

        [Test]
        public void TerrainAndWaterExposeReplaceablePresentationLayers()
        {
            CollectionAssert.AreEquivalent(new[]
            {
                "WATER_DEEP", "WATER_SHALLOW", "WATER_FOAM",
                "GROUND_WET_SAND", "GROUND_DRY_SAND", "GROUND_DIRT", "GROUND_GRASS",
                "TRANSITION_SAND_GRASS", "TRANSITION_DIRT_GRASS"
            }, System.Enum.GetNames(typeof(PresentationLayerKey)));
        }
    }
}
