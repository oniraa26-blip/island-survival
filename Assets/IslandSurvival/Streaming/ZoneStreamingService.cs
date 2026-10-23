using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace IslandSurvival.Streaming
{
    public sealed class ZoneStreamingService : MonoBehaviour
    {
        readonly LinkedList<WorldZoneDefinition> loadedZones = new LinkedList<WorldZoneDefinition>();
        readonly HashSet<string> pendingScenes = new HashSet<string>();
        int maxLoadedZones = 1;
        public event Action<string> ZoneLoaded;
        public event Action<string> ZoneUnloaded;

        public void Initialize(int maxLoadedZones)
        { this.maxLoadedZones = Mathf.Max(1, maxLoadedZones); }

        public bool IsLoaded(string sceneName)
        {
            foreach (var zone in loadedZones) if (zone.sceneName == sceneName) return true;
            return pendingScenes.Contains(sceneName);
        }

        public void Load(WorldZoneDefinition zone)
        {
            if (zone == null || string.IsNullOrEmpty(zone.sceneName) || IsLoaded(zone.sceneName)) return;
            StartCoroutine(LoadRoutine(zone));
        }

        IEnumerator LoadRoutine(WorldZoneDefinition zone)
        {
            pendingScenes.Add(zone.sceneName);
            while (loadedZones.Count >= maxLoadedZones)
            {
                var node = loadedZones.First;
                while (node != null && node.Value.keepLoaded) node = node.Next;
                if (node == null) { Debug.LogWarning("Zone budget is occupied by persistent zones; skipped " + zone.zoneId); pendingScenes.Remove(zone.sceneName); yield break; }
                var candidate = node.Value.sceneName;
                var unload = SceneManager.UnloadSceneAsync(candidate);
                if (unload != null) while (!unload.isDone) yield return null;
                loadedZones.Remove(node); if (ZoneUnloaded != null) ZoneUnloaded(candidate);
            }
            var operation = SceneManager.LoadSceneAsync(zone.sceneName, LoadSceneMode.Additive);
            if (operation == null) { pendingScenes.Remove(zone.sceneName); yield break; }
            operation.allowSceneActivation = true;
            while (!operation.isDone) yield return null;
            pendingScenes.Remove(zone.sceneName); loadedZones.AddLast(zone);
            if (ZoneLoaded != null) ZoneLoaded(zone.sceneName);
        }
    }
}
