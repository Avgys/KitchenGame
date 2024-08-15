using UnityEngine;

[CreateAssetMenu(fileName = "DishCatalog", menuName = "Scriptable Objects/DishCatalog")]
public class DishCatalog : ScriptableObject
{
    public Dish[] Dishes;
}
