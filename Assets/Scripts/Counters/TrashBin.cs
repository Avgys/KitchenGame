using System;

public class TrashBin : BaseCounter
{
    public static event Action<TrashBin> Used;
    public static void ResetStatic() => Used = null;

    public override void Interact(Player source)
    {
        if (!source.Node.IsEmpty)
        {
            var item = source.Node.Pop();
            Used?.Invoke(this);
            item.Value.DestroySelfRpc();
        }
    }
}
