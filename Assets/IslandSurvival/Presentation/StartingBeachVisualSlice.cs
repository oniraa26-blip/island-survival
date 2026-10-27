using IslandSurvival.Visual;
using IslandSurvival.World;
using IslandSurvival.Core;
using UnityEngine;

namespace IslandSurvival.Presentation
{
    public static class StartingBeachVisualSlice
    {
        public static void Create(Transform parent)
        {
            if (!PresentationRuntime.IsTwoPointFiveD) return;
            var root = new GameObject("2.5D Starting Beach Authored Composition").transform;
            root.SetParent(parent, false);

            var created = 0;
            // Wreck cluster hugs the coastline while the spawn corridor around x=0 remains open.
            created += CreateVisual(root, "Wreck Crate", VisualAssetKey.PROP_CRATE, Grounded(11f, 7f)) != null ? 1 : 0;
            created += CreateVisual(root, "Wreck Barrel", VisualAssetKey.PROP_BARREL, Grounded(19f, 8f)) != null ? 1 : 0;
            created += CreateVisual(root, "Wreck Driftwood", VisualAssetKey.PROP_DRIFTWOOD, Grounded(8f, 3f)) != null ? 1 : 0;
            created += CreateVisual(root, "Wreck Rope", VisualAssetKey.PropRope, Grounded(13f, 8f)) != null ? 1 : 0;
            created += CreateVisual(root, "West Flower Cluster", VisualAssetKey.ENV_FLOWER_A, Grounded(-18f, 22f)) != null ? 1 : 0;
            created += CreateVisual(root, "East Flower Cluster", VisualAssetKey.ENV_FLOWER_B, Grounded(18f, 25f)) != null ? 1 : 0;
            created += CreateVisual(root, "West Grass Accent", VisualAssetKey.ENV_GRASS_B, Grounded(-15f, 25f)) != null ? 1 : 0;
            created += CreateVisual(root, "East Grass Accent", VisualAssetKey.ENV_GRASS_C, Grounded(15f, 29f)) != null ? 1 : 0;

            var house = new GameObject("House L1 Skin Preview"); house.transform.SetParent(root, false);
            house.transform.localPosition = Grounded(-23f, 38f);
            var houseFootprint = PresentationFootprintPolicy.For(VisualAssetKey.HouseLevel1Default);
            var houseCollider = house.AddComponent<BoxCollider>(); houseCollider.size = houseFootprint.size; houseCollider.center = houseFootprint.center;
            var preview = house.AddComponent<BuildingSkinPreview>();
            preview.Initialize();

            var farm = new GameObject("Farming Visual Proof").transform;
            farm.SetParent(root, false); farm.localPosition = Grounded(20f, 38f);
            CreateVisual(farm, "Dry Soil", VisualAssetKey.FarmSoilDry, Vector3.zero, new Vector2(2.8f, 1.8f), new Color(0.78f, 0.58f, 0.35f));
            CreateVisual(farm, "Wet Soil", VisualAssetKey.FarmSoilWet, new Vector3(3.2f, 0f, 0f), new Vector2(2.8f, 1.8f), new Color(0.48f, 0.32f, 0.2f));
            CreateVisual(farm, "Crop Stage 1", VisualAssetKey.CropTestStage1, new Vector3(0f, 0f, 2.4f), new Vector2(2.3f, 1.45f));
            CreateVisual(farm, "Crop Stage 2", VisualAssetKey.CropTestStage2, new Vector3(2.7f, 0f, 2.4f), new Vector2(2.5f, 1.6f));
            CreateVisual(farm, "Crop Stage 3", VisualAssetKey.CropTestStage3, new Vector3(5.5f, 0f, 2.4f), new Vector2(2.7f, 1.75f));
            Debug.Log("2.5D Starting Beach authored composition created with " + created + " clustered prop/accent sprites; central spawn and inland path remain open.");
        }

        internal static GameObject CreateVisual(Transform parent, string name, VisualAssetKey key, Vector3 localPosition)
        { return CreateVisual(parent, name, key, localPosition, PresentationScaleProfile.RuntimeDefault.WorldSize(key)); }

        internal static GameObject CreateVisual(Transform parent, string name, VisualAssetKey key, Vector3 localPosition, Vector2 size, Color? tint = null)
        {
            var slot = new GameObject(name + " Presentation Slot"); slot.transform.SetParent(parent, false); slot.transform.localPosition = localPosition;
            if (ServiceRegistry.Visuals != null)
            {
                var presentation = ServiceRegistry.Visuals.CreatePresentation(key, slot.transform, name,
                    fallbackParent => CreateTemporaryFallback(fallbackParent, name, key, size, tint));
                if (presentation == null) { Object.Destroy(slot); return null; }
                return slot;
            }
            var fallback = CreateTemporaryFallback(slot.transform, name, key, size, tint);
            if (fallback == null) { Object.Destroy(slot); return null; }
            return slot;
        }

        static GameObject CreateTemporaryFallback(Transform parent, string name, VisualAssetKey key, Vector2 size, Color? tint)
        {
            if (!PresentationRuntime.PlaceholderArtEnabled) return null;
            Sprite sprite; if (!TemporaryArtLibrary.TryGet(key, SpriteDirection.SE, out sprite)) return null;
            var visual = new GameObject(name + " Temporary Fallback"); visual.transform.SetParent(parent, false);
            visual.AddComponent<SpriteWorldVisual>().Initialize(sprite, size, SpriteSortCategory.WorldObject, false, key);
            if (tint.HasValue) visual.GetComponent<SpriteRenderer>().color = tint.Value;
            return visual;
        }

        static Vector3 Grounded(float x, float z)
        { return new Vector3(x, StylizedGeometry.GroundHeight(x, z), z); }
    }

}
