using UnityEngine;

public class StoveWarningUI : MonoBehaviour
{
    [SerializeField] private StoveCounter stove;

    private void Start()
    {
        stove.Burning += gameObject.SetActive;
        gameObject.SetActive(false);
    }
}
