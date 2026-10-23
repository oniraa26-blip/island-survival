using UnityEngine;

namespace IslandSurvival.World
{
    [RequireComponent(typeof(MeshFilter))]
    public sealed class OceanSurface : MonoBehaviour
    {
        Mesh mesh;
        Vector3[] baseVertices;
        Vector3[] animatedVertices;
        float amplitude;
        float speed;
        float nextUpdate;

        public void Initialize(float waveAmplitude, float waveSpeed)
        {
            mesh = GetComponent<MeshFilter>().mesh;
            baseVertices = mesh.vertices;
            animatedVertices = new Vector3[baseVertices.Length];
            amplitude = waveAmplitude; speed = waveSpeed;
        }

        void Update()
        {
            if (mesh == null || UnityEngine.Time.unscaledTime < nextUpdate) return;
            nextUpdate = UnityEngine.Time.unscaledTime + 0.083f;
            var time = UnityEngine.Time.time * speed;
            for (var i = 0; i < baseVertices.Length; i++)
            {
                var source = baseVertices[i];
                source.y += (Mathf.Sin(source.x * 0.09f + time) + Mathf.Sin(source.z * 0.14f - time * 0.7f)) * amplitude;
                animatedVertices[i] = source;
            }
            mesh.vertices = animatedVertices;
            mesh.RecalculateNormals();
        }
    }
}
