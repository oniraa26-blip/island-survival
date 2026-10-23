using System;
using System.Collections.Generic;
using IslandSurvival.Gathering;
using IslandSurvival.Platform;
using UnityEngine;

namespace IslandSurvival.World
{
    public static class WorldFactory
    {
        static readonly Dictionary<Color32, Material> Materials = new Dictionary<Color32, Material>();

        public static Material Material(Color color)
        {
            var key = (Color32)color;
            Material existing;
            if (Materials.TryGetValue(key, out existing) && existing != null) return existing;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            var material = new Material(shader); material.color = color; material.enableInstancing = true;
            Materials[key] = material; return material;
        }
        public static GameObject Primitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color, Transform parent = null)
        {
            var obj = GameObject.CreatePrimitive(type); obj.name = name; obj.transform.SetParent(parent);
            obj.transform.position = position; obj.transform.localScale = scale; obj.GetComponent<Renderer>().sharedMaterial = Material(color); return obj;
        }
        public static void Tint(GameObject root, Color color)
        { foreach (var renderer in root.GetComponentsInChildren<Renderer>()) renderer.material.color = color; }

        public static GameObject CreateBuildableVisual(string id, bool ghost)
        {
            var root = new GameObject((ghost ? "Ghost_" : "Buildable_") + id);
            if (id == "campfire")
            {
                for (var i = 0; i < 8; i++)
                {
                    var angle = i * Mathf.PI * 2f / 8f;
                    Primitive("Stone", PrimitiveType.Sphere, Vector3.zero, new Vector3(0.38f, 0.2f, 0.3f), new Color(0.38f, 0.4f, 0.38f), root.transform).transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.58f, 0.12f, Mathf.Sin(angle) * 0.58f);
                }
                var logA = Primitive("Log", PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.12f, 0.72f, 0.12f), new Color(0.3f, 0.14f, 0.06f), root.transform); logA.transform.localPosition = Vector3.up * 0.18f; logA.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                var logB = Primitive("Log", PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.12f, 0.72f, 0.12f), new Color(0.3f, 0.14f, 0.06f), root.transform); logB.transform.localPosition = Vector3.up * 0.2f; logB.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                Primitive("Flame", PrimitiveType.Sphere, Vector3.zero, new Vector3(0.35f, 0.65f, 0.35f), new Color(1f, 0.45f, 0.08f), root.transform).transform.localPosition = Vector3.up * 0.65f;
            }
            else
            {
                Primitive("Bed", PrimitiveType.Cube, Vector3.zero, new Vector3(1.45f, 0.22f, 2.25f), new Color(0.3f, 0.68f, 0.25f), root.transform).transform.localPosition = Vector3.up * 0.2f;
                var roof = Primitive("LeafRoof", PrimitiveType.Cube, Vector3.zero, new Vector3(1.7f, 0.12f, 1.4f), new Color(0.22f, 0.58f, 0.2f), root.transform); roof.transform.localPosition = new Vector3(0f, 1.45f, 0.55f); roof.transform.localRotation = Quaternion.Euler(18f, 0f, 0f);
                foreach (var x in new[] { -0.7f, 0.7f }) foreach (var z in new[] { 0f, 1.1f }) Primitive("Post", PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.06f, 0.75f, 0.06f), new Color(0.35f, 0.2f, 0.08f), root.transform).transform.localPosition = new Vector3(x, 0.72f, z);
            }
            if (ghost)
            {
                foreach (var collider in root.GetComponentsInChildren<Collider>()) collider.enabled = false;
                Tint(root, new Color(0.25f, 0.85f, 0.55f, 0.65f));
            }
            return root;
        }

        public static void CreateStartingBeach(PlatformProfile profile)
        {
            var world = new GameObject("StartingBeach_World").transform;
            Primitive("Beach", PrimitiveType.Cube, new Vector3(0f, -0.75f, 18f), new Vector3(140f, 1.5f, 75f), new Color(0.88f, 0.75f, 0.48f), world);
            Primitive("Water", PrimitiveType.Cube, new Vector3(0f, -1.15f, -38f), new Vector3(240f, 1f, 55f), new Color(0.08f, 0.62f, 0.7f), world);
            Primitive("Inland", PrimitiveType.Cube, new Vector3(0f, 0f, 67f), new Vector3(150f, 3f, 35f), new Color(0.18f, 0.58f, 0.26f), world);
            for (var i = 0; i < 20; i++)
            {
                var angle = i * 0.87f; var radius = 32f + i * 2.5f;
                Primitive("RockBoundary", PrimitiveType.Sphere, new Vector3(Mathf.Sin(angle) * radius, 1.5f, 30f + Mathf.Cos(angle) * radius), new Vector3(5f, 3.5f, 4f), new Color(0.29f, 0.36f, 0.34f), world);
            }
            var palmCount = Mathf.Max(6, Mathf.RoundToInt(12 * profile.vegetationDensity));
            for (var i = 0; i < palmCount; i++) CreatePalm(new Vector3(-28f + (i % 6) * 11f, 0.2f, 37f + (i / 6) * 13f), "tree_" + i, world);
            var pickupData = new[] { "stick", "stone", "coconut", "fiber", "stick", "stone", "coconut", "fiber", "stick", "stone", "fiber", "stone", "stick", "coconut", "fiber", "stone", "stick", "fiber" };
            for (var i = 0; i < pickupData.Length; i++) CreatePickup(pickupData[i], new Vector3(-18f + (i % 6) * 7f, 0.35f, 4f + (i / 6) * 8f), world);
            for (var i = 0; i < 7; i++) Primitive("DistantOriginSilhouette", PrimitiveType.Sphere, new Vector3(-70f + i * 23f, 15f + (i % 3) * 7f, 105f + Mathf.Abs(3 - i) * 4f), new Vector3(30f, 28f, 18f), new Color(0.12f, 0.34f, 0.25f), world);
        }
        static void CreatePickup(string id, Vector3 position, Transform parent)
        {
            var type = id == "stick" || id == "fiber" ? PrimitiveType.Cylinder : PrimitiveType.Sphere;
            var color = id == "stone" ? new Color(0.44f, 0.47f, 0.44f) : id == "coconut" ? new Color(0.35f, 0.18f, 0.06f) : id == "fiber" ? new Color(0.4f, 0.75f, 0.22f) : new Color(0.38f, 0.22f, 0.08f);
            var obj = Primitive("Pickup_" + id, type, position, id == "stick" ? new Vector3(0.1f, 0.7f, 0.1f) : new Vector3(0.42f, 0.42f, 0.42f), color, parent);
            if (id == "stick") obj.transform.rotation = Quaternion.Euler(90f, 0f, 35f);
            var pickup = obj.AddComponent<PickupResource>(); pickup.itemId = id;
        }
        static void CreatePalm(Vector3 position, string id, Transform parent)
        {
            var root = new GameObject("Tree_Test"); root.transform.SetParent(parent); root.transform.position = position;
            Primitive("Trunk", PrimitiveType.Cylinder, Vector3.zero, new Vector3(0.55f, 4.2f, 0.55f), new Color(0.42f, 0.25f, 0.1f), root.transform).transform.localPosition = Vector3.up * 4.2f;
            for (var i = 0; i < 6; i++)
            {
                var leaf = Primitive("PalmLeaf", PrimitiveType.Cube, Vector3.zero, new Vector3(0.45f, 0.12f, 3.7f), new Color(0.12f, 0.56f, 0.2f), root.transform);
                leaf.transform.localPosition = Vector3.up * 8.2f; leaf.transform.localRotation = Quaternion.Euler(18f, i * 60f, 0f);
            }
            var tree = root.AddComponent<TreeGatherable>(); tree.persistentId = id;
            var lodGroup = root.AddComponent<LODGroup>();
            lodGroup.fadeMode = LODFadeMode.CrossFade;
            lodGroup.animateCrossFading = true;
            lodGroup.SetLODs(new[] { new LOD(0.035f, root.GetComponentsInChildren<Renderer>()) });
            lodGroup.RecalculateBounds();
        }
    }
}
