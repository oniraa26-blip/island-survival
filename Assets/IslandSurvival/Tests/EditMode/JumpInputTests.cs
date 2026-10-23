using IslandSurvival.InputSystem;
using NUnit.Framework;
using UnityEngine;

namespace IslandSurvival.Tests
{
    public sealed class JumpInputTests
    {
        [Test]
        public void JumpRequestIsConsumedExactlyOnce()
        {
            var gameObject = new GameObject("JumpInputTest");
            try
            {
                var input = gameObject.AddComponent<MobileInput>();

                input.RequestJump();

                Assert.That(input.ConsumeJump(), Is.True);
                Assert.That(input.ConsumeJump(), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
