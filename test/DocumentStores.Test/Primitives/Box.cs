using System;
using System.Collections.Generic;

namespace DocumentStores.Test
{
    internal class Box<T>
    {
        public Box(T value) => 
            Value = value;

        public T Value {get;}

        public override bool Equals(object obj) => 
            obj is Box<T> box &&
                EqualityComparer<T>.Default.Equals(Value, box.Value);

        public override int GetHashCode() => 
            HashCode.Combine(Value);
    }


}