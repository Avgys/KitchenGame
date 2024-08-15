using System;
using Unity.Netcode;
using UnityEngine;

public class StoveCounter : ModifyCounter, ITrigger, ICombinable
{
    [SerializeField] private GameObject pan;
    public float PlayingSpeed => 1f;
    protected override OperationType operationType => OperationType.Cooking;

    private bool manualEnable;
    public event Action<bool> EnableChanged;
    public event Action<bool> Burning;

    protected override void Start()
    {
        base.Start();
        Node.Offset = new Vector3(0, 1.8f, 0);
    }

    private void Update()
    {
        if (IsServer)
            HandleCookingRpc();
    }

    public override void Interact(Player source)
    {
        base.Interact(source);
        TrySetRecipeRpc(operationType);
    }

    public override void InteractAlternate(Player player)
    {
        base.InteractAlternate(player);
        ToggleManualFireRpc();
    }

    [Rpc(SendTo.Server)]
    private void ToggleManualFireRpc()
    {
        manualEnable = !manualEnable;
        SyncManualFireRpc(manualEnable);
    }

    [Rpc(SendTo.Everyone)]
    private void SyncManualFireRpc(bool isEnabled) => EnableChanged?.Invoke(isEnabled);

    protected override void ResetCounter()
    {
        base.ResetCounter();
        EnableChanged?.Invoke(manualEnable);
        Burning?.Invoke(false);
    }

    //[Rpc(SendTo.Server)]
    private void HandleCookingRpc()
    {
        if (IsEmpty || currentRecipe == null || currentRecipe.Input != StoredItem.prefab || !manualEnable)
            return;

        AddDeltaRpc(Time.deltaTime);        
    }
}
