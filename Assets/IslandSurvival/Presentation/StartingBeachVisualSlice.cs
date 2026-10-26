using IslandSurvival.Visual;
using IslandSurvival.World;
using UnityEngine;

namespace IslandSurvival.Presentation
{
    public static class StartingBeachVisualSlice
    {
        public static void Create(Transform parent)
        {
            if (!PresentationRuntime.IsTwoPointFiveD || !PresentationRuntime.PlaceholderArtEnabled) return;
            var root = new GameObject("2.5D Starting Beach Visual Proofs").transform;
            root.SetParent(parent, false);

            var created = 0;
            created += CreateVisual(root, "Palm A Showcase", VisualAssetKey.PalmA, Grounded(-10f, 11f), new Vector2(4.2f, 7.2f)) != null ? 1 : 0;
            created += CreateVisual(root, "Palm B Showcase", VisualAssetKey.PalmB, Grounded(10f, 15f), new Vector2(3.8f, 5.8f)) != null ? 1 : 0;
            created += CreateVisual(root, "Palm C Showcase", VisualAssetKey.PalmC, Grounded(-15f, 20f), new Vector2(5f, 6.5f)) != null ? 1 : 0;
            created += CreateVisual(root, "Tropical Tree Showcase", VisualAssetKey.TropicalTreeA, Grounded(15f, 23f), new Vector2(5f, 6.2f)) != null ? 1 : 0;
            created += CreateVisual(root, "Bush Showcase", VisualAssetKey.BushA, Grounded(-5f, 13f), new Vector2(2.3f, 1.7f)) != null ? 1 : 0;
            created += CreateVisual(root, "Grass Showcase", VisualAssetKey.GrassA, Grounded(4f, 12f), new Vector2(1.5f, 1.1f)) != null ? 1 : 0;
            created += CreateVisual(root, "Small Rock Showcase", VisualAssetKey.RockSmall, Grounded(-6f, 7f), new Vector2(1.1f, 0.75f)) != null ? 1 : 0;
            created += CreateVisual(root, "Medium Rock Showcase", VisualAssetKey.RockMedium, Grounded(6f, 10f), new Vector2(2.3f, 1.55f)) != null ? 1 : 0;
            created += CreateVisual(root, "Coastal Rock Showcase", VisualAssetKey.RockCoastal, Grounded(-14f, 5f), new Vector2(3.8f, 2.8f)) != null ? 1 : 0;
            created += CreateVisual(root, "Crate", VisualAssetKey.PropCrate, Grounded(8f, 9f), new Vector2(1.5f, 1.5f)) != null ? 1 : 0;
            created += CreateVisual(root, "Barrel", VisualAssetKey.Barrel, Grounded(10f, 11f), new Vector2(1.3f, 1.5f)) != null ? 1 : 0;
            created += CreateVisual(root, "Driftwood", VisualAssetKey.PropDriftwood, Grounded(-7f, 7f), new Vector2(2.5f, 1.5f)) != null ? 1 : 0;
            created += CreateVisual(root, "Flower Accent A", VisualAssetKey.FlowerA, Grounded(-9f, 13f), new Vector2(1.5f, 1.1f)) != null ? 1 : 0;
            created += CreateVisual(root, "Flower Accent B", VisualAssetKey.FlowerA, Grounded(7f, 16f), new Vector2(1.25f, 0.95f)) != null ? 1 : 0;

            var house = new GameObject("House L1 Skin Preview"); house.transform.SetParent(root, false);
            house.transform.localPosition = Grounded(11f, 20f);
            var preview = house.AddComponent<BuildingSkinPreview>();
            preview.Initialize();

            var farm = new GameObject("Farming Visual Proof").transform;
            farm.SetParent(root, false); farm.localPosition = Grounded(-12f, 19f);
            CreateVisual(farm, "Dry Soil", VisualAssetKey.FarmSoilDry, Vector3.zero, new Vector2(2.8f, 1.8f), new Color(0.78f, 0.58f, 0.35f));
            CreateVisual(farm, "Wet Soil", VisualAssetKey.FarmSoilWet, new Vector3(3.2f, 0f, 0f), new Vector2(2.8f, 1.8f), new Color(0.48f, 0.32f, 0.2f));
            CreateVisual(farm, "Crop Stage 1", VisualAssetKey.CropTestStage1, new Vector3(0f, 0f, 2.4f), new Vector2(2.3f, 1.45f));
            CreateVisual(farm, "Crop Stage 2", VisualAssetKey.CropTestStage2, new Vector3(2.7f, 0f, 2.4f), new Vector2(2.5f, 1.6f));
            CreateVisual(farm, "Crop Stage 3", VisualAssetKey.CropTestStage3, new Vector3(5.5f, 0f, 2.4f), new Vector2(2.7f, 1.75f));
            Debug.Log("2.5D Starting Beach visual slice created with " + created + " nearby prop/accent sprites; house and farming proofs attached.");
        }

        internal static GameObject CreateVisual(Transform parent, string name, VisualAssetKey key, Vector3 localPosition, Vector2 size, Color? tint = null)
        {
            Sprite sprite;
            if (!TemporaryArtLibrary.TryGet(key, SpriteDirection.SE, out sprite)) return null;
            var visual = new GameObject(name); visual.transform.SetParent(parent, false); visual.transform.localPosition = localPosition;
            var worldVisual = visual.AddComponent<SpriteWorldVisual>();
            worldVisual.Initialize(sprite, size, SpriteSortCategory.WorldObject, false, key);
            if (tint.HasValue) visual.GetComponent<SpriteRenderer>().color = tint.Value;
            return visual;
        }

        static Vector3 Grounded(float x, float z)
        { return new Vector3(x, StylizedGeometry.GroundHeight(x, z), z); }
    }

}
