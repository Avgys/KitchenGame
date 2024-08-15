using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dishServedCount;
    [SerializeField] private Button mainMenuButton;

    private void Start()
    {
        GameManager.StateChanged += Gamemode_StateChanged;
        gameObject.SetActive(false);

        mainMenuButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.Shutdown();
            SceneLoader.Load(SceneLoader.Scene.MainMenuScene, false, false);
        });
    }

    private void Gamemode_StateChanged(GameManager.State state)
    {
        if (state == GameManager.State.GameOver)
        {
            gameObject.SetActive(true);
            dishServedCount.text = DeliveryManager.Instance.TotalDishServed.ToString();
        }
        else
            gameObject.SetActive(false);
    }
}
