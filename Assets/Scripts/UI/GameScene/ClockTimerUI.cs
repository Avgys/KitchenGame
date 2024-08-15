using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClockTimerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timer;
    [SerializeField] private Image Circle;

    private void Update()
    {
        timer.text = TimeSpan.FromSeconds(
            GameManager.Singleton.GameTimer.currentTime).ToString(@"mm\:ss\.f");

        Circle.fillAmount = GameManager.Singleton.GameTimer.Percentage;
    }
}
