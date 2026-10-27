using UnityEngine;

namespace IslandSurvival.Presentation
{
    public enum PresentationLayerKey
    {
        WATER_DEEP, WATER_SHALLOW, WATER_FOAM,
        GROUND_WET_SAND, GROUND_DRY_SAND, GROUND_DIRT, GROUND_GRASS,
        TRANSITION_SAND_GRASS, TRANSITION_DIRT_GRASS
    }

    public sealed class PresentationLayerSlot : MonoBehaviour
    {
        [SerializeField] PresentationLayerKey key;
        [SerializeField] Renderer targetRenderer;
        public PresentationLayerKey Key { get { return key; } }
        public Renderer TargetRenderer { get { return targetRenderer; } }
        public void Initialize(PresentationLayerKey layerKey, Renderer renderer) { key = layerKey; targetRenderer = renderer; }
    }
}
