using System.Collections.Generic;
using IslandSurvival.Building;
using IslandSurvival.Exploration;
using IslandSurvival.Gathering;
using IslandSurvival.Interaction;
using IslandSurvival.Visual;
using IslandSurvival.World;
using UnityEngine;

namespace IslandSurvival.Editor
{
    public static class VisualFoundationValidator
    {
        public static int Validate()
        {
            var failures = 0;
            failures += RequireShader("Visual/Shaders/StylizedSurface");
            failures += RequireShader("Visual/Shaders/StylizedTerrain");
            failures += RequireShader("Visual/Shaders/TropicalWater");

            var terrain = StylizedGeometry.Terrain();
            if (terrain.colors == null || terrain.colors.Length != terrain.vertexCount)
            { Debug.LogError("Visual foundation terrain must contain one RGBA blend mask per vertex."); failures++; }

            var catalog = Resources.Load<VisualAssetCatalog>("Visual/VisualAssetCatalog");
            if (catalog != null)
            {
                failures += ValidateUnique(catalog.Prefabs, entry => entry.key.ToString(), "prefab");
                failures += ValidateUnique(catalog.Materials, entry => entry.key.ToString(), "material");
                foreach (var entry in catalog.Prefabs)
                    if (entry != null && entry.prefab != null) failures += ValidatePresentationOnly(entry);
            }
            return failures;
        }

        static int RequireShader(string path)
        {
            if (Resources.Load<Shader>(path) != null) return 0;
            Debug.LogError("Missing Visual Upgrade shader resource: " + path); return 1;
        }

        static int ValidateUnique<T>(IReadOnlyList<T> entries, System.Func<T, string> key, string label) where T : class
        {
            var failures = 0; var keys = new HashSet<string>();
            foreach (var entry in entries)
            {
                if (entry == null || keys.Add(key(entry))) continue;
                Debug.LogError("Duplicate Visual Asset Catalog " + label + " key: " + key(entry)); failures++;
            }
            return failures;
        }

        static int ValidatePresentationOnly(VisualAssetCatalog.PrefabEntry entry)
        {
            var failures = 0;
            if (entry.prefab.GetComponentsInChildren<Collider>(true).Length > 0)
            { Debug.LogError("Visual prefab must not own gameplay colliders: " + entry.key); failures++; }
            foreach (var component in entry.prefab.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (component is IInteractable || component is PickupResource || component is TreeGatherable
                    || component is PlacedBuildable || component is DiscoveryTrigger || component is MysteryCollectible)
                { Debug.LogError("Visual prefab contains gameplay state/interaction component " + component.GetType().Name + ": " + entry.key); failures++; }
            }
            return failures;
        }
    }
}
