using Unity.Netcode;
using UnityEngine;

public abstract class BaseVisual : NetworkBehaviour
{
    protected virtual IInteractable source { get; set; }

    protected virtual void Start()
    {
        source = GetComponentInParent<IInteractable>();
    }
}

public abstract class BaseCounterVisual<T> : BaseVisual where T : BaseCounter
{
    protected override IInteractable source
    {
        get => counter;
        set => counter = (T)value;
    }

    [SerializeField] protected T counter;

}