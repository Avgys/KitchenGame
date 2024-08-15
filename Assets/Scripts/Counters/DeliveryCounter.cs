using System;
using UnityEngine;

public class DeliveryCounter : ClearCounter
{
    [SerializeField] private ProductType productToDelete;

    public event Action<bool> OnDelivery;

    protected override void Start()
    {
        base.Start();

        DeliveryManager.Instance.AddCounter(this);
    }

    public override void Interact(Player source)
    {
        base.Interact(source);

        if (Node.FirstItem != null)
            DeliveryManager.Instance.TryDeliverDish(Node.FirstItem.Value, NetworkObjectId);
    }

    public void SuccessDelivery()
    {
        Node.FirstItem?.Value.DestroySelfRpc();
        OnDelivery?.Invoke(true);
    }

    public void FailureDelivery()
    {
        OnDelivery?.Invoke(false);
    }
}
