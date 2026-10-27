using System;
using System.Collections.Generic;
using IslandSurvival.Core;
using IslandSurvival.World;
using IslandSurvival.Exploration;
using UnityEngine;

namespace IslandSurvival.UI
{
    [Serializable]
    public sealed class WorldMapRegionDefinition
    {
        public string zoneId;
        public string nameKey;
        public string descriptionKey;
        public Vector3 worldCenter;
        public float mapRadius = 0.1f;
    }

    [CreateAssetMenu(menuName = "Island Survival/UI/World Map Definition", fileName = "OriginIslandMap")]
    public sealed class WorldMapDefinition : ScriptableObject
    {
        [SerializeField] Vector2 worldMin = new Vector2(-180f, -25f);
        [SerializeField] Vector2 worldMax = new Vector2(180f, 325f);
        [SerializeField] List<WorldMapRegionDefinition> regions = new List<WorldMapRegionDefinition>();

        public Vector2 WorldMin { get { return worldMin; } }
        public Vector2 WorldMax { get { return worldMax; } }
        public IReadOnlyList<WorldMapRegionDefinition> Regions { get { return regions; } }

        public Vector2 WorldToNormalized(Vector3 worldPosition, bool clamp = true)
        {
            var width = Mathf.Max(0.01f, worldMax.x - worldMin.x);
            var height = Mathf.Max(0.01f, worldMax.y - worldMin.y);
            var result = new Vector2((worldPosition.x - worldMin.x) / width, (worldPosition.z - worldMin.y) / height);
            return clamp ? new Vector2(Mathf.Clamp01(result.x), Mathf.Clamp01(result.y)) : result;
        }

        public bool IsRegionVisible(WorldMapRegionDefinition region)
        { return IsRegionVisible(region, ServiceRegistry.Discovery); }

        public bool IsRegionVisible(WorldMapRegionDefinition region, DiscoverySystem discovery)
        {
            return region != null && (region.zoneId == OriginDemoZones.StartingBeach
                || discovery != null && discovery.IsDiscovered(region.zoneId));
        }

        public static WorldMapDefinition CreateOriginPlaceholder()
        {
            var definition = CreateInstance<WorldMapDefinition>();
            definition.name = "Origin Island Temporary Map Definition";
            definition.regions.Add(Region(OriginDemoZones.StartingBeach, 13f, 11f, 0.11f));
            definition.regions.Add(Region(OriginDemoZones.FreshwaterStream, 0f, 95f, 0.09f));
            definition.regions.Add(Region(OriginDemoZones.GiantTree, 0f, 155f, 0.11f));
            definition.regions.Add(Region(OriginDemoZones.BlueLagoon, -105f, 215f, 0.1f));
            definition.regions.Add(Region(OriginDemoZones.TwinFalls, 95f, 225f, 0.09f));
            definition.regions.Add(Region(OriginDemoZones.FlowerValley, 0f, 292f, 0.1f));
            definition.regions.Add(Region(OriginDemoZones.SplitRock, 105f, 72f, 0.09f));
            definition.regions.Add(Region(OriginDemoZones.SeaCave, 158f, 58f, 0.08f));
            return definition;
        }

        static WorldMapRegionDefinition Region(string id, float x, float z, float radius)
        {
            return new WorldMapRegionDefinition { zoneId = id, nameKey = "location." + id,
                descriptionKey = "map.description." + id, worldCenter = new Vector3(x, 0f, z), mapRadius = radius };
        }
    }
}
