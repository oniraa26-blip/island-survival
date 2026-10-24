using UnityEngine;

namespace IslandSurvival.Visual
{
    public sealed class VisualPrefabInstance : MonoBehaviour
    {
        [SerializeField] VisualAssetKey assetKey;
        [SerializeField] string gameplayIdentity;
        [SerializeField] bool proceduralFallback;

        public VisualAssetKey AssetKey { get { return assetKey; } }
        public string GameplayIdentity { get { return gameplayIdentity; } }
        public bool IsProceduralFallback { get { return proceduralFallback; } }

        public void Initialize(VisualAssetKey key, string stableGameplayIdentity, bool isFallback)
        {
            assetKey = key;
            gameplayIdentity = stableGameplayIdentity ?? string.Empty;
            proceduralFallback = isFallback;
        }
    }
}
