using TMPro;
using UnityEngine;

public class DishUI : MonoBehaviour
{
    [SerializeField] private IngredientUI ingredientUIPrefab;
    [SerializeField] private Transform ingredientParent;
    [SerializeField] private TextMeshProUGUI textMesh;
    public string DishName
    {
        get => textMesh.text;
        set => textMesh.text = value;
    }


    internal void AddIngredient(KitchenObjectState ko)
    {
        var item = Instantiate(ingredientUIPrefab, ingredientParent);
        item.gameObject.SetActive(true);
        item.Image.sprite = ko.Sprite;
    }
}
