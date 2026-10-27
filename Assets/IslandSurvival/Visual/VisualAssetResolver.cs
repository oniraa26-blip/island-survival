using System;
using System.Collections.Generic;
using IslandSurvival.Platform;
using IslandSurvival.Presentation;
using UnityEngine;

namespace IslandSurvival.Visual
{
    public sealed class VisualAssetResolver
    {
        const string CatalogResourcePath = "Visual/VisualAssetCatalog";
        readonly VisualAssetCatalog catalog;
        readonly VisualQualityPolicy quality;
        readonly Dictionary<VisualMaterialKey, Material> generatedMaterials = new Dictionary<VisualMaterialKey, Material>();

        public VisualAssetResolver(VisualAssetCatalog assetCatalog, PlatformProfile profile)
        { catalog = assetCatalog; quality = new VisualQualityPolicy(profile); }

        public static VisualAssetResolver Load(PlatformProfile profile)
        { return new VisualAssetResolver(Resources.Load<VisualAssetCatalog>(CatalogResourcePath), profile); }

        public bool HasCatalog { get { return catalog != null; } }
        public TropicalDaylightProfile Daylight { get { return catalog != null ? catalog.TropicalDaylight : null; } }

        public GameObject CreatePresentation(VisualAssetKey key, Transform parent, string gameplayIdentity, Func<Transform, GameObject> proceduralFallback)
        {
            GameObject visual;
            SpriteVisualDefinition spriteDefinition;
            Sprite authoredSprite;
            var initialSpriteSlot = key == VisualAssetKey.PlayerCharacter || key == VisualAssetKey.PlayerStarter
                ? SpriteDirectionResolver.AtlasSlot(ScreenFacing.Front)
                : SpriteDirection.S;
            if (PresentationRuntime.IsTwoPointFiveD && catalog != null
                && catalog.TryGetSpriteVisual(key, out spriteDefinition)
                && spriteDefinition.Idle.TryGet(initialSpriteSlot, out authoredSprite))
            {
                visual = new GameObject("2.5D " + key);
                visual.transform.SetParent(parent, false);
                var worldVisual = visual.AddComponent<SpriteWorldVisual>();
                worldVisual.Initialize(spriteDefinition, authoredSprite,
                    SpriteSortCategory.WorldObject,
                    key == VisualAssetKey.PlayerCharacter);
                var authoredMarker = visual.AddComponent<VisualPrefabInstance>();
                authoredMarker.Initialize(key, gameplayIdentity, false);
                return visual;
            }
            GameObject prefab = null;
            var hasPrefab = catalog != null && catalog.TryGetPrefab(key, out prefab);
            var isFallback = !hasPrefab;
            if (isFallback)
            {
                if (proceduralFallback == null) return null;
                visual = proceduralFallback(parent);
            }
            else
            {
                visual = UnityEngine.Object.Instantiate(prefab, parent, false);
                visual.name = prefab.name;
            }
            if (visual == null) return null;
            if (visual.transform.parent != parent) visual.transform.SetParent(parent, false);
            var marker = visual.GetComponent<VisualPrefabInstance>();
            if (marker == null) marker = visual.AddComponent<VisualPrefabInstance>();
            marker.Initialize(key, gameplayIdentity, isFallback);
            quality.Apply(visual, key);
            if (PresentationRuntime.IsTwoPointFiveD)
                TwoPointFiveDVisualAdapter.Attach(visual, key);
            return visual;
        }

        public Material ResolveMaterial(VisualMaterialKey key)
        {
            Material material;
            if (catalog != null && catalog.TryGetMaterial(key, out material)) return material;
            if (generatedMaterials.TryGetValue(key, out material) && material != null) return material;
            material = VisualFallbackMaterials.Create(key);
            generatedMaterials[key] = material;
            return material;
        }
    }

    static class VisualFallbackMaterials
    {
        public static Material Create(VisualMaterialKey key)
        {
            var shaderPath = key == VisualMaterialKey.TerrainBlend ? "Visual/Shaders/StylizedTerrain"
                : key == VisualMaterialKey.Ocean || key == VisualMaterialKey.Lagoon || key == VisualMaterialKey.ShoreFoam
                    ? "Visual/Shaders/TropicalWater" : "Visual/Shaders/StylizedSurface";
            var shader = Resources.Load<Shader>(shaderPath);
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");
            var material = new Material(shader) { name = "Runtime Fallback " + key, enableInstancing = true };
            Configure(material, key); return material;
        }

        static void Configure(Material material, VisualMaterialKey key)
        {
            Color color;
            switch (key)
            {
                case VisualMaterialKey.Vegetation: color = new Color(0.13f, 0.52f, 0.22f); break;
                case VisualMaterialKey.BarkWood: color = new Color(0.39f, 0.21f, 0.09f); break;
                case VisualMaterialKey.Rock: color = new Color(0.42f, 0.43f, 0.38f); break;
                case VisualMaterialKey.WetSand: color = new Color(0.66f, 0.48f, 0.28f); break;
                case VisualMaterialKey.Dirt: color = new Color(0.45f, 0.29f, 0.14f); break;
                case VisualMaterialKey.Grass: color = new Color(0.24f, 0.57f, 0.25f); break;
                case VisualMaterialKey.Prop: color = new Color(0.48f, 0.27f, 0.1f); break;
                default: color = new Color(0.93f, 0.76f, 0.48f); break;
            }
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.08f);
            if (key == VisualMaterialKey.TerrainBlend)
            {
                material.SetColor("_DrySandColor", new Color(0.96f, 0.76f, 0.43f));
                material.SetColor("_WetSandColor", new Color(0.76f, 0.56f, 0.32f));
                material.SetColor("_GrassColor", new Color(0.2f, 0.62f, 0.25f));
                material.SetColor("_DirtColor", new Color(0.52f, 0.34f, 0.16f));
            }
            if (key == VisualMaterialKey.Ocean || key == VisualMaterialKey.Lagoon || key == VisualMaterialKey.ShoreFoam)
            {
                material.SetColor("_DeepColor", key == VisualMaterialKey.Lagoon ? new Color(0.025f, 0.45f, 0.55f, 0.86f) : new Color(0.015f, 0.31f, 0.48f, 0.88f));
                material.SetColor("_ShallowColor", key == VisualMaterialKey.ShoreFoam ? new Color(0.94f, 1f, 0.97f, 0.9f) : new Color(0.06f, 0.8f, 0.74f, 0.82f));
                material.SetFloat("_Lagoon", key == VisualMaterialKey.Lagoon ? 1f : 0f);
                material.SetFloat("_FoamOnly", key == VisualMaterialKey.ShoreFoam ? 1f : 0f);
            }
        }
    }
}
