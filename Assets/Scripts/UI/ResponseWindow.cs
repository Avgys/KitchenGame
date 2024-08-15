using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ResponseWindow : NetworkBehaviour
{
    [SerializeField] private Button sceneButton;
    [SerializeField] private SceneLoader.Scene backScene;
    [SerializeField] private TextMeshProUGUI responseText;

    void Start()
    {
        gameObject.SetActive(false);

        sceneButton.GetComponentInChildren<TextMeshProUGUI>().text = backScene.ToString();
        sceneButton.onClick.AddListener(async () =>
        {
            gameObject.SetActive(false);
            await MultiplayerManager.Singleton.Shutdown();

            if (backScene == SceneLoader.Scene.None)
                return;

            SceneLoader.Load(backScene, false, false);
        });

        if (NetworkManager == null || !NetworkManager.IsServer)
            MultiplayerManager.Singleton.OnConnectionFailed += Singleton_OnConnectionFailed;
    }

    private void Singleton_OnConnectionFailed(string reason)
    {
        if (gameObject != null)
        {
            gameObject.SetActive(true);
            responseText.text = "Response reason:\n" + (string.IsNullOrEmpty(reason) ? "Timeout" : reason);
        }
    }

    public override void OnDestroy()
    {
        if (MultiplayerManager.Singleton != null)
            MultiplayerManager.Singleton.OnConnectionFailed -= Singleton_OnConnectionFailed;

        base.OnDestroy();
    }
}
