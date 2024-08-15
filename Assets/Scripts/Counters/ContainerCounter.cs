using System;
using UnityEngine;

public class ContainerCounter : SpawnerCounter
{
    public Sprite Sprite => prefab != null ? prefab.Sprite : null;
    public event Action Initiated;


    protected override void Start()
    {
        base.Start();
        Initiated?.Invoke();
        GameManager.StateChanged += GameManager_StateChanged;
    }

    private void GameManager_StateChanged(GameManager.State obj)
    {
        if (obj == GameManager.State.GamePlaying)
            SpawnKitchenObjectRpc();
    }

    public override void Interact(Player player)
    {
        base.Interact(player);

        if (Node.IsEmpty)
            SpawnKitchenObjectRpc();
    }

    public override bool TryCombine(ICombinable kitchenObject, out ICombinable result)
    {
        var isSuccess = base.TryCombine(kitchenObject, out result);

        if (isSuccess)
            SpawnKitchenObjectRpc();

        return isSuccess;
    }

    public override void OnDestroy()
    {
        GameManager.StateChanged -= GameManager_StateChanged;
        base.OnDestroy();
    }
}