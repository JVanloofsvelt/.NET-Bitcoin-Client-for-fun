using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace BTCClient.Encoding
{
    /// <summary>
    /// Base58Check encoding / decoding.
    /// </summary>
    /// <remarks>
    /// See here for more details: https://en.bitcoin.it/wiki/Base58Check_encoding
    /// </remarks>
    public class Base58Check
    {
        public const string Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        static readonly int AlphabetSize = Alphabet.Length;

        /// <summary>
        /// Encodes data with a 4-byte checksum.
        /// </summary>
        /// <param name="data">Data to be encoded.</param>
        /// <returns></returns>
        public static string Encode(byte[] data)
        {
            return EncodePlain(AddCheckSum(data));  // TODO: avoid unnecessary array copy
        }

        /// <summary>
        /// Encodes data in plain Base58, without any checksum.
        /// </summary>
        /// <param name="data">The data to be encoded.</param>
        /// <returns></returns>
        public static string EncodePlain(byte[] data)
        {
            // Decode byte[] to BigInteger
            var integer = data.Aggregate<byte, BigInteger>(0, (current, t) => current * 256 + t);


            // Encode BigInteger to Base58 string
            var result = new List<char>(2 * data.Length);
            
            while (integer > 0)
            {
                var remainder = (int)(integer % AlphabetSize);

                integer /= AlphabetSize;
                result.Add(Alphabet[remainder]);
            }


            // Append '1' for each leading 0 byte
            for (var i = 0; i < data.Length && data[i] == 0; i++)
                result.Add('1');

            return new string(result.Reverse<char>().ToArray());
        }

        /// <summary>
        /// Decodes data in Base58Check format (with 4 byte checksum).
        /// </summary>
        /// <param name="data">Data to be decoded.</param>
        /// <returns>Returns decoded data if valid; throws FormatException if invalid.</returns>
        public static byte[] Decode(string data)
        {
            var dataWithCheckSum = DecodePlain(data);
            var dataWithoutCheckSum = VerifyAndRemoveCheckSum(dataWithCheckSum);

            if (dataWithoutCheckSum == null)
                throw new FormatException("Base58 checksum is invalid");

            return dataWithoutCheckSum;
        }

        /// <summary>
        /// Decodes data in plain Base58, without any checksum.
        /// </summary>
        /// <param name="data">Data to be decoded.</param>
        /// <returns>Returns decoded data if valid; throws FormatException if invalid.</returns>
        public static byte[] DecodePlain(string data)
        {
            // Decode Base58 string to BigInteger 
            BigInteger integer = 0;
            
            for (var i = 0; i < data.Length; i++)
            {
                var digit = Alphabet.IndexOf(data[i]); // Slow? Don't think so, what could be faster?

                if (digit < 0)
                    throw new FormatException($"Invalid Base58 character `{data[i]}` at position {i}");

                integer = integer * AlphabetSize + digit;
            }

            // Encode BigInteger to byte[]
            // Leading zero bytes get encoded as leading '1' characters
            var leadingZeroCount = data.TakeWhile(c => c == '1').Count();
            var leadingZeros = Enumerable.Repeat((byte)0, leadingZeroCount);

            var bytes = integer
                .ToByteArray()
                .Reverse()  // to big endian
                .SkipWhile(b => b == 0);  // strip sign byte

            return leadingZeros.Concat(bytes).ToArray();
        }

        private static byte[] AddCheckSum(byte[] data)
        {
            var checkSum = DoubleSHA256.ComputeChecksum(data);
            return data.Concat(checkSum).ToArray();
        }

        //  Returns null if the checksum is invalid
        private static byte[] VerifyAndRemoveCheckSum(byte[] data)
        {
            var result = data.Take(data.Length - DoubleSHA256.ChecksumSize).ToArray();
            var givenCheckSum = data.Skip(data.Length - DoubleSHA256.ChecksumSize).Take(DoubleSHA256.ChecksumSize).ToArray();
            var correctCheckSum = DoubleSHA256.ComputeChecksum(result);  // TODO: Use a new VerifyChecksum method

            return givenCheckSum.SequenceEqual(correctCheckSum) ? result : null;
        }
    }
}
