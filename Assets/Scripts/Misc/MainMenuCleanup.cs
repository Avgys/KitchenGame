using Unity.Netcode;
using UnityEngine;

public class MainMenuCleanup : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(NetworkManager.Singleton != null)
            Destroy(NetworkManager.Singleton.gameObject);

        if(MultiplayerManager.Singleton != null)
            Destroy(MultiplayerManager.Singleton.gameObject);
    }
}
