using System;
using UnityEngine;

public class LocalPlayerData
{
    public static PlayerData localData;

    private static readonly string PLAYER_DATA = "PlayerData";

    static LocalPlayerData()
    {
        localData = GetDefaultData();
    }

    public static event Action<PlayerData> PlayerDataChanged;

    public static void SetColor(Color color, int colorId)
    {
        localData.Color = color;
        localData.ColorId = colorId;
        SaveData();
    }

    public static void SetName(string name)
    {
        localData.PlayerName = name;
        SaveData();
    }

    private static void SaveData()
    {
        var dataJson = JsonUtility.ToJson(localData);
        PlayerPrefs.SetString(PLAYER_DATA, dataJson);
        PlayerPrefs.Save();

        PlayerDataChanged?.Invoke(localData);
    }

    public static PlayerData GetDefaultData()
    {
        if (PlayerPrefs.HasKey(PLAYER_DATA))
        {
            var dataJson = PlayerPrefs.GetString(PLAYER_DATA);
            return JsonUtility.FromJson<PlayerData>(dataJson);
        }
        else
        {
            localData = new PlayerData
            {
                ColorId = 0,
                Color = Color.white,
                PlayerName = "Default name"
            };

            SaveData();
        }

        return localData;
    }
}