using UnityEngine;
using UnityEngine.UI;

public class ColorSelect : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private GameObject selectUI;

    private static ColorSelect lastSelected;

    public Color Color
    {
        get => image.color;
        set => image.color = value;
    }

    public int ColorId;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(Select);
        if(LocalPlayerData.localData.ColorId == ColorId)
            Select();
        else
            Deselect();
    }

    public void Deselect() => selectUI.SetActive(false);

    public void Select()
    {
        lastSelected?.Deselect();
        lastSelected = this;
        selectUI.SetActive(true);
        LocalPlayerData.SetColor(Color, ColorId);
    }

    private void OnDestroy()
    {
        if(lastSelected == this)
            lastSelected = null;
    }
}
