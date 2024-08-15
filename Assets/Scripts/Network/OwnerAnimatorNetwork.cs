using Unity.Netcode.Components;

public class OwnerAnimatorNetwork : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
