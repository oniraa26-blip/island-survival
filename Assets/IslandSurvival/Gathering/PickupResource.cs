using IslandSurvival.Core;
using IslandSurvival.Interaction;
using UnityEngine;

namespace IslandSurvival.Gathering
{
    public sealed class PickupResource : MonoBehaviour, IInteractable
    {
        public string itemId;
        public int amount = 1;
        public string ActionLabel { get { return ServiceRegistry.Localization.Format("interaction.pick_up_item", ServiceRegistry.Localization.Get(ServiceRegistry.Items.Get(itemId).displayNameKey)); } }
        public bool CanInteract { get { return enabled && gameObject.activeSelf; } }
        public void Interact()
        {
            if (ServiceRegistry.Inventory.Add(itemId, amount)) gameObject.SetActive(false);
        }
    }
}
