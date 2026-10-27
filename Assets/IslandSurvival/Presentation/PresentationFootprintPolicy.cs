using IslandSurvival.Visual;
using UnityEngine;

namespace IslandSurvival.Presentation
{
    public struct PresentationFootprint
    {
        public Vector3 size;
        public Vector3 center;
        public PresentationFootprint(Vector3 footprintSize, Vector3 footprintCenter) { size = footprintSize; center = footprintCenter; }
    }

    public static class PresentationFootprintPolicy
    {
        public static PresentationFootprint For(VisualAssetKey key)
        {
            switch (key)
            {
                case VisualAssetKey.ENV_PALM_A:
                case VisualAssetKey.ENV_PALM_B:
                case VisualAssetKey.ENV_PALM_C: return new PresentationFootprint(new Vector3(0.9f, 4.4f, 0.9f), new Vector3(0f, 2.2f, 0f));
                case VisualAssetKey.ENV_TREE_A:
                case VisualAssetKey.ENV_TREE_B: return new PresentationFootprint(new Vector3(1.15f, 3.6f, 1.15f), new Vector3(0f, 1.8f, 0f));
                case VisualAssetKey.HouseLevel1Default:
                case VisualAssetKey.HouseLevel1Tropical: return new PresentationFootprint(new Vector3(5.2f, 1.5f, 3.4f), new Vector3(0f, 0.75f, 0f));
                case VisualAssetKey.ENV_ROCK_MEDIUM_A: return new PresentationFootprint(new Vector3(1.7f, 1.1f, 1.4f), new Vector3(0f, 0.55f, 0f));
                case VisualAssetKey.ENV_ROCK_LARGE_A: return new PresentationFootprint(new Vector3(2.7f, 1.7f, 2.2f), new Vector3(0f, 0.85f, 0f));
                default: return new PresentationFootprint(new Vector3(0.8f, 0.6f, 0.8f), new Vector3(0f, 0.3f, 0f));
            }
        }

        public static bool IsBaseOnly(VisualAssetKey key, PresentationFootprint footprint, Vector2 visualSize)
        { return footprint.size.x <= visualSize.x && footprint.size.z <= visualSize.x && footprint.center.y - footprint.size.y * 0.5f <= 0.01f; }
    }
}
