using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
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
            Validate();
            Directory.CreateDirectory("Builds/Android");
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Android/IslandSurvivalPrototype.apk",
                target = BuildTarget.Android,
                options = BuildOptions.Development
            };
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Android build failed: " + report.summary.result);
            Debug.Log("Android build succeeded: " + report.summary.outputPath);
        }

        static int RequireFile(string path)
        {
            if (File.Exists(path)) return 0;
            Debug.LogError("Missing required file: " + path); return 1;
        }
        static int RequireLandscape()
        {
            if (PlayerSettings.defaultInterfaceOrientation == UIOrientation.LandscapeLeft || PlayerSettings.defaultInterfaceOrientation == UIOrientation.LandscapeRight) return 0;
            Debug.LogError("Default orientation must be landscape."); return 1;
        }
        static int RequireSceneInBuild()
        {
            foreach (var scene in EditorBuildSettings.scenes) if (scene.enabled && scene.path == ScenePath) return 0;
            Debug.LogError("Prototype_StartingBeach must be enabled in Build Settings."); return 1;
        }
    }
}
