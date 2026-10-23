using System;
using System.Collections.Generic;
using UnityEngine;

namespace IslandSurvival.World
{
    public sealed class OriginDemoWorld : MonoBehaviour
    {
        sealed class RuntimeZone
        {
            public string id;
            public Vector3 center;
            public float radius;
            public bool keepLoaded;
            public Transform container;
            public Func<Transform, GameObject> build;
            public GameObject activeVisuals;
        }

        readonly List<RuntimeZone> zones = new List<RuntimeZone>();
        readonly HashSet<string> zoneIds = new HashSet<string>(StringComparer.Ordinal);
        Transform player;
        float nextEvaluation;

        public IEnumerable<string> ZoneIds
        {
            get { foreach (var zone in zones) yield return zone.id; }
        }

        public void Register(string id, Vector3 center, float radius, bool keepLoaded, Func<Transform, GameObject> builder)
        {
            if (string.IsNullOrEmpty(id) || !zoneIds.Add(id)) throw new ArgumentException("Duplicate or empty Origin demo zone ID: " + id);
            var container = new GameObject(id + "Zone").transform;
            container.SetParent(transform, false);
            var zone = new RuntimeZone { id = id, center = center, radius = radius, keepLoaded = keepLoaded, container = container, build = builder };
            zones.Add(zone);
            if (keepLoaded) Load(zone);
        }

        public void BindPlayer(Transform playerTransform)
        {
            player = playerTransform;
            Evaluate();
        }

        void Update()
        {
            if (player == null || UnityEngine.Time.unscaledTime < nextEvaluation) return;
            nextEvaluation = UnityEngine.Time.unscaledTime + 0.45f;
            Evaluate();
        }

        void Evaluate()
        {
            foreach (var zone in zones)
            {
                var distance = Vector2.Distance(new Vector2(player.position.x, player.position.z), new Vector2(zone.center.x, zone.center.z));
                if (zone.keepLoaded || distance <= zone.radius) Load(zone);
                else if (zone.activeVisuals != null && distance > zone.radius + 28f) Unload(zone);
            }
        }

        static void Load(RuntimeZone zone)
        {
            if (zone.activeVisuals == null) zone.activeVisuals = zone.build(zone.container);
        }

        static void Unload(RuntimeZone zone)
        {
            UnityEngine.Object.Destroy(zone.activeVisuals);
            zone.activeVisuals = null;
        }
    }
}
