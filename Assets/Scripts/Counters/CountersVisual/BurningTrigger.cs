using Unity.VisualScripting;
using UnityEngine;

public class BurningTrigger : MonoBehaviour
{
    [SerializeField] StoveCounter stove;

    private float warningSoundTimer;
    private bool burning;

    void Start()
    {
        stove.Burning += (bool value) => burning = value;
    }

    private void Update()
    {
        warningSoundTimer -= Time.deltaTime;
        if (burning && warningSoundTimer < 0)
        {
            float warningSoundTimerMax = .2f;
            warningSoundTimer = warningSoundTimerMax;
            SoundManager.Instance.PlayWarningSound(transform.position);
        }
    }
}
