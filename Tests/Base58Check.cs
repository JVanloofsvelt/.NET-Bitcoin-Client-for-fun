using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BTCClient;
using BTCClient.Encoding;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class Base58CheckTests
    {
        static readonly byte[] testValue;
        static readonly string testValueEncoded;

        static readonly string testValueWithChecksumEncoded;

        static Base58CheckTests()
        {
            testValue = Utils.HexStringToBytes("000111D38E5FC9071FFCD20B4A763CC9AE4F252BB4E48FD66A835E252ADA93FF480D6DD43DC62A641155A5"); // Should match the base58 alphabet when encoded
            testValueEncoded = Base58Check.Alphabet;

            var testValueChecksum = DoubleSHA256.ComputeChecksum(testValue);
            testValueWithChecksumEncoded = Base58Check.EncodePlain(testValue.Concat(testValueChecksum).ToArray());
        }

        /// <summary>
        /// Test a forward and backward pass of the encoding
        /// </summary>
        [TestMethod]
        public void TestEncodeDecode()
        {
            // Without checksum
            string encoded = Base58Check.EncodePlain(testValue);
            Assert.AreEqual(encoded, testValueEncoded);

            byte[] decoded = Base58Check.DecodePlain(encoded);
            CollectionAssert.AreEqual(decoded, testValue);


            // With checksum
            encoded = Base58Check.Encode(testValue);
            Assert.AreEqual(encoded, testValueWithChecksumEncoded);

            decoded = Base58Check.Decode(encoded);
            CollectionAssert.AreEqual(decoded, testValue);
        }

        [TestMethod]
        public void TestEncodingBigArray()
        {
            Base58Check.EncodePlain(Enumerable.Repeat<byte>(0xFF, 1024).ToArray());
        }
    }
}
