using System;
using System.Collections.Generic;

namespace IslandSurvival.Exploration
{
    public sealed class DiscoverySystem
    {
        readonly HashSet<string> discovered = new HashSet<string>(StringComparer.Ordinal);
        readonly HashSet<string> collected = new HashSet<string>(StringComparer.Ordinal);
        public event Action<string> Discovered;

        public IEnumerable<string> DiscoveredIds { get { return discovered; } }
        public IEnumerable<string> CollectedIds { get { return collected; } }

        public bool Discover(string id)
        {
            if (string.IsNullOrEmpty(id) || !discovered.Add(id)) return false;
            if (Discovered != null) Discovered(id);
            return true;
        }

        public bool IsDiscovered(string id) { return discovered.Contains(id); }
        public bool IsCollected(string id) { return collected.Contains(id); }
        public bool Collect(string id) { return !string.IsNullOrEmpty(id) && collected.Add(id); }

        public void Restore(IEnumerable<string> discoveredIds, IEnumerable<string> collectedIds)
        {
            discovered.Clear(); collected.Clear();
            if (discoveredIds != null) foreach (var id in discoveredIds) if (!string.IsNullOrEmpty(id)) discovered.Add(id);
            if (collectedIds != null) foreach (var id in collectedIds) if (!string.IsNullOrEmpty(id)) collected.Add(id);
        }
    }
}
