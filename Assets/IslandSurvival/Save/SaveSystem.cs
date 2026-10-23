using System;
using System.Collections.Generic;
using IslandSurvival.Building;
using IslandSurvival.Core;
using IslandSurvival.Inventory;
using IslandSurvival.Survival;
using UnityEngine;

namespace IslandSurvival.Save
{
    [Serializable] public sealed class TransformData
    {
        public float px, py, pz, rx, ry, rz, rw;
        public TransformData() { rw = 1f; }
        public TransformData(Vector3 p, Quaternion r) { px = p.x; py = p.y; pz = p.z; rx = r.x; ry = r.y; rz = r.z; rw = r.w; }
        public Vector3 Position { get { return new Vector3(px, py, pz); } }
        public Quaternion Rotation { get { return new Quaternion(rx, ry, rz, rw); } }
    }
    [Serializable] public sealed class PlacedData { public string id, persistentId; public TransformData transform; }
    [Serializable] public sealed class PrototypeSaveData
    {
        public int version = 2;
        public TransformData player = new TransformData();
        public bool hasRespawn;
        public TransformData respawn = new TransformData();
        public List<InventorySlot> inventory = new List<InventorySlot>();
        public string mainHand = string.Empty;
        public SurvivalSnapshot survival = new SurvivalSnapshot();
        public float timeOfDay;
        public List<PlacedData> placed = new List<PlacedData>();
        public List<string> discoveries = new List<string>();
        public List<string> collectibles = new List<string>();
    }

    public sealed class SaveSystem
    {
        readonly ISaveStore store;
        Vector3 defaultSpawn;
        Vector3 respawn;
        bool hasRespawn;
        public SaveSystem(ISaveStore store, Vector3 defaultSpawn) { this.store = store; this.defaultSpawn = defaultSpawn; }
        public static bool IsSupportedVersion(int version) { return version == 1 || version == 2; }
        public void SetRespawn(Vector3 position) { respawn = position; hasRespawn = true; }
        public Vector3 RespawnPosition { get { return hasRespawn ? respawn : defaultSpawn; } }
        public void RespawnPlayer() { ServiceRegistry.Player.Teleport(RespawnPosition, Quaternion.identity); ServiceRegistry.Survival.Revive(); }
        public void SaveNow()
        {
            if (ServiceRegistry.Player == null) return;
            var data = new PrototypeSaveData { version = 2 };
            data.player = new TransformData(ServiceRegistry.Player.transform.position, ServiceRegistry.Player.transform.rotation);
            data.hasRespawn = hasRespawn; data.respawn = new TransformData(RespawnPosition, Quaternion.identity);
            foreach (var slot in ServiceRegistry.Inventory.Slots) data.inventory.Add(new InventorySlot(slot.itemId, slot.quantity));
            data.mainHand = ServiceRegistry.Equipment.MainHandItemId;
            data.survival = ServiceRegistry.Survival.Snapshot(); data.timeOfDay = ServiceRegistry.Time.normalizedTime;
            foreach (var placed in ServiceRegistry.Building.Placed)
                data.placed.Add(new PlacedData { id = placed.buildableId, persistentId = placed.persistentId, transform = new TransformData(placed.transform.position, placed.transform.rotation) });
            foreach (var discovery in ServiceRegistry.Discovery.DiscoveredIds) data.discoveries.Add(discovery);
            foreach (var collectible in ServiceRegistry.Discovery.CollectedIds) data.collectibles.Add(collectible);
            store.Write(JsonUtility.ToJson(data, true));
        }
        public bool Load()
        {
            string json;
            if (!store.TryRead(out json)) return false;
            PrototypeSaveData data;
            try { data = JsonUtility.FromJson<PrototypeSaveData>(json); }
            catch (Exception e) { Debug.LogWarning("Save ignored: " + e.Message); return false; }
            if (data == null || !IsSupportedVersion(data.version)) return false;
            ServiceRegistry.Inventory.Replace(data.inventory ?? new List<InventorySlot>()); ServiceRegistry.Equipment.Restore(data.mainHand ?? string.Empty);
            ServiceRegistry.Survival.RestoreSnapshot(data.survival); ServiceRegistry.Time.Restore(data.timeOfDay);
            hasRespawn = data.hasRespawn; if (hasRespawn) respawn = data.respawn.Position;
            ServiceRegistry.Player.Teleport(data.player.Position, data.player.Rotation);
            ServiceRegistry.Building.ClearPlaced();
            if (data.placed != null)
                foreach (var placed in data.placed)
                    if (placed != null && placed.transform != null) ServiceRegistry.Building.Restore(placed.id, placed.persistentId, placed.transform.Position, placed.transform.Rotation);
            ServiceRegistry.Discovery.Restore(data.discoveries, data.collectibles);
            return true;
        }
    }
}
