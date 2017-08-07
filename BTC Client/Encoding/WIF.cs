using System;
using System.Linq;

namespace BTCClient.Encoding
{
    /// <summary>
    /// Encodes and decodes private keys to and from the Wallet Interchange Format
    /// </summary>
    public static class WIF
    {
        const int PrivateKeySize = 32;
        const int MainNetPrefix = 0x80;

        public static string Encode(byte[] privateKey, byte prefix = MainNetPrefix)
        {
            if (privateKey.Length != PrivateKeySize)
                throw new ArgumentException($"Value should contain {PrivateKeySize} bytes", nameof(privateKey));

            return Base58Check.Encode(new[] { prefix }.Concat(privateKey).ToArray());
        }

        public static byte[] Decode(string wifPrivateKey, byte prefix = MainNetPrefix)
        {
            if (wifPrivateKey.Length > 2 * PrivateKeySize)
                throw new ArgumentException($"Value seems to be unusually long, something probably went wrong", nameof(wifPrivateKey));

            var bytes = Base58Check.Decode(wifPrivateKey);

            if (bytes.Length != PrivateKeySize + 1)
                throw new Exception($"Private key does not contain exactly {PrivateKeySize} bytes (not counting the prefix)");

            if (prefix != bytes[0])
                throw new Exception($"WIF private key is not prefixed with byte '{prefix.ToString("X2")}'");

            return bytes.Skip(1).ToArray();
        }
    }
}
