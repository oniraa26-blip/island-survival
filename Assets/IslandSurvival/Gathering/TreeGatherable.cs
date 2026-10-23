using System;
using IslandSurvival.Core;
using IslandSurvival.Interaction;
using UnityEngine;

namespace IslandSurvival.Gathering
{
    public sealed class TreeGatherable : MonoBehaviour, IInteractable
    {
        int hitsRemaining;
        public string persistentId;
        public string ActionLabel { get { return CanInteract ? "Chop palm" : "Stone Axe required"; } }
        public bool CanInteract { get { return hitsRemaining > 0 && ServiceRegistry.Equipment.MainHandItemId == "stone_axe"; } }
        void Awake() { hitsRemaining = ServiceRegistry.Tuning == null ? 3 : ServiceRegistry.Tuning.treeHitCount; }
        public void Interact()
        {
            if (!CanInteract) return;
            hitsRemaining--;
            transform.localScale *= 0.97f;
            if (hitsRemaining <= 0)
            {
                ServiceRegistry.Inventory.Add("wood", 4);
                transform.rotation *= Quaternion.Euler(0f, 0f, 78f);
                enabled = false;
            }
        }
    }
}
