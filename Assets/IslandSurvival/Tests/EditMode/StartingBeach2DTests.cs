using System.Collections.Generic;
using IslandSurvival.Core;
using IslandSurvival.Presentation;
using IslandSurvival.Platform;
using IslandSurvival.Visual;
using IslandSurvival.World;
using NUnit.Framework;
using UnityEngine;

namespace IslandSurvival.Tests
{
    public sealed class StartingBeach2DTests
    {
        [Test]
        public void TemporaryCatalogResolvesDistinctPalmVariants()
        {
            Sprite a, b, c;
            Assert.IsTrue(TemporaryArtLibrary.TryGet(VisualAssetKey.PalmA, SpriteDirection.SE, out a));
            Assert.IsTrue(TemporaryArtLibrary.TryGet(VisualAssetKey.PalmB, SpriteDirection.SE, out b));
            Assert.IsTrue(TemporaryArtLibrary.TryGet(VisualAssetKey.PalmC, SpriteDirection.SE, out c));
            Assert.AreNotEqual(a.rect, b.rect); Assert.AreNotEqual(b.rect, c.rect);
        }

        [Test]
        public void ExplorerResolvesEveryDirection()
        {
            foreach (SpriteDirection direction in System.Enum.GetValues(typeof(SpriteDirection)))
            {
                Sprite sprite;
                Assert.IsTrue(TemporaryArtLibrary.TryGet(VisualAssetKey.PlayerCharacter, direction, out sprite), direction.ToString());
                Assert.IsNotNull(sprite);
            }
        }

        [Test]
        public void ExplorerWalkHasTwoDistinctFramesPerDirection()
        {
            foreach (SpriteDirection direction in System.Enum.GetValues(typeof(SpriteDirection)))
            {
                Sprite first, second;
                Assert.IsTrue(TemporaryArtLibrary.TryGetWalk(direction, 0, out first), direction.ToString());
                Assert.IsTrue(TemporaryArtLibrary.TryGetWalk(direction, 1, out second), direction.ToString());
                Assert.AreNotEqual(first.rect, second.rect);
            }
        }

        [Test]
        public void HouseSkinSwitchChangesPresentationOnly()
        {
            Sprite defaultSkin, tropicalSkin;
            Assert.IsTrue(TemporaryArtLibrary.TryGet(VisualAssetKey.HouseLevel1Default, SpriteDirection.NE, out defaultSkin));
            Assert.IsTrue(TemporaryArtLibrary.TryGet(VisualAssetKey.HouseLevel1Tropical, SpriteDirection.NE, out tropicalSkin));
            Assert.AreNotEqual(defaultSkin.textureRect, tropicalSkin.textureRect);
        }

        [Test]
        public void UnknownKeyLeavesExistingFallbackPathAvailable()
        {
            Sprite sprite;
            Assert.IsFalse(TemporaryArtLibrary.TryGet(VisualAssetKey.None, SpriteDirection.S, out sprite));
            Assert.IsNull(sprite);
        }

        [Test]
        public void AuthoredCatalogSpriteSubstitutesTemporaryFallback()
        {
            var texture = new Texture2D(4, 4); var sprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0f));
            var definition = ScriptableObject.CreateInstance<SpriteVisualDefinition>();
            var catalog = ScriptableObject.CreateInstance<VisualAssetCatalog>(); var parent = new GameObject("Catalog Test Root");
            try
            {
                var directions = new SpriteDirectionSet();
                SetField(directions, "sprites", new List<DirectionalSprite> { new DirectionalSprite { direction = SpriteDirection.N, sprite = sprite } });
                SetField(definition, "assetKey", VisualAssetKey.PlayerCharacter); SetField(definition, "idle", directions); SetField(definition, "worldSize", new Vector2(1f, 2f));
                SetField(catalog, "spriteVisuals", new List<SpriteVisualDefinition> { definition });
                var fallbackCalled = false;
                var visual = new VisualAssetResolver(catalog, PlatformProfile.WebMobile()).CreatePresentation(VisualAssetKey.PlayerCharacter, parent.transform, "player", p =>
                { fallbackCalled = true; return new GameObject("Unexpected Fallback"); });
                Assert.IsFalse(fallbackCalled); Assert.IsNotNull(visual.GetComponent<SpriteWorldVisual>());
                Assert.IsFalse(visual.GetComponent<VisualPrefabInstance>().IsProceduralFallback);
            }
            finally
            {
                Object.DestroyImmediate(parent); Object.DestroyImmediate(catalog); Object.DestroyImmediate(definition);
                Object.DestroyImmediate(sprite); Object.DestroyImmediate(texture);
            }
        }

        static void SetField(object target, string name, object value)
        { target.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).SetValue(target, value); }

        [Test]
        public void SortingIsStableForSameInputs()
        {
            var position = new Vector3(12.5f, 0f, 31.25f); var forward = new Vector3(1f, -1f, 1f).normalized;
            Assert.AreEqual(SpriteWorldSorting.Calculate(position, forward, SpriteSortCategory.WorldObject),
                SpriteWorldSorting.Calculate(position, forward, SpriteSortCategory.WorldObject));
        }

        [Test]
        public void PlayerAndTreeSortFromGroundContactInFrontAndBehind()
        {
            var forward = new Vector3(1f, 0f, 1f).normalized;
            var tree = new Vector3(5f, 0f, 5f);
            var playerBehind = new Vector3(9f, 0f, 9f);
            var playerInFront = Vector3.zero;
            var treeOrder = SpriteWorldSorting.Calculate(tree, forward, SpriteSortCategory.WorldObject);
            Assert.Less(SpriteWorldSorting.Calculate(playerBehind, forward, SpriteSortCategory.WorldObject), treeOrder);
            Assert.Greater(SpriteWorldSorting.Calculate(playerInFront, forward, SpriteSortCategory.WorldObject), treeOrder);
        }

        [Test]
        public void BesideTreeSortingHasDeterministicLateralTieBreak()
        {
            var forward = new Vector3(1f, 0f, 1f).normalized;
            var treeOrder = SpriteWorldSorting.Calculate(new Vector3(5f, 0f, 5f), forward, SpriteSortCategory.WorldObject, 0);
            var besideOrder = SpriteWorldSorting.Calculate(new Vector3(6f, 0f, 4f), forward, SpriteSortCategory.WorldObject, 1);
            Assert.AreNotEqual(treeOrder, besideOrder);
            Assert.AreEqual(besideOrder, SpriteWorldSorting.Calculate(new Vector3(6f, 0f, 4f), forward, SpriteSortCategory.WorldObject, 1));
        }

        [Test]
        public void RequiredVisualsUseExplicitGroundAnchorRoles()
        {
            Assert.AreEqual(GroundAnchorRole.PlayerFeet, SpriteGroundAnchor.RoleFor(VisualAssetKey.PlayerCharacter));
            Assert.AreEqual(GroundAnchorRole.PalmBase, SpriteGroundAnchor.RoleFor(VisualAssetKey.ENV_PALM_A));
            Assert.AreEqual(GroundAnchorRole.TreeBase, SpriteGroundAnchor.RoleFor(VisualAssetKey.ENV_TREE_A));
            Assert.AreEqual(GroundAnchorRole.BuildingBase, SpriteGroundAnchor.RoleFor(VisualAssetKey.HouseLevel1Default));
            Assert.AreEqual(GroundAnchorRole.ShipwreckContact, SpriteGroundAnchor.RoleFor(VisualAssetKey.ENV_SHIPWRECK));
        }

        [TestCase(VisualAssetKey.ENV_PALM_A, GroundAnchorRole.PalmBase)]
        [TestCase(VisualAssetKey.ENV_TREE_A, GroundAnchorRole.TreeBase)]
        [TestCase(VisualAssetKey.HouseLevel1Default, GroundAnchorRole.BuildingBase)]
        [TestCase(VisualAssetKey.ENV_SHIPWRECK, GroundAnchorRole.ShipwreckContact)]
        public void GroundAnchorUsesAuthoredContactTransform(VisualAssetKey key, GroundAnchorRole expectedRole)
        {
            var obj = new GameObject("Ground Contact"); obj.transform.position = new Vector3(3f, 1.25f, 7f);
            try
            {
                var anchor = obj.AddComponent<SpriteGroundAnchor>(); anchor.Initialize(key);
                Assert.AreEqual(expectedRole, anchor.Role); Assert.AreEqual(obj.transform.position, anchor.WorldPosition);
            }
            finally { Object.DestroyImmediate(obj); }
        }

        [Test]
        public void PresentationScaleProfileKeepsNormalVegetationWithinLockedLimits()
        {
            var profile = PresentationScaleProfile.CreateDefault();
            try
            {
                AssertScaleWithin(profile, VisualAssetKey.ENV_PALM_A, 2f, 2.8f);
                AssertScaleWithin(profile, VisualAssetKey.ENV_TREE_A, 2f, 3f);
                AssertScaleWithin(profile, VisualAssetKey.ENV_BUSH_A, 0.35f, 0.65f);
                AssertScaleWithin(profile, VisualAssetKey.ENV_GRASS_A, 0.15f, 0.35f);
                AssertScaleWithin(profile, VisualAssetKey.ENV_ROCK_SMALL_A, 0.25f, 0.45f);
                AssertScaleWithin(profile, VisualAssetKey.ENV_ROCK_MEDIUM_A, 0.5f, 0.8f);
                AssertScaleWithin(profile, VisualAssetKey.HouseLevel1Default, 2.2f, 3.2f);
                AssertScaleWithin(profile, VisualAssetKey.ENV_SHIPWRECK, 2.5f, 4f);
            }
            finally { Object.DestroyImmediate(profile); }
        }

        [Test]
        public void StartingBeachObstacleClustersPreserveSpawnClearing()
        {
            foreach (var palm in StartingBeachComposition.PalmPositions) Assert.IsTrue(StartingBeachComposition.IsSpawnClear(palm, 0.6f), palm.ToString());
            foreach (var rock in StartingBeachComposition.RockClusterCenters) Assert.IsTrue(StartingBeachComposition.IsSpawnClear(rock, 1.4f), rock.ToString());
            Assert.IsTrue(StartingBeachComposition.IsSpawnClear(StartingBeachComposition.Shipwreck, 3f));
        }

        [TestCase(VisualAssetKey.ENV_PALM_A)]
        [TestCase(VisualAssetKey.ENV_TREE_A)]
        [TestCase(VisualAssetKey.HouseLevel1Default)]
        [TestCase(VisualAssetKey.ENV_ROCK_MEDIUM_A)]
        [TestCase(VisualAssetKey.ENV_ROCK_LARGE_A)]
        public void ObstacleColliderIsAValidBaseFootprint(VisualAssetKey key)
        {
            var profile = PresentationScaleProfile.CreateDefault();
            try { Assert.IsTrue(PresentationFootprintPolicy.IsBaseOnly(key, PresentationFootprintPolicy.For(key), profile.WorldSize(key)), key.ToString()); }
            finally { Object.DestroyImmediate(profile); }
        }

        [Test]
        public void ProductionEnvironmentSpriteOverridesTemporarySlot()
        {
            var texture = new Texture2D(4, 4); var sprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0f));
            var definition = ScriptableObject.CreateInstance<SpriteVisualDefinition>(); var catalog = ScriptableObject.CreateInstance<VisualAssetCatalog>(); var parent = new GameObject("Production Palm Slot");
            try
            {
                var directions = new SpriteDirectionSet();
                SetField(directions, "sprites", new List<DirectionalSprite> { new DirectionalSprite { direction = SpriteDirection.S, sprite = sprite } });
                SetField(definition, "assetKey", VisualAssetKey.ENV_PALM_A); SetField(definition, "idle", directions); SetField(definition, "worldSize", new Vector2(3f, 4.75f));
                SetField(catalog, "spriteVisuals", new List<SpriteVisualDefinition> { definition });
                var fallbackCalled = false;
                var visual = new VisualAssetResolver(catalog, PlatformProfile.WebMobile()).CreatePresentation(VisualAssetKey.ENV_PALM_A, parent.transform, "palm_slot", p => { fallbackCalled = true; return null; });
                Assert.IsNotNull(visual); Assert.IsFalse(fallbackCalled); Assert.AreSame(sprite, visual.GetComponent<SpriteRenderer>().sprite);
            }
            finally { Object.DestroyImmediate(parent); Object.DestroyImmediate(catalog); Object.DestroyImmediate(definition); Object.DestroyImmediate(sprite); Object.DestroyImmediate(texture); }
        }

        static void AssertScaleWithin(PresentationScaleProfile profile, VisualAssetKey key, float min, float max)
        {
            var ratio = profile.WorldSize(key).y / PresentationScaleProfile.PlayerWorldHeight;
            Assert.That(ratio, Is.InRange(min, max), key.ToString());
        }

        [Test]
        public void InitialViewportIncludesStartingShoreline()
        {
            var tuning = new PrototypeTuning();
            Assert.GreaterOrEqual(tuning.isometricSize, 10f);
            Assert.Less(Mathf.Abs(StylizedGeometry.ShoreZ(0f)), tuning.isometricSize);
        }

        [Test]
        public void SpriteWorldSizeCompensatesForScaledFallbackParent()
        {
            var parent = new GameObject("Scaled Gameplay Visual"); var child = new GameObject("Sprite Visual");
            var texture = new Texture2D(100, 100); var sprite = Sprite.Create(texture, new Rect(0f, 0f, 100f, 100f), new Vector2(0.5f, 0f), 100f);
            try
            {
                parent.transform.localScale = new Vector3(0.25f, 2f, 0.5f); child.transform.SetParent(parent.transform, false);
                child.AddComponent<SpriteWorldVisual>().Initialize(sprite, new Vector2(2f, 3f), SpriteSortCategory.WorldObject, false);
                var spriteSize = sprite.bounds.size;
                var worldSize = Vector2.Scale(new Vector2(spriteSize.x, spriteSize.y), new Vector2(child.transform.lossyScale.x, child.transform.lossyScale.y));
                Assert.AreEqual(2f, worldSize.x, 0.001f); Assert.AreEqual(3f, worldSize.y, 0.001f);
            }
            finally { Object.DestroyImmediate(parent); Object.DestroyImmediate(sprite); Object.DestroyImmediate(texture); }
        }
    }
}
