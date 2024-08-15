using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;

public class OptionsUI : MonoBehaviour
{
    [SerializeField] private Button SFXButton;
    [SerializeField] private Button musicButton;
    [SerializeField] private Button closeButton;

    [SerializeField] private TextMeshProUGUI SFXButtonText;
    [SerializeField] private TextMeshProUGUI musicText;

    [SerializeField] private ActionSetting actionSettingPrefab;
    [SerializeField] private Transform bindingsGrid;

    private List<ActionSetting> bindingPool;

    [SerializeField] private GameObject waitForKey;

    private IEnumerable<(GameInput.Binding binding, string name, int bindingsCount)> keyBindings;

    public event Action OnClosed;

    private void Awake()
    {
        bindingPool = new();

        SFXButton.onClick.AddListener(() =>
        {
            SoundManager.Instance.SetVolume();
            UpdateVisual();
        });

        musicButton.onClick.AddListener(() =>
        {
            MusicManager.Instance.ChangeVolume();
            UpdateVisual();
        });

        closeButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
            OnClosed?.Invoke();
        });

        closeButton.Select();
    }

    private void Start()
    {
        GameManager.Singleton.PauseToggled += (bool paused) =>
        {
            if (!paused)
                gameObject.SetActive(false);
        };

        gameObject.SetActive(false);
        GetGameInputSettings();
        UpdateVisual();
    }

    void UpdateVisual()
    {
        SFXButtonText.text = "Sound Effects: "
            + (Mathf.CeilToInt(SoundManager.Instance.Volume * 10f)).ToString();

        musicText.text = "Music sound: "
            + (Mathf.CeilToInt(MusicManager.Instance.Volume * 10f)).ToString();
    }

    void GetGameInputSettings()
    {
        if (bindingPool.Any())
            return;

        keyBindings = GameInput.Instance.GetGameInputSettings().ToArray();

        foreach (var setting in keyBindings)
        {
            var keyBindingsGroup = Instantiate(actionSettingPrefab, bindingsGrid);
            bindingPool.Add(keyBindingsGroup);
            keyBindingsGroup.Binding = setting.binding;
            keyBindingsGroup.BindingName = setting.name;
            keyBindingsGroup.name = setting.name;
            for (int i = 0; i < setting.bindingsCount; i++)
            {
                var bindingIndex = i;
                keyBindingsGroup.AddButton(bindingIndex,
                    (callback) =>
                    {
                        waitForKey.SetActive(true);
                        GameInput.Instance.SetBinding(setting.binding, bindingIndex, () =>
                        {
                            waitForKey.SetActive(false);
                            callback();
                        });
                    });
            }


            keyBindingsGroup.gameObject.SetActive(true);
        }
    }
}
