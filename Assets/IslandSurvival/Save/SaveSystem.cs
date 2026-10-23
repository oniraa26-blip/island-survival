using System;
using System.Collections.Generic;
using System.IO;
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
        public int version = 1;
        public TransformData player = new TransformData();
        public bool hasRespawn;
        public TransformData respawn = new TransformData();
        public List<InventorySlot> inventory = new List<InventorySlot>();
        public string mainHand = string.Empty;
        public SurvivalSnapshot survival = new SurvivalSnapshot();
        public float timeOfDay;
        public List<PlacedData> placed = new List<PlacedData>();
    }

    public sealed class SaveSystem
    {
        readonly string path;
        Vector3 defaultSpawn;
        Vector3 respawn;
        bool hasRespawn;
        public SaveSystem(string path, Vector3 defaultSpawn) { this.path = path; this.defaultSpawn = defaultSpawn; }
        public void SetRespawn(Vector3 position) { respawn = position; hasRespawn = true; }
        public Vector3 RespawnPosition { get { return hasRespawn ? respawn : defaultSpawn; } }
        public void RespawnPlayer() { ServiceRegistry.Player.Teleport(RespawnPosition, Quaternion.identity); ServiceRegistry.Survival.Revive(); }
        public void SaveNow()
        {
            if (ServiceRegistry.Player == null) return;
            var data = new PrototypeSaveData();
            data.player = new TransformData(ServiceRegistry.Player.transform.position, ServiceRegistry.Player.transform.rotation);
            data.hasRespawn = hasRespawn; data.respawn = new TransformData(RespawnPosition, Quaternion.identity);
            foreach (var slot in ServiceRegistry.Inventory.Slots) data.inventory.Add(new InventorySlot(slot.itemId, slot.quantity));
            data.mainHand = ServiceRegistry.Equipment.MainHandItemId;
            data.survival = ServiceRegistry.Survival.Snapshot(); data.timeOfDay = ServiceRegistry.Time.normalizedTime;
            foreach (var placed in ServiceRegistry.Building.Placed)
                data.placed.Add(new PlacedData { id = placed.buildableId, persistentId = placed.persistentId, transform = new TransformData(placed.transform.position, placed.transform.rotation) });
            File.WriteAllText(path, JsonUtility.ToJson(data, true));
        }
        public bool Load()
        {
            if (!File.Exists(path)) return false;
            PrototypeSaveData data;
            try { data = JsonUtility.FromJson<PrototypeSaveData>(File.ReadAllText(path)); }
            catch (Exception e) { Debug.LogWarning("Save ignored: " + e.Message); return false; }
            if (data == null || data.version != 1) return false;
            ServiceRegistry.Inventory.Replace(data.inventory); ServiceRegistry.Equipment.Restore(data.mainHand);
            ServiceRegistry.Survival.RestoreSnapshot(data.survival); ServiceRegistry.Time.Restore(data.timeOfDay);
            hasRespawn = data.hasRespawn; if (hasRespawn) respawn = data.respawn.Position;
            ServiceRegistry.Player.Teleport(data.player.Position, data.player.Rotation);
            ServiceRegistry.Building.ClearPlaced();
            foreach (var placed in data.placed) ServiceRegistry.Building.Restore(placed.id, placed.persistentId, placed.transform.Position, placed.transform.Rotation);
            return true;
        }
    }
}
