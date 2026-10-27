using System;
using IslandSurvival.Core;
using IslandSurvival.Presentation;
using IslandSurvival.Visual;
using IslandSurvival.World;
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
            VisualAssetKey.ENV_SHIPWRECK, VisualAssetKey.PROP_CRATE, VisualAssetKey.PROP_BARREL, VisualAssetKey.PROP_DRIFTWOOD
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
            if (palmRatio < 1.6f || palmRatio > 2.2f) { Debug.LogError("Normal palm presentation scale is outside the locked range."); failures++; }
            if (treeRatio < 1.5f || treeRatio > 2f) { Debug.LogError("Normal tree presentation scale is outside the locked range."); failures++; }
            UnityEngine.Object.DestroyImmediate(scaleProfile);
            if (Resources.Load<Shader>("Visual/Shaders/TropicalWater") == null) { Debug.LogError("TropicalWater shader is missing."); failures++; }
            var foundStartingBeach = false;
            foreach (var id in OriginDemoZones.All) if (id == OriginDemoZones.StartingBeach) foundStartingBeach = true;
            if (!foundStartingBeach || OriginDemoZones.StartingBeach != "starting_beach") { Debug.LogError("Starting Beach stable zone identity changed."); failures++; }
            return failures;
        }
    }
}
