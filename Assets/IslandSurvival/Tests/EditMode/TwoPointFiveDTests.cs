using System.Collections.Generic;
using IslandSurvival.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace IslandSurvival.Tests
{
    public sealed class TwoPointFiveDTests
    {
        [TestCase(0f, 1f, SpriteDirection.N)]
        [TestCase(1f, 1f, SpriteDirection.NE)]
        [TestCase(1f, 0f, SpriteDirection.E)]
        [TestCase(1f, -1f, SpriteDirection.SE)]
        [TestCase(0f, -1f, SpriteDirection.S)]
        [TestCase(-1f, -1f, SpriteDirection.SW)]
        [TestCase(-1f, 0f, SpriteDirection.W)]
        [TestCase(-1f, 1f, SpriteDirection.NW)]
        public void DirectionResolverCoversEightDirections(float x, float y, SpriteDirection expected)
        { Assert.AreEqual(expected, SpriteDirectionResolver.FromMovement(new Vector2(x, y))); }

        [Test]
        public void SortingPlacesNearObjectInFrontOfFarObject()
        {
            var cameraForward = new Vector3(1f, -1f, 1f).normalized;
            var near = SpriteWorldSorting.Calculate(Vector3.zero, cameraForward, SpriteSortCategory.WorldObject);
            var far = SpriteWorldSorting.Calculate(new Vector3(5f, 0f, 5f), cameraForward, SpriteSortCategory.WorldObject);
            Assert.Greater(near, far);
        }

        [Test]
        public void BuildingLookupSelectsLevelSkinAndDirectionWithoutGameplayState()
        {
            var texture = new Texture2D(2, 2);
            var sprite = Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), Vector2.zero);
            try
            {
                var directions = new SpriteDirectionSet();
                var entriesField = typeof(SpriteDirectionSet).GetField("sprites", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                entriesField.SetValue(directions, new List<DirectionalSprite> { new DirectionalSprite { direction = SpriteDirection.NE, sprite = sprite } });
                var levels = new List<BuildingLevelVisual>
                {
                    new BuildingLevelVisual { level = 1, skins = new List<BuildingSkinVisual> { new BuildingSkinVisual { skinId = "tropical", directions = directions } } }
                };
                Sprite result;
                Assert.IsTrue(BuildingVisualLookup.TryGet(levels, 1, "tropical", SpriteDirection.NE, out result));
                Assert.AreSame(sprite, result);
                Assert.IsFalse(BuildingVisualLookup.TryGet(levels, 2, "tropical", SpriteDirection.NE, out result));
            }
            finally { UnityEngine.Object.DestroyImmediate(sprite); UnityEngine.Object.DestroyImmediate(texture); }
        }
    }
}
