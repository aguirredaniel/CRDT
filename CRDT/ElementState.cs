using System;

namespace CRDT
{
    public class ElementState<TKey, TElement> : IComparable<ElementState<TKey, TElement>>
    {
        public TKey Id { get; }
        public TElement Element { get; }

        public long Timestamp { get; }
        public bool Removed { get; }


        public ElementState(TElement element, TKey id, long timestamp, bool removed = false)
        {
            Element = element;
            Id = id;
            Removed = removed;
            Timestamp = timestamp;
        }

        public int CompareTo(ElementState<TKey, TElement> other)
        {
            if (other == null)
                return 1;
            return Timestamp < other.Timestamp ? -1 : Timestamp > other.Timestamp ? 1 : 0;
        }

     
    }
}