using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;

public class KitchenObject : NetworkBehaviour, INode, IInteractable, ICombinable
{
    public KitchenObjectState prefab;
    [SerializeField] protected Transform visualParent;

    protected Transform currentVisual;
    protected Transform additionalVisual;
    protected float additionalVisualHeight;

    protected Rigidbody rb;
    [SerializeField] private LayerMask ExcludeMask;
    protected NetworkRigidbody netRb;

    protected Collider[] originColliders;
    protected List<Collider> meshColliders;

    public event Action Interacted;
    public event Action PreppedToInteract;

    public ProductState CurrentState => prefab.State;
    public ProductType ProductType => prefab.ProductType;

    public Node<KitchenObject> Node { get; private set; }
    public event Action<KitchenObject> OnInitiated;

    public bool IsHeld => Node.Parent != null;

    public KitchenObject Value => this;

    private bool initited = false;

    public void Start()
    {
        Initiate();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Initiate();
    }

    public void Initiate()
    {
        if (!initited && prefab != null)
        {
            meshColliders ??= new List<Collider>(4);

            var recipe = prefab.CombiningRecipe;

            Node = recipe == null
                ? new Node<KitchenObject>(this, 0)
                : new Node<KitchenObject>(this, recipe.Capacity, recipe.HierarchyPriority);

            Node.OnParentChanged += OnParentChange;

            initited = true;

            UpdateVisual();
            OnInitiated?.Invoke(this);
        }
    }

    private void OnSizeChanged()
    {
        //Node<KitchenObject>.ForEachUpward(parent.Node, (x) => UpdateAdditionVisual(x.Value));
        UpdateAdditionVisual();
    }

    void UpdateAdditionVisual()
    {
        if (additionalVisual == null)
            return;

        var position = additionalVisual.localPosition;
        position.y = Node.CombinedSize - additionalVisualHeight;
        additionalVisual.localPosition = position;
    }

    public void SetProductState(KitchenObjectState scriptableObject)
    => SyncProductStateRpc(scriptableObject.ProductType, scriptableObject.State);

    [Rpc(SendTo.Server)]
    public void SetProductStateRpc(ProductType productType, ProductState state)
    {
        if (AssetCollection.Instance.GetProductState(productType, state) != null)
            SyncProductStateRpc(productType, state);
    }

    [Rpc(SendTo.Everyone)]
    private void SyncProductStateRpc(ProductType productType, ProductState state)
    {
        prefab = AssetCollection.Instance.GetProductState(productType, state);
        Initiate();
        UpdateVisual();
    }

    protected void UpdateVisual()
    {
        if (currentVisual != null)
            Destroy(currentVisual.gameObject);

        currentVisual = Instantiate(prefab.Model, visualParent).transform;
        currentVisual.localPosition = Vector3.zero;
        currentVisual.rotation = Quaternion.identity;

        void UpdateMeshColliders(Transform container)
        {
            var isHeld = IsHeld;
            if (originColliders != null)
                meshColliders.RemoveAll(x => originColliders.Contains(x));
            originColliders = container.GetComponentsInChildren<Collider>();
            meshColliders.AddRange(originColliders);
        }

        UpdateMeshColliders(currentVisual);

        var combinedBound = new Bounds();
        var bounds = currentVisual.GetComponentsInChildren<MeshRenderer>().Select(x => x.bounds).ToArray();

        foreach (var bound in bounds)
            combinedBound.Encapsulate(bound);

        Node.SelfSize = combinedBound.size.y - transform.position.y;

        var offset = Node.Offset;

        if (currentVisual.TryGetComponent<MultiVisual>(out var multi))
        {
            additionalVisual = multi.parts[1].transform;
            additionalVisualHeight = multi.parts[1].GetComponentInChildren<MeshRenderer>().bounds.size.y;

            offset.y = multi.parts[0].GetComponentInChildren<MeshRenderer>().bounds.size.y;
            Node.OnSizeChanged += OnSizeChanged;
        }
        else
            offset.y = Node.SelfSize;

        Node.Offset = offset;
    }

    public static void SpawnProduct(KitchenObjectState scriptableObject, INode parentNode = null)
    {
        NetworkObjectReference parentRef = parentNode != null ? parentNode.NetworkObject : default;
        KitchenObjectFactory.SpawnKitcheObject(scriptableObject.ProductType, scriptableObject.State, parentRef);
    }

    public void Interact(Player source) => Interacted?.Invoke();
    public void PrepareToInteract() => PreppedToInteract?.Invoke();
    public void InteractAlternate(Player player) => Interacted?.Invoke();

    public bool IsOperationAvailable(OperationType type, KitchenObject kitchenObjects = null)
    {
        return prefab.Recipes.Any(x => x.OperationType == type)
            || kitchenObjects != null && kitchenObjects.prefab.Recipes.Any(x => x.OperationType == type);
    }

    public BaseRecipe GetRecipe(OperationType type)
        => prefab.Recipes.First(x => x.OperationType == type);

    //[Rpc(SendTo.Everyone)]
    public void DoOperationRpc(OperationType operationType)
    {
        if (!IsOperationAvailable(operationType))
            return;

        switch (operationType)
        {
            case OperationType.Slicing:
            case OperationType.Cooking:
                Modify(operationType);
                break;
        }

        return;
    }

    private void Modify(OperationType operationType)
    {
        var recipe = GetRecipe(operationType) as ModifyRecipe;
        SetProductState(recipe.Output);
    }

    bool ICombinable.TryCombine(ICombinable addition, out ICombinable result)
    {
        if (addition is KitchenObject ko && TryCombine(ko, out KitchenObject combineResult))
        {
            result = combineResult;
            return true;
        }

        result = null;
        return false;
    }

    private bool TryCombine(KitchenObject addition, out KitchenObject result)
    {
        var source = this;
        result = null;

        if (source.prefab.CombiningRecipe == null && addition.prefab.CombiningRecipe == null
            || source.prefab == addition.prefab && !source.prefab.CombiningRecipe.WithItSelf)
            return false;

        (KitchenObject parent, KitchenObject child) =
            (source.Node.HierarchyPriority) <= (addition.Node.HierarchyPriority)
            ? (source, addition)
            : (addition, source);

        if (!parent.Node.IsFreeSpace)
            return false;

        parent.Node.Push(child.Node);

        result = parent;
        return true;
    }

    private static void IncludeChildPhysics(KitchenObject parent, KitchenObject child)
    {
        parent.meshColliders.AddRange(child.meshColliders);
        child.RemoveRigidBody();
    }

    private void SyncMeshColliders(bool state)
    {
        if (meshColliders != null)
            foreach (var collider in meshColliders)
                collider.enabled = state;
    }

    [Rpc(SendTo.Server)]
    public void DivideRpc()
    {
        var poppedNode = Node.Pop();

        UpdateChildRpc(poppedNode.Value.NetworkObject);
        poppedNode.Value.TossRpc();
    }


    [Rpc(SendTo.Everyone)]
    private void UpdateChildRpc(NetworkObjectReference childRef)
    {
        var parent = this;
        childRef.TryGet(out var childNO);
        var child = childNO.GetComponent<KitchenObject>();

        void CollectChildCollider(Node<KitchenObject> node)
        {
            foreach (var collider in node.Value.originColliders)
                if (!child.meshColliders.Contains(collider))
                    child.meshColliders.Add(collider);
        }

        Node<KitchenObject>.ForEachDownward(child.Node, CollectChildCollider);

        meshColliders.RemoveAll(x => child.meshColliders.Contains(x));

        child.SyncMeshColliders(!child.IsHeld);
    }

    [Rpc(SendTo.Server)]
    public void ThrowByVectorRpc(Vector3 throwVector, float strength) => rb.AddForce(strength * throwVector, ForceMode.Impulse);

    [Rpc(SendTo.Server)]
    public void TossRpc()
    {
        var dropDirection = new Vector3(
            UnityEngine.Random.Range(-1.0f, 1.0f),
            0.5f,
            UnityEngine.Random.Range(-1.0f, 1.0f));

        ThrowByVectorRpc(dropDirection, 4f);
    }

    public void AddRigidBody()
    {
        if (rb != null)
            return;

        rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = true;

        netRb = rb.AddComponent<NetworkRigidbody>();
    }

    public void RemoveRigidBody()
    {
        if (rb == null)
            return;

        Destroy(netRb);
        Destroy(rb);

        netRb = null;
        rb = null;
    }

    private void OnParentChange(Node<KitchenObject> parentNode)
    {
        NetworkObjectReference parentRef = parentNode == null ? default : parentNode.INode.NetworkObject;
        SetTransformParentRpc(parentRef);
    }

    [Rpc(SendTo.Server)]
    private void SetTransformParentRpc(NetworkObjectReference parentRef)
    {
        if (parentRef.TryGet(out var parent))
            NetworkObject.TrySetParent(parent, false);
        else
            NetworkObject.TryRemoveParent(true);
    }

    public override void OnNetworkObjectParentChanged(NetworkObject parentNetworkObject)
    {
        base.OnNetworkObjectParentChanged(parentNetworkObject);

        if (!initited)
        {
            Debug.Log($"Parent {parentNetworkObject} was set while kitchen not initited");
            return;
        }

        var isHeld = parentNetworkObject != null;

        ToggleRigidBody(!isHeld);
        INode parentNode = parentNetworkObject != null ? parentNetworkObject.GetComponent<INode>() : null;
        SyncParentNode(parentNode);

        if (isHeld && parentNetworkObject.TryGetComponent<KitchenObject>(out var parent))
            IncludeChildPhysics(parent, this);

        TogglePhysics(!isHeld);
    }

    private void ToggleRigidBody(bool isActive)
    {
        if (isActive && rb == null)
            AddRigidBody();

        if (rb != null)
            rb.isKinematic = !isActive;
    }

    void TogglePhysics(bool isActive)
    {
        SyncMeshColliders(isActive);

        var networkTransform = GetComponent<NetworkTransform>();
        networkTransform.enabled = isActive;
    }

#nullable enable
    private void SyncParentNode(INode? parentNode)
    {
        Node.OnParentChanged -= OnParentChange;

        Node.SetParent(parentNode?.Node);

        if (parentNode != null)
        {
            transform.localPosition = Node.Position;
            transform.rotation = Node.Parent!.INode.transform.rotation;
        }

        Node.OnParentChanged += OnParentChange;
    }
#nullable disable

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();


    }

    public override void OnDestroy()
    {
        if (Node == null)
            return;

        Node.OnParentChanged -= OnParentChange;
        Node.OnSizeChanged -= OnSizeChanged;

        base.OnDestroy();
    }

    [Rpc(SendTo.Server)]
    internal void DestroySelfRpc()
    {
        Node.SetParent(null);

        foreach (var childNode in Node.GetChildrenNodes())
            childNode.Value.DestroySelfRpc();

        NetworkObject.Despawn(true);
    }
}