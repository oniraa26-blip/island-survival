using System.Collections.Generic;
using IslandSurvival.Core;
using IslandSurvival.InputSystem;
using IslandSurvival.World;
using UnityEngine;
using UnityEngine.UI;

namespace IslandSurvival.UI
{
    public sealed class WorldMapController : MonoBehaviour
    {
        const float RefreshInterval = 0.2f;
        readonly Dictionary<WorldMapRegionDefinition, Image> fogViews = new Dictionary<WorldMapRegionDefinition, Image>();
        readonly Dictionary<MinimapMarker, RectTransform> markerViews = new Dictionary<MinimapMarker, RectTransform>();
        readonly List<GameObject> hiddenHud = new List<GameObject>();
        WorldMapDefinition definition;
        MinimapMarkerRegistry registry;
        MobileInput input;
        Transform player;
        RectTransform mapRect;
        RectTransform hudRoot;
        GameObject root;
        Text title;
        Text details;
        Text closeLabel;
        float nextRefresh;

        public bool IsOpen { get { return root != null && root.activeSelf; } }
        public WorldMapDefinition Definition { get { return definition; } }
        public MinimapMarkerRegistry Registry { get { return registry; } }
        public RectTransform MapRect { get { return mapRect; } }
        public bool GameplayInputLockConfigured { get { return input != null; } }
        public bool HasCloseAction { get { return root != null; } }

        public void Initialize(RectTransform safeArea, MobileInput mobileInput, Transform localPlayer, MinimapMarkerRegistry markerRegistry)
        {
            input = mobileInput; player = localPlayer; registry = markerRegistry; hudRoot = safeArea;
            definition = Resources.Load<WorldMapDefinition>("UI/OriginIslandMap");
            if (definition == null) definition = WorldMapDefinition.CreateOriginPlaceholder();
            var ownRect = GetComponent<RectTransform>(); Stretch(ownRect);
            Build(ownRect); root.SetActive(false); enabled = false; ServiceRegistry.Localization.LanguageChanged += RefreshLocalization;
        }

        public void Open()
        {
            if (root == null || IsOpen) return;
            hiddenHud.Clear();
            var parent = hudRoot;
            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i).gameObject;
                if (child != gameObject && child.activeSelf) { hiddenHud.Add(child); child.SetActive(false); }
            }
            root.SetActive(true); root.transform.SetAsLastSibling(); enabled = true; input.SetGameplayBlocked(true); RefreshLocalization();
            SyncMarkerViews(); Refresh();
        }

        public void Close()
        {
            if (!IsOpen) return;
            root.SetActive(false); input.SetGameplayBlocked(false);
            for (var i = 0; i < hiddenHud.Count; i++) if (hiddenHud[i] != null) hiddenHud[i].SetActive(true);
            hiddenHud.Clear(); enabled = false;
        }

        void Update()
        {
            if (!IsOpen) return;
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape)) { Close(); return; }
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + RefreshInterval; SyncMarkerViews(); Refresh();
        }

        void Build(RectTransform safeArea)
        {
            root = Panel("WorldMapRoot", safeArea, new Color(0.01f, 0.035f, 0.05f, 0.94f));
            Stretch(root.GetComponent<RectTransform>());
            var frame = Panel("IslandMapFrame", root.transform, new Color(0.06f, 0.22f, 0.21f, 1f));
            var frameRect = frame.GetComponent<RectTransform>(); frameRect.anchorMin = new Vector2(0.16f, 0.06f); frameRect.anchorMax = new Vector2(0.84f, 0.94f); frameRect.offsetMin = frameRect.offsetMax = Vector2.zero;
            title = Label("Title", frame.transform, ServiceRegistry.Localization.Get("map.title"), 34, TextAnchor.UpperCenter); title.rectTransform.anchorMin = new Vector2(0f, 0.91f); title.rectTransform.anchorMax = Vector2.one; title.rectTransform.offsetMin = title.rectTransform.offsetMax = Vector2.zero;
            var map = Panel("TemporaryOriginIslandArtwork", frame.transform, new Color(0.08f, 0.42f, 0.5f, 1f));
            mapRect = map.GetComponent<RectTransform>(); mapRect.anchorMin = new Vector2(0.08f, 0.13f); mapRect.anchorMax = new Vector2(0.92f, 0.89f); mapRect.offsetMin = mapRect.offsetMax = Vector2.zero;
            map.GetComponent<Image>().sprite = TemporaryOriginMapArt.Get();
            BuildRegions();
            details = Label("Details", frame.transform, ServiceRegistry.Localization.Get("map.tap_landmark"), 20, TextAnchor.MiddleCenter);
            details.rectTransform.anchorMin = new Vector2(0.12f, 0.025f); details.rectTransform.anchorMax = new Vector2(0.88f, 0.12f); details.rectTransform.offsetMin = details.rectTransform.offsetMax = Vector2.zero;
            var close = Panel("Close", root.transform, new Color(0.12f, 0.45f, 0.42f, 1f));
            var closeRect = close.GetComponent<RectTransform>(); closeRect.anchorMin = closeRect.anchorMax = new Vector2(0.88f, 0.88f); closeRect.sizeDelta = new Vector2(110f, 64f); closeRect.pivot = new Vector2(0.5f, 0.5f);
            close.AddComponent<Button>().onClick.AddListener(Close);
            closeLabel = Label("Label", close.transform, ServiceRegistry.Localization.Get("ui.close"), 23, TextAnchor.MiddleCenter); Stretch(closeLabel.rectTransform);
        }

        void BuildRegions()
        {
            for (var i = 0; i < definition.Regions.Count; i++)
            {
                var region = definition.Regions[i]; var normalized = definition.WorldToNormalized(region.worldCenter);
                var fog = Panel("Fog_" + region.zoneId, mapRect, new Color(0.015f, 0.025f, 0.04f, 0.82f)).GetComponent<Image>();
                var rect = fog.rectTransform; rect.anchorMin = rect.anchorMax = normalized; rect.pivot = new Vector2(0.5f, 0.5f); rect.sizeDelta = Vector2.one * (region.mapRadius * 720f);
                var button = fog.gameObject.AddComponent<Button>(); var captured = region; button.onClick.AddListener(() => SelectRegion(captured));
                var label = Label("Label", fog.transform, "?", 20, TextAnchor.MiddleCenter); Stretch(label.rectTransform); fogViews.Add(region, fog);
            }
        }

        void SyncMarkerViews()
        {
            if (registry == null) return;
            var markers = registry.Markers;
            for (var i = 0; i < markers.Count; i++)
            {
                var marker = markers[i]; if (markerViews.ContainsKey(marker)) continue;
                var icon = Panel("Marker_" + marker.Id, mapRect, MinimapIconLibrary.Get(marker.Category).color).GetComponent<Image>();
                var rect = icon.rectTransform; rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f); var size = marker.Category == MinimapMarkerCategory.Player ? 28f : 20f; rect.sizeDelta = Vector2.one * size;
                var glyph = Label("Glyph", icon.transform, MarkerGlyph(marker.Category), marker.Category == MinimapMarkerCategory.Player ? 20 : 14, TextAnchor.MiddleCenter); Stretch(glyph.rectTransform);
                var button = icon.gameObject.AddComponent<Button>(); var captured = marker; button.onClick.AddListener(() => SelectMarker(captured)); markerViews.Add(marker, rect);
            }
        }

        void Refresh()
        {
            foreach (var pair in fogViews)
            {
                var visible = definition.IsRegionVisible(pair.Key); pair.Value.color = visible ? new Color(0.2f, 0.7f, 0.43f, 0.28f) : new Color(0.015f, 0.025f, 0.04f, 0.82f);
                pair.Value.GetComponentInChildren<Text>().text = visible ? ServiceRegistry.Localization.Get(pair.Key.nameKey) : "?";
            }
            foreach (var pair in markerViews)
            {
                var marker = pair.Key; var discovered = string.IsNullOrEmpty(marker.RequiredDiscoveryId) || ServiceRegistry.Discovery.IsDiscovered(marker.RequiredDiscoveryId) || marker.RequiredDiscoveryId == OriginDemoZones.StartingBeach;
                var visible = registry.Contains(marker) && marker.Active && discovered; pair.Value.gameObject.SetActive(visible); if (!visible) continue;
                var normalized = definition.WorldToNormalized(marker.Position); pair.Value.anchorMin = pair.Value.anchorMax = normalized; pair.Value.anchoredPosition = Vector2.zero;
            }
        }

        void SelectRegion(WorldMapRegionDefinition region)
        {
            details.text = definition.IsRegionVisible(region) ? ServiceRegistry.Localization.Get(region.nameKey) + "  •  " + ServiceRegistry.Localization.Get("map.discovered") : ServiceRegistry.Localization.Get("map.undiscovered");
        }

        void SelectMarker(MinimapMarker marker)
        { details.text = marker.Category == MinimapMarkerCategory.Player ? ServiceRegistry.Localization.Get("map.you_are_here")
            : marker.Category == MinimapMarkerCategory.Landmark || marker.Category == MinimapMarkerCategory.BedHome ? ServiceRegistry.Localization.Get(marker.DisplayName) : marker.DisplayName; }

        void RefreshLocalization()
        {
            if (title != null) title.text = ServiceRegistry.Localization.Get("map.title");
            if (closeLabel != null) closeLabel.text = ServiceRegistry.Localization.Get("ui.close");
            if (details != null) details.text = ServiceRegistry.Localization.Get("map.tap_landmark");
            if (IsOpen) Refresh();
        }

        void OnDestroy()
        { if (ServiceRegistry.Localization != null) ServiceRegistry.Localization.LanguageChanged -= RefreshLocalization; }

        static string MarkerGlyph(MinimapMarkerCategory category)
        {
            switch (category)
            {
                case MinimapMarkerCategory.Player: return "▲";
                case MinimapMarkerCategory.Teammate: return "●";
                case MinimapMarkerCategory.BedHome: return "⌂";
                case MinimapMarkerCategory.Landmark: return "◆";
                default: return "•";
            }
        }

        static GameObject Panel(string name, Transform parent, Color color)
        { var obj = new GameObject(name, typeof(RectTransform), typeof(Image)); obj.transform.SetParent(parent, false); obj.GetComponent<Image>().color = color; return obj; }
        static Text Label(string name, Transform parent, string value, int size, TextAnchor anchor)
        { var obj = new GameObject(name, typeof(RectTransform), typeof(Text)); obj.transform.SetParent(parent, false); var text = obj.GetComponent<Text>(); text.font = Resources.Load<Font>("Fonts/NotoSans-Regular"); text.text = value; text.fontSize = size; text.color = Color.white; text.alignment = anchor; return text; }
        static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
    }

    static class TemporaryOriginMapArt
    {
        static Sprite sprite;
        public static Sprite Get()
        {
            if (sprite != null) return sprite;
            const int width = 256, height = 256; var texture = new Texture2D(width, height, TextureFormat.RGBA32, false) { name = "TEMP_OriginIslandMap", filterMode = FilterMode.Bilinear };
            var pixels = new Color32[width * height]; var water = new Color32(24, 139, 164, 255); var sand = new Color32(234, 199, 119, 255); var green = new Color32(69, 145, 73, 255);
            for (var y = 0; y < height; y++) for (var x = 0; x < width; x++)
            {
                var nx = (x - 128f) / 92f; var ny = (y - 128f) / 112f;
                var wobble = 0.1f * Mathf.Sin(ny * 8f) + 0.06f * Mathf.Sin(ny * 17f);
                var island = (nx + wobble) * (nx + wobble) + ny * ny;
                pixels[y * width + x] = island < 0.78f ? green : island < 0.95f ? sand : water;
            }
            texture.SetPixels32(pixels); texture.Apply(false, true); sprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f); sprite.name = "TEMP_REPLACEABLE_OriginIslandMap"; return sprite;
        }
    }
}
