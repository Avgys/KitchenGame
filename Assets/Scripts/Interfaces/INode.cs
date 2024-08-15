using Unity.Netcode;
using UnityEngine;

public interface INode
{
    Node<KitchenObject> Node { get; }
    public Transform transform { get; }
    NetworkObject NetworkObject { get; }
}
