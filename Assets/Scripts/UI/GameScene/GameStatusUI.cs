using System;
using TMPro;
using UnityEngine;

public class GameStatusUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dishServedCount;
    private string textBase;
    

    private void Start()
    {
        GameManager.StateChanged += Gamemode_StateChanged;
        textBase = dishServedCount.text;

        gameObject.SetActive(false);
    }

    private void Update()
    {
        dishServedCount.text = textBase + DeliveryManager.Instance.TotalDishServed.ToString();
     }

    private void Gamemode_StateChanged(GameManager.State obj) 
        => gameObject.SetActive(obj == GameManager.State.GamePlaying);
}
