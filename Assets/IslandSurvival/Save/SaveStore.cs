using System.IO;
using UnityEngine;

namespace IslandSurvival.Save
{
    public interface ISaveStore
    {
        bool Exists { get; }
        bool TryRead(out string json);
        bool Write(string json);
    }

    public sealed class FileSaveStore : ISaveStore
    {
        readonly string path;
        public FileSaveStore(string path) { this.path = path; }
        public bool Exists { get { return File.Exists(path); } }
        public bool TryRead(out string json)
        {
            json = string.Empty;
            if (!Exists) return false;
            try { json = File.ReadAllText(path); return true; }
            catch (IOException exception) { Debug.LogWarning("Save read failed: " + exception.Message); return false; }
        }
        public bool Write(string json)
        {
            try { File.WriteAllText(path, json); return true; }
            catch (IOException exception) { Debug.LogWarning("Save write failed: " + exception.Message); return false; }
        }
    }

    public sealed class PlayerPrefsSaveStore : ISaveStore
    {
        readonly string key;
        public PlayerPrefsSaveStore(string key) { this.key = key; }
        public bool Exists { get { return PlayerPrefs.HasKey(key); } }
        public bool TryRead(out string json)
        {
            if (!Exists) { json = string.Empty; return false; }
            json = PlayerPrefs.GetString(key, string.Empty); return !string.IsNullOrEmpty(json);
        }
        public bool Write(string json)
        {
            PlayerPrefs.SetString(key, json); PlayerPrefs.Save(); return true;
        }
    }

    public static class SaveStoreFactory
    {
        public static ISaveStore Create(string persistentDataPath)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // Unity's WebGL PlayerPrefs backend persists to IndexedDB. Explicit Save flushes prototype state.
            return new PlayerPrefsSaveStore("island_survival.prototype_0_1.save_v1");
#else
            return new FileSaveStore(Path.Combine(persistentDataPath, "prototype_0_1_save.json"));
#endif
        }
    }
}
