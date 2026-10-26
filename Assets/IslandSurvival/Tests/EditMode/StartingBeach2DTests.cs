using System.Collections.Generic;
using IslandSurvival.Presentation;
using IslandSurvival.Platform;
using IslandSurvival.Visual;
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
                SetField(directions, "sprites", new List<DirectionalSprite> { new DirectionalSprite { direction = SpriteDirection.S, sprite = sprite } });
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
    }
}
