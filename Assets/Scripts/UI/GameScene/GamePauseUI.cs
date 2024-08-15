using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GamePauseUI : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private OptionsUI OptionsUI;

    void Start()
    {
        mainMenuButton.onClick.AddListener(async () =>
        {
            await MultiplayerManager.Singleton.Shutdown();
            SceneLoader.Load(SceneLoader.Scene.MainMenuScene, true, false);
            Time.timeScale = 1.0f;
        });

        resumeButton.onClick.AddListener(() =>
        {
            GameManager.Singleton.ToggleGamePauseRpc();
        });

        optionsButton.onClick.AddListener(() =>
        {
            ToggleButtons(false);
            OptionsUI.gameObject.SetActive(!OptionsUI.gameObject.activeSelf);            
        });

        GameManager.Singleton.PauseToggled += (bool paused) =>
        {
            if (paused)
                ToggleButtons(true);

            gameObject.SetActive(paused);
        };

        OptionsUI.OnClosed += () => ToggleButtons(true);
        gameObject.SetActive(false);
    }

    void ToggleButtons(bool value)
    {
        resumeButton.gameObject.SetActive(value);
        mainMenuButton.gameObject.SetActive(value);
        optionsButton.gameObject.SetActive(value);

        if(value)
            resumeButton.Select();
    }
}
