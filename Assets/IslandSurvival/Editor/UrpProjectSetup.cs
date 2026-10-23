using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace IslandSurvival.Editor
{
    [InitializeOnLoad]
    public static class UrpProjectSetup
    {
        const string SettingsFolder = "Assets/Settings";
        const string RendererPath = SettingsFolder + "/MobileRenderer.asset";
        const string PipelinePath = SettingsFolder + "/MobileURP.asset";

        static UrpProjectSetup() { EditorApplication.delayCall += EnsurePipeline; }

        [MenuItem("Island Survival/Ensure Mobile URP Settings")]
        public static void EnsurePipeline()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!AssetDatabase.IsValidFolder(SettingsFolder)) AssetDatabase.CreateFolder("Assets", "Settings");
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                renderer.name = "MobileRenderer"; AssetDatabase.CreateAsset(renderer, RendererPath);
            }
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
            if (pipeline == null)
            {
                pipeline = ScriptableObject.CreateInstance<UniversalRenderPipelineAsset>();
                pipeline.name = "MobileURP";
                var serialized = new SerializedObject(pipeline);
                var list = serialized.FindProperty("m_RendererDataList");
                list.arraySize = 1; list.GetArrayElementAtIndex(0).objectReferenceValue = renderer;
                serialized.FindProperty("m_DefaultRendererIndex").intValue = 0;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                AssetDatabase.CreateAsset(pipeline, PipelinePath);
            }
            pipeline.renderScale = 1f; pipeline.msaaSampleCount = 2; pipeline.supportsHDR = false;
            GraphicsSettings.renderPipelineAsset = pipeline; QualitySettings.renderPipeline = pipeline;
            EditorUtility.SetDirty(pipeline); AssetDatabase.SaveAssets();
        }
    }
}
