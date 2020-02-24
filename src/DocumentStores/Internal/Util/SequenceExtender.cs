using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace DocumentStores.Internal
{
    internal static class SequenceExtender
    {
        /// <summary>
        /// Tries to extend the specified <paramref name="sequence"/>,
        /// using the specified <paramref name="defaultItem"/>, so that its length
        /// equals the next largest value from the specified <paramref name="acceptedLengths"/>.
        /// Returns true and yields the extended sequence if the extension was possible, 
        /// otherwise false, yielding the original sequence.
        /// </summary>
       public static bool TryExtendToNextLargerSequence<T>(
            IEnumerable<T> sequence, IEnumerable<int> acceptedLengths, T defaultItem, out IEnumerable<T> extendedSequence
        )
        {
            var lengthDiffSorted = ImmutableSortedSet.CreateRange(
                acceptedLengths.Select(l => l - sequence.Count()).Where(l => l >= 0)
            );

            if (lengthDiffSorted.Count == 0)
            {
                extendedSequence = sequence;
                return false;
            }
            else
            {
                extendedSequence = sequence.Concat(Enumerable.Repeat(defaultItem, lengthDiffSorted[0]));
                return true;
            }
        }
    }
}