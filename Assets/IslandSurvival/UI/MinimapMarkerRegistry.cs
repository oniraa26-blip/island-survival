using System;
using System.Collections.Generic;
using UnityEngine;

namespace IslandSurvival.UI
{
    public sealed class MinimapMarkerRegistry
    {
        readonly List<MinimapMarker> markers = new List<MinimapMarker>(24);
        readonly HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
        int teammateCount;
        public IReadOnlyList<MinimapMarker> Markers { get { return markers; } }

        public bool Register(MinimapMarker marker)
        {
            if (marker == null || string.IsNullOrEmpty(marker.Id) || !ids.Add(marker.Id)) return false;
            if (marker.Category == MinimapMarkerCategory.Player)
                foreach (var existing in markers) if (existing.Category == MinimapMarkerCategory.Player) { ids.Remove(marker.Id); return false; }
            if (marker.Category == MinimapMarkerCategory.Teammate && teammateCount >= 3) { ids.Remove(marker.Id); return false; }
            markers.Add(marker);
            if (marker.Category == MinimapMarkerCategory.Teammate) teammateCount++;
            return true;
        }

        public MinimapMarker RegisterLocalPlayer(Transform target)
        {
            var marker = new MinimapMarker("local_player", MinimapMarkerCategory.Player, target, Vector3.zero, "Player", new Color(1f, 0.9f, 0.2f), string.Empty);
            return Register(marker) ? marker : null;
        }

        public MinimapMarker RegisterTeammate(string id, string playerName, Color color, Transform target)
        {
            var marker = new MinimapMarker("teammate_" + id, MinimapMarkerCategory.Teammate, target, Vector3.zero, playerName, color, string.Empty);
            return Register(marker) ? marker : null;
        }

        public bool RegisterLandmark(string discoveryId, Vector3 position)
        { return Register(new MinimapMarker("landmark_" + discoveryId, MinimapMarkerCategory.Landmark, null, position, discoveryId, new Color(0.25f, 0.9f, 0.85f), discoveryId)); }
    }
}
