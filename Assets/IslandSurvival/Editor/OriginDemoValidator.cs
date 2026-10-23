using System.Collections.Generic;
using IslandSurvival.Localization;
using IslandSurvival.Save;
using IslandSurvival.World;
using UnityEngine;

namespace IslandSurvival.Editor
{
    public static class OriginDemoValidator
    {
        public static int Validate()
        {
            var failures = 0;
            var ids = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (var id in OriginDemoZones.All)
            {
                if (string.IsNullOrEmpty(id) || !ids.Add(id)) { Debug.LogError("Duplicate or empty Origin demo zone ID: " + id); failures++; }
            }
            if (ids.Count != 8) { Debug.LogError("Origin Web Demo requires exactly 8 prototype zones; found " + ids.Count); failures++; }

            var spawnHeight = StylizedGeometry.GroundHeight(0f, 0f);
            if (float.IsNaN(spawnHeight) || float.IsInfinity(spawnHeight) || spawnHeight < -0.5f || spawnHeight > 1f)
            { Debug.LogError("Starting Beach spawn terrain height is invalid: " + spawnHeight); failures++; }

            try
            {
                var localization = LocalizationService.LoadDefault();
                foreach (var id in OriginDemoZones.All)
                {
                    var key = "location." + id;
                    if (localization.Get(key) == "[" + key + "]") { Debug.LogError("Missing location localization key: " + key); failures++; }
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogError("Origin demo localization validation failed: " + exception.Message); failures++;
            }
            if (!SaveSystem.IsSupportedVersion(1) || !SaveSystem.IsSupportedVersion(2))
            { Debug.LogError("Save schema must load both Prototype 0.1 v1 and Origin Demo v2."); failures++; }
            return failures;
        }
    }
}
