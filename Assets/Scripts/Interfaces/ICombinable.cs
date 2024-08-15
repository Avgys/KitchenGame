public interface ICombinable
{
    bool TryCombine(ICombinable kitchenObject, out ICombinable result);
}