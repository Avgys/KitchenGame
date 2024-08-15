using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DishBase", menuName = "Kitchen/DishBase")]
public class Dish : ScriptableObject
{
    public KitchenObjectState[] Ingredients;
    [SerializeField] private string dishName;
    public bool IsSpecial = false;
    public string DishName => string.IsNullOrEmpty(dishName) ? name : dishName;
}
