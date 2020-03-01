using System;
using System.Collections.Generic;

namespace DocumentStores.Internal
{
    internal sealed class NoEncryptionOptions : EncryptionOptions
    {
        public NoEncryptionOptions() : base(Array.Empty<byte>(), Array.Empty<byte>())
        {
        }

        public override EncryptionOptions WithIV(IEnumerable<byte> iv) => this;

        public override EncryptionOptions WithKey(IEnumerable<byte> iv) => this;
    }
}
