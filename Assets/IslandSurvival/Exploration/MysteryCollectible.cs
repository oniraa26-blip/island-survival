using IslandSurvival.Core;
using IslandSurvival.Interaction;
using UnityEngine;

namespace IslandSurvival.Exploration
{
    public sealed class MysteryCollectible : MonoBehaviour, IInteractable
    {
        public string collectibleId;
        public string itemId;
        public string ActionLabel { get { return ServiceRegistry.Localization.Format("interaction.collect_item", ServiceRegistry.Localization.Get(ServiceRegistry.Items.Get(itemId).displayNameKey)); } }
        public bool CanInteract { get { return !ServiceRegistry.Discovery.IsCollected(collectibleId); } }

        void Start() { if (ServiceRegistry.Discovery.IsCollected(collectibleId)) gameObject.SetActive(false); }

        public void Interact()
        {
            if (!CanInteract || !ServiceRegistry.Inventory.Add(itemId, 1)) return;
            ServiceRegistry.Discovery.Collect(collectibleId);
            gameObject.SetActive(false);
            ServiceRegistry.Save.SaveNow();
        }
    }
}
