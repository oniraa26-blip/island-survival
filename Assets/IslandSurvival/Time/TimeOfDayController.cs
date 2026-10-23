using System;
using IslandSurvival.Core;
using UnityEngine;

namespace IslandSurvival.TimeSystem
{
    public sealed class TimeOfDayController : MonoBehaviour
    {
        Light sun;
        PrototypeTuning tuning;
        [Range(0f, 1f)] public float normalizedTime = 0.3f;
        public string PhaseKey
        {
            get
            {
                if (normalizedTime < 0.25f) return "phase.morning";
                if (normalizedTime < 0.58f) return "phase.day";
                if (normalizedTime < 0.75f) return "phase.evening";
                return "phase.night";
            }
        }
        public event Action Changed;
        public void Initialize(Light sun, PrototypeTuning tuning) { this.sun = sun; this.tuning = tuning; Apply(); }
        void Update()
        {
            if (sun == null) return;
            normalizedTime = Mathf.Repeat(normalizedTime + Time.deltaTime / tuning.dayLengthSeconds, 1f);
            Apply(); if (Changed != null) Changed();
        }
        public void AdvanceToMorning() { normalizedTime = 0.23f; Apply(); }
        public void Restore(float value) { normalizedTime = Mathf.Repeat(value, 1f); Apply(); }
        void Apply()
        {
            if (sun == null) return;
            var angle = normalizedTime * 360f - 90f;
            sun.transform.rotation = Quaternion.Euler(angle, 145f, 0f);
            var daylight = Mathf.Clamp01(Vector3.Dot(-sun.transform.forward, Vector3.up) * 1.5f + 0.1f);
            sun.intensity = Mathf.Lerp(0.04f, 1.15f, daylight);
            sun.color = Color.Lerp(new Color(1f, 0.52f, 0.32f), new Color(1f, 0.95f, 0.82f), daylight);
            RenderSettings.ambientIntensity = Mathf.Lerp(0.18f, 1f, daylight);
            RenderSettings.fogColor = Color.Lerp(new Color(0.035f, 0.07f, 0.16f), new Color(0.45f, 0.72f, 0.78f), daylight);
        }
    }
}
