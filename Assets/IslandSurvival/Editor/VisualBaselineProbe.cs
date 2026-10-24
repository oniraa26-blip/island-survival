using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace IslandSurvival.Editor
{
    public static class VisualBaselineProbe
    {
        const double DurationSeconds = 10.0;
        static readonly List<float> FrameTimes = new List<float>();
        static double startedAt;
        static int lastFrame;

        [MenuItem("Island Survival/Visual/Capture 10 Second Baseline")]
        public static void Capture()
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.LogError("Enter Play Mode, stand at the documented Starting Beach baseline camera, then run this command.");
                return;
            }
            FrameTimes.Clear();
            startedAt = EditorApplication.timeSinceStartup;
            lastFrame = Time.frameCount;
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
            if (Time.frameCount == lastFrame) return;
            lastFrame = Time.frameCount;
            var delta = Time.unscaledDeltaTime;
            if (delta > 0f && delta < 1f) FrameTimes.Add(delta);
        }

        static void Report()
        {
            if (FrameTimes.Count == 0) { Debug.LogWarning("Visual baseline capture ended without frame samples."); return; }
            var elapsed = Mathf.Max(0.0001f, Sum(FrameTimes));
            var averageFps = FrameTimes.Count / elapsed;
            FrameTimes.Sort();
            var slowFrameCount = Mathf.Clamp(Mathf.CeilToInt(FrameTimes.Count * 0.05f), 1, FrameTimes.Count);
            var slowFrameTotal = 0f;
            for (var i = FrameTimes.Count - slowFrameCount; i < FrameTimes.Count; i++) slowFrameTotal += FrameTimes[i];
            var lowFivePercentFps = slowFrameCount / Mathf.Max(0.0001f, slowFrameTotal);
            var report = "VISUAL_BASELINE_04"
                + " frames=" + FrameTimes.Count
                + " measuredSeconds=" + elapsed.ToString("F2")
                + " avgFps=" + averageFps.ToString("F1")
                + " low5PercentFps=" + lowFivePercentFps.ToString("F1")
                + " totalAllocatedMB=" + (Profiler.GetTotalAllocatedMemoryLong() / (1024f * 1024f)).ToString("F1")
                + ". Batches, triangles and GPU timing require the Unity Profiler. Editor figures are diagnostic only; repeat in the Web build and on iPhone.";
            Debug.Log(report);
        }

        static float Sum(List<float> values)
        {
            var total = 0f; foreach (var value in values) total += value;
            return total;
        }
    }
}
