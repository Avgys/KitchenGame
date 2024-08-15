using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "KitchenObjectSO", menuName = "Kitchen/KitchenObjectSO")]
public class KitchenObjectSO : ScriptableObject
{
    public ProductType ProductType => States.First().ProductType;
    public KitchenObjectState[] States;

    public void Init(RecipeCatalog recipes)
    {
        foreach (var item in States)
        {
            item.Init(recipes); 
        }
    }
}
