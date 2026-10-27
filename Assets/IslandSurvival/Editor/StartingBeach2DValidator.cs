using System;
using IslandSurvival.Core;
using IslandSurvival.Presentation;
using IslandSurvival.Visual;
using IslandSurvival.World;
using IslandSurvival.UI;
using UnityEditor;
using UnityEngine;

namespace IslandSurvival.Editor
{
    public static class StartingBeach2DValidator
    {
        static readonly VisualAssetKey[] Required =
        {
            VisualAssetKey.PlayerCharacter, VisualAssetKey.PalmA, VisualAssetKey.PalmB, VisualAssetKey.PalmC,
            VisualAssetKey.TropicalTreeA, VisualAssetKey.BushA, VisualAssetKey.GrassA, VisualAssetKey.FlowerA,
            VisualAssetKey.RockSmall, VisualAssetKey.RockMedium, VisualAssetKey.RockLarge, VisualAssetKey.RockCoastal,
            VisualAssetKey.Shipwreck, VisualAssetKey.HouseLevel1Default, VisualAssetKey.HouseLevel1Tropical,
            VisualAssetKey.FarmSoilDry, VisualAssetKey.FarmSoilWet,
            VisualAssetKey.CropTestStage1, VisualAssetKey.CropTestStage2, VisualAssetKey.CropTestStage3,
            VisualAssetKey.ENV_PALM_A, VisualAssetKey.ENV_PALM_B, VisualAssetKey.ENV_PALM_C,
            VisualAssetKey.ENV_TREE_A, VisualAssetKey.ENV_TREE_B, VisualAssetKey.ENV_BUSH_A, VisualAssetKey.ENV_BUSH_B,
            VisualAssetKey.ENV_GRASS_A, VisualAssetKey.ENV_GRASS_B, VisualAssetKey.ENV_GRASS_C,
            VisualAssetKey.ENV_FLOWER_A, VisualAssetKey.ENV_FLOWER_B,
            VisualAssetKey.ENV_ROCK_SMALL_A, VisualAssetKey.ENV_ROCK_SMALL_B, VisualAssetKey.ENV_ROCK_MEDIUM_A, VisualAssetKey.ENV_ROCK_LARGE_A,
            VisualAssetKey.ENV_SHIPWRECK, VisualAssetKey.PROP_CRATE, VisualAssetKey.PROP_BARREL, VisualAssetKey.PROP_DRIFTWOOD, VisualAssetKey.PropRope
        };

        [MenuItem("Island Survival/Visual/Validate 2.5D Starting Beach")]
        public static void ValidateMenu()
        {
            var failures = Validate();
            if (failures > 0) throw new InvalidOperationException("2.5D Starting Beach validation failed with " + failures + " issue(s). See Console.");
            Debug.Log("2.5D Starting Beach static asset validation passed. Play Mode and target-device validation are still required.");
        }

        public static int Validate()
        {
            var failures = 0;
            if (!PresentationRuntime.IsTwoPointFiveD) { Debug.LogError("PresentationMode.TwoPointFiveD must be active."); failures++; }
            if (!PresentationRuntime.PlaceholderArtEnabled) { Debug.LogError("Phase 02 placeholder art is disabled."); failures++; }
            var tuning = new PrototypeTuning();
            if (tuning.isometricSize < 10f || Mathf.Abs(StylizedGeometry.ShoreZ(0f)) > tuning.isometricSize)
            { Debug.LogError("Initial 2.5D viewport does not include enough of the Starting Beach shoreline/composition."); failures++; }
            foreach (var key in Required)
            {
                Sprite sprite;
                if (!TemporaryArtLibrary.TryGet(key, SpriteDirection.SE, out sprite) || sprite == null)
                { Debug.LogError("Missing Phase 02 sprite resolution for " + key + ". Check Resources/Art2D/Temporary or the catalog substitution path."); failures++; }
            }
            foreach (SpriteDirection direction in Enum.GetValues(typeof(SpriteDirection)))
            {
                Sprite first; Sprite second;
                if (!TemporaryArtLibrary.TryGetWalk(direction, 0, out first) || !TemporaryArtLibrary.TryGetWalk(direction, 1, out second))
                { Debug.LogError("Missing explorer walk frames for direction " + direction + "."); failures++; }
            }
            if (Enum.GetValues(typeof(SpriteSortCategory)).Length < 7) { Debug.LogError("Required 2.5D sorting categories are missing."); failures++; }
            var scaleProfile = PresentationScaleProfile.CreateDefault();
            var palmRatio = scaleProfile.WorldSize(VisualAssetKey.ENV_PALM_A).y / PresentationScaleProfile.PlayerWorldHeight;
            var treeRatio = scaleProfile.WorldSize(VisualAssetKey.ENV_TREE_A).y / PresentationScaleProfile.PlayerWorldHeight;
            if (palmRatio < 2f || palmRatio > 2.8f) { Debug.LogError("Normal palm presentation scale is outside the locked range."); failures++; }
            if (treeRatio < 2f || treeRatio > 3f) { Debug.LogError("Normal tree presentation scale is outside the locked range."); failures++; }
            foreach (var key in new[] { VisualAssetKey.ENV_PALM_A, VisualAssetKey.ENV_TREE_A, VisualAssetKey.HouseLevel1Default, VisualAssetKey.ENV_ROCK_MEDIUM_A, VisualAssetKey.ENV_ROCK_LARGE_A })
            {
                var footprint = PresentationFootprintPolicy.For(key);
                if (!PresentationFootprintPolicy.IsBaseOnly(key, footprint, scaleProfile.WorldSize(key)))
                { Debug.LogError("Collider footprint is not constrained to the visual base: " + key); failures++; }
            }
            for (var i = 0; i < StartingBeachComposition.PalmPositions.Length; i++)
                if (!StartingBeachComposition.IsSpawnClear(StartingBeachComposition.PalmPositions[i], 0.6f)) { Debug.LogError("Palm overlaps the protected spawn clearing."); failures++; }
            for (var i = 0; i < StartingBeachComposition.RockClusterCenters.Length; i++)
                if (!StartingBeachComposition.IsSpawnClear(StartingBeachComposition.RockClusterCenters[i], 1.4f)) { Debug.LogError("Rock overlaps the protected spawn clearing."); failures++; }
            for (var i = 0; i < StartingBeachComposition.TropicalTreePositions.Length; i++)
                if (!StartingBeachComposition.IsSpawnClear(StartingBeachComposition.TropicalTreePositions[i], 0.75f)) { Debug.LogError("Tropical tree overlaps the protected spawn clearing."); failures++; }
            for (var i = 0; i < StartingBeachComposition.BushPositions.Length; i++)
                if (!StartingBeachComposition.IsSpawnClear(StartingBeachComposition.BushPositions[i], 0.5f)) { Debug.LogError("Bush cluster overlaps the protected spawn clearing."); failures++; }
            if (!StartingBeachComposition.IsSpawnClear(StartingBeachComposition.Shipwreck, 3f)) { Debug.LogError("Shipwreck overlaps the protected spawn clearing."); failures++; }
            if (!StartingBeachComposition.IsSpawnClear(StartingBeachComposition.House, 3f)) { Debug.LogError("House overlaps the protected spawn clearing."); failures++; }
            var shore = StylizedGeometry.ShoreZ(0f);
            var wetBand = StylizedGeometry.TerrainBlendWeights(0f, shore + 2f);
            var dryBand = StylizedGeometry.TerrainBlendWeights(-10f, 10f);
            var grassBand = StylizedGeometry.TerrainBlendWeights(18f, 45f);
            const float pathZ = 35f;
            var dirtBand = StylizedGeometry.TerrainBlendWeights(StylizedGeometry.DirtPathCenterX(pathZ), pathZ);
            if (wetBand.g <= wetBand.r || dryBand.r <= dryBand.g || grassBand.b <= grassBand.r || dirtBand.a <= dirtBand.b)
            { Debug.LogError("Ground masks do not resolve into distinct shoreline, sand, grass and dirt-path bands."); failures++; }
            if (Mathf.Abs(StartingBeachComposition.House.x - StylizedGeometry.DirtPathCenterX(StartingBeachComposition.House.z)) >= 12f)
            { Debug.LogError("House L1 is no longer composed beside the curved inland path."); failures++; }
            var playerSize = PresentationScaleProfile.ApplyReadabilityScale(VisualAssetKey.PlayerCharacter, scaleProfile.WorldSize(VisualAssetKey.PlayerCharacter));
            if (playerSize.y < PresentationScaleProfile.PlayerWorldHeight * 1.15f || playerSize.y > PresentationScaleProfile.PlayerWorldHeight * 1.25f)
            { Debug.LogError("Player presentation readability scale must remain between 115% and 125%."); failures++; }
            UnityEngine.Object.DestroyImmediate(scaleProfile);
            if (Resources.Load<Shader>("Visual/Shaders/TropicalWater") == null) { Debug.LogError("TropicalWater shader is missing."); failures++; }
            var foundStartingBeach = false;
            foreach (var id in OriginDemoZones.All) if (id == OriginDemoZones.StartingBeach) foundStartingBeach = true;
            if (!foundStartingBeach || OriginDemoZones.StartingBeach != "starting_beach") { Debug.LogError("Starting Beach stable zone identity changed."); failures++; }
            if (Application.isPlaying)
            {
                var minimap = UnityEngine.Object.FindFirstObjectByType<MinimapController>();
                if (minimap == null || minimap.Registry == null || minimap.Player == null) { Debug.LogError("Runtime minimap wiring regressed."); failures++; }
            }
            return failures;
        }
    }
}
