using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PopupMessages : MonoBehaviour
{
    static PopupMessages Singleton;

    [SerializeField] private PopupMessage prefab;
    [SerializeField] private Transform gridTransform;

    List<PopupMessage> messages = new();

    public void Awake()
    {
        if (Singleton != null)
            Destroy(gameObject);
        Singleton = this;

        ShowMessage("ready");
    }

    public static void ShowMessage(string message)
    {
        if (Singleton == null)
            return;

        var popupMessage = Singleton.GetMessageContainer();

        popupMessage.Show(message);
    }

    private PopupMessage GetMessageContainer()
    {
        var message = messages.FirstOrDefault(x => !x.gameObject.activeSelf);

        if (message == null)
        {
            message = Instantiate(prefab, gridTransform);
            messages.Add(message);
        }

        return message;
    }

    private void OnDestroy()
    {
        if (this == Singleton)
            Singleton = null;
    }
}
