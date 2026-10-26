using System;
using System.Collections.Generic;
using IslandSurvival.Presentation;
using UnityEngine;

namespace IslandSurvival.Visual
{
    [CreateAssetMenu(fileName = "VisualAssetCatalog", menuName = "Island Survival/Visual Asset Catalog")]
    public sealed class VisualAssetCatalog : ScriptableObject
    {
        [Serializable]
        public sealed class PrefabEntry
        {
            public VisualAssetKey key;
            public GameObject prefab;
        }

        [Serializable]
        public sealed class MaterialEntry
        {
            public VisualMaterialKey key;
            public Material material;
        }

        [SerializeField] List<PrefabEntry> prefabs = new List<PrefabEntry>();
        [SerializeField] List<MaterialEntry> materials = new List<MaterialEntry>();
        [SerializeField] List<SpriteVisualDefinition> spriteVisuals = new List<SpriteVisualDefinition>();
        [SerializeField] List<BuildingVisualDefinition> buildingVisuals = new List<BuildingVisualDefinition>();
        [SerializeField] TropicalDaylightProfile tropicalDaylight;

        public TropicalDaylightProfile TropicalDaylight { get { return tropicalDaylight; } }
        public IReadOnlyList<PrefabEntry> Prefabs { get { return prefabs; } }
        public IReadOnlyList<MaterialEntry> Materials { get { return materials; } }
        public IReadOnlyList<SpriteVisualDefinition> SpriteVisuals { get { return spriteVisuals; } }
        public IReadOnlyList<BuildingVisualDefinition> BuildingVisuals { get { return buildingVisuals; } }

        public bool TryGetSpriteVisual(VisualAssetKey key, out SpriteVisualDefinition definition)
        {
            foreach (var entry in spriteVisuals)
                if (entry != null && entry.AssetKey == key) { definition = entry; return true; }
            definition = null; return false;
        }

        public bool TryGetPrefab(VisualAssetKey key, out GameObject prefab)
        {
            foreach (var entry in prefabs)
                if (entry != null && entry.key == key && entry.prefab != null) { prefab = entry.prefab; return true; }
            prefab = null; return false;
        }

        public bool TryGetMaterial(VisualMaterialKey key, out Material material)
        {
            foreach (var entry in materials)
                if (entry != null && entry.key == key && entry.material != null) { material = entry.material; return true; }
            material = null; return false;
        }
    }
}
