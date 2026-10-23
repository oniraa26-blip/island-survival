using System.Collections.Generic;
using UnityEngine;

namespace IslandSurvival.Pooling
{
    public sealed class GameObjectPool
    {
        readonly GameObject prefab;
        readonly Transform root;
        readonly Stack<GameObject> available = new Stack<GameObject>();
        readonly HashSet<GameObject> leased = new HashSet<GameObject>();
        readonly int hardLimit;

        public GameObjectPool(GameObject prefab, Transform root, int prewarm, int hardLimit)
        {
            this.prefab = prefab; this.root = root; this.hardLimit = Mathf.Max(1, hardLimit);
            for (var i = 0; i < Mathf.Min(prewarm, this.hardLimit); i++) available.Push(CreateInactive());
        }

        public GameObject Rent(Vector3 position, Quaternion rotation)
        {
            GameObject instance;
            if (available.Count > 0) instance = available.Pop();
            else if (available.Count + leased.Count < hardLimit) instance = CreateInactive();
            else return null;
            instance.transform.SetPositionAndRotation(position, rotation); instance.SetActive(true); leased.Add(instance); return instance;
        }

        public bool Return(GameObject instance)
        {
            if (instance == null || !leased.Remove(instance)) return false;
            instance.SetActive(false); instance.transform.SetParent(root, false); available.Push(instance); return true;
        }

        GameObject CreateInactive()
        {
            var instance = Object.Instantiate(prefab, root); instance.SetActive(false); return instance;
        }
    }
}
