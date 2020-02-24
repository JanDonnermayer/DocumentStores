using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DocumentStores.Internal
{
    internal static class SequenceExtender
    {
        public static bool TryExtendToNextLargerSequence<T>(
            IEnumerable<T> source, IEnumerable<int> acceptedLengths, T defaultItem, out IEnumerable<T> extendedSequence
        )
        {
            var lengthDiffSorted = ImmutableSortedSet.CreateRange(
                acceptedLengths.Select(l => l - source.Count()).Where(l => l >= 0)
            );

            if (lengthDiffSorted.Count == 0)
            {
                extendedSequence = source;
                return false;
            }
            else
            {
                extendedSequence = source.Concat(Enumerable.Repeat(defaultItem, lengthDiffSorted[0]));
                return true;
            }
        }
    }
}