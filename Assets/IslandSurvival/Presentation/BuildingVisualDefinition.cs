using System;
using System.Collections.Generic;
using UnityEngine;

namespace IslandSurvival.Presentation
{
    [Serializable]
    public sealed class BuildingSkinVisual
    {
        public string skinId = "default";
        public SpriteDirectionSet directions = new SpriteDirectionSet();
    }

    [Serializable]
    public sealed class BuildingLevelVisual
    {
        [Min(1)] public int level = 1;
        public List<BuildingSkinVisual> skins = new List<BuildingSkinVisual>();
    }

    [CreateAssetMenu(fileName = "BuildingVisualDefinition", menuName = "Island Survival/2.5D/Building Visual Definition")]
    public sealed class BuildingVisualDefinition : ScriptableObject
    {
        [SerializeField] string buildingId;
        [SerializeField] List<BuildingLevelVisual> levels = new List<BuildingLevelVisual>();
        public string BuildingId { get { return buildingId; } }

        public bool TryGet(int level, string skinId, SpriteDirection direction, out Sprite sprite)
        {
            return BuildingVisualLookup.TryGet(levels, level, skinId, direction, out sprite);
        }
    }

    public static class BuildingVisualLookup
    {
        public static bool TryGet(IReadOnlyList<BuildingLevelVisual> levels, int level, string skinId, SpriteDirection direction, out Sprite sprite)
        {
            if (levels == null) { sprite = null; return false; }
            foreach (var levelVisual in levels)
            {
                if (levelVisual == null || levelVisual.level != level) continue;
                foreach (var skin in levelVisual.skins)
                    if (skin != null && skin.skinId == skinId && skin.directions.TryGet(direction, out sprite)) return true;
            }
            sprite = null; return false;
        }
    }
}
