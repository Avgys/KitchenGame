using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private Slider slider;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    public void SetProgress(float value)
    {
        value = Mathf.Clamp01(value);
        if (value > float.Epsilon && value < 0.99f && !gameObject.activeSelf)
            gameObject.SetActive(true);
        else if ((value < float.Epsilon || value > 0.99f) && gameObject.activeSelf)
            gameObject.SetActive(false);
        slider.value = value;
    }
}
