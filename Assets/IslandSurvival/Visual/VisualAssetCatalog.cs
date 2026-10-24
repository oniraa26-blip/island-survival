using System;
using System.Collections.Generic;
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
        [SerializeField] TropicalDaylightProfile tropicalDaylight;

        public TropicalDaylightProfile TropicalDaylight { get { return tropicalDaylight; } }
        public IReadOnlyList<PrefabEntry> Prefabs { get { return prefabs; } }
        public IReadOnlyList<MaterialEntry> Materials { get { return materials; } }

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
