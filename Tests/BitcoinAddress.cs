using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BTCClient;

namespace Tests
{
    [TestClass]
    public class BitoinAddressTest
    {
        static readonly byte[] publicKey = Utils.HexStringToBytes("0450863AD64A87AE8A2FE83C1AF1A8403CB53F53E486D8511DAD8A04887E5B23522CD470243453A299FA9E77237716103ABC11A1DF38855ED6F2EE187E9C582BA6");
        static readonly string address = "16UwLL9Risc3QfPqBUvKofHmBQ7wMtjvM";

        [TestMethod]
        public void TestCreation()
        {
            var generatedAddress = BitcoinAddress.FromPublicKey(publicKey);
            Assert.AreEqual(generatedAddress, address);
        }
    }
}
