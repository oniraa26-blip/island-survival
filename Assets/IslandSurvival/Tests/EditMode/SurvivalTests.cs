using IslandSurvival.Core;
using IslandSurvival.Survival;
using NUnit.Framework;

namespace IslandSurvival.Tests
{
    public sealed class SurvivalTests
    {
        [Test] public void SprintDrainsAndRestRegeneratesStamina()
        {
            var tuning = new PrototypeTuning { staminaRegenDelay = 0f };
            var survival = new SurvivalModel(tuning); survival.Tick(1f, true); var drained = survival.Stamina;
            Assert.Less(drained, tuning.maxStamina); survival.Tick(1f, false); Assert.Greater(survival.Stamina, drained);
        }
        [Test] public void CoconutStyleRestoreIsClamped()
        {
            var tuning = new PrototypeTuning(); var survival = new SurvivalModel(tuning); survival.Tick(60f, false);
            survival.Restore(1000f, 1000f); Assert.AreEqual(tuning.maxHunger, survival.Hunger); Assert.AreEqual(tuning.maxThirst, survival.Thirst);
        }
    }
}
