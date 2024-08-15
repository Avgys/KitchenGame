using Unity.Netcode;
using UnityEngine;

public class SpawnerCounter : BaseCounter, ICombinable
{
    [SerializeField] protected KitchenObjectState prefab;
    [SerializeField] protected bool isPoolHidden;
    protected bool spawning = false;
    [SerializeField] protected int defaultNodeCapacity = 1;
    public KitchenObject Value => StoredItem;

    protected override void Start()
    {
        base.Start();
        Node.Capacity = defaultNodeCapacity;
        GameManager.StateChanged += GameManager_StateChanged;
    }

    private void GameManager_StateChanged(GameManager.State state) => spawning = state == GameManager.State.GamePlaying;

    [Rpc(SendTo.Server)]
    protected void SpawnKitchenObjectRpc()
    {
        if (!Node.IsFreeSpace || !spawning)
            return;

        KitchenObjectFactory.SpawnKitcheObject(prefab.ProductType, prefab.State, NetworkObject);
    }

    public override void Interact(Player player)
    {
        if (Node.IsEmpty || !player.Node.IsEmpty)
            return;

        base.Interact(player);
        Node.LastItem?.SetParent(player.Node);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        spawning = false;
    }

    public override void OnDestroy()
    {
        GameManager.StateChanged -= GameManager_StateChanged;
        base.OnDestroy();
    }
}