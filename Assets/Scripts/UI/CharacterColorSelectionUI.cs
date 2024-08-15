using Unity.Netcode;
using UnityEngine;

public class CharacterColorSelectionUI : MonoBehaviour
{
    [SerializeField] private ColorSelect prefab;

    private void Start()
    {
        if (NetworkData.Singleton != null)
            PreparePallete();
        else
            NetworkData.OnSpawned += PreparePallete;

        prefab.gameObject.SetActive(false);
    }

    void PreparePallete()
    {
        var colors = NetworkData.Singleton.PlayerColors.Colors;
        SetColorPalette(colors);
        NetworkData.OnSpawned -= PreparePallete;
    }

    private void SetColorPalette(Color[] colors)
    {
        for (int i = 0; i < colors.Length; i++)
        {
            var colorSelect = Instantiate(prefab, transform);
            colorSelect.gameObject.SetActive(true);
            colorSelect.Color = colors[i];
            colorSelect.ColorId = i;
        }
    }
}