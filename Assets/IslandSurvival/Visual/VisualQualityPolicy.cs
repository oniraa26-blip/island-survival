using IslandSurvival.Platform;
using UnityEngine;
using UnityEngine.Rendering;

namespace IslandSurvival.Visual
{
    public sealed class VisualQualityPolicy
    {
        readonly PlatformProfile profile;
        public VisualQualityPolicy(PlatformProfile platformProfile) { profile = platformProfile; }
        public bool IsWebMobile { get { return profile.tier == PlatformTier.WebMobileTest; } }

        public void Apply(GameObject visualRoot, VisualAssetKey key)
        {
            if (visualRoot == null) return;
            var renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                renderer.allowOcclusionWhenDynamic = true;
                if (IsWebMobile && (key == VisualAssetKey.GrassCluster || key == VisualAssetKey.Bush))
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
            }
            foreach (var lod in visualRoot.GetComponentsInChildren<LODGroup>(true))
            {
                lod.fadeMode = IsWebMobile ? LODFadeMode.None : LODFadeMode.CrossFade;
                lod.animateCrossFading = !IsWebMobile;
            }
        }
    }
}
