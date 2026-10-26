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

            CreateVisual(root, "Crate", VisualAssetKey.PropCrate, new Vector3(17f, 0f, 15f), new Vector2(1.5f, 1.5f));
            CreateVisual(root, "Barrel", VisualAssetKey.Barrel, new Vector3(19f, 0f, 17f), new Vector2(1.3f, 1.5f));
            CreateVisual(root, "Driftwood", VisualAssetKey.PropDriftwood, new Vector3(-11f, 0f, 12f), new Vector2(2.5f, 1.5f));
            CreateVisual(root, "Flower Accent A", VisualAssetKey.FlowerA, new Vector3(-19f, 0f, 31f), new Vector2(1.5f, 1.1f));
            CreateVisual(root, "Flower Accent B", VisualAssetKey.FlowerA, new Vector3(22f, 0f, 38f), new Vector2(1.25f, 0.95f));

            var house = new GameObject("House L1 Skin Preview"); house.transform.SetParent(root, false);
            house.transform.localPosition = Grounded(31f, 55f);
            var preview = house.AddComponent<BuildingSkinPreview>();
            preview.Initialize();

            var farm = new GameObject("Farming Visual Proof").transform;
            farm.SetParent(root, false); farm.localPosition = Grounded(-29f, 53f);
            CreateVisual(farm, "Dry Soil", VisualAssetKey.FarmSoilDry, Vector3.zero, new Vector2(2.8f, 1.8f), new Color(0.78f, 0.58f, 0.35f));
            CreateVisual(farm, "Wet Soil", VisualAssetKey.FarmSoilWet, new Vector3(3.2f, 0f, 0f), new Vector2(2.8f, 1.8f), new Color(0.48f, 0.32f, 0.2f));
            CreateVisual(farm, "Crop Stage 1", VisualAssetKey.CropTestStage1, new Vector3(0f, 0f, 2.4f), new Vector2(2.3f, 1.45f));
            CreateVisual(farm, "Crop Stage 2", VisualAssetKey.CropTestStage2, new Vector3(2.7f, 0f, 2.4f), new Vector2(2.5f, 1.6f));
            CreateVisual(farm, "Crop Stage 3", VisualAssetKey.CropTestStage3, new Vector3(5.5f, 0f, 2.4f), new Vector2(2.7f, 1.75f));
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
