using System;
using System.Linq;
using System.Security.Cryptography;

namespace BTCClient
{
    using Encoding;

    public class BitcoinAddress
    {
        const int PublicKeySize = 65;
        const byte MainNetAddressPrefix = 0x00; 

        public static string FromPublicKey(byte[] publicKey)
        {
            if (publicKey.Length != PublicKeySize)
                throw new ArgumentException($"Value should contain {PublicKeySize} bytes", nameof(publicKey));

            byte[] hash;

            using (var hasher1 = SHA256.Create())
            using (var hasher2 = RIPEMD160.Create())
            {
                hash = hasher2.ComputeHash(hasher1.ComputeHash(publicKey));  // TODO: Use TransformBlock or something to avoid array copy when adding the prefix later
            }

            return Base58Check.Encode(new byte[] { MainNetAddressPrefix }.Concat(hash).ToArray());
        }
    }
}
