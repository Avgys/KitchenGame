using UnityEngine;

[CreateAssetMenu(fileName = "CombineRecipe", menuName = "Kitchen/CombineRecipe")]
public class CombineRecipe : BaseRecipe
{
    public override OperationType OperationType => OperationType.Combining;
    public byte HierarchyPriority;
    public byte Capacity;
    public bool WithItSelf;
}
