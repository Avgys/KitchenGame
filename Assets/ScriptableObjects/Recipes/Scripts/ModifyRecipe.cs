using UnityEngine;

[CreateAssetMenu(fileName = "ModifyRecipe", menuName = "Kitchen/ModifyRecipe")]
public class ModifyRecipe : BaseRecipe
{
    [SerializeField]
    private OperationType SelectType;
    public override OperationType OperationType => SelectType;
    public float TimeToModify;

    public KitchenObjectState Output;
}