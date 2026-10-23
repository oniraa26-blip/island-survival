using IslandSurvival.Core;
using IslandSurvival.InputSystem;
using IslandSurvival.Survival;
using UnityEngine;

namespace IslandSurvival.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        MobileInput input;
        CharacterController controller;
        Transform cameraTransform;
        PrototypeTuning tuning;
        SurvivalModel survival;
        float verticalVelocity;
        public bool IsSprinting { get; private set; }

        public void Initialize(MobileInput input, UnityEngine.Camera camera, PrototypeTuning tuning, SurvivalModel survival)
        { this.input = input; cameraTransform = camera.transform; this.tuning = tuning; this.survival = survival; controller = GetComponent<CharacterController>(); }

        void Update()
        {
            if (input == null || survival.IsDead) return;
            var forward = cameraTransform.forward; forward.y = 0f; forward.Normalize();
            var right = cameraTransform.right; right.y = 0f; right.Normalize();
            var desired = forward * input.Move.y + right * input.Move.x;
            desired = Vector3.ClampMagnitude(desired, 1f);
            IsSprinting = input.SprintHeld && desired.sqrMagnitude > 0.1f && survival.Stamina > 0f;
            var speed = IsSprinting ? tuning.sprintSpeed : tuning.walkSpeed;
            if (desired.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desired), tuning.turnSpeed * Time.deltaTime);
            if (controller.isGrounded && verticalVelocity < 0f) verticalVelocity = -2f;
            verticalVelocity += tuning.gravity * Time.deltaTime;
            controller.Move((desired * speed + Vector3.up * verticalVelocity) * Time.deltaTime);
            survival.Tick(Time.deltaTime, IsSprinting);
        }

        public void Teleport(Vector3 position, Quaternion rotation)
        { controller.enabled = false; transform.SetPositionAndRotation(position, rotation); controller.enabled = true; verticalVelocity = 0f; }
    }
}
