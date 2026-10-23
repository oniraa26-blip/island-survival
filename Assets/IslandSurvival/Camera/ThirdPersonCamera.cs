using IslandSurvival.Core;
using IslandSurvival.InputSystem;
using UnityEngine;

namespace IslandSurvival.CameraSystem
{
    public sealed class ThirdPersonCamera : MonoBehaviour
    {
        Transform target;
        MobileInput input;
        PrototypeTuning tuning;
        float yaw = 25f, pitch = 28f;
        Vector3 velocity;
        public void Initialize(Transform target, MobileInput input, PrototypeTuning tuning)
        { this.target = target; this.input = input; this.tuning = tuning; }
        void LateUpdate()
        {
            if (target == null) return;
            yaw += input.LookDelta.x * tuning.cameraSensitivity;
            pitch = Mathf.Clamp(pitch + input.LookDelta.y * tuning.cameraSensitivity, tuning.minPitch, tuning.maxPitch);
            var pivot = target.position + Vector3.up * 1.55f;
            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            var desired = pivot - rotation * Vector3.forward * tuning.cameraDistance;
            RaycastHit hit;
            if (Physics.SphereCast(pivot, 0.25f, (desired - pivot).normalized, out hit, tuning.cameraDistance, ~(1 << 2), QueryTriggerInteraction.Ignore))
                desired = pivot + (desired - pivot).normalized * Mathf.Max(0.8f, hit.distance - 0.2f);
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, 0.08f);
            transform.rotation = rotation;
        }
    }
}
