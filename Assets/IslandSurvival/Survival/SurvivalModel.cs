using System;
using IslandSurvival.Core;

namespace IslandSurvival.Survival
{
    [Serializable]
    public sealed class SurvivalSnapshot
    { public float health, hunger, thirst, stamina; }

    public sealed class SurvivalModel
    {
        readonly PrototypeTuning tuning;
        float regenDelay;
        public float Health { get; private set; }
        public float Hunger { get; private set; }
        public float Thirst { get; private set; }
        public float Stamina { get; private set; }
        public bool IsDead { get { return Health <= 0f; } }
        public event Action Changed;
        public event Action Died;

        public SurvivalModel(PrototypeTuning tuning)
        { this.tuning = tuning; Health = tuning.maxHealth; Hunger = tuning.maxHunger; Thirst = tuning.maxThirst; Stamina = tuning.maxStamina; }

        public void Tick(float deltaTime, bool sprinting)
        {
            Hunger = Clamp(Hunger - tuning.hungerLossPerMinute * deltaTime / 60f, tuning.maxHunger);
            Thirst = Clamp(Thirst - tuning.thirstLossPerMinute * deltaTime / 60f, tuning.maxThirst);
            if (sprinting && Stamina > 0f) { Stamina = Clamp(Stamina - tuning.sprintStaminaPerSecond * deltaTime, tuning.maxStamina); regenDelay = tuning.staminaRegenDelay; }
            else if (regenDelay > 0f) regenDelay -= deltaTime;
            else Stamina = Clamp(Stamina + tuning.staminaRegenPerSecond * deltaTime, tuning.maxStamina);
            if (Hunger <= 0f || Thirst <= 0f) Damage(tuning.criticalDamagePerSecond * deltaTime);
            Notify();
        }

        public void Restore(float hunger, float thirst)
        { Hunger = Clamp(Hunger + hunger, tuning.maxHunger); Thirst = Clamp(Thirst + thirst, tuning.maxThirst); Notify(); }
        public void Damage(float amount)
        {
            if (IsDead) return;
            Health = Clamp(Health - amount, tuning.maxHealth); Notify();
            if (IsDead && Died != null) Died();
        }
        public void Revive() { Health = tuning.maxHealth; Hunger = Math.Max(Hunger, 25f); Thirst = Math.Max(Thirst, 25f); Stamina = tuning.maxStamina; Notify(); }
        public SurvivalSnapshot Snapshot() { return new SurvivalSnapshot { health = Health, hunger = Hunger, thirst = Thirst, stamina = Stamina }; }
        public void RestoreSnapshot(SurvivalSnapshot data)
        { Health = Clamp(data.health, tuning.maxHealth); Hunger = Clamp(data.hunger, tuning.maxHunger); Thirst = Clamp(data.thirst, tuning.maxThirst); Stamina = Clamp(data.stamina, tuning.maxStamina); Notify(); }
        static float Clamp(float value, float max) { return Math.Max(0f, Math.Min(max, value)); }
        void Notify() { if (Changed != null) Changed(); }
    }
}
