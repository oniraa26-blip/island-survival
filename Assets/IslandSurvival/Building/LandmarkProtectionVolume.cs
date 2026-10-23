using System.Collections.Generic;
using UnityEngine;

namespace IslandSurvival.Building
{
    public sealed class LandmarkProtectionVolume : MonoBehaviour
    {
        static readonly List<LandmarkProtectionVolume> Active = new List<LandmarkProtectionVolume>();
        public float radius = 8f;

        void OnEnable() { Active.Add(this); }
        void OnDisable() { Active.Remove(this); }

        public static bool Contains(Vector3 point)
        {
            foreach (var volume in Active)
            {
                var offset = point - volume.transform.position; offset.y = 0f;
                if (offset.sqrMagnitude < volume.radius * volume.radius) return true;
            }
            return false;
        }
    }
}
