using System;
using IslandSurvival.UI;
using UnityEditor;
using UnityEngine;

namespace IslandSurvival.Editor
{
    public static class MinimapValidator
    {
        [MenuItem("Island Survival/UI/Validate Minimap")]
        public static void ValidateMenu()
        {
            var failures = Validate();
            if (failures > 0) throw new InvalidOperationException("Minimap validation failed with " + failures + " issue(s). See Console.");
            Debug.Log("Minimap foundation validation passed.");
        }

        public static int Validate()
        {
            var failures = 0;
            if (MinimapController.ReferenceWidthFraction < 0.14f || MinimapController.ReferenceWidthFraction > 0.18f)
            { Debug.LogError("Minimap width must remain 14–18% of the 1920 reference landscape width."); failures++; }
            foreach (MinimapMarkerCategory category in Enum.GetValues(typeof(MinimapMarkerCategory)))
            {
                var icon = MinimapIconLibrary.Get(category);
                if (icon == null || icon.category != category || icon.size <= 0f) { Debug.LogError("Missing minimap icon definition: " + category); failures++; }
            }
            if (Application.isPlaying)
            {
                var controller = UnityEngine.Object.FindFirstObjectByType<MinimapController>();
                if (controller == null) { Debug.LogError("Runtime MinimapController root is missing."); failures++; }
                else
                {
                    if (controller.Player == null) { Debug.LogError("Minimap local player tracking is missing."); failures++; }
                    var localCount = 0;
                    foreach (var marker in controller.Registry.Markers) if (marker.Category == MinimapMarkerCategory.Player) localCount++;
                    if (localCount != 1) { Debug.LogError("Minimap requires exactly one local player marker; found " + localCount); failures++; }
                }
            }
            return failures;
        }
    }
}
