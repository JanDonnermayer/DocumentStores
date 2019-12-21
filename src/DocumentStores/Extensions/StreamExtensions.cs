using System.IO;
using System.Security.Cryptography;

namespace DocumentStores
{    
    static class StreamExtensions
    {
        public static byte[] GetMd5Hash(this Stream stream) => 
            MD5.Create().ComputeHash(stream);
    }
}