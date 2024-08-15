using TMPro;
using UnityEngine;

public class GameStartCountDownUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;
    private Animator animator;
    private int previousNumber;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        GameManager.StateChanged += GameMode_StateChanged;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        int number = Mathf.CeilToInt(GameManager.Singleton.StartGameTimer);
        countdownText.text = number.ToString();
        if (previousNumber != number)
        {
            previousNumber = number;
            animator.SetTrigger("Countdown");
            SoundManager.Instance.PlayCountdownSound();
        }
    }

    private void GameMode_StateChanged(GameManager.State state) 
        => gameObject.SetActive(state == GameManager.State.CountdownToStart);

    private void OnDestroy()
    {
        GameManager.StateChanged -= GameMode_StateChanged;
    }
}
