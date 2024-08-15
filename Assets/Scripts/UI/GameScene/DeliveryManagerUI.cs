using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DeliveryManagerUI : MonoBehaviour
{
    [SerializeField] Transform recipeParent;

    [SerializeField] private DishUI recipePrefab;
    private List<(Dish dish, DishUI ui)> dishOnDashBoard;

    private void Start()
    {
        dishOnDashBoard = new(DeliveryManager.Instance.WaitingDishesMax);
        DeliveryManager.Instance.DishRequested += AddDishUI;
        DeliveryManager.Instance.DishServed += RemoveDIshUI;
    }

    private void AddDishUI(DeliveryManager source, Dish dish)
    {
        var dishUI = Instantiate(recipePrefab, recipeParent);
        dishUI.gameObject.SetActive(true);
        dishUI.DishName = dish.DishName;
        foreach (var item in dish.Ingredients)
            dishUI.AddIngredient(item);

        dishOnDashBoard.Add((dish, dishUI));

    }

    private void RemoveDIshUI(DeliveryManager source, Dish dish)
    {
        var dishUI = dishOnDashBoard.FirstOrDefault(x => x.dish == dish);
        if (dishUI == default)
            return;

        dishOnDashBoard.Remove(dishUI);
        Destroy(dishUI.ui.gameObject);
    }
}
