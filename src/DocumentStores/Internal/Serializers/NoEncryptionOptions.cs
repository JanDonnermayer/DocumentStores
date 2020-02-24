using System;

namespace DocumentStores.Internal
{
    internal sealed class NoEncryptionOptions : EncryptionOptions
    {
        public NoEncryptionOptions() : base(Array.Empty<byte>(), Array.Empty<byte>())
        {
        }
    }
}
