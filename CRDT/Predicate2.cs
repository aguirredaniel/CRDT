namespace CRDT
{
    public interface Predicate2<in TElement>
    {
        bool Call(TElement first, TElement second);
    }
}