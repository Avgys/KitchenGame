using UnityEngine;

public class ResetStaticEvents : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        CuttingCounter.ResetStatic();
        TrashBin.ResetStatic();
        GameManager.ResetStatic();
        Player.ResetStatic();
    }
}
