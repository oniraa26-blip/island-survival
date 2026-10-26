using System.Collections.Generic;
using IslandSurvival.Building;
using IslandSurvival.Core;
using IslandSurvival.Exploration;
using IslandSurvival.Gathering;
using IslandSurvival.Platform;
using IslandSurvival.Visual;
using UnityEngine;
using UnityEngine.Rendering;

namespace IslandSurvival.World
{
    public static class WorldFactory
    {
        static readonly Dictionary<Color32, Material> Materials = new Dictionary<Color32, Material>();
        static readonly Color Sand = new Color(0.93f, 0.76f, 0.48f);
        static readonly Color Grass = new Color(0.24f, 0.57f, 0.25f);
        static readonly Color DeepGreen = new Color(0.08f, 0.35f, 0.18f);
        static readonly Color LeafGreen = new Color(0.13f, 0.52f, 0.22f);
        static readonly Color TrunkBrown = new Color(0.39f, 0.21f, 0.09f);
        static readonly Color RockWarm = new Color(0.38f, 0.42f, 0.39f);
        static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
        static readonly MaterialPropertyBlock TintProperties = new MaterialPropertyBlock();
        static readonly List<Renderer> TintRenderers = new List<Renderer>();

        public static Material Material(Color color)
        {
            var key = (Color32)color;
            Material existing;
            if (Materials.TryGetValue(key, out existing) && existing != null) return existing;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            var material = new Material(shader) { color = color, enableInstancing = true };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", color.b > 0.6f ? 0.42f : 0.08f);
            Materials[key] = material;
            return material;
        }

        public static GameObject Primitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color, Transform parent = null)
        {
            var obj = GameObject.CreatePrimitive(type); obj.name = name; obj.transform.SetParent(parent);
            obj.transform.position = position; obj.transform.localScale = scale; obj.GetComponent<Renderer>().sharedMaterial = Material(color); return obj;
        }

        public static void Tint(GameObject root, Color color)
        {
            TintRenderers.Clear(); root.GetComponentsInChildren(false, TintRenderers);
            foreach (var renderer in TintRenderers)
            {
                renderer.GetPropertyBlock(TintProperties);
                TintProperties.SetColor(BaseColorProperty, color);
                renderer.SetPropertyBlock(TintProperties);
                TintProperties.Clear();
            }
            TintRenderers.Clear();
        }

        public static Transform CreatePlayerVisualRoot(Transform player)
        {
            var root = new GameObject("PlayerVisualRoot").transform;
            root.SetParent(player, false);
            if (ServiceRegistry.Visuals != null)
                ServiceRegistry.Visuals.CreatePresentation(VisualAssetKey.PlayerCharacter, root, "player", CreateFallbackPlayer);
            else CreateFallbackPlayer(root);
            return root;
        }

        static GameObject CreateFallbackPlayer(Transform parent)
        {
            var fallback = Primitive("FallbackCapsule_ReplaceWithCharacterArt", PrimitiveType.Capsule, parent.position, new Vector3(0.72f, 0.9f, 0.72f), new Color(0.9f, 0.42f, 0.14f), parent);
            fallback.layer = 2; fallback.transform.localPosition = Vector3.up * 0.9f;
            Object.Destroy(fallback.GetComponent<Collider>()); return fallback;
        }

        public static GameObject CreateBuildableVisual(string id, bool ghost)
        {
            var root = new GameObject((ghost ? "Ghost_" : "Buildable_") + id);
            if (id == "campfire")
            {
                for (var i = 0; i < 8; i++)
                {
                    var angle = i * Mathf.PI * 2f / 8f;
                    var stone = MeshObject("Fire Ring Stone", StylizedGeometry.Rock(i % 3), Material(RockWarm), root.transform, false);
                    stone.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.58f, 0.14f, Mathf.Sin(angle) * 0.58f);
                    stone.transform.localScale = new Vector3(0.34f, 0.2f, 0.28f);
                }
                AddBranch(root.transform, new Vector3(0f, 0.18f, 0f), new Vector3(0.16f, 0.72f, 0.16f), Quaternion.Euler(0f, 0f, 90f), "Fire Log");
                AddBranch(root.transform, new Vector3(0f, 0.2f, 0f), new Vector3(0.16f, 0.72f, 0.16f), Quaternion.Euler(90f, 0f, 0f), "Fire Log");
                Primitive("Prototype Flame", PrimitiveType.Sphere, Vector3.zero, new Vector3(0.35f, 0.65f, 0.35f), new Color(1f, 0.45f, 0.08f), root.transform).transform.localPosition = Vector3.up * 0.65f;
            }
            else
            {
                var bed = MeshObject("Leaf Bed", StylizedGeometry.Plank(), Material(LeafGreen), root.transform, true);
                bed.transform.localPosition = Vector3.up * 0.2f; bed.transform.localScale = new Vector3(1.45f, 0.22f, 2.25f);
                var roof = MeshObject("Leaf Shelter", StylizedGeometry.PalmLeaf(), Material(LeafGreen), root.transform, false);
                roof.transform.localPosition = new Vector3(0f, 1.45f, 0.2f); roof.transform.localRotation = Quaternion.Euler(8f, 0f, 0f); roof.transform.localScale = new Vector3(1.25f, 1f, 0.75f);
                foreach (var x in new[] { -0.7f, 0.7f }) foreach (var z in new[] { 0f, 1.1f }) AddBranch(root.transform, new Vector3(x, 0f, z), new Vector3(0.5f, 1f, 0.5f), Quaternion.identity, "Shelter Post");
            }
            if (ghost)
            {
                foreach (var collider in root.GetComponentsInChildren<Collider>()) collider.enabled = false;
                Tint(root, new Color(0.25f, 0.85f, 0.55f, 0.65f));
            }
            return root;
        }

        public static OriginDemoWorld CreateStartingBeach(PlatformProfile profile)
        {
            ConfigureEnvironment(profile);
            var world = new GameObject("StartingBeach_World").transform;
            var terrainRoot = Child("Terrain_Coastline", world);
            var oceanRoot = Child("Ocean_And_Shore", world);
            var resourceRoot = Child("Gameplay_Resources", world);

            var terrainMaterial = ServiceRegistry.Visuals != null ? ServiceRegistry.Visuals.ResolveMaterial(VisualMaterialKey.TerrainBlend) : Material(Sand);
            var beach = MeshObject("Organic Beach Terrain", StylizedGeometry.Terrain(), terrainMaterial, terrainRoot, true);
            beach.GetComponent<MeshRenderer>().receiveShadows = true;

            var oceanMaterial = ServiceRegistry.Visuals != null ? ServiceRegistry.Visuals.ResolveMaterial(VisualMaterialKey.Ocean) : Material(new Color(0.055f, 0.58f, 0.67f));
            var foamMaterial = ServiceRegistry.Visuals != null ? ServiceRegistry.Visuals.ResolveMaterial(VisualMaterialKey.ShoreFoam) : Material(new Color(0.79f, 0.94f, 0.86f));
            MeshObject("Turquoise Ocean", StylizedGeometry.Ocean(), oceanMaterial, oceanRoot, false, false);
            MeshObject("Lightweight Shore Foam", CreateFoamRibbon(), foamMaterial, oceanRoot, false, false);

            CreateResources(resourceRoot);
            CreateDiscoveryTriggers(world);
            CreateGiantTreeProxy(world);

            var demo = world.gameObject.AddComponent<OriginDemoWorld>();
            var activationRadius = profile.tier == PlatformTier.WebMobileTest ? 105f : 135f;
            demo.Register(OriginDemoZones.StartingBeach, new Vector3(0f, 0f, 25f), activationRadius, true, parent => CreateStartingBeachZone(profile, parent));
            demo.Register(OriginDemoZones.FreshwaterStream, new Vector3(0f, 0f, 95f), activationRadius, false, CreateStreamZone);
            demo.Register(OriginDemoZones.GiantTree, new Vector3(0f, 0f, 155f), activationRadius, false, CreateGiantTreeZone);
            demo.Register(OriginDemoZones.BlueLagoon, new Vector3(-105f, 0f, 215f), activationRadius, false, CreateLagoonZone);
            demo.Register(OriginDemoZones.TwinFalls, new Vector3(95f, 0f, 225f), activationRadius, false, CreateTwinFallsZone);
            demo.Register(OriginDemoZones.FlowerValley, new Vector3(0f, 0f, 292f), activationRadius, false, CreateFlowerValleyZone);
            demo.Register(OriginDemoZones.SplitRock, new Vector3(105f, 0f, 72f), activationRadius, false, CreateSplitRockZone);
            demo.Register(OriginDemoZones.SeaCave, new Vector3(158f, 0f, 58f), activationRadius, false, CreateSeaCaveZone);
            return demo;
        }

        static GameObject CreateStartingBeachZone(PlatformProfile profile, Transform parent)
        {
            var root = ZoneRoot("Starting Beach Visuals", parent);
            CreateWreck(new Vector3(13f, StylizedGeometry.GroundHeight(13f, 11f), 11f), root.transform);
            CreateRockComposition(root.transform); CreateVegetation(profile, root.transform);
            StartingBeachVisualSlice.Create(root.transform);
            CreateAudioHook("Ocean Ambience Hook", new Vector3(0f, 0f, -8f), root.transform);
            return root;
        }

        static GameObject CreateStreamZone(Transform parent)
        {
            var root = ZoneRoot("Freshwater Stream Visuals", parent);
            var points = new[] { new Vector3(-8f,0f,45f), new Vector3(7f,0f,65f), new Vector3(-5f,0f,87f), new Vector3(5f,0f,110f), new Vector3(0f,0f,135f) };
            var streamMaterial = ServiceRegistry.Visuals != null ? ServiceRegistry.Visuals.ResolveMaterial(VisualMaterialKey.Lagoon) : Material(new Color(0.08f, 0.57f, 0.65f));
            MeshObject("Freshwater Navigation Stream", CreateRibbon(points, 4.5f, 0.08f), streamMaterial, root.transform, false, false);
            for (var i = 0; i < points.Length; i++)
            {
                var p = points[i]; p.y = StylizedGeometry.GroundHeight(p.x, p.z);
                CreateBush(p + new Vector3(i % 2 == 0 ? -6f : 6f, 0f, 2f), root.transform, i);
                var rock = RockVisual("Stream Bank Rock", StylizedGeometry.Rock(i % 4), Material(RockWarm), root.transform, false, false, "stream_rock_" + i);
                rock.transform.localPosition = p + new Vector3(i % 2 == 0 ? 4.5f : -4.5f, 0.45f, 0f); rock.transform.localScale = new Vector3(1.2f, 0.7f, 1f);
            }
            CreateAudioHook("Freshwater Ambience Hook", new Vector3(0f, 1f, 92f), root.transform);
            return root;
        }

        static GameObject CreateGiantTreeZone(Transform parent)
        {
            var root = ZoneRoot("Giant Tree Meadow Visuals", parent);
            for (var i = 0; i < 11; i++)
            {
                var angle = i / 11f * Mathf.PI * 2f; var radius = 25f + (i % 3) * 4f;
                var p = new Vector3(Mathf.Cos(angle) * radius, 0f, 155f + Mathf.Sin(angle) * radius); p.y = StylizedGeometry.GroundHeight(p.x, p.z);
                if (i % 2 == 0) CreateBush(p, root.transform, i); else CreateGrassCluster(p, root.transform, i);
            }
            var campMarker = Child("Central Meadow Camp Placement Area", root.transform); campMarker.localPosition = new Vector3(-18f, StylizedGeometry.GroundHeight(-18f, 150f), 150f);
            return root;
        }

        static GameObject CreateLagoonZone(Transform parent)
        {
            var root = ZoneRoot("Blue Lagoon Visuals", parent); var center = new Vector3(-105f, StylizedGeometry.GroundHeight(-105f, 215f) + 0.08f, 215f);
            var lagoonMaterial = ServiceRegistry.Visuals != null ? ServiceRegistry.Visuals.ResolveMaterial(VisualMaterialKey.Lagoon) : Material(new Color(0.05f, 0.68f, 0.72f));
            var water = MeshObject("Blue Lagoon Water", CreateDisc(32f, 28), lagoonMaterial, root.transform, false, false); water.transform.localPosition = center;
            for (var i = 0; i < 13; i++)
            {
                var angle = i / 13f * Mathf.PI * 2f; var radius = 31f + (i % 3) * 2f; var p = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                var rock = RockVisual("Lagoon Rounded Rock", StylizedGeometry.Rock(i % 4), Material(RockWarm), root.transform, i % 4 == 0, true, "lagoon_rock_" + i); rock.transform.localPosition = p; rock.transform.localScale = new Vector3(2.2f, 1.25f, 1.8f);
                if (i % 3 == 0) CreateBush(p + Vector3.forward * 2f, root.transform, i);
            }
            var fishingHook = Child("Fishing Shoreline Foundation", root.transform); fishingHook.localPosition = center + new Vector3(24f, 0f, -12f);
            CreateAudioHook("Lagoon Ambience Hook", center, root.transform);
            return root;
        }

        static GameObject CreateTwinFallsZone(Transform parent)
        {
            var root = ZoneRoot("Twin Falls Visuals", parent); var poolCenter = new Vector3(95f, StylizedGeometry.GroundHeight(95f, 218f) + 0.15f, 218f);
            var poolMaterial = ServiceRegistry.Visuals != null ? ServiceRegistry.Visuals.ResolveMaterial(VisualMaterialKey.Lagoon) : Material(new Color(0.07f, 0.61f, 0.68f));
            var pool = MeshObject("Twin Falls Pool", CreateDisc(21f, 24), poolMaterial, root.transform, false, false); pool.transform.localPosition = poolCenter;
            for (var i = 0; i < 10; i++)
            {
                var rock = RockVisual("Waterfall Cliff Rock", StylizedGeometry.Rock(i % 4), Material(new Color(0.32f, 0.39f, 0.36f)), root.transform, true, true, "falls_rock_" + i);
                rock.transform.localPosition = new Vector3(77f + i * 4f, poolCenter.y + 3f + (i % 3) * 2.5f, 238f + Mathf.Sin(i) * 2f); rock.transform.localScale = new Vector3(3.5f, 5f, 3.4f);
            }
            CreateWaterfall(root.transform, new Vector3(89f, poolCenter.y + 8f, 236f), 7f, 8f);
            CreateWaterfall(root.transform, new Vector3(101f, poolCenter.y + 9.5f, 237f), 6f, 9.5f);
            CreateAudioHook("Waterfall Ambience Hook", poolCenter, root.transform);
            return root;
        }

        static GameObject CreateFlowerValleyZone(Transform parent)
        {
            var root = ZoneRoot("Flower Valley Visuals", parent); var colors = new[] { new Color(0.92f,0.35f,0.44f), new Color(0.95f,0.72f,0.2f), new Color(0.58f,0.42f,0.86f) };
            for (var i = 0; i < 24; i++)
            {
                var x = -45f + (i % 8) * 13f + Mathf.Sin(i * 1.7f) * 3f; var z = 268f + (i / 8) * 18f + Mathf.Cos(i) * 3f;
                var flower = MeshObject("Solid Petal Flower", StylizedGeometry.PalmLeaf(), Material(colors[i % colors.Length]), root.transform, false, false);
                flower.transform.localPosition = new Vector3(x, StylizedGeometry.GroundHeight(x, z) + 0.18f, z); flower.transform.localRotation = Quaternion.Euler(-82f, i * 47f, 0f); flower.transform.localScale = new Vector3(0.11f, 0.12f, 0.14f);
            }
            for (var i = 0; i < 10; i++) { var x = -42f + i * 9f; var z = 284f + Mathf.Sin(i) * 12f; CreateGrassCluster(new Vector3(x, StylizedGeometry.GroundHeight(x, z), z), root.transform, i); }
            return root;
        }

        static GameObject CreateSplitRockZone(Transform parent)
        {
            var root = ZoneRoot("Split Rock Visuals", parent); var y = StylizedGeometry.GroundHeight(105f, 72f);
            var protection = Child("Split Rock Build Protection", root.transform); protection.localPosition = new Vector3(105f, y, 72f); protection.gameObject.AddComponent<LandmarkProtectionVolume>().radius = 15f;
            var left = MeshObject("Split Rock Left", StylizedGeometry.Rock(1), Material(new Color(0.36f,0.4f,0.37f)), root.transform, true); left.transform.localPosition = new Vector3(99f, y + 7f, 72f); left.transform.localScale = new Vector3(7f, 10f, 8f); left.transform.localRotation = Quaternion.Euler(0f, -12f, -8f);
            var right = MeshObject("Split Rock Right", StylizedGeometry.Rock(2), Material(new Color(0.39f,0.42f,0.38f)), root.transform, true); right.transform.localPosition = new Vector3(112f, y + 7f, 72f); right.transform.localScale = new Vector3(7f, 10f, 8f); right.transform.localRotation = Quaternion.Euler(0f, 14f, 9f);
            return root;
        }

        static GameObject CreateSeaCaveZone(Transform parent)
        {
            var root = ZoneRoot("Sea Cave Visuals", parent); var y = StylizedGeometry.GroundHeight(158f, 58f);
            var protection = Child("Sea Cave Build Protection", root.transform); protection.localPosition = new Vector3(158f, y, 61f); protection.gameObject.AddComponent<LandmarkProtectionVolume>().radius = 14f;
            for (var i = 0; i < 9; i++)
            {
                var angle = Mathf.Lerp(15f, 165f, i / 8f) * Mathf.Deg2Rad;
                var rock = MeshObject("Sea Cave Arch Rock", StylizedGeometry.Rock(i % 4), Material(new Color(0.25f,0.31f,0.3f)), root.transform, true);
                rock.transform.localPosition = new Vector3(158f + Mathf.Cos(angle) * 12f, y + Mathf.Sin(angle) * 10f, 61f); rock.transform.localScale = new Vector3(5f, 5f, 7f);
            }
            var back = MeshObject("Sea Cave Dark Backdrop", StylizedGeometry.Rock(3), Material(new Color(0.055f,0.09f,0.1f)), root.transform, true); back.transform.localPosition = new Vector3(158f, y + 3f, 70f); back.transform.localScale = new Vector3(13f, 8f, 5f);
            var crystal = new GameObject("Blue Crystal Fragment"); crystal.transform.SetParent(root.transform, false); crystal.transform.localPosition = new Vector3(158f, y + 1f, 65f); crystal.transform.localScale = new Vector3(0.42f, 1.1f, 0.42f); crystal.transform.localRotation = Quaternion.Euler(0f, 25f, 12f);
            var crystalMesh = StylizedGeometry.Rock(0); var crystalCollider = crystal.AddComponent<MeshCollider>(); crystalCollider.sharedMesh = crystalMesh;
            var collectible = crystal.AddComponent<MysteryCollectible>(); collectible.collectibleId = "sea_cave_blue_crystal"; collectible.itemId = "blue_crystal_fragment";
            if (ServiceRegistry.Visuals != null) ServiceRegistry.Visuals.CreatePresentation(VisualAssetKey.BlueCrystal, crystal.transform, "sea_cave_blue_crystal",
                visualParent => MeshObject("Procedural Crystal Fallback", crystalMesh, Material(new Color(0.18f,0.75f,0.9f)), visualParent, false, false));
            else MeshObject("Procedural Crystal Fallback", crystalMesh, Material(new Color(0.18f,0.75f,0.9f)), crystal.transform, false, false);
            CreateAudioHook("Sea Cave Ambience Hook", new Vector3(158f, y + 2f, 63f), root.transform);
            return root;
        }

        static void CreateGiantTreeProxy(Transform parent)
        {
            var root = Child("Persistent Giant Tree Landmark Proxy", parent); root.localPosition = new Vector3(0f, StylizedGeometry.GroundHeight(0f, 155f), 155f);
            root.gameObject.AddComponent<LandmarkProtectionVolume>().radius = 13f;
            var trunk = MeshObject("Giant Tree Trunk", StylizedGeometry.PalmTrunk(), Material(new Color(0.28f,0.15f,0.065f)), root, false); trunk.transform.localScale = new Vector3(4.2f, 3.1f, 4.2f);
            for (var i = 0; i < 7; i++) { var crown = MeshObject("Giant Tree Crown", StylizedGeometry.Rock(i % 4), Material(i % 2 == 0 ? DeepGreen : LeafGreen), root, false); crown.transform.localPosition = new Vector3(Mathf.Sin(i * 1.8f) * 7f, 22f + (i % 3) * 3f, Mathf.Cos(i * 1.8f) * 6f); crown.transform.localScale = new Vector3(7f, 5.5f, 6.5f); }
            var collider = root.gameObject.AddComponent<CapsuleCollider>(); collider.center = new Vector3(0f, 10f, 0f); collider.height = 20f; collider.radius = 2.1f;
        }

        static void CreateDiscoveryTriggers(Transform parent)
        {
            AddDiscoveryTrigger(parent, OriginDemoZones.StartingBeach, new Vector3(0f, 1f, 3f), new Vector3(28f, 5f, 25f));
            AddDiscoveryTrigger(parent, OriginDemoZones.FreshwaterStream, new Vector3(0f, 2f, 82f), new Vector3(26f, 8f, 32f));
            AddDiscoveryTrigger(parent, OriginDemoZones.GiantTree, new Vector3(0f, 3f, 155f), new Vector3(45f, 10f, 45f));
            AddDiscoveryTrigger(parent, OriginDemoZones.BlueLagoon, new Vector3(-105f, 3f, 215f), new Vector3(58f, 10f, 58f));
            AddDiscoveryTrigger(parent, OriginDemoZones.TwinFalls, new Vector3(95f, 3f, 220f), new Vector3(48f, 10f, 48f));
            AddDiscoveryTrigger(parent, OriginDemoZones.FlowerValley, new Vector3(0f, 3f, 292f), new Vector3(80f, 10f, 55f));
            AddDiscoveryTrigger(parent, OriginDemoZones.SplitRock, new Vector3(105f, 4f, 72f), new Vector3(38f, 14f, 38f));
            AddDiscoveryTrigger(parent, OriginDemoZones.SeaCave, new Vector3(158f, 4f, 58f), new Vector3(36f, 14f, 38f));
        }

        static void AddDiscoveryTrigger(Transform parent, string id, Vector3 position, Vector3 size)
        {
            var obj = new GameObject("Discovery_" + id); obj.transform.SetParent(parent, false); obj.transform.localPosition = position;
            var collider = obj.AddComponent<BoxCollider>(); collider.isTrigger = true; collider.size = size;
            obj.AddComponent<DiscoveryTrigger>().discoveryId = id;
        }

        static GameObject ZoneRoot(string name, Transform parent) { var root = new GameObject(name); root.transform.SetParent(parent, false); return root; }
        static void CreateAudioHook(string name, Vector3 position, Transform parent)
        {
            // Hierarchy-only placement marker. The prototype has no licensed audio clips yet,
            // so do not require Unity's optional Audio module until an audio system is authored.
            var hook = new GameObject(name);
            hook.transform.SetParent(parent, false);
            hook.transform.localPosition = position;
        }

        static void ConfigureEnvironment(PlatformProfile profile)
        {
            TropicalDaylightProfile.Apply(ServiceRegistry.Visuals != null ? ServiceRegistry.Visuals.Daylight : null, profile, null, null);
        }

        static void CreateRockComposition(Transform parent)
        {
            var positions = new[] { new Vector3(-35f,0f,8f), new Vector3(-30f,0f,14f), new Vector3(37f,0f,18f), new Vector3(31f,0f,31f), new Vector3(-42f,0f,43f), new Vector3(45f,0f,52f), new Vector3(-12f,0f,47f), new Vector3(22f,0f,61f) };
            var scales = new[] { new Vector3(4.8f,2.7f,3.5f), new Vector3(2.1f,1.4f,1.8f), new Vector3(5.8f,3.8f,4.2f), new Vector3(2.4f,1.5f,2.1f), new Vector3(6.8f,4.8f,4.5f), new Vector3(3.4f,2.1f,2.8f), new Vector3(1.6f,1f,1.3f), new Vector3(4.5f,3f,3.6f) };
            for (var i = 0; i < positions.Length; i++)
            {
                var p = positions[i]; p.y = StylizedGeometry.GroundHeight(p.x, p.z) + scales[i].y * 0.38f;
                var rock = RockVisual("Rounded Rock Formation", StylizedGeometry.Rock(i % 4), Material(i % 3 == 0 ? new Color(0.43f, 0.45f, 0.4f) : RockWarm), parent, i < 6, true, "starting_rock_" + i);
                rock.transform.localPosition = p; rock.transform.localScale = scales[i]; rock.transform.localRotation = Quaternion.Euler(0f, i * 37f, i % 2 == 0 ? -5f : 4f);
            }
            for (var i = 0; i < 12; i++)
            {
                var x = -25f + (i % 6) * 10f; var z = 2f + (i / 6) * 12f;
                var rock = RockVisual("Small Beach Rock", StylizedGeometry.Rock(i % 4), Material(RockWarm), parent, false, false, "starting_small_rock_" + i);
                rock.transform.localPosition = new Vector3(x, StylizedGeometry.GroundHeight(x, z) + 0.18f, z); rock.transform.localScale = new Vector3(0.48f + i % 3 * 0.15f, 0.3f, 0.42f);
            }
        }

        static void CreateVegetation(PlatformProfile profile, Transform parent)
        {
            var palmPositions = new[] { new Vector3(-27f,0f,31f), new Vector3(-15f,0f,39f), new Vector3(29f,0f,34f), new Vector3(39f,0f,47f), new Vector3(-42f,0f,57f), new Vector3(-5f,0f,62f), new Vector3(18f,0f,68f), new Vector3(48f,0f,72f), new Vector3(-25f,0f,82f), new Vector3(7f,0f,89f), new Vector3(33f,0f,94f), new Vector3(-51f,0f,95f) };
            var palmCount = Mathf.Clamp(Mathf.RoundToInt(palmPositions.Length * profile.vegetationDensity), 6, palmPositions.Length);
            for (var i = 0; i < palmCount; i++) CreatePalm(palmPositions[i], "tree_" + i, parent, i);
            var broadleafCount = Mathf.Clamp(Mathf.RoundToInt(7 * profile.vegetationDensity), 3, 7);
            for (var i = 0; i < broadleafCount; i++) { var x = -46f + i * 15f; var z = 58f + (i % 3) * 12f; CreateBroadleafTree(new Vector3(x, StylizedGeometry.GroundHeight(x, z), z), parent, i); }
            var bushCount = Mathf.Clamp(Mathf.RoundToInt(18 * profile.vegetationDensity), 8, 18);
            for (var i = 0; i < bushCount; i++) { var x = -43f + (i % 7) * 14f + Mathf.Sin(i * 2.1f) * 2f; var z = 27f + (i / 7) * 18f + Mathf.Cos(i) * 3f; CreateBush(new Vector3(x, StylizedGeometry.GroundHeight(x, z), z), parent, i, true); }
            var grassCount = Mathf.Clamp(Mathf.RoundToInt(28 * profile.vegetationDensity), 12, 28);
            for (var i = 0; i < grassCount; i++) { var x = -48f + (i % 9) * 12f; var z = 25f + (i / 9) * 17f + Mathf.Sin(i) * 2f; CreateGrassCluster(new Vector3(x, StylizedGeometry.GroundHeight(x, z), z), parent, i, true); }
            for (var i = 0; i < 7; i++) { var x = -32f + i * 11f; var z = 17f + (i % 2) * 6f; AddBranch(parent, new Vector3(x, StylizedGeometry.GroundHeight(x, z) + 0.08f, z), new Vector3(0.45f, 1.4f, 0.45f), Quaternion.Euler(72f, i * 28f, 10f), "Driftwood"); }
        }

        static void CreateResources(Transform parent)
        {
            var pickupData = new[] { "stick", "stone", "coconut", "stick", "stone", "coconut", "fiber", "stick", "stone", "fiber", "stick", "stone", "wood", "fiber", "stone", "wood", "fiber", "stick", "stone", "fiber", "wood", "stone", "fiber", "stick", "stone", "fiber", "stone", "wood" };
            var positions = new[]
            {
                new Vector2(-18f,5f), new Vector2(-8f,8f), new Vector2(4f,7f), new Vector2(17f,14f), new Vector2(-21f,20f), new Vector2(11f,24f),
                new Vector2(-12f,58f), new Vector2(15f,72f), new Vector2(-8f,96f), new Vector2(14f,116f),
                new Vector2(-24f,142f), new Vector2(22f,166f), new Vector2(-38f,181f), new Vector2(34f,189f),
                new Vector2(-112f,187f), new Vector2(-126f,226f), new Vector2(-82f,238f),
                new Vector2(77f,205f), new Vector2(112f,214f), new Vector2(86f,244f),
                new Vector2(-28f,272f), new Vector2(18f,286f), new Vector2(42f,306f),
                new Vector2(88f,61f), new Vector2(121f,82f), new Vector2(143f,51f), new Vector2(164f,75f), new Vector2(151f,41f)
            };
            for (var i = 0; i < pickupData.Length; i++) { var x = positions[i].x; var z = positions[i].y; CreatePickup(pickupData[i], new Vector3(x, StylizedGeometry.GroundHeight(x, z) + 0.32f, z), parent, i); }
        }

        static void CreateWreck(Vector3 position, Transform parent)
        {
            var root = Child("Starting Wreck Landmark", parent); root.localPosition = position; root.localRotation = Quaternion.Euler(0f, -24f, 0f);
            root.gameObject.AddComponent<LandmarkProtectionVolume>().radius = 9f;
            CreateWreckGameplayColliders(root);
            if (ServiceRegistry.Visuals != null) ServiceRegistry.Visuals.CreatePresentation(VisualAssetKey.Shipwreck, root, "starting_beach_wreck", CreateFallbackWreck);
            else CreateFallbackWreck(root);
        }

        static GameObject CreateFallbackWreck(Transform parent)
        {
            var root = Child("Procedural Shipwreck Fallback", parent);
            for (var i = 0; i < 6; i++)
            {
                var plank = MeshObject("Broken Hull Plank", StylizedGeometry.Plank(), Material(new Color(0.38f + i * 0.012f, 0.2f, 0.075f)), root, false);
                plank.transform.localPosition = new Vector3((i - 2.5f) * 0.62f, 0.48f + Mathf.Abs(i - 2.5f) * 0.18f, 0f); plank.transform.localScale = new Vector3(0.52f, 0.14f, 4.2f - Mathf.Abs(i - 2.5f) * 0.3f); plank.transform.localRotation = Quaternion.Euler(6f + i * 2f, 0f, (i - 2.5f) * -10f);
            }
            for (var i = 0; i < 4; i++) { var rib = MeshObject("Exposed Hull Rib", StylizedGeometry.Branch(), Material(TrunkBrown), root, false); rib.transform.localPosition = new Vector3((i - 1.5f) * 1.05f, 0.28f, -0.3f); rib.transform.localScale = new Vector3(1.1f, 2.4f, 1.1f); rib.transform.localRotation = Quaternion.Euler(68f, 0f, i % 2 == 0 ? 16f : -16f); }
            CreateCrate(root, new Vector3(-3.2f, 0.45f, 1.8f), Quaternion.Euler(0f, 18f, -7f)); CreateCrate(root, new Vector3(3f, 0.36f, 2.6f), Quaternion.Euler(0f, -28f, 4f));
            AddBranch(root, new Vector3(-1.3f, 0.2f, 4.3f), new Vector3(0.65f, 2.1f, 0.65f), Quaternion.Euler(75f, 24f, 0f), "Scattered Mast Fragment");
            return root.gameObject;
        }

        static void CreateWreckGameplayColliders(Transform parent)
        {
            var collisionRoot = Child("Wreck Gameplay Colliders", parent);
            for (var i = 0; i < 6; i++)
            {
                var plank = Child("Hull Collider", collisionRoot); plank.localPosition = new Vector3((i - 2.5f) * 0.62f, 0.48f + Mathf.Abs(i - 2.5f) * 0.18f, 0f);
                plank.localScale = new Vector3(0.52f, 0.14f, 4.2f - Mathf.Abs(i - 2.5f) * 0.3f); plank.localRotation = Quaternion.Euler(6f + i * 2f, 0f, (i - 2.5f) * -10f);
                var collider = plank.gameObject.AddComponent<MeshCollider>(); collider.sharedMesh = StylizedGeometry.Plank();
            }
            CreateCrateCollider(collisionRoot, new Vector3(-3.2f, 0.45f, 1.8f), Quaternion.Euler(0f, 18f, -7f));
            CreateCrateCollider(collisionRoot, new Vector3(3f, 0.36f, 2.6f), Quaternion.Euler(0f, -28f, 4f));
        }

        static void CreateCrateCollider(Transform parent, Vector3 position, Quaternion rotation)
        {
            var root = Child("Crate Collider", parent); root.localPosition = position; root.localRotation = rotation;
            var collider = root.gameObject.AddComponent<BoxCollider>(); collider.size = new Vector3(1.55f, 1.3f, 1.3f);
        }

        static void CreateCrate(Transform parent, Vector3 position, Quaternion rotation)
        {
            var root = Child("Damaged Crate", parent); root.localPosition = position; root.localRotation = rotation; var wood = Material(new Color(0.47f, 0.27f, 0.1f));
            foreach (var y in new[] { -0.42f, 0f, 0.42f }) { var slat = MeshObject("Crate Slat", StylizedGeometry.Plank(), wood, root, false); slat.transform.localPosition = new Vector3(0f, y, 0f); slat.transform.localScale = new Vector3(1.5f, 0.22f, 1.25f); }
        }

        static void CreatePalm(Vector3 position, string id, Transform parent, int variant)
        {
            position.y = StylizedGeometry.GroundHeight(position.x, position.z); var root = Child("Stylized Palm " + id, parent); root.localPosition = position; root.localRotation = Quaternion.Euler(0f, variant * 31f, 0f);
            var collider = root.gameObject.AddComponent<CapsuleCollider>(); collider.center = new Vector3(0.15f, 3.6f, 0f); collider.height = 7.2f; collider.radius = 0.48f;
            var tree = root.gameObject.AddComponent<TreeGatherable>(); tree.persistentId = id;
            var key = variant % 3 == 0 ? VisualAssetKey.PalmA : variant % 3 == 1 ? VisualAssetKey.PalmB : VisualAssetKey.PalmC;
            if (ServiceRegistry.Visuals != null) ServiceRegistry.Visuals.CreatePresentation(key, root, id, visualParent => CreateFallbackPalm(visualParent, variant));
            else CreateFallbackPalm(root, variant);
        }

        static GameObject CreateFallbackPalm(Transform parent, int variant)
        {
            var visual = Child("Procedural Palm Fallback", parent);
            var full = Child("LOD0 Handcrafted Palm", visual); var trunk = MeshObject("Curved Trunk", StylizedGeometry.PalmTrunk(), Material(TrunkBrown), full, false).GetComponent<Renderer>(); var fullRenderers = new List<Renderer> { trunk };
            for (var i = 0; i < 7; i++) { var leaf = MeshObject("Palm Frond", StylizedGeometry.PalmLeaf(), Material(i % 3 == 0 ? DeepGreen : LeafGreen), full, false).GetComponent<Renderer>(); leaf.transform.localPosition = new Vector3(0.32f, 7.55f, 0.1f); leaf.transform.localRotation = Quaternion.Euler(variant % 2 == 0 ? -5f : 5f, i * 51.43f, 0f); fullRenderers.Add(leaf); }
            var low = Child("LOD1 Simplified Palm", visual); var lowRenderers = new List<Renderer>(); var lowTrunk = MeshObject("Low Trunk", StylizedGeometry.Branch(), Material(TrunkBrown), low, false).GetComponent<Renderer>(); lowTrunk.transform.localScale = new Vector3(2.6f, 5.1f, 2.6f); lowRenderers.Add(lowTrunk);
            for (var i = 0; i < 4; i++) { var leaf = MeshObject("Low Frond", StylizedGeometry.PalmLeaf(), Material(DeepGreen), low, false, false).GetComponent<Renderer>(); leaf.transform.localPosition = Vector3.up * 7.5f; leaf.transform.localRotation = Quaternion.Euler(0f, i * 90f, 0f); leaf.transform.localScale = new Vector3(1.15f, 1f, 1.05f); lowRenderers.Add(leaf); }
            var lodGroup = visual.gameObject.AddComponent<LODGroup>(); lodGroup.fadeMode = LODFadeMode.CrossFade; lodGroup.animateCrossFading = true; lodGroup.SetLODs(new[] { new LOD(0.18f, fullRenderers.ToArray()), new LOD(0.045f, lowRenderers.ToArray()) }); lodGroup.RecalculateBounds();
            return visual.gameObject;
        }

        static void CreateBroadleafTree(Vector3 position, Transform parent, int variant)
        {
            var root = Child("Tropical Broadleaf Tree", parent); root.localPosition = position; root.localRotation = Quaternion.Euler(0f, variant * 47f, 0f);
            var collider = root.gameObject.AddComponent<CapsuleCollider>(); collider.center = new Vector3(0f, 2.2f, 0f); collider.height = 4.4f; collider.radius = 0.42f;
            if (ServiceRegistry.Visuals != null) ServiceRegistry.Visuals.CreatePresentation(VisualAssetKey.TropicalTreeA, root, "broadleaf_" + variant, visualParent => CreateFallbackBroadleaf(visualParent, variant));
            else CreateFallbackBroadleaf(root, variant);
        }

        static GameObject CreateFallbackBroadleaf(Transform parent, int variant)
        {
            var visual = Child("Procedural Broadleaf Fallback", parent); var trunk = MeshObject("Broadleaf Trunk", StylizedGeometry.Branch(), Material(TrunkBrown), visual, false); trunk.transform.localScale = new Vector3(2.1f, 3.1f, 2.1f);
            for (var i = 0; i < 5; i++) { var canopy = MeshObject("Rounded Leaf Crown", StylizedGeometry.Rock((variant + i) % 4), Material(i % 2 == 0 ? LeafGreen : DeepGreen), visual, false); canopy.transform.localPosition = new Vector3(Mathf.Sin(i * 2.2f) * 1.3f, 4.4f + (i % 2) * 0.8f, Mathf.Cos(i * 2.2f) * 1.1f); canopy.transform.localScale = new Vector3(2.2f, 1.65f, 2f); }
            return visual.gameObject;
        }

        static void CreateBush(Vector3 position, Transform parent, int variant, bool phase02Art = false)
        {
            var root = Child("Rounded Tropical Bush", parent); root.localPosition = position;
            if (ServiceRegistry.Visuals != null) ServiceRegistry.Visuals.CreatePresentation(phase02Art ? VisualAssetKey.BushA : VisualAssetKey.Bush, root, "bush_" + variant, visualParent => CreateFallbackBush(visualParent, variant));
            else CreateFallbackBush(root, variant);
        }

        static GameObject CreateFallbackBush(Transform parent, int variant)
        {
            var visual = Child("Procedural Bush Fallback", parent);
            for (var i = 0; i < 3; i++) { var lobe = MeshObject("Bush Lobe", StylizedGeometry.Rock((variant + i) % 4), Material(i == 1 ? LeafGreen : DeepGreen), visual, false, false); lobe.transform.localPosition = new Vector3((i - 1) * 0.65f, 0.65f + (i % 2) * 0.25f, Mathf.Sin(i * 2f) * 0.35f); lobe.transform.localScale = new Vector3(1.1f, 0.85f, 0.9f); }
            return visual.gameObject;
        }

        static void CreateGrassCluster(Vector3 position, Transform parent, int variant, bool phase02Art = false)
        {
            var root = Child("Broad Grass Cluster", parent); root.localPosition = position; root.localRotation = Quaternion.Euler(0f, variant * 53f, 0f);
            if (ServiceRegistry.Visuals != null) ServiceRegistry.Visuals.CreatePresentation(phase02Art ? VisualAssetKey.GrassA : VisualAssetKey.GrassCluster, root, "grass_" + variant, visualParent => CreateFallbackGrass(visualParent));
            else CreateFallbackGrass(root);
        }

        static GameObject CreateFallbackGrass(Transform parent)
        {
            var visual = Child("Procedural Grass Fallback", parent);
            for (var i = 0; i < 3; i++) { var blade = MeshObject("Solid Grass Blade", StylizedGeometry.PalmLeaf(), Material(i == 0 ? LeafGreen : Grass), visual, false, false); blade.transform.localRotation = Quaternion.Euler(-72f + i * 5f, i * 120f, 0f); blade.transform.localScale = new Vector3(0.16f, 0.22f, 0.34f); }
            return visual.gameObject;
        }

        static void CreatePickup(string id, Vector3 position, Transform parent, int variant)
        {
            Mesh mesh; Color color; Vector3 scale; Quaternion rotation;
            if (id == "stone") { mesh = StylizedGeometry.Rock(variant % 4); color = new Color(0.46f, 0.48f, 0.43f); scale = new Vector3(0.45f, 0.3f, 0.4f); rotation = Quaternion.Euler(0f, variant * 31f, 0f); }
            else if (id == "coconut") { mesh = StylizedGeometry.Rock(variant % 4); color = new Color(0.32f, 0.16f, 0.055f); scale = new Vector3(0.32f, 0.38f, 0.32f); rotation = Quaternion.Euler(12f, variant * 29f, 0f); }
            else if (id == "fiber") { mesh = StylizedGeometry.PalmLeaf(); color = new Color(0.48f, 0.7f, 0.22f); scale = new Vector3(0.15f, 0.2f, 0.28f); rotation = Quaternion.Euler(-68f, variant * 41f, 0f); }
            else { mesh = StylizedGeometry.Branch(); color = TrunkBrown; scale = new Vector3(0.65f, 0.9f, 0.65f); rotation = Quaternion.Euler(72f, variant * 37f, 15f); }
            var root = new GameObject("Pickup_" + id); root.transform.SetParent(parent, false); root.transform.localPosition = position; root.transform.localScale = scale; root.transform.localRotation = rotation;
            var collider = root.AddComponent<MeshCollider>(); collider.sharedMesh = mesh;
            var pickup = root.AddComponent<PickupResource>(); pickup.itemId = id;
            var visualKey = id == "stone" ? VisualAssetKey.PickupStone : id == "coconut" ? VisualAssetKey.PickupCoconut
                : id == "fiber" ? VisualAssetKey.PickupFiber : id == "wood" ? VisualAssetKey.PickupWood : VisualAssetKey.PickupStick;
            if (position.z < 40f && id == "stone") visualKey = VisualAssetKey.ResourceStone;
            else if (position.z < 40f && (id == "wood" || id == "stick")) visualKey = VisualAssetKey.ResourceWood;
            if (ServiceRegistry.Visuals != null) ServiceRegistry.Visuals.CreatePresentation(visualKey, root.transform, id, visualParent => CreateFallbackPickup(visualParent, id, mesh, color));
            else CreateFallbackPickup(root.transform, id, mesh, color);
        }

        static GameObject CreateFallbackPickup(Transform parent, string id, Mesh mesh, Color color)
        { return MeshObject("Procedural " + id + " Fallback", mesh, Material(color), parent, false, false); }

        static void CreateInteriorBackdrop(Transform parent)
        {
            for (var i = 0; i < 7; i++) { var silhouette = MeshObject("Origin Interior Silhouette", StylizedGeometry.Rock(i % 4), Material(new Color(0.08f, 0.28f, 0.17f)), parent, false, false); silhouette.transform.localPosition = new Vector3(-70f + i * 23f, 14f + (i % 3) * 6f, 113f + Mathf.Abs(3 - i) * 3f); silhouette.transform.localScale = new Vector3(25f, 24f + (i % 3) * 5f, 16f); }
        }

        static Mesh CreateRibbon(Vector3[] points, float width, float heightOffset)
        {
            var vertices = new List<Vector3>(points.Length * 2); var triangles = new List<int>((points.Length - 1) * 6);
            for (var i = 0; i < points.Length; i++)
            {
                var before = points[Mathf.Max(0, i - 1)]; var after = points[Mathf.Min(points.Length - 1, i + 1)];
                var direction = (after - before).normalized; var side = new Vector3(-direction.z, 0f, direction.x) * width * 0.5f;
                var center = points[i]; center.y = StylizedGeometry.GroundHeight(center.x, center.z) + heightOffset;
                vertices.Add(center - side); vertices.Add(center + side);
            }
            for (var i = 0; i < points.Length - 1; i++) { var a = i * 2; var b = a + 1; var c = a + 2; var d = a + 3; triangles.Add(a); triangles.Add(c); triangles.Add(b); triangles.Add(b); triangles.Add(c); triangles.Add(d); }
            return BuildMesh("Curved Water Ribbon", vertices, triangles);
        }

        static Mesh CreateDisc(float radius, int segments)
        {
            var vertices = new List<Vector3> { Vector3.zero }; var triangles = new List<int>(segments * 3);
            for (var i = 0; i <= segments; i++) { var angle = i / (float)segments * Mathf.PI * 2f; var variation = 1f + Mathf.Sin(i * 2.37f) * 0.045f; vertices.Add(new Vector3(Mathf.Cos(angle) * radius * variation, 0f, Mathf.Sin(angle) * radius * variation)); }
            for (var i = 0; i < segments; i++) { triangles.Add(0); triangles.Add(i + 2); triangles.Add(i + 1); }
            return BuildMesh("Organic Water Pool", vertices, triangles);
        }

        static void CreateWaterfall(Transform parent, Vector3 topCenter, float width, float height)
        {
            var vertices = new List<Vector3>(); var triangles = new List<int>(); const int rows = 5;
            for (var row = 0; row <= rows; row++)
            {
                var t = row / (float)rows; var center = topCenter + Vector3.down * height * t + Vector3.forward * Mathf.Sin(t * Mathf.PI) * 1.2f;
                vertices.Add(center + Vector3.left * width * 0.5f); vertices.Add(center + Vector3.right * width * 0.5f);
            }
            for (var i = 0; i < rows; i++) { var a = i * 2; var b = a + 1; var c = a + 2; var d = a + 3; triangles.Add(a); triangles.Add(c); triangles.Add(b); triangles.Add(b); triangles.Add(c); triangles.Add(d); }
            var material = ServiceRegistry.Visuals != null ? ServiceRegistry.Visuals.ResolveMaterial(VisualMaterialKey.Lagoon) : Material(new Color(0.55f,0.88f,0.9f));
            MeshObject("Lightweight Waterfall", BuildMesh("Waterfall Ribbon", vertices, triangles), material, parent, false, false);
        }

        static Mesh BuildMesh(string name, List<Vector3> vertices, List<int> triangles)
        {
            var mesh = new Mesh { name = name }; mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }

        static Mesh CreateFoamRibbon()
        {
            const int segments = 32; var vertices = new List<Vector3>((segments + 1) * 2); var triangles = new List<int>(segments * 6);
            for (var i = 0; i <= segments; i++) { var x = Mathf.Lerp(-52f, 52f, i / (float)segments); var shore = StylizedGeometry.ShoreZ(x); vertices.Add(new Vector3(x, -0.46f, shore - 1.5f)); vertices.Add(new Vector3(x, -0.45f, shore + 0.25f)); }
            for (var i = 0; i < segments; i++) { var a = i * 2; var b = a + 1; var c = a + 2; var d = a + 3; triangles.Add(a); triangles.Add(c); triangles.Add(b); triangles.Add(b); triangles.Add(c); triangles.Add(d); }
            var mesh = new Mesh { name = "Curved Shore Foam Ribbon" }; mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }

        static GameObject MeshObject(string name, Mesh mesh, Material material, Transform parent, bool addCollider, bool castShadows = true)
        {
            var obj = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer)); obj.transform.SetParent(parent, false); obj.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = obj.GetComponent<MeshRenderer>(); renderer.sharedMaterial = material; renderer.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off; renderer.receiveShadows = castShadows;
            if (addCollider) { var collider = obj.AddComponent<MeshCollider>(); collider.sharedMesh = mesh; }
            return obj;
        }

        static GameObject RockVisual(string name, Mesh collisionMesh, Material fallbackMaterial, Transform parent, bool addCollider, bool castShadows, string identity)
        {
            var root = new GameObject(name); root.transform.SetParent(parent, false);
            if (addCollider) { var collider = root.AddComponent<MeshCollider>(); collider.sharedMesh = collisionMesh; }
            if (ServiceRegistry.Visuals != null)
                ServiceRegistry.Visuals.CreatePresentation(StartingRockKey(identity), root.transform, identity,
                    visualParent => MeshObject("Procedural Rock Fallback", collisionMesh, fallbackMaterial, visualParent, false, castShadows));
            else MeshObject("Procedural Rock Fallback", collisionMesh, fallbackMaterial, root.transform, false, castShadows);
            return root;
        }

        static VisualAssetKey StartingRockKey(string identity)
        {
            if (identity.StartsWith("starting_small")) return VisualAssetKey.RockSmall;
            if (identity == "starting_rock_0" || identity == "starting_rock_4") return VisualAssetKey.RockCoastal;
            if (identity == "starting_rock_1" || identity == "starting_rock_3" || identity == "starting_rock_6") return VisualAssetKey.RockMedium;
            return identity.StartsWith("starting_rock") ? VisualAssetKey.RockLarge : VisualAssetKey.Rock;
        }

        static Transform Child(string name, Transform parent) { var child = new GameObject(name).transform; child.SetParent(parent, false); return child; }
        static void AddBranch(Transform parent, Vector3 position, Vector3 scale, Quaternion rotation, string name) { var branchObject = MeshObject(name, StylizedGeometry.Branch(), Material(TrunkBrown), parent, false); branchObject.transform.localPosition = position; branchObject.transform.localScale = scale; branchObject.transform.localRotation = rotation; }
    }
}
