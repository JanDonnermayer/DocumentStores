using System.IO;
using System.Security.Cryptography;

namespace DocumentStores
{
    internal static class StreamExtensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            category: "Security",
            checkId: "CA5351:Do Not Use Broken Cryptographic Algorithms",
            Justification = "Value is not security relevant.")]
        public static byte[] GetMd5Hash(this Stream stream)
        {
            using var md5 = MD5.Create();
            return md5.ComputeHash(stream);
        }
    }
}