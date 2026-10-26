using System;
using UnityEngine;

namespace IslandSurvival.UI
{
    public enum MinimapMarkerCategory
    {
        Player, Teammate, NPC, Quest, Building, BedHome, Market, Boat, Resource, Danger, Boss, Landmark
    }

    [Serializable]
    public sealed class MinimapIconDefinition
    {
        public MinimapMarkerCategory category;
        public Color color = Color.white;
        public float size = 16f;
    }

    public sealed class MinimapMarker
    {
        public string Id { get; private set; }
        public MinimapMarkerCategory Category { get; private set; }
        public Transform Target { get; private set; }
        public Vector3 FixedPosition { get; private set; }
        public string DisplayName { get; private set; }
        public Color IdentifierColor { get; private set; }
        public bool Active { get; set; }
        public bool ClampToEdge { get; set; }
        public string RequiredDiscoveryId { get; private set; }
        public Vector3 Position { get { return Target != null ? Target.position : FixedPosition; } }
        public float DistanceFrom(Vector3 position) { return Vector3.Distance(Position, position); }

        public MinimapMarker(string id, MinimapMarkerCategory category, Transform target, Vector3 fixedPosition,
            string displayName, Color identifierColor, string requiredDiscoveryId)
        {
            Id = id; Category = category; Target = target; FixedPosition = fixedPosition;
            DisplayName = displayName ?? string.Empty; IdentifierColor = identifierColor;
            RequiredDiscoveryId = requiredDiscoveryId ?? string.Empty; Active = true; ClampToEdge = category == MinimapMarkerCategory.Teammate;
        }
    }

    public static class MinimapIconLibrary
    {
        static readonly MinimapIconDefinition[] Definitions =
        {
            new MinimapIconDefinition { category = MinimapMarkerCategory.Player, color = new Color(1f, 0.9f, 0.2f), size = 24f },
            new MinimapIconDefinition { category = MinimapMarkerCategory.Teammate, color = new Color(0.3f, 0.75f, 1f), size = 18f },
            new MinimapIconDefinition { category = MinimapMarkerCategory.Landmark, color = new Color(0.35f, 0.95f, 0.9f), size = 15f },
            new MinimapIconDefinition { category = MinimapMarkerCategory.NPC, color = new Color(0.65f, 1f, 0.45f), size = 14f },
            new MinimapIconDefinition { category = MinimapMarkerCategory.Quest, color = new Color(1f, 0.72f, 0.2f), size = 15f },
            new MinimapIconDefinition { category = MinimapMarkerCategory.Building, color = new Color(0.85f, 0.7f, 0.48f), size = 14f },
            new MinimapIconDefinition { category = MinimapMarkerCategory.BedHome, color = new Color(1f, 0.62f, 0.5f), size = 15f },
            new MinimapIconDefinition { category = MinimapMarkerCategory.Market, color = new Color(0.95f, 0.55f, 0.95f), size = 14f },
            new MinimapIconDefinition { category = MinimapMarkerCategory.Boat, color = new Color(0.35f, 0.8f, 1f), size = 14f },
            new MinimapIconDefinition { category = MinimapMarkerCategory.Resource, color = new Color(0.55f, 0.9f, 0.45f), size = 12f },
            new MinimapIconDefinition { category = MinimapMarkerCategory.Danger, color = new Color(1f, 0.35f, 0.2f), size = 16f },
            new MinimapIconDefinition { category = MinimapMarkerCategory.Boss, color = new Color(0.85f, 0.15f, 0.2f), size = 19f }
        };

        public static MinimapIconDefinition Get(MinimapMarkerCategory category)
        {
            for (var i = 0; i < Definitions.Length; i++) if (Definitions[i].category == category) return Definitions[i];
            return Definitions[0];
        }
    }
}
