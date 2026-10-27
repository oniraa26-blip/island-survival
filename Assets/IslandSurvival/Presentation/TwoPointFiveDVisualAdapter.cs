using System.Collections.Generic;
using IslandSurvival.Visual;
using UnityEngine;

namespace IslandSurvival.Presentation
{
    public static class TwoPointFiveDVisualAdapter
    {
        static readonly Dictionary<VisualAssetKey, Sprite> Sprites = new Dictionary<VisualAssetKey, Sprite>();
        static Sprite shadowSprite;

        public static void Attach(GameObject fallbackVisual, VisualAssetKey key)
        {
            if (fallbackVisual == null || fallbackVisual.GetComponentInChildren<SpriteWorldVisual>(true) != null) return;
            var renderers = fallbackVisual.GetComponentsInChildren<Renderer>(true);
            // Gameplay factories place their visual root at the authored ground/water contact.
            // Renderer bounds include canopy, tilted planks and other artwork and must never
            // drive the sorting anchor.
            var groundWorldY = fallbackVisual.transform.position.y;
            if (key == VisualAssetKey.PlayerCharacter && renderers.Length > 0)
            {
                // The legacy capsule fallback is centered above the gameplay root; its lower
                // bound is the verified feet contact used while production character art is absent.
                groundWorldY = renderers[0].bounds.min.y;
                for (var i = 1; i < renderers.Length; i++) groundWorldY = Mathf.Min(groundWorldY, renderers[i].bounds.min.y);
            }
            foreach (var renderer in renderers) renderer.enabled = false;

            var spec = GetSpec(key);
            var root = new GameObject("2.5D Presentation");
            root.transform.SetParent(fallbackVisual.transform, false);
            root.transform.position = new Vector3(fallbackVisual.transform.position.x, groundWorldY, fallbackVisual.transform.position.z);
            var worldVisual = root.AddComponent<SpriteWorldVisual>();
            worldVisual.Initialize(GetSprite(key, spec.color), spec.size, SpriteSortCategory.WorldObject, key == VisualAssetKey.PlayerCharacter, key);

            if (!spec.shadow) return;
            var shadow = new GameObject("Blob Shadow");
            shadow.transform.SetParent(fallbackVisual.transform, false);
            shadow.transform.position = new Vector3(fallbackVisual.transform.position.x, groundWorldY + 0.025f, fallbackVisual.transform.position.z);
            shadow.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            shadow.transform.localScale = new Vector3(spec.size.x * 0.7f, spec.size.x * 0.32f, 1f);
            var renderer2D = shadow.AddComponent<SpriteRenderer>();
            renderer2D.sprite = GetShadowSprite();
            renderer2D.color = new Color(0.04f, 0.08f, 0.08f, 0.28f);
            renderer2D.sortingOrder = (int)SpriteSortCategory.GroundDecoration;
        }

        static Sprite GetSprite(VisualAssetKey key, Color color)
        {
            Sprite sprite;
            var initialSlot = key == VisualAssetKey.PlayerCharacter || key == VisualAssetKey.PlayerStarter
                ? SpriteDirectionResolver.AtlasSlot(ScreenFacing.Front)
                : SpriteDirection.S;
            if (PresentationRuntime.PlaceholderArtEnabled && TemporaryArtLibrary.TryGet(key, initialSlot, out sprite)) return sprite;
            if (Sprites.TryGetValue(key, out sprite) && sprite != null) return sprite;
            const int width = 32, height = 48;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = "Fallback2D_" + key, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color32[width * height];
            var fill = (Color32)color;
            for (var y = 0; y < height; y++) for (var x = 0; x < width; x++)
            {
                var nx = (x + 0.5f - width * 0.5f) / (width * 0.5f);
                var ny = (y + 0.5f) / height;
                var halfWidth = Mathf.Lerp(0.35f, 0.78f, Mathf.Sin(ny * Mathf.PI));
                pixels[y * width + x] = Mathf.Abs(nx) <= halfWidth && ny > 0.04f ? fill : new Color32(0, 0, 0, 0);
            }
            texture.SetPixels32(pixels); texture.Apply(false, true);
            sprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0f), 32f, 0, SpriteMeshType.Tight);
            sprite.name = "Fallback2D_" + key; Sprites[key] = sprite; return sprite;
        }

        static Sprite GetShadowSprite()
        {
            if (shadowSprite != null) return shadowSprite;
            const int size = 32; var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "Fallback2D_BlobShadow", filterMode = FilterMode.Bilinear };
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++) for (var x = 0; x < size; x++)
            {
                var p = new Vector2((x + 0.5f) / size * 2f - 1f, (y + 0.5f) / size * 2f - 1f);
                var alpha = (byte)(Mathf.Clamp01(1f - p.sqrMagnitude) * 255f);
                pixels[y * size + x] = new Color32(255, 255, 255, alpha);
            }
            texture.SetPixels32(pixels); texture.Apply(false, true);
            shadowSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            return shadowSprite;
        }

        static VisualSpec GetSpec(VisualAssetKey key)
        {
            var size = PresentationScaleProfile.RuntimeDefault.WorldSize(key);
            switch (key)
            {
                case VisualAssetKey.PlayerCharacter: return new VisualSpec(size, new Color(0.96f, 0.43f, 0.13f), true);
                case VisualAssetKey.PalmTree:
                case VisualAssetKey.PalmA:
                case VisualAssetKey.PalmB:
                case VisualAssetKey.PalmC:
                case VisualAssetKey.ENV_PALM_A:
                case VisualAssetKey.ENV_PALM_B:
                case VisualAssetKey.ENV_PALM_C: return new VisualSpec(size, new Color(0.12f, 0.55f, 0.22f), true);
                case VisualAssetKey.TropicalTree:
                case VisualAssetKey.TropicalTreeA:
                case VisualAssetKey.ENV_TREE_A:
                case VisualAssetKey.ENV_TREE_B: return new VisualSpec(size, new Color(0.08f, 0.42f, 0.18f), true);
                case VisualAssetKey.Bush:
                case VisualAssetKey.BushA:
                case VisualAssetKey.ENV_BUSH_A:
                case VisualAssetKey.ENV_BUSH_B: return new VisualSpec(size, new Color(0.18f, 0.62f, 0.25f), true);
                case VisualAssetKey.GrassCluster:
                case VisualAssetKey.GrassA:
                case VisualAssetKey.ENV_GRASS_A:
                case VisualAssetKey.ENV_GRASS_B:
                case VisualAssetKey.ENV_GRASS_C: return new VisualSpec(size, new Color(0.32f, 0.7f, 0.28f), false);
                case VisualAssetKey.Rock:
                case VisualAssetKey.RockMedium:
                case VisualAssetKey.ENV_ROCK_MEDIUM_A: return new VisualSpec(size, new Color(0.48f, 0.5f, 0.45f), true);
                case VisualAssetKey.RockSmall:
                case VisualAssetKey.ENV_ROCK_SMALL_A:
                case VisualAssetKey.ENV_ROCK_SMALL_B: return new VisualSpec(size, new Color(0.48f, 0.5f, 0.45f), true);
                case VisualAssetKey.RockLarge:
                case VisualAssetKey.RockCoastal:
                case VisualAssetKey.ENV_ROCK_LARGE_A: return new VisualSpec(size, new Color(0.48f, 0.5f, 0.45f), true);
                case VisualAssetKey.Shipwreck:
                case VisualAssetKey.ENV_SHIPWRECK: return new VisualSpec(size, new Color(0.45f, 0.24f, 0.1f), true);
                default: return new VisualSpec(size, new Color(0.94f, 0.75f, 0.28f), true);
            }
        }

        readonly struct VisualSpec
        {
            public readonly Vector2 size; public readonly Color color; public readonly bool shadow;
            public VisualSpec(Vector2 size, Color color, bool shadow) { this.size = size; this.color = color; this.shadow = shadow; }
        }
    }
}
