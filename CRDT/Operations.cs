using System;
using System.Collections.Generic;
using System.Linq;

namespace CRDT
{
    public class Operations
    {

        public static TElement Select<TElement>(IEnumerable<TElement> set, Predicate2<TElement> predicate)
        {
            if (set == null || !set.Any())
                throw new ArgumentOutOfRangeException(nameof(set), "Empty set.");

            var winner = set.First();

            foreach (var element in set)
                if (predicate.Call(winner, element))
                    winner = element;

            //return set.FirstOrDefault(e => predicate.Call(winner, e));

            return winner;
        }
    }
}