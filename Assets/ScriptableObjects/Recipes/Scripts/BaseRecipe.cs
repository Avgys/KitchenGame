using UnityEngine;

public abstract class BaseRecipe : ScriptableObject
{
    public KitchenObjectState Input;
    public virtual OperationType OperationType { get; }
    //public abstract void Execute(params KitchenObject[] inputs);
}
