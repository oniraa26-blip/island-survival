using IslandSurvival.Platform;
using UnityEngine;
using UnityEngine.Rendering;

namespace IslandSurvival.Visual
{
    [CreateAssetMenu(fileName = "TropicalDaylight", menuName = "Island Survival/Tropical Daylight Profile")]
    public sealed class TropicalDaylightProfile : ScriptableObject
    {
        public Color sunlight = new Color(1f, 0.91f, 0.72f);
        [Range(0f, 2f)] public float sunlightIntensity = 1.15f;
        public Vector3 sunlightEuler = new Vector3(48f, -32f, 0f);
        public Color tropicalAmbient = new Color(0.46f, 0.58f, 0.52f);
        public Color atmosphericBlue = new Color(0.43f, 0.72f, 0.82f);
        public Color fogColor = new Color(0.49f, 0.76f, 0.8f);
        public float nativeFogStart = 105f;
        public float webFogStart = 72f;

        public static void Apply(TropicalDaylightProfile profile, PlatformProfile platform, Light sun, Camera camera)
        {
            var sunlight = profile != null ? profile.sunlight : new Color(1f, 0.91f, 0.72f);
            var intensity = profile != null ? profile.sunlightIntensity : 1.15f;
            var rotation = profile != null ? profile.sunlightEuler : new Vector3(48f, -32f, 0f);
            var ambient = profile != null ? profile.tropicalAmbient : new Color(0.46f, 0.58f, 0.52f);
            var atmosphere = profile != null ? profile.atmosphericBlue : new Color(0.43f, 0.72f, 0.82f);
            var fog = profile != null ? profile.fogColor : new Color(0.49f, 0.76f, 0.8f);
            var fogStart = platform.tier == PlatformTier.WebMobileTest
                ? (profile != null ? profile.webFogStart : 72f)
                : (profile != null ? profile.nativeFogStart : 105f);

            if (sun != null)
            {
                sun.color = sunlight; sun.intensity = intensity;
                sun.transform.rotation = Quaternion.Euler(rotation);
                sun.shadows = platform.useSoftShadows ? LightShadows.Soft : LightShadows.Hard;
            }
            if (camera != null) { camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = atmosphere; }
            RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = ambient;
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = fogStart; RenderSettings.fogEndDistance = platform.cameraFarClip; RenderSettings.fogColor = fog;
        }
    }
}
