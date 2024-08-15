using Unity.Netcode;
using UnityEngine;

public class CuttingCounterVisual : BaseCounterVisual<CuttingCounter>
{
    private string CUTTING = "Cut";
    [SerializeField] private Animator animator;
    [SerializeField] private ProgressBar progressBar;

    protected override void Start()
    {
        //counter = GetComponentInParent<CuttingCounter>();
        counter.ProgressChanged += (float value) =>
        {
            progressBar.gameObject.SetActive(value < 0.1f);
            progressBar.SetProgress(value);
        };
        counter.AlternateInteracted += TriggerCutRpc;
    }

    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    private void TriggerCutRpc()
    {
            animator.SetTrigger(CUTTING);
    }
}
