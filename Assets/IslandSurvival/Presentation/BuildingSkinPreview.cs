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
            defaultSkin = StartingBeachVisualSlice.CreateVisual(transform, "House L1 Default", VisualAssetKey.HouseLevel1Default, Vector3.zero, new Vector2(6.4f, 5.1f));
            tropicalSkin = StartingBeachVisualSlice.CreateVisual(transform, "House L1 Tropical", VisualAssetKey.HouseLevel1Tropical, Vector3.zero, new Vector2(6.4f, 5.1f));
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
