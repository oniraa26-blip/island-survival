using System;
using System.IO;
using IslandSurvival.Core;
using IslandSurvival.CameraSystem;
using IslandSurvival.Presentation;
using IslandSurvival.Platform;
using IslandSurvival.Visual;
using UnityEditor;
using UnityEngine;

namespace IslandSurvival.Editor
{
    public static class TwoPointFiveDValidator
    {
        const string StartingBeachScene = "Assets/Scenes/Prototype_StartingBeach.unity";

        [MenuItem("Island Survival/Visual/Validate 2.5D Foundation")]
        public static void ValidateMenu()
        {
            var failures = Validate();
            if (failures > 0) throw new InvalidOperationException("2.5D foundation validation failed with " + failures + " issue(s). See Console.");
            Debug.Log("2.5D foundation validation passed.");
        }

        public static int Validate()
        {
            var failures = 0;
            if (!File.Exists(StartingBeachScene)) { Debug.LogError("Starting Beach scene is missing."); failures++; }
            if (new PrototypeTuning().presentationMode != PresentationMode.TwoPointFiveD) { Debug.LogError("2.5D must be the prototype default while ThreeD remains available as rollback."); failures++; }
            if (typeof(IsometricCameraController) == null) { Debug.LogError("Isometric camera controller is unavailable."); failures++; }
            if (Enum.GetValues(typeof(SpriteDirection)).Length != 8) { Debug.LogError("Directional sprite architecture must expose eight directions."); failures++; }

            var resolver = new VisualAssetResolver(null, PlatformProfile.WebMobile());
            var parent = new GameObject("2.5D Validator Root");
            try
            {
                PresentationRuntime.Mode = PresentationMode.TwoPointFiveD;
                var visual = resolver.CreatePresentation(VisualAssetKey.PlayerCharacter, parent.transform, "validator_player", p =>
                {
                    var fallback = new GameObject("Fallback"); fallback.transform.SetParent(p, false); return fallback;
                });
                if (visual == null || visual.GetComponentInChildren<SpriteWorldVisual>(true) == null)
                { Debug.LogError("2.5D player fallback did not resolve to a sprite visual."); failures++; }
                var markers = visual != null ? visual.GetComponentsInChildren<VisualPrefabInstance>(true) : Array.Empty<VisualPrefabInstance>();
                if (markers.Length != 1 || markers[0].GameplayIdentity != "validator_player")
                { Debug.LogError("2.5D presentation must preserve exactly one gameplay identity marker."); failures++; }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(parent);
                PresentationRuntime.Mode = PresentationMode.TwoPointFiveD;
            }
            return failures;
        }
    }
}
