using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryUISuccess : MonoBehaviour
{
    [SerializeField] private DeliveryCounter counter;

    [SerializeField] private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI textMeshPro;

    [SerializeField] private Color successColor;
    [SerializeField] private Color failedColor;

    [SerializeField] private Sprite successSprite;
    [SerializeField] private Sprite failedSprite;

    private Animator animator;

    private const string POPUP_TRIGGER = "POPUP_TRIGGER";

    private void Start()
    {
        animator = GetComponent<Animator>();
        counter.OnDelivery += Counter_OnDelivery; 
        gameObject.SetActive(false);
    }

    private void Counter_OnDelivery(bool delivered)
    {
        gameObject.SetActive(true);
        animator.SetTrigger(POPUP_TRIGGER);
        if (delivered)
        {
            ShowMessage(successColor, successSprite, "DELIVERY\nSUCCESS");
        }
        else
        {
            ShowMessage(failedColor, failedSprite, "DELIVERY\nFAILED");
        }
    }

    private void ShowMessage(Color color, Sprite sprite, string text)
    {
        background.color = color;
        icon.sprite = sprite;
        textMeshPro.text = text;
    }
}
