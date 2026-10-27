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

        [TestCase(0f, 1f, ScreenFacing.Back)]
        [TestCase(0f, -1f, ScreenFacing.Front)]
        [TestCase(-1f, 0f, ScreenFacing.Left)]
        [TestCase(1f, 0f, ScreenFacing.Right)]
        [TestCase(-1f, 1f, ScreenFacing.BackLeft)]
        [TestCase(1f, 1f, ScreenFacing.BackRight)]
        [TestCase(-1f, -1f, ScreenFacing.FrontLeft)]
        [TestCase(1f, -1f, ScreenFacing.FrontRight)]
        public void CameraRelativeMovementResolvesScreenFacingSemantics(float screenX, float screenY, ScreenFacing expected)
        {
            var camera = new GameObject("IsometricCamera");
            try
            {
                camera.transform.rotation = Quaternion.Euler(40f, 45f, 0f);
                var right = camera.transform.right; right.y = 0f; right.Normalize();
                var up = camera.transform.forward; up.y = 0f; up.Normalize();
                var worldMovement = right * screenX + up * screenY;
                Vector2 screenMovement;
                Assert.AreEqual(expected, SpriteDirectionResolver.FromWorldMovement(worldMovement, camera.transform, ScreenFacing.Front, out screenMovement));
                Assert.AreEqual(screenX, screenMovement.x, 0.001f, "Horizontal movement must round-trip into screen space.");
                Assert.AreEqual(screenY, screenMovement.y, 0.001f, "Vertical movement must round-trip into screen space.");
            }
            finally { Object.DestroyImmediate(camera); }
        }

        [Test]
        public void ZeroMovementRetainsLastSemanticFacing()
        {
            Assert.AreEqual(ScreenFacing.BackLeft,
                SpriteDirectionResolver.FromScreenMovement(Vector2.zero, ScreenFacing.BackLeft));
        }

        [Test]
        public void VerifiedAtlasSlotsDoNotReverseFrontAndBack()
        {
            Assert.AreEqual(SpriteDirection.N, SpriteDirectionResolver.AtlasSlot(ScreenFacing.Front), "Atlas slot N visibly contains the front-facing explorer.");
            Assert.AreEqual(SpriteDirection.S, SpriteDirectionResolver.AtlasSlot(ScreenFacing.Back), "Atlas slot S visibly contains the back-facing explorer.");
            Assert.AreEqual(SpriteDirection.NW, SpriteDirectionResolver.AtlasSlot(ScreenFacing.FrontLeft));
            Assert.AreEqual(SpriteDirection.SW, SpriteDirectionResolver.AtlasSlot(ScreenFacing.BackLeft));

            Sprite frontFrame;
            Sprite backFrame;
            Assert.IsTrue(TemporaryArtLibrary.TryGetWalk(SpriteDirectionResolver.AtlasSlot(ScreenFacing.Front), 0, out frontFrame));
            Assert.IsTrue(TemporaryArtLibrary.TryGetWalk(SpriteDirectionResolver.AtlasSlot(ScreenFacing.Back), 0, out backFrame));
            Assert.AreEqual("TEMP_PlayerWalk_N_0", frontFrame.name);
            Assert.AreEqual("TEMP_PlayerWalk_S_0", backFrame.name);
            Assert.AreNotEqual(frontFrame.rect.y, backFrame.rect.y, "Verified front and back frames must come from different atlas rows.");
        }

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
