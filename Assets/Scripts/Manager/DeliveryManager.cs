using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

#nullable enable
#pragma warning disable CS8618

public class DeliveryManager : NetworkBehaviour
{
    private struct GrouppedIngredients
    {
        public KitchenObjectState State;
        public int Count;
    }

    private record AwaitingDish
    {
        public int Id;
        public int CatalogIndex;
        public Dish dish;
    }

    public static DeliveryManager Instance { get; private set; }

    [SerializeField] private DishCatalog Catalog;

    private List<AwaitingDish> awaitingDishes;
    int nextDishId = -1;

    [SerializeField] private int waitingMaxDishes = 4;
    public int WaitingDishesMax => waitingMaxDishes;

    [SerializeField] private float spawnTimeDelay = 4f;
    private float timeToSpawn;
    private bool spawning;

    private void Awake() => Instance = this;

    public event Action<DeliveryManager, Dish> DishRequested;
    public event Action<DeliveryManager> WrongDishServed;
    public event Action<DeliveryManager, Dish> DishServed;

    public int TotalDishServed { get; private set; } = 0;

    private readonly List<DeliveryCounter> counters = new();

    private void Start()
    {
        awaitingDishes = new();

        GameManager.StateChanged += Instance_StateChanged;
        timeToSpawn = spawnTimeDelay;
    }

    public void AddCounter(DeliveryCounter counter)
    {        
        if (!counters.Contains(counter)) 
            counters.Add(counter);
    }

    private void Instance_StateChanged(GameManager.State obj)
        => spawning = obj == GameManager.State.GamePlaying;


    private void Update()
    {
        if (!IsServer || !spawning)
            return;

        timeToSpawn -= Time.deltaTime;
        if (timeToSpawn < 0 && awaitingDishes.Count < waitingMaxDishes)
        {
            var dishCatalogId = GetDishCatalogId();
            var dishId = GetDishServeId();
            SpawnDishRpc(dishId, dishCatalogId);
            timeToSpawn = spawnTimeDelay;
        }

        int GetDishCatalogId()
        {
            var index = UnityEngine.Random.Range(0, Catalog.Dishes.Length);
            var dish = Catalog.Dishes[index];
            var tries = 0;

            if (awaitingDishes.Count < Catalog.Dishes.Length)
                while (awaitingDishes.Any(x => x.dish == dish) && tries < Catalog.Dishes.Length)
                {
                    tries++;
                    dish = Catalog.Dishes[(tries + index) % Catalog.Dishes.Length];
                }

            return (tries + index) % Catalog.Dishes.Length;
        }

        int GetDishServeId() => ++nextDishId;
    }

    [Rpc(SendTo.Everyone)]
    void SpawnDishRpc(int dishId, int dishIndex)
    {
        var dish = Catalog.Dishes[dishIndex];
        awaitingDishes.Add(new AwaitingDish { Id = dishId, dish = dish, CatalogIndex = dishIndex });
        DishRequested?.Invoke(this, dish);
    }

    public void TryDeliverDish(KitchenObject ko, ulong counterId)
    {
        if (ko.ProductType != ProductType.Plate)
        {
            WrongDishServed?.Invoke(this);
            return;
        }

        var flattenDish = ko.Node.GetChildrenNodes()
            .SelectMany(node => node.Flatten()
            .Select(x => x.Value!.prefab)).ToArray();

        var countedDish = GroupByIngredients(flattenDish);

        var sameLengthDishes = awaitingDishes
            .Where(x => x.dish.Ingredients.Length == flattenDish.Length)
            .Select(awaitingDish => (awaitingDish, GroupByIngredients(awaitingDish.dish.Ingredients)))
            .Where(x => x.Item2.Count == countedDish.Count)
            .ToArray();

        List<GrouppedIngredients> GroupByIngredients(IEnumerable<KitchenObjectState> mixedDish)
        {
            var list = new List<GrouppedIngredients>();

            foreach (var component in mixedDish)
            {
                if (list.Any(x => x.State == component))
                {
                    var item = list.First(x => x.State == component);
                    item.Count++;
                }
                else
                    list.Add(new GrouppedIngredients { State = component, Count = 1 });
            }

            return list;
        }

        AwaitingDish? FindSameDish(List<GrouppedIngredients> dishToFind)
        {
            bool isEqual(IEnumerable<GrouppedIngredients> x, IEnumerable<GrouppedIngredients> y)
            {
                foreach (var xItem in x)
                    if (!y.Any(yItem => yItem.State == xItem.State && yItem.Count == xItem.Count))
                        return false;

                return true;
            }

            for (int i = 0; i < sameLengthDishes.Length; i++)
            {
                var possibleDish = sameLengthDishes[i];

                if (isEqual(possibleDish.Item2, countedDish))
                    return possibleDish.awaitingDish;
            }

            return null;
        }

        var dish = FindSameDish(countedDish);

        if (dish == null)
        {
            WrongDishServed?.Invoke(this);
            return;
        }

        TryDeliverDishRpc(dish.Id, dish.CatalogIndex, NetworkManager.Singleton.LocalClientId, counterId);
    }

    [Rpc(SendTo.Server)]
    void TryDeliverDishRpc(int dishId, int catalogIndex, ulong clientId, ulong counterId)
    {
        var dish = awaitingDishes.FirstOrDefault(x => x.Id == dishId);
        var catalogDish = Catalog.Dishes.Length > catalogIndex && 0 <= catalogIndex ? Catalog.Dishes[catalogIndex] : null;

        if (dish == null || catalogDish == null || dish.dish != catalogDish)
        {
            DeliveryFailureRpc(counterId, RpcTarget.Single(clientId, RpcTargetUse.Temp));
            return;
        }

        DeliverySuccessRpc(dishId, counterId);
    }

    [Rpc(SendTo.Everyone)]
    void DeliverySuccessRpc(int dishId, ulong counterId)
    {
        var dishToRemove = awaitingDishes.First(x => x.Id == dishId);
        awaitingDishes.Remove(dishToRemove);
        Debug.Log($"Dish delivered {dishToRemove.dish.DishName}");

        TotalDishServed++;
        DishServed?.Invoke(this, dishToRemove.dish);

        var counter = counters.First(x => x.NetworkObjectId == counterId);
        counter.SuccessDelivery();
    }

    [Rpc(SendTo.SpecifiedInParams)]
    void DeliveryFailureRpc(ulong counterId, RpcParams rpcParams)
    {
        WrongDishServed?.Invoke(this);

        var counter = counters.First(x => x.NetworkObjectId == counterId);
        counter.FailureDelivery();
    }
}
