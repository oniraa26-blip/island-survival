using System;
using UnityEngine;

namespace IslandSurvival.Core
{
    [Serializable]
    public sealed class PrototypeTuning
    {
        [Header("Movement")]
        public float walkSpeed = 3.5f;
        public float sprintSpeed = 6f;
        public float turnSpeed = 14f;
        [Min(0.1f)] public float jumpHeight = 1.25f;
        public float gravity = -24f;
        [Range(0f, 1f)] public float airControl = 0.35f;
        [Min(0.1f)] public float groundedStickForce = 2f;
        [Header("Camera")]
        public float cameraDistance = 6f;
        public float cameraSensitivity = 0.16f;
        public float minPitch = 12f;
        public float maxPitch = 65f;
        [Header("Interaction")]
        public float interactionRadius = 3f;
        [Header("Survival")]
        public float maxHealth = 100f;
        public float maxHunger = 100f;
        public float maxThirst = 100f;
        public float maxStamina = 100f;
        public float hungerLossPerMinute = 1.2f;
        public float thirstLossPerMinute = 1.8f;
        public float sprintStaminaPerSecond = 14f;
        public float staminaRegenPerSecond = 20f;
        public float staminaRegenDelay = 1.25f;
        public float criticalDamagePerSecond = 4f;
        [Header("World")]
        public float dayLengthSeconds = 360f;
        public int treeHitCount = 3;
    }
}
