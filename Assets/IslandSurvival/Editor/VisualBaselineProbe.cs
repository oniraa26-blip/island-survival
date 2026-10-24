using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace IslandSurvival.Editor
{
    public static class VisualBaselineProbe
    {
        const double DurationSeconds = 10.0;
        static readonly List<float> FpsSamples = new List<float>();
        static readonly List<int> BatchSamples = new List<int>();
        static readonly List<int> TriangleSamples = new List<int>();
        static double startedAt;
        static double nextSample;

        [MenuItem("Island Survival/Visual/Capture 10 Second Baseline")]
        public static void Capture()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("Enter Play Mode, stand at the documented Starting Beach baseline camera, then run this command.");
                return;
            }
            FpsSamples.Clear(); BatchSamples.Clear(); TriangleSamples.Clear();
            startedAt = EditorApplication.timeSinceStartup; nextSample = startedAt;
            EditorApplication.update -= Sample; EditorApplication.update += Sample;
            Debug.Log("Visual baseline capture started for 10 seconds. Do not move the camera.");
        }

        static void Sample()
        {
            var now = EditorApplication.timeSinceStartup;
            if (!EditorApplication.isPlaying || now - startedAt >= DurationSeconds)
            {
                EditorApplication.update -= Sample; Report(); return;
            }
            if (now < nextSample) return;
            nextSample = now + 0.25;
            FpsSamples.Add(UnityStats.fps); BatchSamples.Add(UnityStats.batches); TriangleSamples.Add(UnityStats.triangles);
        }

        static void Report()
        {
            if (FpsSamples.Count == 0) { Debug.LogWarning("Visual baseline capture ended without samples."); return; }
            FpsSamples.Sort(); BatchSamples.Sort(); TriangleSamples.Sort();
            var p05Index = Mathf.Clamp(Mathf.FloorToInt(FpsSamples.Count * 0.05f), 0, FpsSamples.Count - 1);
            var report = "VISUAL_BASELINE_04"
                + " samples=" + FpsSamples.Count
                + " avgFps=" + Average(FpsSamples).ToString("F1")
                + " low5PercentFps=" + FpsSamples[p05Index].ToString("F1")
                + " medianBatches=" + BatchSamples[BatchSamples.Count / 2]
                + " medianTriangles=" + TriangleSamples[TriangleSamples.Count / 2]
                + " totalAllocatedMB=" + (Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f)).ToString("F1")
                + ". Editor figures are diagnostic only; repeat in the Web build and on iPhone.";
            Debug.Log(report);
        }

        static float Average(List<float> values)
        {
            var total = 0f; foreach (var value in values) total += value;
            return total / values.Count;
        }
    }
}
