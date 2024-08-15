using System;
using Unity.Netcode;

public class KitchenObjectFactory : NetworkBehaviour
{
    public static KitchenObjectFactory Singleton { get; private set; }
    private KitchenObject productPrefab;
    private static Action spawnActions;

    private void Start()
    {
        Singleton = this;
        productPrefab = AssetCollection.Instance.ProductTemplate;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkDespawn();
        spawnActions?.Invoke();
        spawnActions = null;
    }

    [Rpc(SendTo.Server)]
    public void SpawnKitcheObjectRpc(ProductType productType, ProductState productState, NetworkObjectReference parentReference)
    {
        var kitchenObject = SpawnKichenObject(productType, productState);

        if (parentReference.TryGet(out var networkObject) && networkObject.TryGetComponent<INode>(out var parentNode))
            parentNode.Node.Push(kitchenObject.Node);
    }

    public static void SpawnKitcheObject(ProductType productType, ProductState productState, NetworkObjectReference parentReference)
    {
        if (!Singleton.IsSpawned)
        {
            spawnActions += () => Singleton.SpawnKitcheObjectRpc(productType, productState, parentReference);
            return;
        }

        Singleton.SpawnKitcheObjectRpc(productType, productState, parentReference);
    }

    private KitchenObject SpawnKichenObject(ProductType productType, ProductState productState)
    {
        var kitchenObject = Instantiate(Singleton.productPrefab);
        var nko = kitchenObject.GetComponent<NetworkObject>();
        nko.Spawn(true);
        kitchenObject.SetProductStateRpc(productType, productState);
        return kitchenObject;
    }
}
