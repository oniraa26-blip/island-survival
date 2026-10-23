using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace IslandSurvival.Platform
{
    public static class PlatformQualityController
    {
        public static void Apply(PlatformProfile profile, UnityEngine.Camera camera = null)
        {
            Application.targetFrameRate = profile.targetFrameRate;
            QualitySettings.vSyncCount = 0;
            QualitySettings.shadowDistance = profile.shadowDistance;
            QualitySettings.shadowResolution = profile.shadowResolution;
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.softParticles = false;
            QualitySettings.lodBias = profile.lodBias;
            QualitySettings.globalTextureMipmapLimit = profile.textureMipmapLimit;
            if (camera != null) camera.farClipPlane = profile.cameraFarClip;
            var urp = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
            if (urp != null)
            {
                urp.renderScale = profile.renderScale;
                urp.supportsHDR = false;
                urp.msaaSampleCount = profile.tier == PlatformTier.WebMobileTest ? 1 : 2;
                urp.shadowDistance = profile.shadowDistance;
            }
        }
    }
}
