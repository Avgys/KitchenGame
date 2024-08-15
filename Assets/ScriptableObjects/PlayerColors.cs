using UnityEngine;

//[CreateAssetMenu(fileName = "PlayerColors", menuName = "Scriptable Objects/PlayerColors")]
public class PlayerColors : ScriptableObject
{
    public Color[] Colors;

    public Color GetColor(uint id)
    {
        return Colors[id];
    }
}
