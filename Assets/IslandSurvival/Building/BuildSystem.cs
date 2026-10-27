using System;
using System.Collections.Generic;
using IslandSurvival.Core;
using IslandSurvival.Inventory;
using IslandSurvival.World;
using UnityEngine;

namespace IslandSurvival.Building
{
    [Serializable]
    public sealed class BuildRecipe
    {
        public string id, displayNameKey;
        public Dictionary<string, int> costs;
        public BuildRecipe(string id, string nameKey, Dictionary<string, int> costs) { this.id = id; displayNameKey = nameKey; this.costs = costs; }
    }

    public sealed class BuildSystem : MonoBehaviour
    {
        InventoryModel inventory;
        UnityEngine.Camera viewCamera;
        GameObject ghost;
        BuildRecipe active;
        bool valid;
        float rotation;
        public event Action<string> Status;
        public bool IsPlacing { get { return active != null; } }
        public readonly List<BuildRecipe> Recipes = new List<BuildRecipe>();
        public readonly List<PlacedBuildable> Placed = new List<PlacedBuildable>();

        public void Initialize(InventoryModel inventory, UnityEngine.Camera viewCamera)
        {
            this.inventory = inventory; this.viewCamera = viewCamera;
            Recipes.Add(new BuildRecipe("campfire", "building.campfire.name", new Dictionary<string, int> { { "stone", 2 }, { "wood", 2 } }));
            Recipes.Add(new BuildRecipe("leaf_bed", "building.leaf_bed.name", new Dictionary<string, int> { { "wood", 3 }, { "fiber", 3 } }));
        }

        public void Begin(string id)
        {
            Cancel();
            active = Recipes.Find(r => r.id == id);
            if (active == null) return;
            foreach (var cost in active.costs) if (inventory.Count(cost.Key) < cost.Value) { Report(ServiceRegistry.Localization.Format("build.missing_materials", ServiceRegistry.Localization.Get(active.displayNameKey))); active = null; return; }
            ghost = WorldFactory.CreateBuildableVisual(active.id, true);
            Report(ServiceRegistry.Localization.Get("build.move_rotate_confirm"));
        }
        void Update()
        {
            if (ghost == null) return;
            var ray = viewCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.42f));
            RaycastHit hit;
            valid = Physics.Raycast(ray, out hit, 30f, ~0, QueryTriggerInteraction.Ignore) && Vector3.Angle(hit.normal, Vector3.up) < 22f;
            if (valid && LandmarkProtectionVolume.Contains(hit.point)) valid = false;
            if (valid)
            {
                ghost.transform.SetPositionAndRotation(hit.point, Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.Euler(0f, rotation, 0f));
                var overlaps = Physics.OverlapBox(hit.point + Vector3.up * 0.35f, active.id == "leaf_bed" ? new Vector3(0.8f, 0.3f, 1.25f) : new Vector3(0.75f, 0.3f, 0.75f), ghost.transform.rotation, ~0, QueryTriggerInteraction.Ignore);
                foreach (var overlap in overlaps) if (!overlap.transform.IsChildOf(ghost.transform) && !overlap.CompareTag("Player") && overlap.gameObject.name != "Beach") valid = false;
            }
            ghost.transform.localScale = valid ? Vector3.one : Vector3.one * 0.86f;
            WorldFactory.Tint(ghost, valid ? new Color(0.25f, 0.85f, 0.55f, 0.65f) : new Color(1f, 0.35f, 0.25f, 0.65f));
        }
        public void Rotate() { rotation += 45f; }
        public void Confirm()
        {
            if (ghost == null || !valid) { Report(ServiceRegistry.Localization.Get("build.invalid_placement")); return; }
            foreach (var cost in active.costs) if (inventory.Count(cost.Key) < cost.Value) { Report(ServiceRegistry.Localization.Get("build.materials_changed")); Cancel(); return; }
            foreach (var cost in active.costs) inventory.Remove(cost.Key, cost.Value);
            var placedObject = WorldFactory.CreateBuildableVisual(active.id, false);
            placedObject.transform.SetPositionAndRotation(ghost.transform.position, ghost.transform.rotation);
            var placed = placedObject.AddComponent<PlacedBuildable>();
            placed.buildableId = active.id; placed.persistentId = Guid.NewGuid().ToString("N"); Placed.Add(placed);
            if (placed.buildableId == "leaf_bed" && ServiceRegistry.MinimapMarkers != null) ServiceRegistry.MinimapMarkers.RegisterHomeBed(placed.persistentId, placed.transform);
            var placedName = ServiceRegistry.Localization.Get(active.displayNameKey);
            Cancel(); Report(ServiceRegistry.Localization.Format("build.placed", placedName)); ServiceRegistry.Save.SaveNow();
        }
        public PlacedBuildable Restore(string id, string persistentId, Vector3 position, Quaternion rotationValue)
        {
            var obj = WorldFactory.CreateBuildableVisual(id, false); obj.transform.SetPositionAndRotation(position, rotationValue);
            var placed = obj.AddComponent<PlacedBuildable>(); placed.buildableId = id; placed.persistentId = persistentId; Placed.Add(placed);
            if (id == "leaf_bed" && ServiceRegistry.MinimapMarkers != null) ServiceRegistry.MinimapMarkers.RegisterHomeBed(persistentId, placed.transform);
            return placed;
        }
        public void ClearPlaced() { foreach (var placed in Placed) if (placed != null) { if (placed.buildableId == "leaf_bed" && ServiceRegistry.MinimapMarkers != null) ServiceRegistry.MinimapMarkers.Unregister("home_" + placed.persistentId); Destroy(placed.gameObject); } Placed.Clear(); }
        public void Cancel() { if (ghost != null) Destroy(ghost); ghost = null; active = null; valid = false; }
        void Report(string message) { if (Status != null) Status(message); }
    }
}
