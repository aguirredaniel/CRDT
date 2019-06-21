using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace CRDT
{
    public class OurSet<TKey, TElement> : IEquatable<OurSet<TKey, TElement>>
    {
        private class CompareTo : Predicate2<ElementState<TKey, TElement>>
        {
            public bool Call(ElementState<TKey, TElement> first, ElementState<TKey, TElement> second)
            {
                return first.CompareTo(second) < 0;
            }
        }


        private HashSet<ElementState<TKey, TElement>> _elements = new HashSet<ElementState<TKey, TElement>>();

        public OurSet()
        {
        }

        private OurSet(IEnumerable<ElementState<TKey, TElement>> elementStates)
        {
            _elements = new HashSet<ElementState<TKey, TElement>>(elementStates);
        }


        public void Add(ElementState<TKey, TElement> elementState)
        {
            var conflicts = new HashSet<ElementState<TKey, TElement>> {elementState};

            var rest = new HashSet<ElementState<TKey, TElement>>();

            foreach (var state in GetElements())
            {
                if (state.Id.Equals(elementState.Id))
                    conflicts.Add(state);
                else
                    rest.Add(state);
            }

            var winner = Operations.Select(conflicts, new CompareTo());

            rest.Add(winner);

            _elements = rest;
        }

        public void AddRange(params ElementState<TKey, TElement>[] elements)
        {
            if (elements == null || !elements.Any())
                throw new ArgumentOutOfRangeException(nameof(elements), "Empty elements.");

            foreach (var e in elements)
                Add(e);
        }

        public void Remove(ElementState<TKey, TElement> elementState)
        {
            Add(new ElementState<TKey, TElement>(elementState.Element, elementState.Id, elementState.Timestamp, true));
        }


        public IEnumerable<TElement> LookUp()
        {
            return _elements
                .Where(e => !e.Removed)
                .Select(e => e.Element);
        }

        public OurSet<TKey, TElement> Merge(OurSet<TKey, TElement> anotherOurSet)
        {
            var unionElementsResult = GetElements().Union(anotherOurSet.GetElements());

            var unionGrouped = unionElementsResult
                .GroupBy(e => e.Id)
                .ToDictionary(gr => gr.Key, gr => gr);

            var mergeResult = unionGrouped
                .Select(u => u.Value.Max())
                .OrderBy(v => v.Id)
                .ThenBy(v => v.Timestamp);

            return new OurSet<TKey, TElement>(mergeResult);
        }

        public OurSet<TKey, TElement> Diff(OurSet<TKey, TElement> anotherOurSet)
        {
            var mergeResult = Merge(anotherOurSet);
            var diff = mergeResult.GetElements()
                .Where(e => !anotherOurSet.GetElements().Contains(e));

            return new OurSet<TKey, TElement>(diff);
        }

        public IEnumerable<ElementState<TKey, TElement>> GetElements() => _elements.AsEnumerable();


        public bool Equals(OurSet<TKey, TElement> other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || Equals(_elements, other._elements);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((OurSet<TKey, TElement>) obj);
        }

        public override int GetHashCode()
        {
            return _elements?.GetHashCode() ?? 0;
        }
    }
}