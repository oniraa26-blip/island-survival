using IslandSurvival.Core;
using UnityEngine;

namespace IslandSurvival.CameraSystem
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public sealed class IsometricCameraController : MonoBehaviour
    {
        Transform target;
        PrototypeTuning tuning;
        Vector3 velocity;
        Vector3 lastTargetPosition;

        public void Initialize(Transform followTarget, PrototypeTuning prototypeTuning)
        {
            target = followTarget;
            tuning = prototypeTuning;
            lastTargetPosition = target != null ? target.position : Vector3.zero;
            var camera = GetComponent<UnityEngine.Camera>();
            camera.orthographic = true;
            camera.orthographicSize = tuning.isometricSize;
            SnapToTarget();
        }

        void LateUpdate()
        {
            if (target == null || tuning == null) return;
            var delta = target.position - lastTargetPosition;
            var planarMotion = new Vector3(delta.x, 0f, delta.z);
            var lookAhead = planarMotion.sqrMagnitude > 0.000001f ? planarMotion.normalized * tuning.isometricLookAhead : Vector3.zero;
            var rotation = Quaternion.Euler(tuning.isometricPitch, tuning.isometricYaw, 0f);
            var focus = target.position + Vector3.up + lookAhead;
            var distance = Mathf.Max(15f, tuning.cameraDistance * 3f);
            var desired = focus - rotation * Vector3.forward * distance;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, tuning.isometricFollowDamping);
            transform.rotation = rotation;
            lastTargetPosition = target.position;
        }

        void SnapToTarget()
        {
            if (target == null || tuning == null) return;
            var rotation = Quaternion.Euler(tuning.isometricPitch, tuning.isometricYaw, 0f);
            transform.SetPositionAndRotation(target.position + Vector3.up - rotation * Vector3.forward * Mathf.Max(15f, tuning.cameraDistance * 3f), rotation);
        }
    }
}
