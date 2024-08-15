using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button playerSoloButton;
    [SerializeField] private Button quitButton;

    void Awake()
    {
        playButton.onClick.AddListener(() =>
            SceneLoader.Load(SceneLoader.Scene.LobbyScene, false, false)
        );

        playerSoloButton.onClick.AddListener(() =>
        {
            MultiplayerManager.Singleton.StartSolo();
            SceneLoader.Load(SceneLoader.Scene.CharacterSelectScene, false, true);
        });

        quitButton.onClick.AddListener(() =>
        {
            Debug.Log("Quit game");
            Application.Quit();
        });

        playButton.Select();
    }
}
