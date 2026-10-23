using IslandSurvival.Core;
using IslandSurvival.Interaction;
using UnityEngine;

namespace IslandSurvival.Building
{
    public sealed class PlacedBuildable : MonoBehaviour, IInteractable
    {
        public string persistentId;
        public string buildableId;
        public string ActionLabel { get { return ServiceRegistry.Localization.Get(buildableId == "leaf_bed" ? "interaction.sleep_respawn" : "interaction.inspect_campfire"); } }
        public bool CanInteract { get { return true; } }
        public void Interact()
        {
            if (buildableId == "leaf_bed")
            {
                ServiceRegistry.Save.SetRespawn(transform.position + transform.forward * 1.5f + Vector3.up * 0.2f);
                ServiceRegistry.Time.AdvanceToMorning();
                ServiceRegistry.Save.SaveNow();
            }
        }
    }
}
