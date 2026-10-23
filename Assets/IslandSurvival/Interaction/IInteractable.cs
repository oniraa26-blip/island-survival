namespace IslandSurvival.Interaction
{
    public interface IInteractable
    {
        string ActionLabel { get; }
        bool CanInteract { get; }
        void Interact();
    }
}
