using System.Linq;
using UnityEngine;

public class AssetCollection : MonoBehaviour
{
    public static AssetCollection Instance { get; private set; }
    public KitchenObject ProductTemplate;
    //public KitchenObject PlateTemplate;
    [SerializeField] IngredientCatalog Catalog;
    [SerializeField] RecipeCatalog RecipeCatalog;

    private void Awake()
    {
        Instance = this;
        PrepareProducts();
    }

    private void PrepareProducts()
    {
        foreach (var product in Catalog.Ingredients)
            product.Init(RecipeCatalog);
    }

    internal KitchenObjectSO GetProduct(ProductType productType)
    {
        return Catalog.Ingredients.FirstOrDefault(x => x.ProductType == productType);
    }

    internal KitchenObjectState GetProductState(ProductType productType, ProductState productState)
    {
        return GetProduct(productType).States.FirstOrDefault(x => x.State == productState);
    }
}
