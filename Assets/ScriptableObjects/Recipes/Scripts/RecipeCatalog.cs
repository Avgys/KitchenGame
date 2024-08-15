using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//[CreateAssetMenu(fileName = "RecipeCatalog", menuName = "Kitchen/RecipeCatalog")]
public class RecipeCatalog : ScriptableObject
{
    public BaseRecipe[] Recipes;

    public IEnumerable<BaseRecipe> GetRecipes(KitchenObjectState productState)
    {
        return Recipes.Where(x => x.Input == productState);
    }
}