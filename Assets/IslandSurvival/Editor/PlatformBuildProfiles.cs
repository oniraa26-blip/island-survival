using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;

namespace IslandSurvival.Editor
{
    public static class PlatformBuildProfiles
    {
        const string ScenePath = "Assets/Scenes/Prototype_StartingBeach.unity";

        [MenuItem("Island Survival/Build/iOS Native (Xcode)")]
        public static void BuildIOS()
        {
            ApplyIOS();
            Build(new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = "Builds/iOS", target = BuildTarget.iOS, options = BuildOptions.Development });
        }

        [MenuItem("Island Survival/Build/Web PWA Mobile Test")]
        public static void BuildWebPWA()
        {
            ApplyWeb();
            Build(new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = "Builds/WebPWA", target = BuildTarget.WebGL, options = BuildOptions.Development });
        }

        [MenuItem("Island Survival/Build/Android Native")]
        public static void BuildAndroid()
        {
            ApplyAndroid();
            Build(new BuildPlayerOptions { scenes = new[] { ScenePath }, locationPathName = "Builds/Android/IslandSurvivalPrototype.apk", target = BuildTarget.Android, options = BuildOptions.Development });
        }

        public static void ApplyIOS()
        {
            ApplyShared();
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new[] { GraphicsDeviceType.Metal });
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            PlayerSettings.iOS.requiresFullScreen = true;
            PlayerSettings.SetMobileMTRendering(BuildTargetGroup.iOS, true);
        }

        public static void ApplyWeb()
        {
            ApplyShared();
            PlayerSettings.WebGL.template = "PROJECT:IslandSurvivalPWA";
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;
            // Derive the named target from WebGL so this survives the Web platform naming transition.
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.FromBuildTargetGroup(BuildTargetGroup.WebGL), ManagedStrippingLevel.Medium);
        }

        public static void ApplyAndroid()
        {
            ApplyShared();
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, true);
        }

        static void ApplyShared()
        {
            UrpProjectSetup.EnsurePipeline();
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.gcIncremental = true;
        }

        static void Build(BuildPlayerOptions options)
        {
            PrototypeBuildTools.Validate();
            var directory = options.target == BuildTarget.Android ? Path.GetDirectoryName(options.locationPathName) : options.locationPathName;
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException(options.target + " build failed: " + report.summary.result);
            Debug.Log(options.target + " build succeeded: " + report.summary.outputPath);
        }
    }
}
