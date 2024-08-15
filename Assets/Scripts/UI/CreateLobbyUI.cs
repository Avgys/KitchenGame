using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateLobbyUI : MonoBehaviour
{
    [SerializeField] private Button createLobbyButton;
    [SerializeField] private Toggle isPrivateToggle;
    [SerializeField] private TMP_InputField lobbyNameInput;
    [SerializeField] private Button HideWindow;

    private void Start()
    {
        createLobbyButton.onClick.AddListener(async () =>
        {
            var isCreated = await MultiplayerManager.Singleton.CreateLobbyAsync(lobbyNameInput.text, isPrivateToggle.isOn);
            if (isCreated)
                SceneLoader.Load(SceneLoader.Scene.CharacterSelectScene, false, true);
        });

        HideWindow.onClick.AddListener(() => gameObject.SetActive(false));

        gameObject.SetActive(false);
    }
}