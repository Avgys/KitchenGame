using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerAnimator : NetworkBehaviour
{
    private Animator animator;
    private NetworkAnimator netAnimator;
    private Player playerControl;
    private const string WALKING_SPEED = "Speed";
    private const string ANIMATION_SPEED = "AnimationSpeed";

    private const float DEFAULT_SPEED = 5f;

    void Start()
    {
        if (transform.parent != null)
            playerControl = transform.parent.GetComponent<Player>();

        animator = GetComponent<Animator>();
    }

    public void Update()
    {
        if (!IsOwner)
            return;

        var playerSpeed = playerControl?.LinearSpeed ?? 0f;
        
        animator.SetFloat(WALKING_SPEED, playerSpeed);
        animator.SetFloat(ANIMATION_SPEED, playerSpeed / DEFAULT_SPEED);
    }
}
