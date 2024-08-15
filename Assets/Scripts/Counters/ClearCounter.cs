public class ClearCounter : BaseCounter, ICombinable
{
    public KitchenObject Value => StoredItem;

    public override void Interact(Player source)
    {
        base.Interact(source);
        Node.SwapChildren(source.Node);
    }
}
