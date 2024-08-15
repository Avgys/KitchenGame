using System;
using UnityEngine;

public class FadingScript : MonoBehaviour
{
    private bool _isFading = false;
    private Action<float> SetProgress;
    [SerializeField] private float fadingTimeMax = 2f;
    private float fadeTimer;


    private void SetIntensity(float intensity)
    {
        intensity = Mathf.Clamp(intensity, 0f, 1f);
    }

    private void Update()
    {
        if (!_isFading)
            return;

        SetProgress(fadeTimer / fadingTimeMax);
        fadeTimer -= Time.deltaTime;
        if (fadeTimer <= 0)
        {
            gameObject.SetActive(false);
            enabled = false;
            SetProgress = null;
        }
    }

    internal void Fade(Action<float> setProgress)
    {
        fadeTimer = fadingTimeMax;
        _isFading = true;
        SetProgress = setProgress;
        enabled = true;
        gameObject.SetActive(true);
    }
}