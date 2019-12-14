using System;

namespace DocumentStores.Test
{
    internal class ImmutableCounter
    {
        public int Count { get; }

        public ImmutableCounter(int count) => this.Count = count;

        public static ImmutableCounter Default => new ImmutableCounter(0);

        public ImmutableCounter Increment() => new ImmutableCounter(Count + 1);

        public override bool Equals(object obj) =>
            obj is ImmutableCounter counter &&
                   Count == counter.Count;

        public override int GetHashCode() =>
            HashCode.Combine(Count);
    }


}