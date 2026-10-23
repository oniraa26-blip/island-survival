using System;
using UnityEngine;

namespace IslandSurvival.Platform
{
    public enum PlatformTier { IOSNative, WebMobileTest, AndroidNative, DesktopDevelopment }

    [Serializable]
    public sealed class PlatformProfile
    {
        public PlatformTier tier;
        public int targetFrameRate;
        public int baselineFrameRate;
        public float renderScale;
        public float shadowDistance;
        public ShadowResolution shadowResolution;
        public float cameraFarClip;
        public float lodBias;
        public int textureMipmapLimit;
        public float vegetationDensity;
        public int maxActiveWorldObjects;
        public int maxLoadedZones;
        public bool useSoftShadows;
        public bool useAsyncZoneLoading;

        public static PlatformProfile Detect()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return WebMobile();
#elif UNITY_IOS && !UNITY_EDITOR
            return IOS();
#elif UNITY_ANDROID && !UNITY_EDITOR
            return Android();
#else
            return Desktop();
#endif
        }

        public static PlatformProfile IOS()
        {
            return new PlatformProfile
            {
                tier = PlatformTier.IOSNative, targetFrameRate = 60, baselineFrameRate = 30,
                renderScale = 1f, shadowDistance = 42f, shadowResolution = ShadowResolution.Medium,
                cameraFarClip = 220f, lodBias = 1f, textureMipmapLimit = 0, vegetationDensity = 1f,
                maxActiveWorldObjects = 900, maxLoadedZones = 3, useSoftShadows = true, useAsyncZoneLoading = true
            };
        }

        public static PlatformProfile WebMobile()
        {
            return new PlatformProfile
            {
                tier = PlatformTier.WebMobileTest, targetFrameRate = 30, baselineFrameRate = 30,
                renderScale = 0.8f, shadowDistance = 18f, shadowResolution = ShadowResolution.Low,
                cameraFarClip = 125f, lodBias = 0.65f, textureMipmapLimit = 1, vegetationDensity = 0.58f,
                maxActiveWorldObjects = 350, maxLoadedZones = 1, useSoftShadows = false, useAsyncZoneLoading = true
            };
        }

        public static PlatformProfile Android()
        {
            return new PlatformProfile
            {
                tier = PlatformTier.AndroidNative, targetFrameRate = 60, baselineFrameRate = 30,
                renderScale = 0.9f, shadowDistance = 32f, shadowResolution = ShadowResolution.Medium,
                cameraFarClip = 180f, lodBias = 0.85f, textureMipmapLimit = 0, vegetationDensity = 0.85f,
                maxActiveWorldObjects = 700, maxLoadedZones = 2, useSoftShadows = true, useAsyncZoneLoading = true
            };
        }

        public static PlatformProfile Desktop()
        {
            var profile = IOS(); profile.tier = PlatformTier.DesktopDevelopment; return profile;
        }
    }
}
