using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputBinding;
using static UnityEngine.UI.Button;

public class BindButton : MonoBehaviour
{
    [SerializeField] private Button button;
    public ButtonClickedEvent onClick { get => button.onClick; }

    private ActionSetting actionSetting;
    public int bindIndex;
    private TextMeshProUGUI textMeshPro;

    private void Awake()
    {
        if (textMeshPro == null)
            textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void UpdateText()
    {
        if (textMeshPro == null)
            textMeshPro = GetComponentInChildren<TextMeshProUGUI>();

        textMeshPro.text = GameInput.Instance.GetBinding(actionSetting.Binding, bindIndex);
    }

    internal void Init(ActionSetting actionSetting, int bindIndex)
    {
        this.bindIndex = bindIndex;
        this.actionSetting = actionSetting;
        UpdateText();
    }
}
