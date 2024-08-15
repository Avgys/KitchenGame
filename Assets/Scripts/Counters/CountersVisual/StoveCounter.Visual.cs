using UnityEngine;

public class StoveCounterVisual : BaseCounterVisual<StoveCounter>
{
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private GameObject redStove;

    [SerializeField] private ProgressBar progressBar;

    protected override void Start()
    {
        counter.ProgressChanged += (float value) =>
        {
            progressBar.SetProgress(value);
        };

        counter.EnableChanged += (bool cookingState) =>
        {
            particles.Play();
            progressBar.gameObject.SetActive(cookingState);
            particles.gameObject.SetActive(cookingState);
            redStove.gameObject.SetActive(cookingState);
        };
        //ContainerCounter.ProductChanged += SetSprite;
    }
}
