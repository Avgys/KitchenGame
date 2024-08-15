using System;
using Unity.Netcode;
using UnityEngine;

public abstract class BaseCounter : NetworkBehaviour, IInteractable, INode
{
    private const byte COUNTER_PRIORITY = 0;
    private const byte COUNTER_CAPACITY = 1;
    private static readonly Vector3 CHILD_OFFSET = new(0, 1.5f, 0);
    public Node<KitchenObject> Node { get; private set; }

    protected KitchenObject StoredItem => Node.LastItem?.Value;
    protected bool IsEmpty => Node.IsEmpty;

    public Transform Parent => transform;

    protected virtual void Start()
    {
        Node = new(this, COUNTER_CAPACITY, COUNTER_PRIORITY, CHILD_OFFSET);
    }

    public event Action Interacted;
    public event Action PreppedToInteract;
    public event Action AlternateInteracted;
    public virtual void PrepareToInteract() => PreppedToInteract?.Invoke();
    public virtual void Interact(Player source) => Interacted?.Invoke();
    public virtual void InteractAlternate(Player player) => AlternateInteracted?.Invoke();

    public virtual bool TryCombine(ICombinable kitchenObject, out ICombinable result)
    {
        result = null;
        if (IsEmpty)
            return false;

        return kitchenObject.TryCombine(StoredItem, out result);
    }
}