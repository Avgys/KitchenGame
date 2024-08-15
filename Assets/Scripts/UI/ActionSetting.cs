using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ActionSetting : MonoBehaviour
{
    public GameInput.Binding Binding;

    public string BindingName { get => nameMesh.text; set => nameMesh.text = value; }

    [SerializeField] TextMeshProUGUI nameMesh;
    [SerializeField] BindButton prefab;
    [SerializeField] Transform keyBindingsParent;

    //public void AddListener(UnityAction action)
    //{
    //    keyBindingButton.onClick.AddListener(action);
    //}

    public void AddButton(int bindIndex, Action<Action> callback)
    {
        var keyBindingButton = Instantiate(prefab, keyBindingsParent);
        
        keyBindingButton.onClick.AddListener(() =>
        {
            callback.Invoke(keyBindingButton.UpdateText);            
        });

        keyBindingButton.Init(this, bindIndex);
        keyBindingButton.gameObject.SetActive(true);
    }
}
