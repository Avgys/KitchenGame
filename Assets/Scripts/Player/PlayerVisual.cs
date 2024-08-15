using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerVisual : NetworkBehaviour
{
    [SerializeField] private MeshRenderer headMesh;
    [SerializeField] private MeshRenderer bodyMesh;

    private Material material;

    private void Awake()
    {
        material = new Material(headMesh.material);

        headMesh.material = material;
        bodyMesh.material = material;
    }

    public void Start()
    {
        LoadPlayerData();
        if (MultiplayerManager.Singleton.IsSolo)
            LocalPlayerData.PlayerDataChanged += UpdateColor;
        else
            NetworkData.Singleton.ListChanged += Singleton_ListChanged;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        var playerData = NetworkData.Singleton.GetPlayerDataClientId(OwnerClientId);
        UpdateColor(playerData.Value);
    }

    private void Singleton_ListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        if (NetworkObject.OwnerClientId == changeEvent.Value.ClientId)
        {
            switch (changeEvent.Type)
            {
                case NetworkListEvent<PlayerData>.EventType.Add:
                case NetworkListEvent<PlayerData>.EventType.Insert:
                case NetworkListEvent<PlayerData>.EventType.Value:
                    material.color = changeEvent.Value.Color;
                    break;
            }
        }
    }

    private void LoadPlayerData()
    {
        UpdateColor(LocalPlayerData.localData);
    }

    void UpdateColor(PlayerData playerData)
    {
        Singleton_ListChanged(new NetworkListEvent<PlayerData>
        {
            Type = NetworkListEvent<PlayerData>.EventType.Add,
            Value = playerData
        });
    }

    public override void OnDestroy()
    {
        if (NetworkData.Singleton != null)
            NetworkData.Singleton.ListChanged -= Singleton_ListChanged;

        LocalPlayerData.PlayerDataChanged -= UpdateColor;
    }
}
