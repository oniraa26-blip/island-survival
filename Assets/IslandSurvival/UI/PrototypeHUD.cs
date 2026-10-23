using System.Text;
using IslandSurvival.Building;
using IslandSurvival.Core;
using IslandSurvival.Crafting;
using IslandSurvival.InputSystem;
using IslandSurvival.Interaction;
using IslandSurvival.Items;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IslandSurvival.UI
{
    public sealed class PrototypeHUD : MonoBehaviour
    {
        MobileInput input; PlayerInteractor interactor; CraftingSystem crafting;
        RectTransform safeArea; Text stats, inventoryText, toast, phase, plan; Button context;
        GameObject inventoryPanel, buildControls; Image stickBase, stickKnob;
        float toastUntil;
        Font font;
        Rect lastSafeArea;
        Vector2Int lastScreenSize;
        public void Initialize(MobileInput input, PlayerInteractor interactor, CraftingSystem crafting)
        {
            this.input = input; this.interactor = interactor; this.crafting = crafting;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); Build(); ApplySafeArea();
            interactor.TargetChanged += TargetChanged; ServiceRegistry.Inventory.Changed += RefreshInventory;
            ServiceRegistry.Building.Status += ShowToast; RefreshInventory();
        }
        void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            safeArea = Panel("SafeArea", transform, new Color(0, 0, 0, 0), Vector2.zero, Vector2.one).GetComponent<RectTransform>();
            stats = Label("Stats", safeArea, "", 28, TextAnchor.UpperLeft, new Vector2(18, -16), new Vector2(390, 185));
            plan = Label("Plan", safeArea, "SURVIVAL PLAN\nWater  •  Food  •  Fire", 27, TextAnchor.UpperCenter, new Vector2(-250, -16), new Vector2(500, 90)); Anchor(plan.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            phase = Label("Phase", safeArea, "Morning", 25, TextAnchor.UpperRight, new Vector2(-300, -18), new Vector2(280, 55)); Anchor(phase.rectTransform, Vector2.one, Vector2.one);
            var inventoryButton = Button("Inventory", safeArea, "BAG", new Vector2(-170, -75), new Vector2(140, 62), ToggleInventory); Anchor(inventoryButton.GetComponent<RectTransform>(), Vector2.one, Vector2.one);
            context = Button("Context", safeArea, "Interact", new Vector2(-245, 95), new Vector2(220, 105), interactor.TriggerCurrent); Anchor(context.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f)); context.gameObject.SetActive(false);
            var sprint = Button("Sprint", safeArea, "RUN", new Vector2(-300, 225), new Vector2(130, 78), null); Anchor(sprint.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f)); AddHold(sprint, input.SetSprintButton);
            var jump = Button("Jump", safeArea, "JUMP", new Vector2(-145, 225), new Vector2(130, 78), input.RequestJump); Anchor(jump.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f));
            toast = Label("Toast", safeArea, "", 28, TextAnchor.MiddleCenter, new Vector2(-330, 120), new Vector2(660, 72)); Anchor(toast.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));
            inventoryPanel = Panel("InventoryPanel", safeArea, new Color(0.035f, 0.08f, 0.09f, 0.94f), new Vector2(0.61f, 0.1f), new Vector2(0.98f, 0.9f)); inventoryPanel.SetActive(false);
            inventoryText = Label("Slots", inventoryPanel.transform, "", 25, TextAnchor.UpperLeft, new Vector2(24, -68), new Vector2(-48, -210)); Stretch(inventoryText.rectTransform);
            Label("Title", inventoryPanel.transform, "INVENTORY  •  16 SLOTS", 31, TextAnchor.UpperLeft, new Vector2(24, -18), new Vector2(-48, 50));
            var eat = Button("Eat", inventoryPanel.transform, "Eat Coconut", new Vector2(25, 125), new Vector2(185, 62), ConsumeCoconut); Anchor(eat.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            var knife = Button("Knife", inventoryPanel.transform, "Craft Knife", new Vector2(220, 125), new Vector2(175, 62), () => Craft("stone_knife")); Anchor(knife.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            var axe = Button("Axe", inventoryPanel.transform, "Craft Axe", new Vector2(405, 125), new Vector2(175, 62), () => Craft("stone_axe")); Anchor(axe.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            var equip = Button("Equip", inventoryPanel.transform, "Equip Axe", new Vector2(25, 50), new Vector2(185, 62), () => { ShowToast(ServiceRegistry.Equipment.Equip("stone_axe") ? "Stone Axe equipped" : "Craft an axe first"); RefreshInventory(); }); Anchor(equip.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            var campfire = Button("Campfire", inventoryPanel.transform, "Place Fire", new Vector2(220, 50), new Vector2(175, 62), () => StartBuild("campfire")); Anchor(campfire.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            var bed = Button("Bed", inventoryPanel.transform, "Place Bed", new Vector2(405, 50), new Vector2(175, 62), () => StartBuild("leaf_bed")); Anchor(bed.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            buildControls = Panel("BuildControls", safeArea, new Color(0, 0, 0, 0), new Vector2(0.55f, 0f), new Vector2(0.98f, 0.2f)); buildControls.SetActive(false);
            var confirm = Button("Confirm", buildControls.transform, "CONFIRM", new Vector2(-350, 20), new Vector2(180, 68), ServiceRegistry.Building.Confirm); Anchor(confirm.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(1, 0));
            var rotate = Button("Rotate", buildControls.transform, "ROTATE", new Vector2(-160, 20), new Vector2(145, 68), ServiceRegistry.Building.Rotate); Anchor(rotate.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(1, 0));
            var cancel = Button("Cancel", buildControls.transform, "CANCEL", new Vector2(-5, 20), new Vector2(130, 68), CancelBuild); Anchor(cancel.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(1, 0));
            stickBase = Image("JoystickBase", safeArea, new Color(1, 1, 1, 0.16f), new Vector2(150, 150)); stickKnob = Image("JoystickKnob", safeArea, new Color(1, 1, 1, 0.4f), new Vector2(68, 68)); stickBase.gameObject.SetActive(false); stickKnob.gameObject.SetActive(false);
        }
        void Update()
        {
            stats.text = string.Format("HP {0:0}   HUNGER {1:0}\nTHIRST {2:0}   STAMINA {3:0}\nHAND: {4}", ServiceRegistry.Survival.Health, ServiceRegistry.Survival.Hunger, ServiceRegistry.Survival.Thirst, ServiceRegistry.Survival.Stamina, string.IsNullOrEmpty(ServiceRegistry.Equipment.MainHandItemId) ? "empty" : "stone axe");
            phase.text = ServiceRegistry.Time.Phase;
            if (toastUntil < UnityEngine.Time.unscaledTime) toast.text = "";
            var visible = input.JoystickVisible; stickBase.gameObject.SetActive(visible); stickKnob.gameObject.SetActive(visible);
            if (visible) { stickBase.rectTransform.position = input.JoystickOrigin; stickKnob.rectTransform.position = input.JoystickPosition; }
            if (UnityEngine.Input.GetKeyDown(KeyCode.Tab)) ToggleInventory();
            if (Screen.safeArea != lastSafeArea || lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height) ApplySafeArea();
        }
        void TargetChanged(IInteractable target) { context.gameObject.SetActive(target != null && target.CanInteract); if (target != null) context.GetComponentInChildren<Text>().text = target.ActionLabel; }
        void ToggleInventory() { inventoryPanel.SetActive(!inventoryPanel.activeSelf); RefreshInventory(); }
        void RefreshInventory()
        {
            if (inventoryText == null) return; var b = new StringBuilder(); var index = 1;
            foreach (var slot in ServiceRegistry.Inventory.Slots) { b.Append(index++.ToString("00")).Append("  "); b.Append(slot.IsEmpty ? "—" : ServiceRegistry.Items.Get(slot.itemId).displayName + "  x" + slot.quantity); b.AppendLine(); }
            inventoryText.text = b.ToString();
        }
        void ConsumeCoconut() { if (!ServiceRegistry.Inventory.Remove("coconut", 1)) { ShowToast("No coconut"); return; } var item = ServiceRegistry.Items.Get("coconut"); ServiceRegistry.Survival.Restore(item.hungerRestore, item.thirstRestore); ShowToast("Coconut restored hunger and thirst"); }
        void Craft(string id) { var recipe = crafting.Recipes.Find(r => r.id == id); string message; crafting.TryCraft(recipe, out message); ShowToast(message); }
        void StartBuild(string id) { inventoryPanel.SetActive(false); ServiceRegistry.Building.Begin(id); buildControls.SetActive(ServiceRegistry.Building.IsPlacing); }
        void CancelBuild() { ServiceRegistry.Building.Cancel(); buildControls.SetActive(false); }
        void ShowToast(string message) { toast.text = message; toastUntil = UnityEngine.Time.unscaledTime + 3f; if (!ServiceRegistry.Building.IsPlacing && buildControls != null) buildControls.SetActive(false); }
        void ApplySafeArea() { var area = Screen.safeArea; safeArea.anchorMin = area.position / new Vector2(Screen.width, Screen.height); safeArea.anchorMax = (area.position + area.size) / new Vector2(Screen.width, Screen.height); safeArea.offsetMin = safeArea.offsetMax = Vector2.zero; lastSafeArea = area; lastScreenSize = new Vector2Int(Screen.width, Screen.height); }
        GameObject Panel(string name, Transform parent, Color color, Vector2 min, Vector2 max) { var obj = new GameObject(name, typeof(RectTransform), typeof(Image)); obj.transform.SetParent(parent, false); var rt = obj.GetComponent<RectTransform>(); rt.anchorMin = min; rt.anchorMax = max; rt.offsetMin = rt.offsetMax = Vector2.zero; obj.GetComponent<Image>().color = color; return obj; }
        Text Label(string name, Transform parent, string value, int size, TextAnchor alignment, Vector2 position, Vector2 dimensions) { var obj = new GameObject(name, typeof(RectTransform), typeof(Text)); obj.transform.SetParent(parent, false); var text = obj.GetComponent<Text>(); text.font = font; text.fontSize = size; text.color = Color.white; text.alignment = alignment; text.text = value; var rt = text.rectTransform; rt.anchorMin = rt.anchorMax = new Vector2(0, 1); rt.pivot = new Vector2(0, 1); rt.anchoredPosition = position; rt.sizeDelta = dimensions; return text; }
        Button Button(string name, Transform parent, string value, Vector2 position, Vector2 dimensions, UnityEngine.Events.UnityAction callback) { var obj = Panel(name, parent, new Color(0.08f, 0.32f, 0.34f, 0.9f), Vector2.zero, Vector2.zero); var rt = obj.GetComponent<RectTransform>(); rt.pivot = Vector2.zero; rt.anchoredPosition = position; rt.sizeDelta = dimensions; var button = obj.AddComponent<Button>(); if (callback != null) button.onClick.AddListener(callback); var label = Label("Label", obj.transform, value, 24, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero); Stretch(label.rectTransform); return button; }
        Image Image(string name, Transform parent, Color color, Vector2 size) { var obj = Panel(name, parent, color, Vector2.zero, Vector2.zero); var image = obj.GetComponent<Image>(); image.rectTransform.sizeDelta = size; image.rectTransform.pivot = new Vector2(0.5f, 0.5f); return image; }
        static void Anchor(RectTransform rt, Vector2 min, Vector2 max) { rt.anchorMin = min; rt.anchorMax = max; }
        static void Stretch(RectTransform rt) { rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero; rt.pivot = new Vector2(0.5f, 0.5f); }
        static void AddHold(Button button, System.Action<bool> action) { var trigger = button.gameObject.AddComponent<EventTrigger>(); foreach (var pair in new[] { new { type = EventTriggerType.PointerDown, value = true }, new { type = EventTriggerType.PointerUp, value = false }, new { type = EventTriggerType.PointerExit, value = false } }) { var entry = new EventTrigger.Entry { eventID = pair.type }; entry.callback.AddListener(_ => action(pair.value)); trigger.triggers.Add(entry); } }
    }
}
