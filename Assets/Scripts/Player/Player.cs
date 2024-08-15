using System;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class Player : NetworkBehaviour, ITrigger, INode
{
    [SerializeField] private float Speed;
    [SerializeField] private float SpeedBoost;
    [SerializeField] private float maxInteractDisctance = 0.5f;
    [SerializeField] private float interactZoneScale = 1f;
    [SerializeField] private LayerMask InteractableLayerMask;
    [SerializeField] private Vector3 HoldOffset;

    private Rigidbody rb;

    public float LinearSpeed => rb.linearVelocity.magnitude;

    private IInteractable Interactable => interactableInFront;
    private IInteractable interactableInFront;

    public byte Priority => (byte)HirachyPriorityes.Player;

    public Node<KitchenObject> Node { get; private set; }

    public byte HierarchyPriority => 0;

    public float PlayingSpeed => GameInput.Instance.SpeedBoost ? SpeedBoost : 1;

    private const int playerCapacity = 1;

    private RaycastHit lastHitInfo;
    private bool isHitted;

    private Vector3 bodyCenter;
    private const byte PLAYER_PRIORITY = 0;

    public static event Action<Player> PickedUpItem;
    public static event Action<Player> DroppedItem;

    [SerializeField] private Vector3[] spawnPositions;

    internal static void ResetStatic()
    {
        DroppedItem = null;
        PickedUpItem = null;
    }

    public event Action<bool> EnableChanged;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //CinemachineScript.Singleton.Follow(transform);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Node = new(this, playerCapacity, PLAYER_PRIORITY, HoldOffset);
        bodyCenter = new Vector3(0, 0.5f, 0);

        if (!IsOwner)
            return;

        GameInput.Instance.InteractEvent += ActivateInteraction;
        GameInput.Instance.AlternateInteractEvent += ActivateAlternateInteraction;
        GameInput.Instance.Combine += Combine;
        GameInput.Instance.DropEvent += (CallbackContext context) => DropItem(false);
        GameInput.Instance.ThrowEvent += (CallbackContext context) => DropItem(true);
        GameInput.Instance.DropPartEvent += (CallbackContext context) => DropPartItem(true);

        transform.position = spawnPositions[NetworkData.Singleton.GetPlayerIndex(OwnerClientId)];
    }

    void DropItem(bool isThrow)
    {
        if (Node.IsEmpty) return;

        var item = Node.Pop(false);
        if (isThrow)
        {
            var throwVector = transform.forward;
            throwVector.y = 0.25f;
            item.Value.ThrowByVectorRpc(throwVector, 10f);
        }

        DroppedItem?.Invoke(this);
    }

    void DropPartItem(bool isThrow)
    {
        if (Node.LastItem?.IsEmpty ?? true)
        {
            DropItem(isThrow);
            return;
        }

        if (Node.LastItem.Value != null)
            Node.LastItem.Value.DivideRpc();

        DroppedItem?.Invoke(this);
    }

    private void ActivateInteraction(CallbackContext context)
    {
        if (Interactable == null)
            return;

        if (interactableInFront is KitchenObject ko)
            Node.Push(ko.Node);

        Interactable.Interact(this);

        PickedUpItem?.Invoke(this);
    }

    private void ActivateAlternateInteraction(CallbackContext context)
    {
        if (Interactable == null)
            return;

        Interactable?.InteractAlternate(this);
    }

    private void Combine(CallbackContext context)
    {
        if (interactableInFront == null || Node.IsEmpty || Node.FirstItem?.Value == null)
            return;

        var holdItemToCombine = Node.FirstItem.Value;

        if (interactableInFront is ICombinable combinable
            && combinable.TryCombine(holdItemToCombine, out var result)
            && result is INode resultNode)
        {
            resultNode.Node.SetParent(Node);
        }
    }

    void Update()
    {
        if (!IsOwner)
            return;

        HandleMovement();
        HandleInteractions();
    }

    private void HandleInteractions()
    {
        Vector3 p1 = transform.position;
        Vector3 p2 = p1 + bodyCenter * 2;
        isHitted = Physics.CapsuleCast(p1, p2, 0.4f, transform.forward, out lastHitInfo, maxInteractDisctance, InteractableLayerMask);

        if (isHitted)
        {
            var item = lastHitInfo.transform.GetComponentInParent<IInteractable>();
            interactableInFront = item;
            item?.PrepareToInteract();
        }
        else
        {
            interactableInFront = null;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        float distance = isHitted ? lastHitInfo.distance : maxInteractDisctance;

        Gizmos.DrawRay(transform.position, transform.forward * distance);
        Gizmos.DrawWireSphere(transform.position + transform.forward * distance, interactZoneScale * transform.localScale.x);
    }

    private void HandleMovement()
    {
        var movementVector = GameInput.Instance.MovementVectorNormalized;

        rb.linearVelocity = (GameInput.Instance.SpeedBoost ? SpeedBoost : 1) * Speed * movementVector;

        const float rotateSpeed = 10f;

        if (movementVector != Vector3.zero)
        {
            transform.forward = Vector3.Slerp(
                transform.forward,
                movementVector,
                Time.deltaTime * rotateSpeed);

            EnableChanged?.Invoke(true);
        }
        else
            EnableChanged?.Invoke(false);
    }

    public override void OnDestroy()
    {
        if (!IsOwner)
            return;

        if (!Node.IsEmpty)
            DropItem(false);

        if (GameInput.Instance != null)
        {
            GameInput.Instance.InteractEvent -= ActivateInteraction;
            GameInput.Instance.AlternateInteractEvent -= ActivateAlternateInteraction;
            GameInput.Instance.Combine -= Combine;
            GameInput.Instance.DropEvent -= (CallbackContext context) => DropItem(false);
            GameInput.Instance.DropPartEvent -= (CallbackContext context) => DropItem(true);
        }

        base.OnDestroy();
    }
}
