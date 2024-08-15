using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


#nullable enable
public class Node<T> where T : class
{
    public Node<T>? Parent { get; private set; }
    private List<Node<T>>? Children;

    public Node<T>[] GetChildrenNodes() => Children?.ToArray() ?? Array.Empty<Node<T>>();

    public INode INode { get; private set; }
    public T? Value => INode as T;

    public Node<T>? LastItem => Children?.LastOrDefault();
    public Node<T>? FirstItem => Children?.FirstOrDefault();

    public float CombinedSize = 0f;
    private float selfSize;
    public float SelfSize
    {
        get => selfSize;
        set
        {
            var newSize = Mathf.Max(0.2f, value);
            CombinedSize = CombinedSize - selfSize + newSize;
            OnSizeChanged?.Invoke();
            selfSize = newSize;
        }
    }

    public bool IsEmpty => !Children?.Any() ?? true;
    public bool IsFreeSpace => Capacity > (Children?.Count ?? 0);

    public int Capacity
    {
        get => Children?.Capacity ?? 0;
        set
        {
            if (Children != null)
                Children.Capacity = value;
            else if (value > 0)
                Children = new List<Node<T>>(value);
        }
    }

    private Vector3 offset;

    public Vector3 Offset
    {
        get => offset;
        set
        {
            offset = value;
            if (!IsEmpty)
                UpdateChildrenPositions(this);
        }
    }

    public byte HierarchyPriority { get; private set; }
    public Vector3 Position { get; private set; }

    public event Action<Node<T>?>? OnParentChanged;
    //public event Action<Vector3>? OnPositionChanged;
    public event Action? OnSizeChanged;

    public Node(INode node, int capacity, byte priority = 127, Vector3 offset = default)
    {
        Capacity = capacity;
        Offset = offset;
        HierarchyPriority = priority;
        INode = node;
    }

    public bool Push(Node<T> itemToAdd)
    {
        if (!IsFreeSpace)
            return false;

        var newParent = FindParentRecursive(this);

        Node<T> FindParentRecursive(Node<T> parent)
        {
            for (int i = parent.Children!.Count - 1; i >= 0; i--)
            {
                var child = parent.Children[i];
                if (child.IsFreeSpace && child.HierarchyPriority < itemToAdd.HierarchyPriority && child != itemToAdd)
                    return FindParentRecursive(child);
            }

            return parent;
        }

        itemToAdd.SetParent(newParent);

        return true;
    }

    public IEnumerable<Node<T>> Flatten()
    {
        var nodes = new List<Node<T>>();

        FlatRecursive(this);

        void FlatRecursive(Node<T> node)
        {
            nodes.Add(node);
            if (node.Children != null && node.Children.Count > 0)
                foreach (var item in node.Children)
                    FlatRecursive(item);
        }

        return nodes;
    }

    public Node<T>? Pop(bool PopLastItem = false)
    {
        if (IsEmpty)
            return null;

        Node<T> result;

        if (PopLastItem)
        {
            static Node<T> PopRecursive(Node<T> holder)
                 => holder.IsEmpty ? holder : PopRecursive(holder.Children.Last());

            result = PopRecursive(Children.Last());
        }
        else
            result = Children.Last();


        result.SetParent(null);

        return result;
    }

    public void SwapChildren(Node<T> secondParent)
    {
        if (Capacity != secondParent.Capacity)
            throw new ArgumentException("Cannot swap children with different capacity");

        var sizeOfFirstChildren = Children.Sum(child => child.CombinedSize);
        var sizeOfFSecondChildren = secondParent.Children.Sum(child => child.CombinedSize);

        (secondParent.Children, Children) = (Children, secondParent.Children);

        var deltaSize = -sizeOfFirstChildren + sizeOfFSecondChildren;

        ModifySizeUpwards(this, deltaSize);
        ModifySizeUpwards(secondParent, -deltaSize);

        static void UpdateChild(Node<T> parentNode)
        {
            foreach (var child in parentNode.Children!)
            {
                child.Parent = parentNode;
                UpdateNodePosition(child);
                child.OnParentChanged?.Invoke(parentNode);
            }
        }

        UpdateChild(this);
        UpdateChild(secondParent);
    }

    static void UpdateChildrenPositions(Node<T> parentNode)
    {
        if (parentNode.Children == null) return;

        foreach (var child in parentNode.Children)
            UpdateNodePosition(child);
    }

    private static void UpdateNodePosition(Node<T> node)
    {
        if (node.Parent == null)
            return;

        static Vector3 getOffsetFromPrevChild(Node<T> node)
        {
            var parent = node.Parent;

            if (parent == null)
                return Vector3.zero;

            var childIndex = parent.Children!.IndexOf(node);

            if (parent.Children!.Count == 1
                || childIndex == 0)
                return parent.Offset;

            var previousChild = parent.Children[childIndex - 1];

            var additionOffset = new Vector3(0, previousChild.CombinedSize, 0);
            return previousChild.Position + additionOffset;
        }

        var offset = getOffsetFromPrevChild(node);

        //if (offset == node.Position)
        //    return;

        node.Position = offset;
        //node.OnPositionChanged?.Invoke(node.Position);
    }

    internal static void ForEachUpward(Node<T> startNode, Action<Node<T>> action)
    {
        UpdateTreeRecursive(startNode);

        void UpdateTreeRecursive(Node<T> node)
        {
            if (node == null) return;
            action(node);

            if (node.Parent != null)
                UpdateTreeRecursive(node.Parent);
        }
    }

    internal static void ForEachDownward(Node<T> startNode, Action<Node<T>> action)
    {
        UpdateTreeRecursive(startNode);

        void UpdateTreeRecursive(Node<T> node)
        {
            if (node == null) return;
            action(node);

            if (node.Children != null)
                foreach (var item in node.Children)
                    UpdateTreeRecursive(item);
        }
    }

    public void ForEachChildren(Action<Node<T>> action)
    {
        if (Children != null)
            foreach (var item in Children)
                action(item);
    }

    static void ModifySizeUpwards(Node<T> startNode, float deltaSize)
    {
        var parentNode = startNode;
        while (parentNode != null)
        {
            parentNode.CombinedSize += deltaSize;
            parentNode.OnSizeChanged?.Invoke();
            parentNode = parentNode.Parent;
        }
    }

    private void RemoveNode(Node<T> childNode)
    {
        if (Children?.Contains(childNode) ?? false)
        {
            Children!.Remove(childNode);
            ModifySizeUpwards(this, -childNode.CombinedSize);
            ForEachUpward(this, UpdateNodePosition);
        }

        childNode.Parent = null;
    }

    private void AddNode(Node<T> childNode)
    {
        if (Capacity > 0 && !Children!.Contains(childNode))
        {
            Children!.Add(childNode);
            ModifySizeUpwards(this, childNode.CombinedSize);
            UpdateNodePosition(childNode);
            if (Parent != null)
                ForEachUpward(Parent, UpdateChildrenPositions);
        }
    }

    internal void SetParent(Node<T>? parent)
    {
        if (Parent == parent)
            return;

        Parent?.RemoveNode(this);
        Parent = parent;
        Parent?.AddNode(this);
        OnParentChanged?.Invoke(parent);
    }
}