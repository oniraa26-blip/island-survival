using System.Collections.Generic;
using System.Text;
using IslandSurvival.Building;
using IslandSurvival.Core;
using IslandSurvival.Crafting;
using IslandSurvival.InputSystem;
using IslandSurvival.Interaction;
using IslandSurvival.Items;
using IslandSurvival.Localization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IslandSurvival.UI
{
    public sealed class PrototypeHUD : MonoBehaviour
    {
        MobileInput input; PlayerInteractor interactor; CraftingSystem crafting;
        RectTransform safeArea; Text stats, inventoryText, toast, phase, plan; Button context;
        GameObject inventoryPanel, settingsPanel, buildControls; Image stickBase, stickKnob;
        readonly Dictionary<Text, string> localizedLabels = new Dictionary<Text, string>();
        float toastUntil;
        Font font;
        Rect lastSafeArea;
        Vector2Int lastScreenSize;
        public void Initialize(MobileInput input, PlayerInteractor interactor, CraftingSystem crafting)
        {
            this.input = input; this.interactor = interactor; this.crafting = crafting;
            font = Resources.Load<Font>("Fonts/NotoSans-Regular");
            if (font == null) { Debug.LogError("Noto Sans localization font is missing."); font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            Build(); ApplySafeArea();
            interactor.TargetChanged += TargetChanged; ServiceRegistry.Inventory.Changed += RefreshInventory;
            ServiceRegistry.Building.Status += ShowToast; ServiceRegistry.Localization.LanguageChanged += RefreshLocalizedText;
            ServiceRegistry.Discovery.Discovered += ShowDiscovery;
            RefreshLocalizedText();
        }
        void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            safeArea = Panel("SafeArea", transform, new Color(0, 0, 0, 0), Vector2.zero, Vector2.one).GetComponent<RectTransform>();
            stats = Label("Stats", safeArea, "", 28, TextAnchor.UpperLeft, new Vector2(18, -16), new Vector2(390, 185));
            plan = LocalizedLabel("Plan", safeArea, "ui.survival_plan", 27, TextAnchor.UpperCenter, new Vector2(-250, -16), new Vector2(500, 90)); Anchor(plan.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            phase = Label("Phase", safeArea, "", 25, TextAnchor.UpperRight, new Vector2(-620, -18), new Vector2(280, 55)); Anchor(phase.rectTransform, Vector2.one, Vector2.one);
            var settings = Button("Settings", safeArea, "ui.settings", new Vector2(-330, -390), new Vector2(140, 62), ToggleSettings); Anchor(settings.GetComponent<RectTransform>(), Vector2.one, Vector2.one);
            var inventoryButton = Button("Inventory", safeArea, "ui.bag", new Vector2(-170, -390), new Vector2(140, 62), ToggleInventory); Anchor(inventoryButton.GetComponent<RectTransform>(), Vector2.one, Vector2.one);
            context = Button("Context", safeArea, "ui.interact", new Vector2(-245, 95), new Vector2(220, 105), interactor.TriggerCurrent); Anchor(context.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f)); localizedLabels.Remove(context.GetComponentInChildren<Text>()); context.gameObject.SetActive(false);
            var sprint = Button("Sprint", safeArea, "ui.run", new Vector2(-300, 225), new Vector2(130, 78), null); Anchor(sprint.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f)); AddHold(sprint, input.SetSprintButton);
            var jump = Button("Jump", safeArea, "ui.jump", new Vector2(-145, 225), new Vector2(130, 78), input.RequestJump); Anchor(jump.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f));
            toast = Label("Toast", safeArea, "", 28, TextAnchor.MiddleCenter, new Vector2(-330, -115), new Vector2(660, 72)); Anchor(toast.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            inventoryPanel = Panel("InventoryPanel", safeArea, new Color(0.035f, 0.08f, 0.09f, 0.94f), new Vector2(0.61f, 0.1f), new Vector2(0.98f, 0.9f)); inventoryPanel.SetActive(false);
            inventoryText = Label("Slots", inventoryPanel.transform, "", 25, TextAnchor.UpperLeft, new Vector2(24, -68), new Vector2(-48, -210)); Stretch(inventoryText.rectTransform);
            LocalizedLabel("Title", inventoryPanel.transform, "ui.inventory_title", 31, TextAnchor.UpperLeft, new Vector2(24, -18), new Vector2(-48, 50));
            var eat = Button("Eat", inventoryPanel.transform, "ui.eat_coconut", new Vector2(25, 125), new Vector2(185, 62), ConsumeCoconut); Anchor(eat.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            var knife = Button("Knife", inventoryPanel.transform, "ui.craft_knife", new Vector2(220, 125), new Vector2(175, 62), () => Craft("stone_knife")); Anchor(knife.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            var axe = Button("Axe", inventoryPanel.transform, "ui.craft_axe", new Vector2(405, 125), new Vector2(175, 62), () => Craft("stone_axe")); Anchor(axe.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            var equip = Button("Equip", inventoryPanel.transform, "ui.equip_axe", new Vector2(25, 50), new Vector2(185, 62), () => { ShowToast(ServiceRegistry.Localization.Get(ServiceRegistry.Equipment.Equip("stone_axe") ? "toast.axe_equipped" : "toast.craft_axe_first")); RefreshInventory(); }); Anchor(equip.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            var campfire = Button("Campfire", inventoryPanel.transform, "ui.place_campfire", new Vector2(220, 50), new Vector2(175, 62), () => StartBuild("campfire")); Anchor(campfire.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            var bed = Button("Bed", inventoryPanel.transform, "ui.place_leaf_bed", new Vector2(405, 50), new Vector2(175, 62), () => StartBuild("leaf_bed")); Anchor(bed.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            settingsPanel = Panel("SettingsPanel", safeArea, new Color(0.035f, 0.08f, 0.09f, 0.97f), new Vector2(0.32f, 0.22f), new Vector2(0.68f, 0.78f)); settingsPanel.SetActive(false);
            LocalizedLabel("SettingsTitle", settingsPanel.transform, "ui.settings", 36, TextAnchor.UpperCenter, new Vector2(45, -25), new Vector2(480, 60));
            LocalizedLabel("LanguageTitle", settingsPanel.transform, "ui.language", 28, TextAnchor.UpperCenter, new Vector2(45, -105), new Vector2(480, 50));
            var vietnamese = Button("Vietnamese", settingsPanel.transform, "language.vi", new Vector2(45, 140), new Vector2(230, 68), () => SetLanguage(LocalizationService.DefaultLocale)); Anchor(vietnamese.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            var english = Button("English", settingsPanel.transform, "language.en", new Vector2(295, 140), new Vector2(230, 68), () => SetLanguage(LocalizationService.EnglishLocale)); Anchor(english.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            var closeSettings = Button("CloseSettings", settingsPanel.transform, "ui.close", new Vector2(190, 50), new Vector2(190, 62), ToggleSettings); Anchor(closeSettings.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            buildControls = Panel("BuildControls", safeArea, new Color(0, 0, 0, 0), new Vector2(0.55f, 0f), new Vector2(0.98f, 0.2f)); buildControls.SetActive(false);
            var confirm = Button("Confirm", buildControls.transform, "ui.confirm", new Vector2(-350, 20), new Vector2(180, 68), ServiceRegistry.Building.Confirm); Anchor(confirm.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(1, 0));
            var rotate = Button("Rotate", buildControls.transform, "ui.rotate", new Vector2(-160, 20), new Vector2(145, 68), ServiceRegistry.Building.Rotate); Anchor(rotate.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(1, 0));
            var cancel = Button("Cancel", buildControls.transform, "ui.cancel", new Vector2(-5, 20), new Vector2(130, 68), CancelBuild); Anchor(cancel.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(1, 0));
            stickBase = Image("JoystickBase", safeArea, new Color(1, 1, 1, 0.16f), new Vector2(150, 150)); stickKnob = Image("JoystickKnob", safeArea, new Color(1, 1, 1, 0.4f), new Vector2(68, 68)); stickBase.gameObject.SetActive(false); stickKnob.gameObject.SetActive(false);
            var minimapObject = new GameObject("MinimapController", typeof(RectTransform)); minimapObject.transform.SetParent(safeArea, false);
            var minimap = minimapObject.AddComponent<MinimapController>(); minimap.Initialize(safeArea, ServiceRegistry.Player.transform); ServiceRegistry.MinimapMarkers = minimap.Registry;
        }
        void Update()
        {
            RefreshStats();
            phase.text = ServiceRegistry.Localization.Get(ServiceRegistry.Time.PhaseKey);
            if (toastUntil < UnityEngine.Time.unscaledTime) toast.text = "";
            var visible = input.JoystickVisible; stickBase.gameObject.SetActive(visible); stickKnob.gameObject.SetActive(visible);
            if (visible) { stickBase.rectTransform.position = input.JoystickOrigin; stickKnob.rectTransform.position = input.JoystickPosition; }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Tab)) ToggleInventory();
            if (Screen.safeArea != lastSafeArea || lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height) ApplySafeArea();
        }
        void TargetChanged(IInteractable target) { context.gameObject.SetActive(target != null && target.CanInteract); if (target != null) context.GetComponentInChildren<Text>().text = target.ActionLabel; }
        void ToggleInventory() { var show = !inventoryPanel.activeSelf; inventoryPanel.SetActive(show); if (show) settingsPanel.SetActive(false); RefreshInventory(); }
        void ToggleSettings() { var show = !settingsPanel.activeSelf; settingsPanel.SetActive(show); if (show) inventoryPanel.SetActive(false); }
        void SetLanguage(string locale) { ServiceRegistry.Localization.SetLocale(locale); }
        void RefreshLocalizedText()
        {
            foreach (var pair in localizedLabels)
                pair.Key.text = pair.Value == "ui.inventory_title" ? ServiceRegistry.Localization.Format(pair.Value, ServiceRegistry.Inventory.Capacity) : ServiceRegistry.Localization.Get(pair.Value);
            phase.text = ServiceRegistry.Localization.Get(ServiceRegistry.Time.PhaseKey);
            RefreshStats(); RefreshInventory(); TargetChanged(interactor.Current);
        }
        void RefreshStats()
        {
            var hand = string.IsNullOrEmpty(ServiceRegistry.Equipment.MainHandItemId) ? ServiceRegistry.Localization.Get("ui.empty") : ServiceRegistry.Localization.Get(ServiceRegistry.Items.Get(ServiceRegistry.Equipment.MainHandItemId).displayNameKey);
            stats.text = ServiceRegistry.Localization.Format("ui.stats", ServiceRegistry.Survival.Health, ServiceRegistry.Survival.Hunger, ServiceRegistry.Survival.Thirst, ServiceRegistry.Survival.Stamina, hand);
        }
        void RefreshInventory()
        {
            if (inventoryText == null) return; var b = new StringBuilder(); var index = 1;
            foreach (var slot in ServiceRegistry.Inventory.Slots) { b.Append(index++.ToString("00")).Append("  "); b.Append(slot.IsEmpty ? "—" : ServiceRegistry.Localization.Get(ServiceRegistry.Items.Get(slot.itemId).displayNameKey) + "  x" + slot.quantity); b.AppendLine(); }
            inventoryText.text = b.ToString();
        }
        void ConsumeCoconut() { if (!ServiceRegistry.Inventory.Remove("coconut", 1)) { ShowToast(ServiceRegistry.Localization.Get("toast.no_coconut")); return; } var item = ServiceRegistry.Items.Get("coconut"); ServiceRegistry.Survival.Restore(item.hungerRestore, item.thirstRestore); ShowToast(ServiceRegistry.Localization.Get("toast.coconut_restored")); }
        void Craft(string id) { var recipe = crafting.Recipes.Find(r => r.id == id); string message; crafting.TryCraft(recipe, out message); ShowToast(message); }
        void StartBuild(string id) { inventoryPanel.SetActive(false); ServiceRegistry.Building.Begin(id); buildControls.SetActive(ServiceRegistry.Building.IsPlacing); }
        void CancelBuild() { ServiceRegistry.Building.Cancel(); buildControls.SetActive(false); }
        void ShowToast(string message) { toast.text = message; toastUntil = UnityEngine.Time.unscaledTime + 3f; if (!ServiceRegistry.Building.IsPlacing && buildControls != null) buildControls.SetActive(false); }
        void ShowDiscovery(string id) { ShowToast(ServiceRegistry.Localization.Format("discovery.found", ServiceRegistry.Localization.Get("location." + id))); }
        void OnDestroy() { if (ServiceRegistry.Localization != null) ServiceRegistry.Localization.LanguageChanged -= RefreshLocalizedText; if (ServiceRegistry.Discovery != null) ServiceRegistry.Discovery.Discovered -= ShowDiscovery; }
        void ApplySafeArea() { var area = Screen.safeArea; safeArea.anchorMin = area.position / new Vector2(Screen.width, Screen.height); safeArea.anchorMax = (area.position + area.size) / new Vector2(Screen.width, Screen.height); safeArea.offsetMin = safeArea.offsetMax = Vector2.zero; lastSafeArea = area; lastScreenSize = new Vector2Int(Screen.width, Screen.height); }
        GameObject Panel(string name, Transform parent, Color color, Vector2 min, Vector2 max) { var obj = new GameObject(name, typeof(RectTransform), typeof(Image)); obj.transform.SetParent(parent, false); var rt = obj.GetComponent<RectTransform>(); rt.anchorMin = min; rt.anchorMax = max; rt.offsetMin = rt.offsetMax = Vector2.zero; obj.GetComponent<Image>().color = color; return obj; }
        Text Label(string name, Transform parent, string value, int size, TextAnchor alignment, Vector2 position, Vector2 dimensions) { var obj = new GameObject(name, typeof(RectTransform), typeof(Text)); obj.transform.SetParent(parent, false); var text = obj.GetComponent<Text>(); text.font = font; text.fontSize = size; text.color = Color.white; text.alignment = alignment; text.text = value; var rt = text.rectTransform; rt.anchorMin = rt.anchorMax = new Vector2(0, 1); rt.pivot = new Vector2(0, 1); rt.anchoredPosition = position; rt.sizeDelta = dimensions; return text; }
        Text LocalizedLabel(string name, Transform parent, string key, int size, TextAnchor alignment, Vector2 position, Vector2 dimensions) { var label = Label(name, parent, ServiceRegistry.Localization.Get(key), size, alignment, position, dimensions); localizedLabels.Add(label, key); return label; }
        Button Button(string name, Transform parent, string key, Vector2 position, Vector2 dimensions, UnityEngine.Events.UnityAction callback) { var obj = Panel(name, parent, new Color(0.08f, 0.32f, 0.34f, 0.9f), Vector2.zero, Vector2.zero); var rt = obj.GetComponent<RectTransform>(); rt.pivot = Vector2.zero; rt.anchoredPosition = position; rt.sizeDelta = dimensions; var button = obj.AddComponent<Button>(); if (callback != null) button.onClick.AddListener(callback); var label = LocalizedLabel("Label", obj.transform, key, 24, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero); Stretch(label.rectTransform); return button; }
        Image Image(string name, Transform parent, Color color, Vector2 size) { var obj = Panel(name, parent, color, Vector2.zero, Vector2.zero); var image = obj.GetComponent<Image>(); image.rectTransform.sizeDelta = size; image.rectTransform.pivot = new Vector2(0.5f, 0.5f); return image; }
        static void Anchor(RectTransform rt, Vector2 min, Vector2 max) { rt.anchorMin = min; rt.anchorMax = max; }
        static void Stretch(RectTransform rt) { rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero; rt.pivot = new Vector2(0.5f, 0.5f); }
        static void AddHold(Button button, System.Action<bool> action) { var trigger = button.gameObject.AddComponent<EventTrigger>(); foreach (var pair in new[] { new { type = EventTriggerType.PointerDown, value = true }, new { type = EventTriggerType.PointerUp, value = false }, new { type = EventTriggerType.PointerExit, value = false } }) { var entry = new EventTrigger.Entry { eventID = pair.type }; entry.callback.AddListener(_ => action(pair.value)); trigger.triggers.Add(entry); } }
    }
}
