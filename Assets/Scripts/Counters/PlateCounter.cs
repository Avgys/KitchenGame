using UnityEngine;

public class PlateCounter : SpawnerCounter
{
    [SerializeField] private float spawnTimer;
    private float currentTime;

    protected override void Start()
    {
        base.Start();
        Node.Capacity = defaultNodeCapacity;
        currentTime = spawnTimer;
        GameManager.StateChanged += GameManager_StateChanged;
    }

    private void GameManager_StateChanged(GameManager.State state) => spawning = state == GameManager.State.GamePlaying;

    protected void Update()
    {
        if (IsServer)
            HandleSpawning();
    }

    private void HandleSpawning()
    {
        currentTime -= Time.deltaTime;
        if (currentTime <= 0f && Node.IsFreeSpace)
        {
            SpawnKitchenObjectRpc();
            currentTime = spawnTimer;
        }
    }
}
