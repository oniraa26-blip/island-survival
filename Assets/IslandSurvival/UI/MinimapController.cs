using System.Collections.Generic;
using IslandSurvival.Core;
using IslandSurvival.World;
using UnityEngine;
using UnityEngine.UI;

namespace IslandSurvival.UI
{
    public sealed class MinimapController : MonoBehaviour
    {
        const float UpdateInterval = 0.1f;
        public const float ReferenceWidthFraction = 286f / 1920f;
        readonly Dictionary<MinimapMarker, RectTransform> views = new Dictionary<MinimapMarker, RectTransform>();
        MinimapMarkerRegistry registry;
        RectTransform markerLayer;
        Transform player;
        float range = 55f;
        float radius = 118f;
        float nextUpdate;
        static Sprite playerArrow;
        static Sprite roundedPanel;
        System.Action openWorldMap;

        public MinimapMarkerRegistry Registry { get { return registry; } }
        public Transform Player { get { return player; } }
        public bool HasWorldMapCallback { get { return openWorldMap != null; } }

        public void Initialize(RectTransform safeArea, Transform localPlayer, System.Action worldMapCallback = null)
        {
            player = localPlayer; openWorldMap = worldMapCallback; registry = new MinimapMarkerRegistry();
            Build(safeArea); registry.RegisterLocalPlayer(player); RegisterOriginLandmarks(); SyncViews(); Refresh();
        }

        void Build(RectTransform safeArea)
        {
            var frame = CreateImage("MinimapFrame", safeArea, new Color(0.02f, 0.12f, 0.14f, 0.88f));
            frame.sprite = GetRoundedPanel();
            frame.raycastTarget = true;
            var button = frame.gameObject.AddComponent<Button>(); button.targetGraphic = frame; button.transition = Selectable.Transition.ColorTint; button.onClick.AddListener(RequestWorldMap);
            var colors = button.colors; colors.highlightedColor = new Color(0.82f, 1f, 0.92f, 1f); colors.pressedColor = new Color(0.65f, 0.9f, 0.78f, 1f); button.colors = colors;
            var frameRect = frame.rectTransform; frameRect.anchorMin = frameRect.anchorMax = new Vector2(1f, 1f);
            frameRect.pivot = new Vector2(1f, 1f); frameRect.anchoredPosition = new Vector2(-22f, -18f); frameRect.sizeDelta = new Vector2(286f, 286f);
            var map = CreateImage("Map", frameRect, new Color(0.12f, 0.42f, 0.34f, 0.92f));
            map.sprite = GetRoundedPanel();
            markerLayer = map.rectTransform; markerLayer.anchorMin = Vector2.zero; markerLayer.anchorMax = Vector2.one; markerLayer.offsetMin = new Vector2(10f, 10f); markerLayer.offsetMax = new Vector2(-10f, -10f);
            var north = CreateText("North", frameRect, "N", 20); north.rectTransform.anchorMin = north.rectTransform.anchorMax = new Vector2(0.5f, 1f); north.rectTransform.pivot = new Vector2(0.5f, 1f); north.rectTransform.anchoredPosition = new Vector2(0f, -5f); north.rectTransform.sizeDelta = new Vector2(30f, 28f);
        }

        public void RequestWorldMap() { if (openWorldMap != null) openWorldMap(); }

        void RegisterOriginLandmarks()
        {
            registry.RegisterLandmark("shipwreck", OriginDemoZones.StartingBeach, "map.shipwreck", new Vector3(13f, 0f, 11f));
            registry.RegisterLandmark(OriginDemoZones.FreshwaterStream, new Vector3(0f, 0f, 95f));
            registry.RegisterLandmark(OriginDemoZones.GiantTree, new Vector3(0f, 0f, 155f));
            registry.RegisterLandmark(OriginDemoZones.BlueLagoon, new Vector3(-105f, 0f, 215f));
            registry.RegisterLandmark(OriginDemoZones.TwinFalls, new Vector3(95f, 0f, 225f));
            registry.RegisterLandmark(OriginDemoZones.FlowerValley, new Vector3(0f, 0f, 292f));
            registry.RegisterLandmark(OriginDemoZones.SplitRock, new Vector3(105f, 0f, 72f));
            registry.RegisterLandmark(OriginDemoZones.SeaCave, new Vector3(158f, 0f, 58f));
        }

        void Update()
        {
            if (Time.unscaledTime < nextUpdate) return;
            nextUpdate = Time.unscaledTime + UpdateInterval; SyncViews(); Refresh();
        }

        void SyncViews()
        {
            var markers = registry.Markers;
            for (var i = 0; i < markers.Count; i++)
            {
                var marker = markers[i]; if (views.ContainsKey(marker)) continue;
                var image = CreateImage(marker.Id, markerLayer, IconColor(marker));
                if (marker.Category == MinimapMarkerCategory.Player) { image.sprite = GetPlayerArrow(); image.preserveAspect = true; }
                var rect = image.rectTransform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
                var size = MinimapIconLibrary.Get(marker.Category).size;
                rect.sizeDelta = new Vector2(size, size); views.Add(marker, rect);
            }
        }

        void Refresh()
        {
            if (player == null || registry == null) return;
            foreach (var pair in views)
            {
                var marker = pair.Key; var view = pair.Value;
                var discovered = string.IsNullOrEmpty(marker.RequiredDiscoveryId) || ServiceRegistry.Discovery.IsDiscovered(marker.RequiredDiscoveryId);
                var visible = registry.Contains(marker) && marker.Active && discovered; view.gameObject.SetActive(visible); if (!visible) continue;
                bool outside; var normalized = MinimapProjection.Project(marker.Position, player.position, range, out outside);
                var magnitude = normalized.magnitude;
                if (outside)
                {
                    if (!marker.ClampToEdge) { view.gameObject.SetActive(false); continue; }
                    normalized /= magnitude; view.localScale = Vector3.one * 0.72f;
                }
                else view.localScale = Vector3.one;
                view.anchoredPosition = normalized * radius;
                if (marker.Category == MinimapMarkerCategory.Player) view.localEulerAngles = new Vector3(0f, 0f, MinimapProjection.PlayerArrowRotation(player.forward));
            }
        }

        static Color IconColor(MinimapMarker marker)
        {
            if (marker.Category == MinimapMarkerCategory.Player || marker.Category == MinimapMarkerCategory.Teammate) return marker.IdentifierColor;
            return MinimapIconLibrary.Get(marker.Category).color;
        }

        static Image CreateImage(string name, Transform parent, Color color)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Image)); obj.transform.SetParent(parent, false);
            var image = obj.GetComponent<Image>(); image.color = color; image.raycastTarget = false; return image;
        }

        static Text CreateText(string name, Transform parent, string value, int size)
        {
            var obj = new GameObject(name, typeof(RectTransform), typeof(Text)); obj.transform.SetParent(parent, false);
            var text = obj.GetComponent<Text>(); text.text = value; text.fontSize = size; text.color = Color.white; text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.Load<Font>("Fonts/NotoSans-Regular"); text.raycastTarget = false; return text;
        }

        static Sprite GetPlayerArrow()
        {
            if (playerArrow != null) return playerArrow;
            const int size = 16; var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "MinimapPlayerArrow" };
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++) for (var x = 0; x < size; x++)
            {
                var halfWidth = Mathf.Lerp(7f, 1f, y / (float)(size - 1));
                pixels[y * size + x] = Mathf.Abs(x - 7.5f) <= halfWidth ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
            }
            texture.SetPixels32(pixels); texture.Apply(false, true);
            playerArrow = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            return playerArrow;
        }

        static Sprite GetRoundedPanel()
        {
            if (roundedPanel != null) return roundedPanel;
            const int size = 32; const float corner = 7f;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "MinimapRoundedPanel" };
            var pixels = new Color32[size * size];
            for (var y = 0; y < size; y++) for (var x = 0; x < size; x++)
            {
                var dx = Mathf.Max(0f, corner - Mathf.Min(x, size - 1 - x));
                var dy = Mathf.Max(0f, corner - Mathf.Min(y, size - 1 - y));
                pixels[y * size + x] = dx * dx + dy * dy <= corner * corner ? new Color32(255, 255, 255, 255) : new Color32(0, 0, 0, 0);
            }
            texture.SetPixels32(pixels); texture.Apply(false, true);
            roundedPanel = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
            return roundedPanel;
        }
    }

    public static class MinimapProjection
    {
        public static float PlayerArrowRotation(Vector3 worldForward)
        {
            return -Mathf.Atan2(worldForward.x, worldForward.z) * Mathf.Rad2Deg;
        }

        public static Vector2 Project(Vector3 markerPosition, Vector3 playerPosition, float range, out bool outside)
        {
            var delta = markerPosition - playerPosition;
            var projected = new Vector2(delta.x, delta.z) / Mathf.Max(1f, range);
            outside = projected.sqrMagnitude > 1f; return projected;
        }
    }
}
