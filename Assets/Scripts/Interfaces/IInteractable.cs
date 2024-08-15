using System;

public interface IInteractable
{
    public event Action Interacted;
    public event Action PreppedToInteract;
    void PrepareToInteract();
    void Interact(Player source);
    void InteractAlternate(Player player);
}