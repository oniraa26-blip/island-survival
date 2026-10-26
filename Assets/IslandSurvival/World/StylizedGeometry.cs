using System.Collections.Generic;
using UnityEngine;

namespace IslandSurvival.World
{
    public static class StylizedGeometry
    {
        static Mesh terrain;
        static Mesh grass;
        static Mesh ocean;
        static Mesh palmTrunk;
        static Mesh palmLeaf;
        static Mesh branch;
        static Mesh plank;
        static readonly Dictionary<int, Mesh> Rocks = new Dictionary<int, Mesh>();

        public static float GroundHeight(float x, float z)
        {
            var inland = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(18f, 100f, z));
            var beachRipple = Mathf.Sin(x * 0.12f + z * 0.08f) * 0.11f + Mathf.Cos(x * 0.055f - z * 0.09f) * 0.08f;
            var rise = inland * (2.35f + Mathf.Sin(x * 0.075f) * 0.55f + Mathf.Cos(z * 0.06f) * 0.35f);
            return beachRipple + rise;
        }

        public static float ShoreZ(float x)
        {
            return -6f + Mathf.Sin(x * 0.09f) * 2.3f + Mathf.Sin(x * 0.035f + 1.2f) * 1.4f;
        }

        public static Mesh Terrain()
        {
            if (terrain == null) terrain = CreateGround(false);
            return terrain;
        }

        public static Mesh GrassTransition()
        {
            if (grass == null) grass = CreateGround(true);
            return grass;
        }

        public static Mesh Ocean()
        {
            if (ocean != null) return ocean;
            const int xSegments = 24, zSegments = 10;
            var vertices = new List<Vector3>((xSegments + 1) * (zSegments + 1));
            var triangles = new List<int>(xSegments * zSegments * 6);
            for (var z = 0; z <= zSegments; z++)
            for (var x = 0; x <= xSegments; x++)
            {
                var px = Mathf.Lerp(-205f, 205f, x / (float)xSegments);
                var pz = Mathf.Lerp(-150f, 3f, z / (float)zSegments);
                vertices.Add(new Vector3(px, -0.62f, pz));
            }
            AddGridTriangles(triangles, xSegments, zSegments);
            ocean = Build("Stylized Ocean Grid", vertices, triangles);
            return ocean;
        }

        public static Mesh Rock(int variant)
        {
            Mesh mesh;
            if (Rocks.TryGetValue(variant, out mesh)) return mesh;
            const int segments = 10, rings = 5;
            var vertices = new List<Vector3>((segments + 1) * (rings + 1));
            var triangles = new List<int>(segments * rings * 6);
            for (var ring = 0; ring <= rings; ring++)
            {
                var v = ring / (float)rings;
                var latitude = Mathf.Lerp(-Mathf.PI * 0.5f, Mathf.PI * 0.5f, v);
                var ringRadius = Mathf.Cos(latitude);
                for (var segment = 0; segment <= segments; segment++)
                {
                    var u = segment / (float)segments;
                    var angle = u * Mathf.PI * 2f;
                    var irregularity = 0.9f + Mathf.Sin(angle * 3f + variant * 1.7f) * 0.09f + Mathf.Cos(latitude * 4f + variant) * 0.06f;
                    vertices.Add(new Vector3(Mathf.Cos(angle) * ringRadius * irregularity, Mathf.Sin(latitude) * 0.88f, Mathf.Sin(angle) * ringRadius * irregularity));
                }
            }
            for (var ring = 0; ring < rings; ring++)
            for (var segment = 0; segment < segments; segment++)
            {
                var a = ring * (segments + 1) + segment;
                var b = a + 1;
                var c = a + segments + 1;
                var d = c + 1;
                triangles.Add(a); triangles.Add(c); triangles.Add(b);
                triangles.Add(b); triangles.Add(c); triangles.Add(d);
            }
            mesh = Build("Organic Rock " + variant, vertices, triangles);
            Rocks.Add(variant, mesh);
            return mesh;
        }

        public static Mesh PalmTrunk()
        {
            if (palmTrunk != null) return palmTrunk;
            const int segments = 8, rings = 8;
            var vertices = new List<Vector3>((segments + 1) * rings);
            var triangles = new List<int>(segments * (rings - 1) * 6);
            for (var ring = 0; ring < rings; ring++)
            {
                var t = ring / (float)(rings - 1);
                var center = new Vector3(Mathf.Sin(t * Mathf.PI) * 0.34f, t * 7.8f, Mathf.Sin(t * Mathf.PI * 0.7f) * 0.18f);
                var radius = Mathf.Lerp(0.48f, 0.24f, t) * (1f + Mathf.Sin(t * 21f) * 0.035f);
                for (var segment = 0; segment <= segments; segment++)
                {
                    var angle = segment / (float)segments * Mathf.PI * 2f;
                    vertices.Add(center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
                }
            }
            AddTubeTriangles(triangles, segments, rings);
            palmTrunk = Build("Curved Palm Trunk", vertices, triangles);
            return palmTrunk;
        }

        public static Mesh PalmLeaf()
        {
            if (palmLeaf != null) return palmLeaf;
            const int sections = 6;
            var vertices = new List<Vector3>((sections + 1) * 2);
            var triangles = new List<int>(sections * 12);
            for (var i = 0; i <= sections; i++)
            {
                var t = i / (float)sections;
                var width = Mathf.Sin(t * Mathf.PI) * 0.72f + 0.03f;
                var y = 0.18f * Mathf.Sin(t * Mathf.PI) - 1.15f * t * t;
                var z = t * 4.4f;
                vertices.Add(new Vector3(-width, y, z));
                vertices.Add(new Vector3(width, y, z));
            }
            for (var i = 0; i < sections; i++)
            {
                var a = i * 2; var b = a + 1; var c = a + 2; var d = a + 3;
                AddDoubleSidedQuad(triangles, a, b, c, d);
            }
            palmLeaf = Build("Sculpted Palm Frond", vertices, triangles);
            return palmLeaf;
        }

        public static Mesh Branch()
        {
            if (branch != null) return branch;
            const int segments = 7;
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            for (var ring = 0; ring < 2; ring++)
            for (var segment = 0; segment <= segments; segment++)
            {
                var angle = segment / (float)segments * Mathf.PI * 2f;
                var radius = ring == 0 ? 0.12f : 0.085f;
                vertices.Add(new Vector3(Mathf.Cos(angle) * radius, ring * 1.5f, Mathf.Sin(angle) * radius));
            }
            AddTubeTriangles(triangles, segments, 2);
            branch = Build("Tapered Branch", vertices, triangles);
            return branch;
        }

        public static Mesh Plank()
        {
            if (plank != null) return plank;
            var vertices = new List<Vector3>
            {
                new Vector3(-0.5f,-0.5f,-0.5f), new Vector3(0.46f,-0.48f,-0.52f), new Vector3(0.5f,0.44f,-0.46f), new Vector3(-0.47f,0.5f,-0.5f),
                new Vector3(-0.48f,-0.47f,0.5f), new Vector3(0.5f,-0.5f,0.47f), new Vector3(0.46f,0.5f,0.5f), new Vector3(-0.52f,0.46f,0.48f)
            };
            var triangles = new List<int>
            {
                0,2,1, 0,3,2, 1,2,6, 1,6,5, 5,6,7, 5,7,4,
                4,7,3, 4,3,0, 3,7,6, 3,6,2, 4,0,1, 4,1,5
            };
            plank = Build("Hand Hewn Plank", vertices, triangles);
            return plank;
        }

        static Mesh CreateGround(bool grassOnly)
        {
            const int xSegments = 32, zSegments = 32;
            var vertices = new List<Vector3>((xSegments + 1) * (zSegments + 1));
            var colors = new List<Color>((xSegments + 1) * (zSegments + 1));
            var triangles = new List<int>(xSegments * zSegments * 6);
            for (var z = 0; z <= zSegments; z++)
            for (var x = 0; x <= xSegments; x++)
            {
                var nx = Mathf.Lerp(-1f, 1f, x / (float)xSegments);
                var t = z / (float)zSegments;
                var expansion = Mathf.Clamp01(t * 4.8f);
                var width = grassOnly ? Mathf.Lerp(52f, 180f, expansion) : Mathf.Lerp(48f, 190f, expansion);
                var px = nx * width;
                var start = grassOnly ? 25f + Mathf.Sin(px * 0.11f) * 3.5f : ShoreZ(px);
                var pz = Mathf.Lerp(start, grassOnly ? 330f : 334f, t);
                vertices.Add(new Vector3(px, GroundHeight(px, pz) + (grassOnly ? 0.045f : 0f), pz));
                var distanceFromShore = pz - ShoreZ(px);
                var wet = 1f - Mathf.SmoothStep(0f, 9f, distanceFromShore);
                var inland = Mathf.SmoothStep(18f, 48f, distanceFromShore);
                var patch = Mathf.PerlinNoise(px * 0.025f + 10f, pz * 0.022f + 20f);
                var pathCenter = Mathf.Sin(pz * 0.026f) * 7f;
                var inlandPath = inland * (1f - Mathf.SmoothStep(3.2f, 9.5f, Mathf.Abs(px - pathCenter))) * 0.82f;
                var dirt = Mathf.Max(inlandPath, inland * Mathf.SmoothStep(0.56f, 0.78f, patch) * 0.58f);
                var grassWeight = inland * (1f - dirt);
                var dry = Mathf.Max(0f, 1f - wet - grassWeight - dirt);
                var total = Mathf.Max(0.0001f, dry + wet + grassWeight + dirt);
                colors.Add(new Color(dry / total, wet / total, grassWeight / total, dirt / total));
            }
            AddGridTriangles(triangles, xSegments, zSegments);
            return Build(grassOnly ? "Organic Grass Transition" : "Organic Starting Beach", vertices, triangles, colors);
        }

        static void AddGridTriangles(List<int> triangles, int xSegments, int zSegments)
        {
            for (var z = 0; z < zSegments; z++)
            for (var x = 0; x < xSegments; x++)
            {
                var a = z * (xSegments + 1) + x;
                var b = a + 1;
                var c = a + xSegments + 1;
                var d = c + 1;
                triangles.Add(a); triangles.Add(c); triangles.Add(b);
                triangles.Add(b); triangles.Add(c); triangles.Add(d);
            }
        }

        static void AddTubeTriangles(List<int> triangles, int segments, int rings)
        {
            for (var ring = 0; ring < rings - 1; ring++)
            for (var segment = 0; segment < segments; segment++)
            {
                var a = ring * (segments + 1) + segment;
                var b = a + 1;
                var c = a + segments + 1;
                var d = c + 1;
                triangles.Add(a); triangles.Add(c); triangles.Add(b);
                triangles.Add(b); triangles.Add(c); triangles.Add(d);
            }
        }

        static void AddDoubleSidedQuad(List<int> triangles, int a, int b, int c, int d)
        {
            triangles.Add(a); triangles.Add(c); triangles.Add(b);
            triangles.Add(b); triangles.Add(c); triangles.Add(d);
            triangles.Add(b); triangles.Add(c); triangles.Add(a);
            triangles.Add(d); triangles.Add(c); triangles.Add(b);
        }

        static Mesh Build(string name, List<Vector3> vertices, List<int> triangles, List<Color> colors = null)
        {
            var mesh = new Mesh { name = name };
            mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0);
            if (colors != null && colors.Count == vertices.Count) mesh.SetColors(colors);
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }
    }
}
