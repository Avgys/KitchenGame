using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HighlightVisual : BaseVisual
{
    private float _maxIntensity = 0.3f;
    private Coroutine _currentCoroutine;
    private bool _isFading = false;

    private const float YIELD_TIME = 0.001f;
    private const float FADING_TIMEOUT = 0.1f;
    private WaitForSeconds checkTimeout = new WaitForSeconds(YIELD_TIME);
    private float fadeTimeout;

    private IEnumerable<Material> materials;
    private const string EMISSION_COLOR = "_EmissionColor";
    private const string EMISSION = "_EMISSION";

    protected override void Start()
    {
        base.Start();
        materials = GetComponentsInChildren<MeshRenderer>().SelectMany(x => x.materials).ToArray();

        source.PreppedToInteract += Highlight;
    }

    public void Highlight()
    {
        if (!_isFading || _currentCoroutine == null)
            _currentCoroutine = StartCoroutine("Fade");
        else
            ResetTimeout();
    }

    private void ResetTimeout()
    {
        fadeTimeout = FADING_TIMEOUT;
    }

    private IEnumerator Fade()
    {
        ResetTimeout();

        ToggleHighlightModel(true);

        while (fadeTimeout > 0)
        {
            //Debug.Log(timeout);
            fadeTimeout -= YIELD_TIME;
            SetIntensity(fadeTimeout / FADING_TIMEOUT);
            yield return checkTimeout;
        }

        ToggleHighlightModel(false);

        yield break;
    }

    private void SetIntensity(float intensity)
    {
        intensity = Mathf.Clamp(intensity, 0, _maxIntensity);
        foreach (var material in materials)
        {
            var color = material.GetColor(EMISSION_COLOR);
            color = Color.white * intensity;
            material.SetColor(EMISSION_COLOR, color);
        }
    }

    public void ToggleHighlightModel()
    {
        ToggleHighlightModel(!_isFading);
    }

    private void ToggleHighlightModel(bool enable)
    {
        _isFading = enable;
        if (enable)
        {
            foreach (var material in materials)
            {
                material.EnableKeyword(EMISSION);
                material.SetColor(EMISSION_COLOR, Color.white);
            }
        }
        else
        {
            foreach (var material in materials)
            {
                material.DisableKeyword(EMISSION);
            }
        }
    }
}