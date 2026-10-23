using IslandSurvival.Core;
using UnityEngine;

namespace IslandSurvival.Exploration
{
    [RequireComponent(typeof(Collider))]
    public sealed class DiscoveryTrigger : MonoBehaviour
    {
        public string discoveryId;

        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) ServiceRegistry.Discovery.Discover(discoveryId);
        }
    }
}
