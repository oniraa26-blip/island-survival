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
        public bool Contains(MinimapMarker marker) { return marker != null && markers.Contains(marker); }

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

        public bool Unregister(string id)
        {
            for (var i = 0; i < markers.Count; i++)
            {
                if (markers[i].Id != id) continue;
                if (markers[i].Category == MinimapMarkerCategory.Teammate) teammateCount--;
                markers.RemoveAt(i); ids.Remove(id); return true;
            }
            return false;
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
        { return RegisterLandmark(discoveryId, discoveryId, "location." + discoveryId, position); }

        public bool RegisterLandmark(string markerId, string discoveryId, string displayNameKey, Vector3 position)
        { return Register(new MinimapMarker("landmark_" + markerId, MinimapMarkerCategory.Landmark, null, position, displayNameKey, new Color(0.25f, 0.9f, 0.85f), discoveryId)); }

        public bool RegisterHomeBed(string persistentId, Transform target)
        { return Register(new MinimapMarker("home_" + persistentId, MinimapMarkerCategory.BedHome, target, Vector3.zero, "map.home_bed", new Color(1f, 0.62f, 0.5f), string.Empty)); }

        public bool RegisterCustomPin(string id, Vector3 position, string displayName)
        { return Register(new MinimapMarker("pin_" + id, MinimapMarkerCategory.Quest, null, position, displayName, new Color(1f, 0.78f, 0.2f), string.Empty)); }
    }
}
