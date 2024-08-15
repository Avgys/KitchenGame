using System;
using Unity.Netcode;

public class CuttingCounter : ModifyCounter, ICombinable
{
    public static event Action<CuttingCounter> OnAnyCut;
    public static void ResetStatic() => OnAnyCut = null;

    protected override OperationType operationType => OperationType.Slicing;

    public override void InteractAlternate(Player player)
    {
        if (IsEmpty || !StoredItem.IsOperationAvailable(operationType))
            return;

        base.InteractAlternate(player);

        ServerCutRpc();
    }

    [Rpc(SendTo.Server)]
    void ServerCutRpc()
    {
        if (currentRecipe == null)
            TrySetRecipeRpc(operationType);

        if (currentRecipe != null)
        {
            OnAnyCut?.Invoke(this);
            AddDeltaRpc(1f);
        }
    }
}
