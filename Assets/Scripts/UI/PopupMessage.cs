using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(FadingScript))]
public class PopupMessage : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;
    private FadingScript fade;
    private Image image;

    //public string Text { get => textMesh.text; set => textMesh.text = value; }

    private void Awake()
    {
        fade = GetComponent<FadingScript>();
        image = GetComponent<Image>();
    }

    public void Show(string text)
    {
        textMesh.text = text;
        gameObject.SetActive(true);
        Awake();
        StartFade();
    }

    private void StartFade()
    {
        fade.Fade((float value) =>
        {
            var color = image.color;
            color.a = value;
            image.color = color;

            color = textMesh.color;
            color.a = value;
            textMesh.color = color;
        });
    }
}
