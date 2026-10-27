using System;
using IslandSurvival.UI;
using IslandSurvival.World;
using IslandSurvival.Exploration;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace IslandSurvival.Editor
{
    public static class WorldMapValidator
    {
        [MenuItem("Island Survival/UI/Validate World Map")]
        public static void ValidateMenu()
        {
            var failures = Validate();
            if (failures > 0) throw new InvalidOperationException("World Map validation failed with " + failures + " issue(s). See Console.");
            Debug.Log("World Map foundation validation passed.");
        }

        public static int Validate()
        {
            var failures = 0;
            var definition = WorldMapDefinition.CreateOriginPlaceholder();
            if (definition == null) { Debug.LogError("Origin Island map definition is missing."); return 1; }
            if (definition.Regions.Count != 8) { Debug.LogError("Origin Island map must define exactly eight existing zones."); failures++; }
            for (var i = 0; i < OriginDemoZones.All.Length; i++)
            {
                var found = false;
                for (var j = 0; j < definition.Regions.Count; j++) if (definition.Regions[j].zoneId == OriginDemoZones.All[i]) found = true;
                if (!found) { Debug.LogError("World map is missing zone: " + OriginDemoZones.All[i]); failures++; }
            }
            if (definition.WorldToNormalized(Vector3.zero).x < 0f) { Debug.LogError("World-to-map conversion is unavailable."); failures++; }
            var discovery = new DiscoverySystem();
            if (definition.IsRegionVisible(definition.Regions[1], discovery)) { Debug.LogError("Undiscovered world-map regions must remain obscured."); failures++; }
            discovery.Discover(definition.Regions[1].zoneId);
            if (!definition.IsRegionVisible(definition.Regions[1], discovery)) { Debug.LogError("World-map discovery integration did not reveal a discovered region."); failures++; }
            UnityEngine.Object.DestroyImmediate(definition);

            if (Application.isPlaying)
            {
                var worldMap = UnityEngine.Object.FindFirstObjectByType<WorldMapController>();
                var minimap = UnityEngine.Object.FindFirstObjectByType<MinimapController>();
                if (worldMap == null) { Debug.LogError("Runtime WorldMapController root is missing."); failures++; }
                else
                {
                    if (worldMap.Definition == null || worldMap.MapRect == null) { Debug.LogError("World map UI references are incomplete."); failures++; }
                    if (!worldMap.GameplayInputLockConfigured) { Debug.LogError("World map gameplay input lock is not configured."); failures++; }
                    if (!worldMap.HasCloseAction) { Debug.LogError("World map close action is missing."); failures++; }
                    if (worldMap.Registry == null) { Debug.LogError("World map marker registry is missing."); failures++; }
                    else
                    {
                        var localPlayers = 0;
                        foreach (var marker in worldMap.Registry.Markers) if (marker.Category == MinimapMarkerCategory.Player) localPlayers++;
                        if (localPlayers != 1) { Debug.LogError("World map requires one local player marker."); failures++; }
                    }
                }
                if (minimap == null || !minimap.HasWorldMapCallback) { Debug.LogError("Minimap cannot invoke the Full Map."); failures++; }
                if (UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length != 1) { Debug.LogError("Runtime must contain exactly one EventSystem."); failures++; }
            }
            return failures;
        }
    }
}
