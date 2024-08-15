using System;
using Unity.Netcode;

public class ModifyCounter : ClearCounter, IProgress
{
    protected float currentTime;
    protected ModifyRecipe currentRecipe;
    public event Action<float> ProgressChanged;

    protected virtual OperationType operationType => OperationType.Cooking;

    protected override void Start()
    {
        base.Start();

        ResetCounter();
    }

    public override void Interact(Player source)
    {
        base.Interact(source);
        ResetCounter();
    }

    protected virtual void ResetCounter()
    {
        currentTime = 0f;
        currentRecipe = null;
    }

    [Rpc(SendTo.Server)]
    protected void TrySetRecipeRpc(OperationType operationType)
    {
        ResetCounter();

        if (StoredItem != null && StoredItem.IsOperationAvailable(operationType))
            currentRecipe = StoredItem.GetRecipe(operationType) as ModifyRecipe;
        else
            currentRecipe = null;
    }

    [Rpc(SendTo.Server)]
    protected void AddDeltaRpc(float delta)
    {
        currentTime += delta;
        UpdateProgressRpc(currentTime / currentRecipe?.TimeToModify ?? float.MaxValue);
        CheckStateChangeRpc();

        void CheckStateChangeRpc()
        {
            if (currentTime < currentRecipe?.TimeToModify)
                return;

            StoredItem.DoOperationRpc(operationType);
            ResetCounter();
            TrySetRecipeRpc(operationType);
        }
    }

    [Rpc(SendTo.Everyone)]
    void UpdateProgressRpc(float value) => ProgressChanged?.Invoke(value);
}
