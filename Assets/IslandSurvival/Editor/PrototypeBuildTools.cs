using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace IslandSurvival.Editor
{
    public static class PrototypeBuildTools
    {
        const string ScenePath = "Assets/Scenes/Prototype_StartingBeach.unity";

        [MenuItem("Island Survival/Validate Prototype 0.1")]
        public static void Validate()
        {
            UrpProjectSetup.EnsurePipeline();
            var failures = 0;
            failures += RequireFile(ScenePath);
            failures += RequireFile("Assets/IslandSurvival/Core/PrototypeBootstrap.cs");
            failures += RequireFile("Assets/IslandSurvival/Input/MobileInput.cs");
            failures += RequireFile("Assets/IslandSurvival/Save/SaveSystem.cs");
            failures += RequireLandscape();
            failures += RequireSceneInBuild();
            failures += RequireOnlyStartingBeachScene();
            failures += RequireFile("Assets/WebGLTemplates/IslandSurvivalPWA/index.html");
            failures += RequireFile("Assets/WebGLTemplates/IslandSurvivalPWA/manifest.json");
            failures += RequireFile("Assets/WebGLTemplates/IslandSurvivalPWA/service-worker.js");
            failures += RequireFile("Assets/WebGLTemplates/IslandSurvivalPWA/icon.svg");
            failures += RequireSingleUrpGlobalSettings();
            failures += LocalizationCatalogValidator.ValidateResources();
            failures += OriginDemoValidator.Validate();
            failures += VisualFoundationValidator.Validate();
            failures += TwoPointFiveDValidator.Validate();
            failures += StartingBeach2DValidator.Validate();
            failures += MinimapValidator.Validate();
            if (failures > 0) throw new InvalidOperationException("Prototype validation failed with " + failures + " issue(s). See Console.");
            Debug.Log("Prototype 0.1 project validation passed.");
        }

        public static void RunEditModeTests()
        {
            Validate();
            Debug.Log("Use Unity Test Runner or -runTests -testPlatform EditMode for the automated model tests.");
        }

        public static void BuildAndroidDevelopment()
        {
            PlatformBuildProfiles.BuildAndroid();
        }

        static int RequireFile(string path)
        {
            if (File.Exists(path)) return 0;
            Debug.LogError("Missing required file: " + path); return 1;
        }
        static int RequireLandscape()
        {
            if ((PlayerSettings.defaultInterfaceOrientation == UIOrientation.LandscapeLeft || PlayerSettings.defaultInterfaceOrientation == UIOrientation.LandscapeRight || PlayerSettings.defaultInterfaceOrientation == UIOrientation.AutoRotation) && !PlayerSettings.allowedAutorotateToPortrait && !PlayerSettings.allowedAutorotateToPortraitUpsideDown) return 0;
            Debug.LogError("Default orientation must be landscape."); return 1;
        }
        static int RequireSceneInBuild()
        {
            foreach (var scene in EditorBuildSettings.scenes) if (scene.enabled && scene.path == ScenePath) return 0;
            Debug.LogError("Prototype_StartingBeach must be enabled in Build Settings."); return 1;
        }
        static int RequireOnlyStartingBeachScene()
        {
            var enabledCount = 0;
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled) continue;
                enabledCount++;
                if (scene.path != ScenePath) { Debug.LogError("Prototype Web milestone cannot include scene: " + scene.path); return 1; }
            }
            if (enabledCount == 1) return 0;
            Debug.LogError("Prototype Web milestone must contain exactly one enabled scene."); return 1;
        }
        static int RequireSingleUrpGlobalSettings()
        {
            var assets = AssetDatabase.FindAssets("t:UniversalRenderPipelineGlobalSettings");
            if (assets.Length == 1) return 0;
            Debug.LogError("Expected exactly one URP Global Settings asset, found " + assets.Length + ". Keep Unity's generated asset and remove duplicates only after checking Graphics Settings.");
            return 1;
        }
    }
}
