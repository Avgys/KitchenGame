using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "KitchenObjectState", menuName = "Kitchen/KitchenObjectState")]
public class KitchenObjectState : ScriptableObject
{
    public ProductType ProductType;
    public ProductState State;

    public IEnumerable<BaseRecipe> Recipes;
    public CombineRecipe CombiningRecipe { get; private set; }

    public GameObject Model;

    public Sprite Sprite;

    private Mesh GetCombinedMesh()
    {
        
        Mesh mesh = null;
        MeshFilter[] childrenMesh = Model.GetComponentsInChildren<MeshFilter>();

        CombineInstance[] combine = new CombineInstance[childrenMesh.Length];
        for (int i = 0; i < childrenMesh.Length; i++)
        {
            combine[i].mesh = childrenMesh[i].sharedMesh;
            combine[i].transform = childrenMesh[i].transform.localToWorldMatrix;
        }

        mesh = new();
        mesh.CombineMeshes(combine);

        return mesh;
    }

    public BaseRecipe GetRecipe(OperationType operationType)
    {
        return Recipes.FirstOrDefault(x => x.OperationType == operationType);
    }

    internal void Init(RecipeCatalog recipes)
    {
        Recipes = recipes.GetRecipes(this);
        CombiningRecipe = GetRecipe(OperationType.Combining) as CombineRecipe;
    }

    public override bool Equals(object other)
    {
        var otherState = other as KitchenObjectState;
        return otherState?.ProductType == ProductType && otherState?.State == State;
    }

    public override int GetHashCode() => base.GetHashCode();
}