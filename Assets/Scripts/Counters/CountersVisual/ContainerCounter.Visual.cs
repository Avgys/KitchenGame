using UnityEngine;

public class ContainerCounterVisual : BaseCounterVisual<ContainerCounter>
{
    private string OPEN_CLOSE = "OpenClose";
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer[] spriteRenderer;

    protected override void Start()
    {
        //counter = GetComponentInParent<ContainerCounter>();
        counter.Interacted += OpenClose;
        counter.Initiated += SetSprite;
        //ContainerCounter.ProductChanged += SetSprite;        
    }

    public void SetSprite()
    {
        for (int i = 0; i < spriteRenderer.Length; i++)
            spriteRenderer[i].sprite = counter.Sprite;
    }

    private void OpenClose()
    {
        animator.SetTrigger(OPEN_CLOSE);
    }
}
