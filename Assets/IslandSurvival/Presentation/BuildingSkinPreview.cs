using IslandSurvival.Visual;
using UnityEngine;

namespace IslandSurvival.Presentation
{
    public sealed class BuildingSkinPreview : MonoBehaviour
    {
        GameObject defaultSkin;
        GameObject tropicalSkin;
        float nextSwitch;

        public void Initialize()
        {
            var size = PresentationScaleProfile.RuntimeDefault.WorldSize(VisualAssetKey.HouseLevel1Default);
            defaultSkin = StartingBeachVisualSlice.CreateVisual(transform, "House L1 Default", VisualAssetKey.HouseLevel1Default, Vector3.zero, size);
            tropicalSkin = StartingBeachVisualSlice.CreateVisual(transform, "House L1 Tropical", VisualAssetKey.HouseLevel1Tropical, Vector3.zero, size);
            if (tropicalSkin != null) tropicalSkin.SetActive(false);
            nextSwitch = Time.unscaledTime + 4f;
        }

        void Update()
        {
            if (defaultSkin == null || tropicalSkin == null || Time.unscaledTime < nextSwitch) return;
            var tropical = !tropicalSkin.activeSelf;
            tropicalSkin.SetActive(tropical); defaultSkin.SetActive(!tropical);
            nextSwitch = Time.unscaledTime + 4f;
        }
    }
}
